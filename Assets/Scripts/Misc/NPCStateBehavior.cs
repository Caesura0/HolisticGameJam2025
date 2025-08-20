using UnityEngine;

/// <summary>
/// Simple 2D top-down NPC FSM for the Granny game.
/// States:
///  - Idle: milling at party
///  - Curious: glances at player if close (pre-reveal, non-hostile)
///  - Panic: flees from player (after reveal or if nearby NPC is eaten)
///  - Hostile: chases/attacks player in later phases (e.g., hunters)
/// Transitions are driven by distance to player, first-eat reveal, and phase.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class NPCStateBehavior : MonoBehaviour
{
    public enum NPCState { Idle, Curious, Panic, Hostile }

    [Header("Setup")]
    [SerializeField] Transform player;                 // Assign Granny at runtime or in Inspector
    [SerializeField] bool canBecomeHostile = false;    // Villager=false, Hunter=true (phase can flip this on)
    [SerializeField] float idleWanderRadius = 2f;
    [SerializeField] float visionRange = 6f;
    [SerializeField] float curiousRange = 4f;          // gets curious if player close (pre-reveal)
    [SerializeField] float fleeRange = 5f;             // panic if player within this (post-reveal)
    [SerializeField] float attackRange = 2.5f;

    [Header("Movement")]
    [SerializeField] float idleSpeed = 0.8f;
    [SerializeField] float panicSpeed = 2.2f;
    [SerializeField] float hostileSpeed = 1.8f;

    [Header("Runtime (debug)")]
    [SerializeField] NPCState state = NPCState.Idle;
    [SerializeField] bool revealTriggered = false;
    [SerializeField] int currentPhase = 1;

    Rigidbody2D rb;
    Vector2 homePos;
    Vector2 targetPos; // minor wander target
    float repathTimer;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        homePos = transform.position;
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
    }

    void OnEnable()
    {
        GameEvents.OnFirstEat += HandleFirstEat;
        GameEvents.OnPhaseChanged += HandlePhaseChanged;
    }
    void OnDisable()
    {
        GameEvents.OnFirstEat -= HandleFirstEat;
        GameEvents.OnPhaseChanged -= HandlePhaseChanged;
    }

    void HandleFirstEat()
    {
        revealTriggered = true;
        // Most civilians jump straight to Panic when reveal happens and player is nearby
        if (DistanceToPlayer() < fleeRange) state = NPCState.Panic;
    }

    void HandlePhaseChanged(int newPhase)
    {
        currentPhase = newPhase;
        if (currentPhase >= 3) canBecomeHostile = true; // Hunters arrive
    }

    void Update()
    {
        if (player == null) return;

        // High-level transitions
        float d = DistanceToPlayer();

        switch (state)
        {
            case NPCState.Idle:
                // Before reveal: mild curiosity if close
                if (!revealTriggered && d < curiousRange) state = NPCState.Curious;

                // After reveal: panic if player near
                if (revealTriggered && d < fleeRange) state = NPCState.Panic;

                // Phase 3+ hunters might decide to be hostile if they see the player
                if (revealTriggered && canBecomeHostile && d < visionRange && currentPhase >= 3)
                    state = NPCState.Hostile;
                break;

            case NPCState.Curious:
                // If player moves away, go back to Idle
                if (d >= curiousRange * 1.25f) state = NPCState.Idle;

                // Reveal flips to panic or hostile
                if (revealTriggered)
                    state = canBecomeHostile && currentPhase >= 3 ? NPCState.Hostile : NPCState.Panic;
                break;

            case NPCState.Panic:
                // If player far enough, calm back to Idle
                if (d > fleeRange * 1.5f && !canBecomeHostile) state = NPCState.Idle;

                // In higher phases, panicked villagers may later shift to Hostile groups (optional)
                if (canBecomeHostile && currentPhase >= 3 && d < visionRange) state = NPCState.Hostile;
                break;

            case NPCState.Hostile:
                // If player escapes far, drop to Idle (or Panic if reveal and scared variant)
                if (d > visionRange * 2f) state = revealTriggered ? NPCState.Panic : NPCState.Idle;
                break;
        }
    }

    void FixedUpdate()
    {
        switch (state)
        {
            case NPCState.Idle:
                DoIdle();
                break;
            case NPCState.Curious:
                DoCurious();
                break;
            case NPCState.Panic:
                DoPanic();
                break;
            case NPCState.Hostile:
                DoHostile();
                break;
        }
    }

    // ---------- STATE ACTIONS ----------

    void DoIdle()
    {
        // light wander around home
        repathTimer -= Time.fixedDeltaTime;
        if (repathTimer <= 0f)
        {
            targetPos = homePos + Random.insideUnitCircle * idleWanderRadius;
            repathTimer = Random.Range(1.5f, 3.5f);
        }

        MoveTowards(targetPos, idleSpeed);
    }

    void DoCurious()
    {
        // step a bit toward player, but not too close (pre-reveal social curiosity)
        var toPlayer = (Vector2)player.position - rb.position;
        var desired = rb.position + toPlayer.normalized * 0.5f; // small step
        MoveTowards(desired, idleSpeed * 1.1f);
    }

    void DoPanic()
    {
        // run directly away from the player
        var away = (rb.position - (Vector2)player.position).normalized;
        var fleeTarget = rb.position + away * 5f;
        MoveTowards(fleeTarget, panicSpeed);
    }

    void DoHostile()
    {
        float d = DistanceToPlayer();
        if (d > attackRange)
        {
            // close in
            MoveTowards(player.position, hostileSpeed);
        }
        else
        {
            // “Attack” placeholder – call into your combat/capture system
            // e.g., Villager hits / Hunter shoots / Granny can be interrupted
            // Here we just face player and simulate an attack tick.
            Face(player.position);
            // TODO: Hook SFX/animation & damage/capture logic
            // Debug.Log($"{name} attacks Granny!");
        }
    }

    // ---------- HELPERS ----------

    void MoveTowards(Vector2 worldPos, float speed)
    {
        var dir = (worldPos - rb.position);
        var step = dir.normalized * speed;
        rb.MovePosition(rb.position + step * Time.deltaTime);
        if (dir.sqrMagnitude > 0.01f) Face(worldPos);
    }

    void Face(Vector2 worldPos)
    {
        var dir = (worldPos - rb.position);
        if (dir.sqrMagnitude > 0.0001f)
        {
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            rb.rotation = angle; // rotate sprite to face move direction (adjust to your art)
        }
    }

    float DistanceToPlayer() => player ? Vector2.Distance(rb.position, player.position) : 999f;

    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green; Gizmos.DrawWireSphere(transform.position, curiousRange);
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, fleeRange);
        Gizmos.color = Color.red; Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(transform.position, visionRange);
    }
}
