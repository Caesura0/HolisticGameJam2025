// Attacking State with Hunter and Guardian Behavior Types
using UnityEngine;

public class AttackingState : INPCSuperState
{
    public enum AttackBehaviorType
    {
        Hunter,     // Actively pursues player, forces movement
        Guardian    // Guards area, patrols, creates denial zones
    }

    NPCSuperStateMachine machine;
    Rigidbody2D rb;
    Transform player;
    NPCAnimator animator;

    // Behavior configuration
    AttackBehaviorType behaviorType;
    float lightStaminaDrainRate = 5f;

    // Hunter behavior variables
    float hunterSpeed = 6f;           // Faster pursuit speed
    float hunterDetectionRange = 10f; // Longer detection range
    float hunterLoseRange = 18f;      // When to give up chase

    // Guardian behavior variables  
    float guardianSpeed = 4f;         // Slower patrol speed
    float guardianDetectionRange = 8f; // Shorter detection range
    float guardianPatrolRadius = 6f;   // Area to patrol around
    Vector2 guardianHomeBase;          // Center point to guard
    Vector2 guardianPatrolTarget;      // Current patrol destination

    // Current behavior state
    bool isPlayerDetected = false;
    float lastPlayerSightTime = 0f;
    float maxChaseTime = 8f; // How long to remember player location

    public AttackingState(NPCSuperStateMachine machine, Rigidbody2D rb, Transform player,
        NPCAnimator animator, float staminaDrainRate, AttackBehaviorType behaviorType)
    {
        this.machine = machine;
        this.rb = rb;
        this.player = player;
        this.animator = animator;
        this.lightStaminaDrainRate = staminaDrainRate > 0 ? staminaDrainRate : 5f;
        this.behaviorType = behaviorType;
    }

    public void Enter()
    {
        Debug.Log($"Entering Attacking State as {behaviorType}!");

        // Check if we have a weapon
        if (!machine.currentWeapon)
        {
            Debug.Log("No weapon! Returning to calm state.");
            machine.SwitchState(NPCSuperStateMachine.SuperStateType.Calm);
            return;
        }

        // Initialize based on behavior type
        switch (behaviorType)
        {
            case AttackBehaviorType.Hunter:
                Debug.Log("Hunter mode: Will actively pursue player!");
                break;
            case AttackBehaviorType.Guardian:
                Debug.Log("Guardian mode: Will protect this area!");
                guardianHomeBase = rb.position; // Set current position as home base
                SetNewPatrolTarget();
                break;
        }

        isPlayerDetected = false;
        lastPlayerSightTime = 0f;
    }

    public void Tick()
    {
        if (!player) return;

        // If we lost our weapon somehow and we're not already transitioning states, handle it
        if (!machine.currentWeapon)
        {
            // Let OnSlimeHit or other systems handle state transitions if they need to
            // Only default to panic if no other system is handling it
            Debug.Log("Weapon lost in attacking state - defaulting to panic.");
            machine.SwitchState(NPCSuperStateMachine.SuperStateType.Panic);
            return;
        }

        // Only drain stamina if stamina system is enabled
        if (machine.IsStaminaEnabled())
        {
            machine.DrainStamina(lightStaminaDrainRate * Time.deltaTime);

            // If exhausted while attacking, panic!
            if (machine.IsExhausted())
            {
                Debug.Log("Exhausted while attacking! Too tired to be threatening - panicking!");
                machine.SwitchState(NPCSuperStateMachine.SuperStateType.Panic);
                return;
            }
        }

        // Behavior-specific logic
        switch (behaviorType)
        {
            case AttackBehaviorType.Hunter:
                HandleHunterBehavior();
                break;
            case AttackBehaviorType.Guardian:
                HandleGuardianBehavior();
                break;
        }
    }

