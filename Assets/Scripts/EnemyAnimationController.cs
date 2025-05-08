using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Animator), typeof(NavMeshAgent))]
public class EnemyAnimationController : MonoBehaviour
{
    private static readonly int IsWalking   = Animator.StringToHash("IsWalking");
    private static readonly int MeleeAttack = Animator.StringToHash("MeleeAttack");
    private static readonly int Taunt       = Animator.StringToHash("Taunt");
    private static readonly int Hurt        = Animator.StringToHash("Hurt");

    [Header("Taunt Settings")]
    [Tooltip("Seconds between possible taunts")]
    [SerializeField] private float tauntCooldown = 10f;

    private NavMeshAgent _agent;
    private Animator       _anim;
    private State          _currentState;
    private Enemy          _enemy;
    private Transform      _player;
    private float          _nextTauntTime;

    private void Awake()
    {
        _anim    = GetComponent<Animator>();
        _agent   = GetComponent<NavMeshAgent>();
        _enemy   = GetComponent<Enemy>();
        _player  = GameObject.FindGameObjectWithTag("Player").transform;
        _agent.updateRotation = false;  // manual rotation only
        _currentState = State.Idle;
    }

    private void Update()
    {
        // If we're playing Hurt, don’t change state until it finishes
        if (_currentState == State.Hurt) return;

        // Decide next state
        float dist = Vector3.Distance(transform.position, _player.position);
        State targetState;
        if (dist <= _enemy.Stats.range)
            targetState = State.MeleeAttack;
        else if (_agent.velocity.magnitude > 0.1f)
            targetState = State.Walking;
        else
            targetState = State.Idle;

        // Taunt cooldown
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
        // Reset all triggers & stop any running coroutines
        _anim.ResetTrigger(MeleeAttack);
        _anim.ResetTrigger(Taunt);
        _anim.ResetTrigger(Hurt);
        StopAllCoroutines();

        _currentState = newState;

        // Stop navigation for any non-walking state
        bool shouldMove = newState == State.Walking || newState == State.Idle;
        if (_agent.isOnNavMesh)
            _agent.isStopped = !shouldMove;

        // Update walking bool
        _anim.SetBool(IsWalking, newState == State.Walking);

        switch (newState)
        {
            case State.Idle:
                // nothing else
                break;

            case State.Walking:
                // nothing else
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
                StartCoroutine(WaitForAnimationToEnd("Hurt"));
                break;
        }
    }

    private IEnumerator WaitForAnimationToEnd(string stateName)
    {
        // Wait until Animator actually enters the state
        while (!_anim.GetCurrentAnimatorStateInfo(0).IsName(stateName))
            yield return null;

        // Then wait until it finishes (normalizedTime ≥ 1)
        var info = _anim.GetCurrentAnimatorStateInfo(0);
        while (info.normalizedTime < 1f)
        {
            yield return null;
            info = _anim.GetCurrentAnimatorStateInfo(0);
        }

        // After animation, resume walking
        if (_agent.isOnNavMesh)
            _agent.isStopped = false;

        _currentState = State.Walking;
        _anim.SetBool(IsWalking, true);
    }

    /// <summary>
    /// Call this externally (e.g. from Target.TakeAttack) to play Hurt
    /// and stun the enemy until the Hurt clip finishes.
    /// </summary>
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
