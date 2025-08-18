// Panic State with Simplified Stamina System - Full recovery when catching breath
using UnityEngine;

public class PanicState : INPCSuperState
{
    enum PanicSubState
    {
        Running,
        CatchingBreath
    }

    NPCSuperStateMachine machine;
    Rigidbody2D rb;
    Transform player;
    NPCAnimator animator;

    [SerializeField] float speed = 10f;
    [SerializeField] float SafeDistance = 10f;
    
    // Stun effect (similar to slime but complete immobilization)
    bool isStunned = false;
    float stunTimer = 0f;

    float currentSpeed;
    PanicSubState currentSubState;

    // Simplified stamina system
    float staminaDrainRate;
    float catchBreathThreshold;
    float catchBreathEndTime = 0f; // When breath catching will complete
    float catchBreathDuration = 2.5f; // Longer duration to create more separation
    float postBreathStaminaBuffer = 0f; // Extra stamina after catching breath

    // Slime effect (keep this as it's a core mechanic)
    bool isSlimed = false;
    float slimeSlowMultiplier = 0.4f;
    float slimeTimer = 0f;

    public PanicState(NPCSuperStateMachine machine, Rigidbody2D rb, Transform player,
        NPCAnimator animator, float maxStamina, float staminaDrainRate,
        float unused_staminaRefillRate, float catchBreathThreshold) // Keep parameters for compatibility
    {
        this.machine = machine;
        this.rb = rb;
        this.player = player;
        this.animator = animator;
        this.staminaDrainRate = staminaDrainRate;
        this.catchBreathThreshold = catchBreathThreshold;
    }

    public void Enter()
    {
        Debug.Log("Entering Panic State");
        currentSpeed = speed;
        currentSubState = PanicSubState.Running;
        postBreathStaminaBuffer = 0f; // Reset stamina buffer on entry

        // If stamina is enabled and already exhausted when entering panic, immediately catch breath
        if (machine.IsStaminaEnabled() && machine.IsExhausted())
        {
            StartCatchingBreath();
        }
    }

    public void Tick()
    {
        if (!player) return;

        // STUN CHECK
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0)
            {
                isStunned = false;
                Debug.Log("Stun wore off!");
            }

            rb.linearVelocity = Vector2.zero;

            // Optional: Show stunned animation
            if (animator != null)
            {
                animator.SetAnimationParameters(0, 0);  // Idle/dizzy pose
            }

            return;  // SKIP EVERYTHING ELSE - no movement, no state changes!
        }

        // SLIME TIME
        if (isSlimed)
        {
            slimeTimer -= Time.deltaTime;
            if (slimeTimer <= 0)
            {
                isSlimed = false;
                Debug.Log("Slime wore off!");
            }
        }

        switch (currentSubState)
        {
            case PanicSubState.Running:
                HandleRunning();
                break;
            case PanicSubState.CatchingBreath:
                HandleCatchingBreath();
                break;
        }
    }

    void HandleRunning()
    {
        if (machine.IsStaminaEnabled())
        {
            // Check if we're in the post-breath buffer period (reduced stamina drain)
            float actualDrainRate = staminaDrainRate;
            if (Time.time < postBreathStaminaBuffer)
            {
                actualDrainRate *= 0.3f; // Much slower drain for a few seconds after catching breath
            }

            machine.DrainStamina(actualDrainRate * Time.deltaTime);

            // Check if exhausted (only if stamina is enabled)
            if (machine.IsExhausted())
            {
                StartCatchingBreath();
                return;
            }
        }

        // Calculate flee direction
        Vector2 directionAwayFromPlayer = (rb.position - (Vector2)player.position).normalized;
        Vector2 targetPosition = rb.position + directionAwayFromPlayer * SafeDistance;

        // Apply speed with slime modifier if applicable
        float actualSpeed = isSlimed ? currentSpeed * slimeSlowMultiplier : currentSpeed;
        rb.MovePosition(Vector2.MoveTowards(rb.position, targetPosition, actualSpeed * Time.deltaTime));

        // Update animation
        if (animator != null)
        {
            float directionX = targetPosition.x - rb.position.x;
            animator.SetAnimationParameters(directionX, 1);
            animator.SetIsCatchingBreath(false);
        }

        // Check if we've escaped far enough and aren't slimed
        float distanceToPlayer = Vector2.Distance(rb.position, player.position);

        // Transition logic depends on whether stamina is enabled
        bool canReturnToCalm = false;
        if (machine.IsStaminaEnabled())
        {
            // With stamina: need good distance, not slimed, and have decent stamina buffer
            float requiredStamina = catchBreathThreshold * 3f; // Require more stamina buffer
            canReturnToCalm = distanceToPlayer > SafeDistance * 1.2f && !isSlimed && machine.currentStamina > requiredStamina;
        }
        else
        {
            // Without stamina: just need distance and not be slimed
            canReturnToCalm = distanceToPlayer > SafeDistance && !isSlimed;
        }

        if (canReturnToCalm)
        {
            machine.SwitchState(NPCSuperStateMachine.SuperStateType.Calm);
        }
    }

    void HandleCatchingBreath()
    {
        // Stop movement completely
        rb.linearVelocity = Vector2.zero;

        // Still face away from player for animation
        if (animator != null)
        {
            float directionX = player.position.x < rb.position.x ? 1 : -1;
            animator.SetAnimationParameters(directionX, 0);
        }

        // Check if catch breath duration is complete
        if (Time.time >= catchBreathEndTime)
        {
            EndCatchingBreath();
        }
    }

    void StartCatchingBreath()
    {
        Debug.Log("NPC is exhausted! Catching breath...");
        currentSubState = PanicSubState.CatchingBreath;
        catchBreathEndTime = Time.time + catchBreathDuration;

        if (animator != null)
        {
            animator.SetIsCatchingBreath(true);
        }
    }

    void EndCatchingBreath()
    {
        Debug.Log("Breath caught! Full stamina restored!");
        currentSubState = PanicSubState.Running;

        // Only restore stamina if stamina system is enabled
        if (machine.IsStaminaEnabled())
        {
            machine.ResetStamina();
        }

        if (animator != null)
        {
            animator.SetIsCatchingBreath(false);
        }
    }

    public void Exit()
    {
        Debug.Log("Exiting Panic State");
        isSlimed = false;

        // Make sure to reset the catching breath animation
        if (animator != null)
        {
            animator.SetIsCatchingBreath(false);
        }

    }

    public void ApplySlime(float duration)
    {
        isSlimed = true;
        slimeTimer = duration;
        postBreathStaminaBuffer = 0f;
        Debug.Log($"Slimed for {duration} seconds!");

        // Being slimed while catching breath interrupts it
        if (currentSubState == PanicSubState.CatchingBreath)
        {
            EndCatchingBreath();
        }
    }

    public void ApplyStun(float duration)
    {
        isStunned = true;
        stunTimer = duration;
        Debug.Log($"Stunned for {duration} seconds! Can't move!");
    }
}