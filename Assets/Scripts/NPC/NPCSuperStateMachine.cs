using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Rigidbody2D))]
public class NPCSuperStateMachine : MonoBehaviour
{
    public enum SuperStateType { Calm, Panic, Attacking }

    [Header("State Configuration")]
    [SerializeField] SuperStateType startingState = SuperStateType.Calm;
    [SerializeField] Transform player;
    [SerializeField] AttackingState.AttackBehaviorType attackBehaviorType = AttackingState.AttackBehaviorType.Hunter;

    [Header("Hit Events")]
    public UnityEvent onSlimeHit;
    public UnityEvent onStunned;
    public UnityEvent OnPlayerCaught;
    public UnityEvent OnNPCCounter; // When NPC's have weapon and can't be captured.

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
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj) this.player = playerObj.transform;
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

    public void OnSlimeHit(float duration = 2f)
    {
        onSlimeHit?.Invoke();
        onSlimeHitWithDuration?.Invoke(duration);

        if (currentWeapon != null)
        {
            currentWeapon.Drop();
            currentWeapon = null;
            Debug.Log("Weapon knocked away by slime!");
        }

        SwitchState(SuperStateType.Panic);
        panicState?.ApplySlime(duration);
    }

    public void OnStunHit(float duration = 1.5f)
    {
        onStunned?.Invoke();
        onStunnedWithDuration?.Invoke(duration);


        if (currentWeapon != null)
        {
            currentWeapon.Drop();
            currentWeapon = null;
            Debug.Log("Weapon knocked away by stun!");
        }

        SwitchState(SuperStateType.Panic);
        panicState?.ApplyStun(duration);
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
}