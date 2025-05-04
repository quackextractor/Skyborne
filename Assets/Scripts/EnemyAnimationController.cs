using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator), typeof(NavMeshAgent))]
public class EnemyAnimationController : MonoBehaviour
{
    private static readonly int IsWalking    = Animator.StringToHash("IsWalking");
    private static readonly int MeleeAttack  = Animator.StringToHash("MeleeAttack");
    private static readonly int Taunt        = Animator.StringToHash("Taunt");

    private enum State { Idle, Walking, MeleeAttack, Taunt }
    private State _currentState;

    private Animator     _anim;
    private NavMeshAgent _agent;
    private Transform    _player;
    private Enemy        _enemy;

    [Header("Taunt Settings")]
    [Tooltip("Seconds between possible taunts")]
    [SerializeField] private float tauntCooldown = 10f;
    private float _nextTauntTime;

    private void Awake()
    {
        _anim          = GetComponent<Animator>();
        _agent         = GetComponent<NavMeshAgent>();
        _enemy         = GetComponent<Enemy>();
        _player        = GameObject.FindGameObjectWithTag("Player").transform;
        _currentState  = State.Idle;

        _agent.updateRotation = false; // prevent Animator from fighting our manual rotation
    }

    private void Update()
    {
        float dist = Vector3.Distance(transform.position, _player.position);
        State targetState;
        if (dist <= _enemy.Stats.range)
            targetState = State.MeleeAttack;
        else if (_agent.velocity.magnitude > 0.1f)
            targetState = State.Walking;
        else
            targetState = State.Idle;

        if (Time.time >= _nextTauntTime && targetState != State.MeleeAttack)
        {
            targetState = State.Taunt;
            _nextTauntTime = Time.time + tauntCooldown;
        }

        if (targetState != _currentState)
            SwitchState(targetState);
    }

    private void SwitchState(State newState)
    {
        _anim.ResetTrigger("MeleeAttack");
        _anim.ResetTrigger("Taunt");
        StopAllCoroutines();

        _currentState = newState;

        switch (_currentState)
        {
            case State.Idle:
                _anim.SetBool(IsWalking, false);
                break;

            case State.Walking:
                _anim.SetBool(IsWalking, true);
                break;

            case State.MeleeAttack:
                if (_agent != null && _agent.isOnNavMesh)
                {
                    _agent.isStopped = true;
                }
                else
                {
                    Debug.LogWarning("Cannot stop NavMeshAgent: Not on NavMesh.");
                }
                _anim.SetBool(IsWalking, false);
                _anim.SetTrigger(MeleeAttack);
                StartCoroutine(ReturnToWalkAfter(_anim.GetCurrentAnimatorStateInfo(0).length));
                break;

            case State.Taunt:
                if (_agent != null && _agent.isOnNavMesh)
                {
                    _agent.isStopped = true;
                }
                else
                {
                    Debug.LogWarning("Cannot stop NavMeshAgent: Not on NavMesh.");
                }
                _anim.SetBool(IsWalking, false);
                _anim.SetTrigger(Taunt);
                StartCoroutine(ReturnToWalkAfter(_anim.GetCurrentAnimatorStateInfo(0).length));
                break;
        }
    }


    private IEnumerator ReturnToWalkAfter(float delay)
    {
        yield return new WaitForSeconds(delay);
        _agent.isStopped = false;
    }
}