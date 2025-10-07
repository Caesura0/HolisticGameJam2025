using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class NPCSuperStateMachine : MonoBehaviour
{
    public enum SuperStateType { Calm, Panic, Attacking }

    [Header("State Configuration")]
    [SerializeField] SuperStateType startingState = SuperStateType.Calm;
    [SerializeField] PlayerInteractionHandler player;
    [SerializeField] Transform weaponTransform;
    [SerializeField] AttackingState.AttackBehaviorType attackBehaviorType = AttackingState.AttackBehaviorType.Hunter;
    [SerializeField] NotificationHandler notificationHandler;
    [SerializeField] private float movementSpeed = 1f;
    [SerializeField] private float panickedMovementSpeed = 2.2f;

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
    InteractableItem item;

    // States
    public CalmState calmState { get; private set; }
    public PanicState panicState { get; private set; }
    public AttackingState attackingState { get; private set; }



    public bool hasGrannyEatenSomeone = false;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (!player)
            player = FindFirstObjectByType<PlayerInteractionHandler>();
        
        player.OnDevourEvent += HandleFirstEatEvent;
    }
    private void OnEnable()
    {
        player.OnHitTarget += HandleOnHitTarget;  
    }
    private void OnDisable()
    {
        player.OnHitTarget -= HandleOnHitTarget;
    }

    void Start()
    {
        animator = GetComponent<NPCAnimator>();

        calmState = new CalmState(this, rb, player.transform, animator);
        panicState = new PanicState(this, rb, player.transform, animator);
        attackingState = new AttackingState(this, rb, player.transform, animator, attackBehaviorType);


        // Spawn with weapon if configured
        if (startWithWeapon)
        {
            SpawnAndEquipWeapon();
        }


        // this should actually be calm state
        SwitchState(SuperStateType.Calm);
    }

    private void HandleFirstEatEvent(ConsumableItem _)
    {
        player.OnDevourEvent -= HandleFirstEatEvent;
        hasGrannyEatenSomeone = true;
        SwitchState(startingState);
    }

    void SpawnAndEquipWeapon()
    {
        if (!weaponPrefab)
        {
            //Debug.LogWarning($"{gameObject.name}: No weapon prefab assigned!");
            SwitchState(startingState);
            return;
        }

        GameObject weaponObj = Instantiate(weaponPrefab, weaponTransform.position, Quaternion.identity);
        NPCWeapon weapon = weaponObj.GetComponent<NPCWeapon>();

        if (weapon)
        {
            currentWeapon = weapon;
            weapon.Pickup(transform); // Will position above head

            //Debug.Log($"{gameObject.name} is armed!");
            SwitchState(SuperStateType.Attacking);
        }
    }
    public void DropWeapon()
    {
        if (!currentWeapon)
            return;

        currentWeapon.Drop();
        currentWeapon = null;
        GameEvents.InvokeOnDisarmed(item);
    }

    void Update()
    {
        currentState?.Tick();
        HandleDebuffState();
        CheckIfStuck();
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
                //Debug.Log($"{gameObject.name} was stuck, applying unstuck force");

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

    public void SwitchState(SuperStateType newState)
    {

        if (!hasGrannyEatenSomeone)
        {
            return;
        }
        
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
    public void HandleOnHitTarget(InteractableItem sender, InteractableItem target)
    {
        if (target != item)
            return;

        switch (sender.EffectType)
        {
            case StatusEffectType.None:
                break;
            case StatusEffectType.Stunned:
                Stun(sender.EffectDuration);
                break;
            case StatusEffectType.Slowed:
                Slow(sender.EffectDuration);
                break;
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
            //Debug.Log("Oh no; I'm dead");
            OnPlayerCaught?.Invoke();
            return true;
        }
    }

    private bool IsWeaponEquipped() => currentWeapon != null;

    public void Stun(float duration)
    {
        OverwriteStatusEffect(duration);
        speedMultiplier = 0;
        rb.linearVelocity = Vector2.zero; // Stop movement
        notificationHandler.PlayNotification(NotificationType.KO);
        //Debug.Log($"{gameObject.name} stunned");
        DropWeapon();
    }
    public void Slow(float duration)
    {
        OverwriteStatusEffect(duration);
        speedMultiplier = .4f;
        notificationHandler.PlayNotification(NotificationType.Slow);
        //Debug.Log($"{gameObject.name} slowed");
        DropWeapon();
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