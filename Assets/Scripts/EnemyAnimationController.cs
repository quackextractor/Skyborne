using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator), typeof(NavMeshAgent), typeof(Rigidbody))]
public class EnemyAnimationController : MonoBehaviour
{
    private static readonly int IsWalking   = Animator.StringToHash("IsWalking");
    private static readonly int MeleeAttack = Animator.StringToHash("MeleeAttack");
    private static readonly int Taunt       = Animator.StringToHash("Taunt");
    private static readonly int Hurt        = Animator.StringToHash("Hurt");

    [Header("Taunt Settings")]  
    [Tooltip("Seconds between possible taunts")]
    [SerializeField] private float tauntCooldown = 10f;
    [Tooltip("Fraction to dampen rigidbody velocity on Hurt exit (0 = full stop)")]
    [Range(0f, 1f)]
    [SerializeField] private float velocityDamping = 0.1f;

    private NavMeshAgent _agent;
    private Animator     _anim;
    private Rigidbody    _rigidbody;
    private State        _currentState;
    private Enemy        _enemy;
    private Transform    _player;
    private float        _nextTauntTime;

    private void Awake()
    {
        _anim       = GetComponent<Animator>();
        _agent      = GetComponent<NavMeshAgent>();
        _rigidbody  = GetComponent<Rigidbody>();
        _enemy      = GetComponent<Enemy>();
        _player     = GameObject.FindGameObjectWithTag("Player").transform;
        _agent.updateRotation = false;
        _rigidbody.interpolation = RigidbodyInterpolation.None;
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;
        _currentState = State.Idle;
    }

    private void Update()
    {
        if (_currentState == State.Hurt) 
            return;

        float dist = Vector3.Distance(transform.position, _player.position);
        State targetState = dist <= _enemy.Stats.range
            ? State.MeleeAttack
            : (dist > _enemy.Stats.range ? State.Walking : State.Idle);

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
        // Reset triggers & stop coroutines
        _anim.ResetTrigger(MeleeAttack);
        _anim.ResetTrigger(Taunt);
        _anim.ResetTrigger(Hurt);
        StopAllCoroutines();

        _currentState = newState;

        bool shouldMove = (newState == State.Walking);
        if (_agent.isOnNavMesh)
            _agent.isStopped = !shouldMove;

        _anim.SetBool(IsWalking, shouldMove);

        if (newState == State.Walking && _agent.isOnNavMesh)
            _agent.SetDestination(_player.position);

        switch (newState)
        {
            case State.Idle:
                break;

            case State.Walking:
                break;

            case State.MeleeAttack:
                _anim.SetTrigger(MeleeAttack);
                StartCoroutine(WaitForAnimationToEnd("MeleeAttack"));
                break;

            case State.Taunt:
                _anim.SetTrigger(Taunt);
                StartCoroutine(WaitForAnimationToEnd("Taunt"));
                break;

            case State.Hurt:
                _anim.SetTrigger(Hurt);
                // Rigidbody velocity will be handled on exit via OnHurtFinished
                break;
        }
    }

    private IEnumerator WaitForAnimationToEnd(string stateName)
    {
        // wait until state enters
        while (!_anim.GetCurrentAnimatorStateInfo(0).IsName(stateName))
            yield return null;

        // wait until clip finishes
        var info = _anim.GetCurrentAnimatorStateInfo(0);
        while (info.normalizedTime < 1f)
        {
            yield return null;
            info = _anim.GetCurrentAnimatorStateInfo(0);
        }

        // resume walking
        if (_agent.isOnNavMesh)
        {
            _agent.isStopped = false;
            _agent.SetDestination(_player.position);
        }

        _currentState = State.Walking;
        _anim.SetBool(IsWalking, true);
    }

    /// <summary>
    /// Called from HurtStateBehaviour when the Hurt clip exits.
    /// </summary>
    public void OnHurtFinished()
    {
        if (!_agent.isOnNavMesh)
            return;

        // dampen any existing rigidbody velocity
        if (_rigidbody != null)
        {
            _rigidbody.velocity *= velocityDamping;
            _rigidbody.angularVelocity *= velocityDamping;
        }

        // resume NavMesh movement
        _agent.isStopped = false;
        _agent.SetDestination(_player.position);
        _currentState = State.Walking;
        _anim.SetBool(IsWalking, true);
    }

    public void Stun()
    {
        if (_currentState != State.Hurt)
            SwitchState(State.Hurt);
    }

    private enum State
    {
        Idle,
        Walking,
        MeleeAttack,
        Taunt,
        Hurt
    }
}