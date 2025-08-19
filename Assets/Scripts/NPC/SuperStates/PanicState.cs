using UnityEngine;

public class PanicState : INPCSuperState
{
    enum PanicPhase
    {
        Fleeing,        // Running away at full speed
        CatchingBreath, // Brief pause to recover
        Recovering      // Getting ready to flee again
    }

    NPCSuperStateMachine machine;
    Rigidbody2D rb;
    Transform player;
    NPCAnimator animator;

    float safeDistance = 8f;

    // Phase timing
    float fleeingDuration = 3f;
    float catchBreathDuration = 1.5f;
    float recoveryDuration = 0.5f;

    // State tracking
    PanicPhase currentPhase;
    float phaseEndTime;
    float totalPanicEndTime;

    // Catch breath movement
    Vector2 lastFleeDirection;
    float catchBreathSpeed = 1.5f; // Slow stumble while catching breath

    // Obstacle avoidance
    private int recalculateCounter = 0;
    private Vector2 currentFleeDirection;

    public PanicState(NPCSuperStateMachine machine, Rigidbody2D rb, Transform player, NPCAnimator animator)
    {
        this.machine = machine;
        this.rb = rb;
        this.player = player;
        this.animator = animator;
    }

    public void Enter()
    {
        Debug.Log("Entering Panic State");
        totalPanicEndTime = Time.time + 10f; // Overall panic duration
        StartPhase(PanicPhase.Fleeing);
    }

    public void Tick()
    {
        if (!player) return;

        float distToPlayer = Vector2.Distance(rb.position, player.position);

        if (ShouldExitPanic(distToPlayer))
        {
            Debug.Log("Panic ended - safe distance or timeout");
            machine.SwitchState(NPCSuperStateMachine.SuperStateType.Calm);
            return;
        }

        switch (currentPhase)
        {
            case PanicPhase.Fleeing:
                HandleFleeing(distToPlayer);
                break;
            case PanicPhase.CatchingBreath:
                HandleCatchingBreath(distToPlayer);
                break;
            case PanicPhase.Recovering:
                HandleRecovering(distToPlayer);
                break;
        }

        if (Time.time >= phaseEndTime)
        {
            TransitionToNextPhase(distToPlayer);
        }
    }

    void HandleFleeing(float distToPlayer)
    {
        // Get ideal flee direction (away from player)
        Vector2 idealFleeDirection = (rb.position - (Vector2)player.position).normalized;

        // Check if we're cornered (multiple rays to detect walls behind us)
        bool isCornered = CheckIfCornered(idealFleeDirection);

        Vector2 actualFleeDirection;

        if (isCornered)
        {
            // We're trapped! Find escape route (might be toward player!)
            actualFleeDirection = FindEscapeDirection(idealFleeDirection, distToPlayer);
            Debug.Log("Cornered! Finding escape route...");
        }
        else
        {
            // Normal fleeing with obstacle avoidance
            actualFleeDirection = machine.GetObstacleAvoidedDirection(idealFleeDirection);
        }

        lastFleeDirection = actualFleeDirection;
        float currentSpeed = machine.GetMovementSpeedPanicked();

        if (currentSpeed > 0)
        {
            rb.MovePosition(rb.position + actualFleeDirection * currentSpeed * Time.deltaTime);
        }

        animator?.SetAnimationParameters(actualFleeDirection.x, currentSpeed > 0 ? 1f : 0);
        animator?.SetIsCatchingBreath(false);
    }

    bool CheckIfCornered(Vector2 fleeDirection)
    {
        // Check behind us in a cone
        int blockedDirections = 0;
        float[] angles = { 0f, 30f, -30f };

        foreach (float angle in angles)
        {
            Vector2 checkDir = Quaternion.Euler(0, 0, angle) * fleeDirection;
            RaycastHit2D hit = Physics2D.Raycast(rb.position, checkDir, 1.5f, machine.obstacleLayerMask);

            if (hit.collider != null)
                blockedDirections++;
        }

        // If 2+ directions blocked, we're likely cornered
        return blockedDirections >= 2;
    }

