using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class NPCSuperStateMachine : MonoBehaviour
{
    public enum SuperStateType { Calm, Panic, Attacking }

    [SerializeField] SuperStateType startingState = SuperStateType.Calm;
    [SerializeField] Transform player;


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
}