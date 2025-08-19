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

    public float speedMultiplier = 1f;
    public float GetMovementSpeed() => movementSpeed * speedMultiplier;
    public float GetMovementSpeedPanicked() => panickedMovementSpeed * speedMultiplier;

    private float debuffTimer;
    public bool debuffed;
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

    // Stuck detection fields
    private Vector2 lastPosition;
    private float stuckTimer = 0f;
    private float stuckCheckInterval = 0.5f;
    private float stuckThreshold = 0.2f;
    private float unstuckForce = 8f;

    // Obstacle avoidance
    [Header("Obstacle Avoidance")]
    [SerializeField] public LayerMask obstacleLayerMask; // Set in Inspector to include desired blockers.
    [SerializeField] private float obstacleCheckDistance = 10f;

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
        CheckIfStuck();
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

    private void CheckIfStuck()
    {
        // Don't check if stunned
        if (speedMultiplier == 0) return;

        stuckTimer += Time.deltaTime;

        if (stuckTimer >= stuckCheckInterval)
        {
            float distanceMoved = Vector2.Distance(rb.position, lastPosition);

            if (distanceMoved < stuckThreshold && currentState != calmState) // Don't unstuck idle NPCs
            {
                // We're stuck! Apply unstuck force
                Vector2 unstuckDirection = GetUnstuckDirection();
                rb.AddForce(unstuckDirection * unstuckForce, ForceMode2D.Impulse);
                Debug.Log($"{gameObject.name} was stuck, applying unstuck force");

                // Also notify the current state it might want to recalculate
                OnStuckDetected();
            }

            lastPosition = rb.position;
            stuckTimer = 0f;
        }
    }

    private Vector2 GetUnstuckDirection()
    {
        // Try to find the best unstuck direction
        Vector2[] directions = {
            Vector2.up, Vector2.down, Vector2.left, Vector2.right,
            (Vector2.up + Vector2.right).normalized,
            (Vector2.up + Vector2.left).normalized,
            (Vector2.down + Vector2.right).normalized,
            (Vector2.down + Vector2.left).normalized
        };

        foreach (var dir in directions)
        {
            RaycastHit2D hit = Physics2D.Raycast(rb.position, dir, 1f, obstacleLayerMask);
            if (hit.collider == null)
            {
                return dir;
            }
        }

        // If all directions blocked, return random
        return Random.insideUnitCircle.normalized;
    }

    private void OnStuckDetected()
    {
        // Force state recalculation for some states
        if (currentState == panicState)
        {
            panicState.RecalculatePath();
        }
        else if (currentState == attackingState)
        {
            attackingState.RecalculatePath();
        }
    }

    // Helper method for obstacle avoidance that all states can use
    public Vector2 GetObstacleAvoidedDirection(Vector2 desiredDirection, float checkDistance = 0f)
    {
        if (checkDistance == 0f) checkDistance = obstacleCheckDistance;

        // Check if desired direction is clear
        RaycastHit2D hit = Physics2D.Raycast(rb.position, desiredDirection, checkDistance, obstacleLayerMask);

        if (hit.collider == null)
            return desiredDirection; // Path is clear

        // Try alternative directions
        float[] angles = { 45f, -45f, 90f, -90f, 135f, -135f };

        foreach (float angle in angles)
        {
            Vector2 alternativeDir = Quaternion.Euler(0, 0, angle) * desiredDirection;
            RaycastHit2D altHit = Physics2D.Raycast(rb.position, alternativeDir, checkDistance, obstacleLayerMask);

            if (altHit.collider == null)
            {
                // Blend original intent with available path
                return Vector2.Lerp(desiredDirection, alternativeDir, 0.6f).normalized;
            }
        }

        // If all forward directions blocked, try moving perpendicular or backwards
        return Quaternion.Euler(0, 0, 180) * desiredDirection;
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