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

    float safeDistance = 12f;

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
        // Calculate ideal flee direction
        Vector2 idealFleeDirection = (rb.position - (Vector2)player.position).normalized;

        // Get obstacle-avoided direction
        currentFleeDirection = machine.GetObstacleAvoidedDirection(idealFleeDirection);

        // Store for catch breath phase
        lastFleeDirection = currentFleeDirection;

        // Use the machine's speed with multiplier
        float currentSpeed = machine.GetMovementSpeedPanicked();

        if (currentSpeed > 0)
        {
            // Additional close-range check for walls
            RaycastHit2D wallCheck = Physics2D.Raycast(rb.position, currentFleeDirection, 0.5f, machine.obstacleLayerMask);

            if (wallCheck.collider != null)
            {
                // Wall sliding behavior when too close
                Vector2 slideDirection = Vector2.Perpendicular(wallCheck.normal);

                // Choose slide direction that moves away from player
                if (Vector2.Dot(slideDirection, idealFleeDirection) < 0)
                    slideDirection = -slideDirection;

                currentFleeDirection = slideDirection;
            }

            rb.MovePosition(rb.position + currentFleeDirection * currentSpeed * Time.deltaTime);
        }

        // Update animation
        float animSpeed = currentSpeed > 0 ? (currentSpeed / machine.GetMovementSpeedPanicked()) : 0;
        animator?.SetAnimationParameters(currentFleeDirection.x, animSpeed);
        animator?.SetIsCatchingBreath(false);
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