    void HandleHunterBehavior()
    {
        float distToPlayer = Vector2.Distance(rb.position, player.position);

        // Detection logic
        if (distToPlayer <= hunterDetectionRange)
        {
            isPlayerDetected = true;
            lastPlayerSightTime = Time.time;
        }

        // Give up chase if player escapes for too long
        if (isPlayerDetected && (distToPlayer > hunterLoseRange || Time.time - lastPlayerSightTime > maxChaseTime))
        {
            Debug.Log("Hunter lost track of player, returning to calm.");
            machine.SwitchState(NPCSuperStateMachine.SuperStateType.Calm);
            return;
        }

        if (isPlayerDetected)
        {
            // Active pursuit!
            Vector2 dirToPlayer = ((Vector2)player.position - rb.position).normalized;
            rb.MovePosition(rb.position + dirToPlayer * hunterSpeed * Time.deltaTime);

            // Face the player and look aggressive
            animator?.SetAnimationParameters(dirToPlayer.x, 1f);

            Debug.DrawLine(rb.position, player.position, Color.red); // Debug visualization
        }
        else
        {
            // Patrol/search behavior when not detected
            Vector2 searchMovement = Random.insideUnitCircle.normalized * (hunterSpeed * 0.5f) * Time.deltaTime;
            rb.MovePosition(rb.position + searchMovement);
            animator?.SetAnimationParameters(searchMovement.x, 0.5f);
        }
    }

    void HandleGuardianBehavior()
    {
        float distToPlayer = Vector2.Distance(rb.position, player.position);
        float distToHome = Vector2.Distance(rb.position, guardianHomeBase);

        // Detection logic (shorter range than hunter)
        if (distToPlayer <= guardianDetectionRange)
        {
            isPlayerDetected = true;
            lastPlayerSightTime = Time.time;
        }

        // Stop detecting if player gets far enough away
        if (isPlayerDetected && distToPlayer > guardianDetectionRange * 1.5f)
        {
            isPlayerDetected = false;
            Debug.Log("Guardian lost sight of player, returning to patrol.");
        }

        if (isPlayerDetected)
        {
            // Push player away from guarded area
            Vector2 pushDirection = ((Vector2)player.position - guardianHomeBase).normalized;
            Vector2 interceptPosition = guardianHomeBase + pushDirection * (guardianPatrolRadius * 0.8f);

            Vector2 moveDirection = (interceptPosition - rb.position).normalized;
            rb.MovePosition(rb.position + moveDirection * guardianSpeed * Time.deltaTime);

            // Face the player threateningly
            float directionX = player.position.x > rb.position.x ? 1 : -1;
            animator?.SetAnimationParameters(directionX, 0.8f);

            Debug.DrawLine(guardianHomeBase, interceptPosition, Color.yellow); // Debug visualization
        }
        else
        {
            // Patrol around home base
            Vector2 dirToPatrolTarget = (guardianPatrolTarget - rb.position).normalized;
            rb.MovePosition(rb.position + dirToPatrolTarget * guardianSpeed * Time.deltaTime);

            animator?.SetAnimationParameters(dirToPatrolTarget.x, 0.6f);

            // Reached patrol target, set new one
            if (Vector2.Distance(rb.position, guardianPatrolTarget) < 1f)
            {
                SetNewPatrolTarget();
            }

            // Occasionally brandish weapon while patrolling
            if (Random.value < 0.005f)
            {
                machine.currentWeapon?.StartSwing();
            }
        }

        // If too far from home base, return (prevents guardian from being lured away)
        if (distToHome > guardianPatrolRadius * 2f)
        {
            Debug.Log("Guardian too far from home, returning to base.");
            guardianPatrolTarget = guardianHomeBase;
        }
    }

    void SetNewPatrolTarget()
    {
        // Random point around home base
        float randomAngle = Random.Range(0f, Mathf.PI * 2f);
        float randomDistance = Random.Range(guardianPatrolRadius * 0.3f, guardianPatrolRadius);

        guardianPatrolTarget = guardianHomeBase + new Vector2(
            Mathf.Cos(randomAngle) * randomDistance,
            Mathf.Sin(randomAngle) * randomDistance
        );
    }

    public void Exit()
    {
        Debug.Log($"Exiting Attacking State ({behaviorType})");

        // End any weapon animation
        machine.currentWeapon?.EndSwing();

        isPlayerDetected = false;
    }

    // Debug visualization
    public void OnDrawGizmosSelected()
    {
        if (behaviorType == AttackBehaviorType.Hunter)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(rb.position, hunterDetectionRange);
            Gizmos.color = Color.darkRed;
            Gizmos.DrawWireSphere(rb.position, hunterLoseRange);
        }
        else if (behaviorType == AttackBehaviorType.Guardian)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(guardianHomeBase, guardianPatrolRadius);
            Gizmos.color = Color.orange;
            Gizmos.DrawWireSphere(rb.position, guardianDetectionRange);
            Gizmos.color = Color.cyan;
        }
    }
}