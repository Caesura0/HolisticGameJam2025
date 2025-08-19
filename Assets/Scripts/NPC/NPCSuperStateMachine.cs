using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class NPCSuperStateMachine : MonoBehaviour
{
    public enum SuperStateType { Calm, Panic, Attacking }

    [SerializeField] SuperStateType startingState = SuperStateType.Calm;
    [SerializeField] Transform player;
    [SerializeField] NotificationHandler notificationHandler;
    [SerializeField] private float movementSpeed = 4f;
    [SerializeField] private float panickedMovementSpeed = 10f;

    private float speedMultiplier = 1f;
    public float GetMovementSpeed() => movementSpeed * speedMultiplier;
    public float GetMovementSpeedPanicked() => panickedMovementSpeed * speedMultiplier;

    private float debuffTimer;
    private bool debuffed;
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
            var player = FindFirstObjectByType<PlayerController>().transform;
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
        HandleDebuffState();
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

    public void ApplySlow(float duration = 2f)
    {
        Debug.Log("PlayerHit by thrown item");
        // Fire any desired events (vfx, sfx, etc)
        onSlimeHit?.Invoke();
        onSlimeHitWithDuration?.Invoke(duration);

        //exit since already in panic mode
        if (currentState == panicState)
            return;

        //Switch to panic state from whatever other state
        SwitchState(SuperStateType.Panic);

        //Drops weapon if in attack state
        if (currentState == attackingState)
        {
            Debug.Log("Dropping Weapon");
            //Drop Weapon Logic
        }
    }

    public bool TryCapture()
    {
        if (IsWeaponEquipped())
        {
            notificationHandler.PlayNotification(NotificationType.KO, false);
            notificationHandler.PlayNotification(NotificationType.Attack);
            return false;
        }
        else
        {
            Debug.Log("Oh no; I'm dead");
            OnPlayerCaught?.Invoke();
            return true;
        }
    }

    private bool IsWeaponEquipped() => currentState == attackingState;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<PickableItem>(out PickableItem item))
            return;

        if (item.BeingHeld)
            return;

        if (item.effectType == StatusEffectType.None)
            return;

        switch (item.effectType)
        {
            case StatusEffectType.None:
                break;
            case StatusEffectType.Stunned:
                {
                    OverwriteStatusEffect(item.effectDuration);
                    speedMultiplier = 0;
                    notificationHandler.PlayNotification(NotificationType.KO);
                    Debug.Log($"{gameObject.name} stunned");
                }
                break;
            case StatusEffectType.Slowed:
                {
                    OverwriteStatusEffect(item.effectDuration);
                    speedMultiplier = .4f;
                    notificationHandler.PlayNotification(NotificationType.Slow);
                    Debug.Log($"{gameObject.name} slowed");
                }
                break;
        }

        if (item.DestroyOnHitNPC)
            Destroy(other.gameObject);
    }

    private void OverwriteStatusEffect(float duration)
    {
        debuffed = true;
        debuffTimer = duration;
    }

    private void HandleDebuffState()
    {
        if (!debuffed)
            return;

        if (debuffTimer > 0)
            debuffTimer -= Time.deltaTime;
        else
            RemoveDebuff();
    }

    private void RemoveDebuff()
    {
        speedMultiplier = 1.0f;
        debuffed = false;
        notificationHandler.ClearNotification();
    }
}