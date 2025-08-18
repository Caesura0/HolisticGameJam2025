using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class NPCSuperStateMachine : MonoBehaviour
{
    public enum SuperStateType { Calm, Panic, Attacking }

    [Header("State Configuration")]
    [SerializeField] SuperStateType startingState = SuperStateType.Calm;
    [SerializeField] Transform player;

    [Header("Stamina Configuration")]
    [SerializeField] bool useStamina = true; // NEW: Toggle stamina system on/off
    [SerializeField] float maxStamina = 100f;
    [SerializeField] float staminaDrainRatePanic = 15f; // per second while running
    [SerializeField] float staminaDrainRateAttack = 5f; // per second while being threatening
    [SerializeField] float staminaRefillRate = 30f; // per second while catching breath
    [SerializeField] float catchBreathThreshold = 5f; // stamina level that triggers catch breath

    [Header("Weapon Configuration")]
    [SerializeField] bool startWithWeapon = true;
    [SerializeField] GameObject pickaxePrefab; // Prefab to spawn if starting with weapon
    [SerializeField] AttackingState.AttackBehaviorType attackBehaviorType = AttackingState.AttackBehaviorType.Hunter; // NEW: Behavior type
    [SerializeField] float weaponDetectionRadius = 3f;
    [SerializeField] Transform weaponAnchorPoint; // Child transform for weapon position
    [SerializeField] Vector2 weaponOffset = new Vector2(0.5f, 0.5f); // Offset from NPC origin

    // Current stamina (public for state access)
    public float currentStamina { get; private set; }

    // Weapon reference
    public NPCPickaxeWeapon currentWeapon { get; private set; }
    private bool hasDroppedWeapon = false; // Track if we've dropped a weapon

    [Header("Hit Events")]
    public UnityEvent onSlimeHit;
    public UnityEvent onStunned;
    public UnityEvent OnPlayerCaught;
    public UnityEvent OnNPCCounter; // When NPC's have weapon and can't be captured.

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
        currentStamina = maxStamina;

        if (!player)
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj) this.player = playerObj.transform;
        }

        // Create weapon anchor if not assigned
        if (!weaponAnchorPoint)
        {
            GameObject anchor = new GameObject("WeaponAnchor");
            anchor.transform.SetParent(transform);
            anchor.transform.localPosition = weaponOffset;
            weaponAnchorPoint = anchor.transform;
        }
    }

    void Start()
    {
        animator = GetComponent<NPCAnimator>();

        // Create states with stamina configuration and behavior type
        calmState = new CalmState(this, rb, player, animator);
        panicState = new PanicState(this, rb, player, animator,
            maxStamina, staminaDrainRatePanic, staminaRefillRate, catchBreathThreshold);
        attackingState = new AttackingState(this, rb, player, animator, staminaDrainRateAttack, attackBehaviorType);

        // Spawn with weapon if configured
        if (startWithWeapon)
        {
            SpawnStartingWeapon();
        }

        // Set initial state
        SwitchState(startingState);
    }

    void SpawnStartingWeapon()
    {
        if (pickaxePrefab != null)
        {
            // Spawn pickaxe at anchor point
            GameObject weaponObj = Instantiate(pickaxePrefab, weaponAnchorPoint.position, Quaternion.identity);
            NPCPickaxeWeapon weapon = weaponObj.GetComponent<NPCPickaxeWeapon>();

            if (weapon == null)
            {
                weapon = weaponObj.AddComponent<NPCPickaxeWeapon>();
            }

            // Subscribe to weapon events
            weapon.OnWeaponLost += OnWeaponLost;

            // Immediately pick it up
            PickupWeapon(weapon);
            Debug.Log($"{gameObject.name} spawned with weapon!");
        }
        else
        {
            // Create a simple placeholder pickaxe
            GameObject weaponObj = new GameObject("Pickaxe_Placeholder");

            // Add required components
            SpriteRenderer sr = weaponObj.AddComponent<SpriteRenderer>();
            weaponObj.AddComponent<BoxCollider2D>().isTrigger = true;
            NPCPickaxeWeapon weapon = weaponObj.AddComponent<NPCPickaxeWeapon>();

            // Make it look like a weapon (brown rectangle)
            sr.sprite = UnityEngine.Sprite.Create(
                Texture2D.whiteTexture,
                new Rect(0, 0, 4, 4),
                new Vector2(0.5f, 0.5f));
            sr.color = new Color(0.5f, 0.3f, 0.1f); // Brown
            weaponObj.transform.localScale = new Vector3(0.3f, 1f, 1f); // Stick shape

            // Subscribe to weapon events
            weapon.OnWeaponLost += OnWeaponLost;

            // Pick it up immediately
            PickupWeapon(weapon);
            Debug.Log($"{gameObject.name} spawned with placeholder weapon!");
        }
    }

    void Update()
    {
        currentState?.Tick();

        // Only look for weapons if we're calm and don't have one
        if (currentState == calmState && !currentWeapon)
        {
            CheckForWeaponPickup();
        }
    }

    void CheckForWeaponPickup()
    {
        Collider2D[] nearbyObjects = Physics2D.OverlapCircleAll(transform.position, weaponDetectionRadius);

        foreach (var col in nearbyObjects)
        {
            NPCPickaxeWeapon weapon = col.GetComponent<NPCPickaxeWeapon>();
            if (weapon && !weapon.IsHeld())
            {
                PickupWeapon(weapon);

                // Immediately become threatening
                SwitchState(SuperStateType.Attacking);
                break;
            }
        }
    }

    public void PickupWeapon(NPCPickaxeWeapon weapon)
    {
        if (currentWeapon) return;

        currentWeapon = weapon;
        weapon.OnWeaponLost += OnWeaponLost; // Subscribe to events
        weapon.Pickup(weaponAnchorPoint);
        Debug.Log($"{gameObject.name} picked up weapon!");
    }

    public void DropWeapon()
    {
        if (!currentWeapon) return;

        currentWeapon.OnWeaponLost -= OnWeaponLost; // Unsubscribe from events
        currentWeapon.Drop();
        currentWeapon = null;
        hasDroppedWeapon = true; // Mark that we've dropped a weapon
        Debug.Log($"{gameObject.name} dropped weapon!");
    }
 
    public void OnWeaponLost()
    {
        Debug.Log("Weapon was lost! NPC becomes capturable again.");

        // Clear our reference
        if (currentWeapon != null)
        {
            currentWeapon.OnWeaponLost -= OnWeaponLost;
            currentWeapon = null;
        }

        // Be conservative about state changes - let other systems (like OnSlimeHit) handle state logic
        // This callback is mainly for cleanup and making the NPC capturable again
        Debug.Log("Weapon cleared, NPC is now capturable.");
    }

    public bool IsCapturable()
    {
        // Simple: if we have a weapon, we're not capturable
        return currentWeapon == null || !currentWeapon.PreventsCapture();
    }

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

    // Stamina management methods (respect the stamina toggle)
    public void DrainStamina(float amount)
    {
        if (!useStamina) return; // Skip stamina drain if disabled
        currentStamina = Mathf.Max(0, currentStamina - amount);
    }

    public void RefillStamina(float amount)
    {
        if (!useStamina) return; // Skip stamina refill if disabled
        currentStamina = Mathf.Min(maxStamina, currentStamina + amount);
    }

    public void ResetStamina()
    {
        if (!useStamina) return; // Skip stamina reset if disabled
        currentStamina = maxStamina;
    }

    public bool IsExhausted()
    {
        if (!useStamina) return false; // Never exhausted if stamina disabled
        return currentStamina <= catchBreathThreshold;
    }

    public bool IsStaminaEnabled()
    {
        return useStamina;
    }

    public void OnSlimeHit(float duration = 2f)
    {
        onSlimeHit?.Invoke();
        onSlimeHitWithDuration?.Invoke(duration);

        if (currentState == attackingState)
        {
            Debug.Log("Slimed while attacking! Dropping weapon and panicking!");

            // First, disarm the weapon
            currentWeapon?.DisarmNPC(); // This will trigger OnWeaponLost callback

            // Then FORCE panic state (override any OnWeaponLost logic)
            SwitchState(SuperStateType.Panic);

            // Finally, apply slime effect to the panic state
            panicState.ApplySlime(duration);
        }
        else if (currentState == panicState)
        {
            panicState.ApplySlime(duration);
        }
        else if (currentState == calmState)
        {
            SwitchState(SuperStateType.Panic);
            panicState.ApplySlime(duration);
        }
    }

    public void OnStunHit(float duration = 1.5f)
    {
        // Invoke events for listeners (particles, sounds, UI, etc)
        onStunned?.Invoke();
        onStunnedWithDuration?.Invoke(duration);

        if (currentState == attackingState)
        {
            Debug.Log("Stunned while attacking! Dropping weapon and panicking!");

            // First, disarm the weapon
            currentWeapon?.DisarmNPC(); // This will trigger OnWeaponLost callback

            // Then FORCE panic state (override any OnWeaponLost logic)
            SwitchState(SuperStateType.Panic);

            // Finally, apply stun effect to the panic state
            panicState.ApplyStun(duration);
        }
        else if (currentState == panicState)
        {
            // Already panicking, just add stun
            panicState.ApplyStun(duration);
        }
        else if (currentState == calmState)
        {
            // Calm NPC gets stunned - panic!
            SwitchState(SuperStateType.Panic);
            panicState.ApplyStun(duration);
        }
    }

    public void OnCaptured()
    {
        // Invoke events for listeners (particles, sounds, UI, etc) 
        OnNPCCounter.Invoke();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check for ANY projectile effect
        IProjectileEffect effect = other.GetComponent<IProjectileEffect>();
        if (effect != null)
        {
            Debug.Log($"Hit by projectile: {effect.GetEffectDescription()}");
            effect.ApplyEffect(this);
            Destroy(other.gameObject);
            return;
        }
    }

    // Debug visualization
    void OnDrawGizmosSelected()
    {
        // Weapon detection radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, weaponDetectionRadius);

        // Attack behavior visualization (delegate to attacking state if active)
        if (currentState == attackingState && attackingState != null)
        {
            attackingState.OnDrawGizmosSelected();
        }
    }
}