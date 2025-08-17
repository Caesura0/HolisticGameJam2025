using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class NPCSuperStateMachine : MonoBehaviour
{
    public enum SuperStateType { Calm, Panic, Attacking }

    [SerializeField] SuperStateType startingState = SuperStateType.Calm;
    [SerializeField] Transform player;

    // New
    [Header("Hit Events")]
    public UnityEvent onSlimeHit;
    public UnityEvent onStunned;
    public UnityEvent OnPlayerCaught;

    // Custom UnityEvents
    public class SlimeHitEvent : UnityEvent<float> { }
    public SlimeHitEvent onSlimeHitWithDuration;


    Rigidbody2D rb;
    INPCSuperState currentState;
    NPCAnimator animator;

    // States
    public CalmState calmState { get; private set; }
    public PanicState panicState { get; private set; }
    public AttackingState attackingState { get; private set; }

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        if (!player)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player) this.player = player.transform;
        }

    }

    void Start()
    {
        animator = GetComponent<NPCAnimator>();
        // Instantiate state scripts, passing this machine and needed refs
        calmState = new CalmState(this, rb, player, animator);
        panicState = new PanicState(this, rb, player, animator);
        attackingState = new AttackingState(this, rb, player, animator);
        // Set initial state
        SwitchState(startingState);
    }

    void Update()
    {
        currentState?.Tick();
    }

    public void SwitchState(SuperStateType newState)
    {
        currentState?.Exit();

        switch (newState)
        {
            case SuperStateType.Calm: currentState = calmState; break;
            case SuperStateType.Panic: currentState = panicState; break;
            case SuperStateType.Attacking: currentState = attackingState; break;//this could also be called "hunting" state
        }

        currentState.Enter();
    }

    // Hit Handling

    public void OnSlimeHit(float duration = 2f)
    {
        // Fire any desired events (vfx, sfx, etc)
        onSlimeHit?.Invoke();
        onSlimeHitWithDuration?.Invoke(duration);

        // handle states
        if (currentState == attackingState)
        {
            Debug.Log("Dropping weapon and panicking!");
            SwitchState(SuperStateType.Panic);
            panicState.ApplySlime(duration);
        }
        else if (currentState == panicState)
        {
            // already panicking; just apply slime.
            panicState.ApplySlime(duration);
        } else if (currentState == calmState)
        {
            // Surprise! Time to panic.
            SwitchState(SuperStateType.Panic);
            panicState.ApplySlime(duration);
        }
    }

    public void OnCaughtByPlayer()
    {
        OnPlayerCaught?.Invoke();
        // Handle being eaten!
        Debug.Log("Oh no; I'm dead");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Slime"))
        {
            Debug.Log("In collider w/ Slime tag");
            // Get slow from projectile or just hardcode that mf otherwise.
            var projectile = other.GetComponent<SlimeProjectile>();
            float duration = projectile ? projectile.slowDuration : 2f;

            OnSlimeHit(duration);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Player")) {
            OnCaughtByPlayer();
        }
    }


}