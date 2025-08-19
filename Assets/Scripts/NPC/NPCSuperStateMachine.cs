using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class NPCSuperStateMachine : MonoBehaviour, IWeapon
{
    public enum SuperStateType { Calm, Panic, Attacking }

    [Header("State Configuration")]
    [SerializeField] SuperStateType startingState = SuperStateType.Calm;
    [SerializeField] Transform player;
    [SerializeField] AttackingState.AttackBehaviorType attackBehaviorType = AttackingState.AttackBehaviorType.Hunter;
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

    [Header("Weapon Configuration")]
    [SerializeField] bool startWithWeapon = true;
    [SerializeField] GameObject weaponPrefab;
    // Weapon reference
    public NPCWeapon currentWeapon { get; private set; }

    // Custom UnityEvents
    public class SlimeHitEvent : UnityEvent<float> {}
    public SlimeHitEvent onSlimeHitWithDuration;

    public class StunHitEvent : UnityEvent<float> {}
    public StunHitEvent onStunnedWithDuration;

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

        calmState = new CalmState(this, rb, player, animator);
        panicState = new PanicState(this, rb, player, animator);
        attackingState = new AttackingState(this, rb, player, animator, attackBehaviorType);

        // Spawn with weapon if configured
        if (startWithWeapon)
        {
            SpawnAndEquipWeapon();
        }

        SwitchState(startingState);
    }

    void SpawnAndEquipWeapon()
    {
        if (!weaponPrefab)
        {
            Debug.LogWarning($"{gameObject.name}: No weapon prefab assigned!");
            SwitchState(startingState);
            return;
        }

        GameObject weaponObj = Instantiate(weaponPrefab, transform.position, Quaternion.identity);
        NPCWeapon weapon = weaponObj.GetComponent<NPCWeapon>();

        if (weapon)
        {
            currentWeapon = weapon;
            weapon.Pickup(transform); // Will position above head

            Debug.Log($"{gameObject.name} is armed!");
            SwitchState(SuperStateType.Attacking);
        }
    }

    void Update()
    {
        currentState?.Tick();
        HandleDebuffState();
    }

    public void DropWeapon()
    {
        if (!currentWeapon) return;
        currentWeapon.Drop();
        currentWeapon = null;
        Debug.Log($"{gameObject.name} dropped weapon!");
    }
 
    public void OnWeaponLost()
    {
        Debug.Log("Weapon was lost! NPC becomes capturable again.");
        if (currentWeapon != null)
        {
            currentWeapon = null;
        }

        Debug.Log("Weapon cleared, NPC is now capturable.");
    }

    public bool IsCapturable() => currentWeapon == null;

    public void SwitchState(SuperStateType newState)
    {
        currentState?.Exit();

        switch (newState)
        {
            case SuperStateType.Calm:
                currentState = calmState;
                break;
            case SuperStateType.Panic:
                currentState = panicState;
                break;
            case SuperStateType.Attacking:
                currentState = attackingState;
                break;
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

    private bool IsWeaponEquipped() => currentWeapon != null;

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
                    DropWeapon();
                }
                break;
            case StatusEffectType.Slowed:
                {
                    OverwriteStatusEffect(item.effectDuration);
                    speedMultiplier = .4f;
                    notificationHandler.PlayNotification(NotificationType.Slow);
                    Debug.Log($"{gameObject.name} slowed");
                    DropWeapon();
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