    Vector2 FindEscapeDirection(Vector2 idealFleeDir, float distToPlayer)
    {
        // Try all directions, even toward player if necessary
        Vector2 bestDirection = idealFleeDir;
        float bestScore = -1f;

        // Check 8 directions around us
        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f;
            Vector2 testDir = Quaternion.Euler(0, 0, angle) * Vector2.right;

            // Check if this direction is clear
            RaycastHit2D hit = Physics2D.Raycast(rb.position, testDir, 2f, machine.obstacleLayerMask);

            if (hit.collider == null)
            {
                // Score based on: 
                // 1. How far we can go in this direction
                // 2. Preference for away from player (but not absolute)
                float clearDistance = 2f; // Max check distance
                float dotToPlayer = Vector2.Dot(testDir, -idealFleeDir); // -1 = toward player, 1 = away

                // Score calculation (escape is priority over direction)
                float score = clearDistance + (dotToPlayer * 0.5f); // Escape weighted more than direction

                // Special case: if very close to player, prefer lateral movement
                if (distToPlayer < 2f)
                {
                    float lateralness = 1f - Mathf.Abs(dotToPlayer); // 1 = perpendicular, 0 = direct
                    score += lateralness * 0.5f;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestDirection = testDir;
                }
            }
        }

        return bestDirection;
    }

    public void RecalculatePath()
    {
        // Called when stuck is detected
        recalculateCounter++;

        // Force a new direction calculation
        if (currentPhase == PanicPhase.Fleeing)
        {
            // Add some randomness to break out of stuck patterns
            Vector2 randomOffset = Random.insideUnitCircle * 0.3f;
            lastFleeDirection = (lastFleeDirection + randomOffset).normalized;
        }
    }

    void HandleCatchingBreath(float distToPlayer)
    {
        // Optional: If player gets too close during catch breath, immediately flee again
        // Check if we're not stunned (speedMultiplier > 0)
        if (distToPlayer < 3f && machine.GetMovementSpeedPanicked() > 0.1f)
        {
            Debug.Log("Player too close! Interrupting catch breath!");
            StartPhase(PanicPhase.Fleeing);
            return;
        }

        // Apply catch breath speed with the machine's multiplier
        float currentSpeed = catchBreathSpeed * machine.speedMultiplier;

        // Move slowly or not at all
        if (currentSpeed > 0)
        {
            rb.MovePosition(rb.position + lastFleeDirection * currentSpeed * Time.deltaTime);
        }

        // Update animation - catching breath
        animator?.SetAnimationParameters(lastFleeDirection.x, 0.2f);
        animator?.SetIsCatchingBreath(true);
    }

    void HandleRecovering(float distToPlayer)
    {
        // Brief transition phase - NPC is getting ready to run again
        Vector2 lookDirection = ((Vector2)player.position - rb.position).normalized;

        // Stand still but face away from player
        animator?.SetAnimationParameters(-lookDirection.x, 0.1f);
        animator?.SetIsCatchingBreath(false);
    }

    void TransitionToNextPhase(float distToPlayer)
    {
        // If stunned, stay in catch breath phase
        if (machine.speedMultiplier == 0)
        {
            StartPhase(PanicPhase.CatchingBreath);
            return;
        }

        switch (currentPhase)
        {
            case PanicPhase.Fleeing:
                // After fleeing, catch breath (unless player is very close)
                if (distToPlayer > 4f)
                {
                    StartPhase(PanicPhase.CatchingBreath);
                }
                else
                {
                    // Keep running if player is close
                    StartPhase(PanicPhase.Fleeing);
                }
                break;

            case PanicPhase.CatchingBreath:
                // After catching breath, brief recovery then flee again
                StartPhase(PanicPhase.Recovering);
                break;

            case PanicPhase.Recovering:
                // After recovery, start fleeing again
                StartPhase(PanicPhase.Fleeing);
                break;
        }
    }

    void StartPhase(PanicPhase newPhase)
    {
        currentPhase = newPhase;

        switch (newPhase)
        {
            case PanicPhase.Fleeing:
                phaseEndTime = Time.time + fleeingDuration;
                Debug.Log("Fleeing!");
                break;
            case PanicPhase.CatchingBreath:
                phaseEndTime = Time.time + catchBreathDuration;
                Debug.Log("Catching breath...");
                break;
            case PanicPhase.Recovering:
                phaseEndTime = Time.time + recoveryDuration;
                Debug.Log("Getting ready to run again...");
                break;
        }
    }

    bool ShouldExitPanic(float distToPlayer)
    {
        // Don't exit if still debuffed (the machine is handling this)
        if (machine.debuffed) return false;

        // Exit if we've reached safe distance
        if (distToPlayer >= safeDistance) return true;

        // Exit if total panic time exceeded (with grace period)
        if (Time.time >= totalPanicEndTime) return true;

        return false;
    }

    public void Exit()
    {
        Debug.Log("Exiting Panic State");
        animator?.SetIsCatchingBreath(false);
    }
}