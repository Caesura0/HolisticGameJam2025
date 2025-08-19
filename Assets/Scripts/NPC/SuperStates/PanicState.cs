using UnityEngine;

public class PanicState : INPCSuperState
{
    NPCSuperStateMachine machine;
    Rigidbody2D rb;
    Transform player;
    NPCAnimator animator;

    // Base configuration
    float panicDuration = 4f;
    float safeDistance = 12f;
    float panicSpeed = 10f;

    // State tracking
    float panicEndTime;

    // Slime effect
    bool isSlimed = false;
    float slimeSlowMultiplier = 0.4f;
    float slimeEndTime = 0f;

    // Stun effect
    bool isStunned = false;
    float stunEndTime = 0f;

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
        panicEndTime = Time.time + panicDuration;
        // Don't reset effects - they might have been applied before entering
    }

    public void Tick()
    {
        if (!player) return;

        // STUN CHECK - Complete immobilization
        if (isStunned)
        {
            stunEndTime -= Time.deltaTime;
            if (stunEndTime <= Time.time)
            {
                isStunned = false;
                Debug.Log("Stun wore off!");
            }

            // No movement while stunned
            rb.linearVelocity = Vector2.zero;
            animator?.SetAnimationParameters(0, 0);
            return; // Skip everything else
        }

        // SLIME CHECK - Slow effect
        if (isSlimed)
        {
            if (Time.time >= slimeEndTime)
            {
                isSlimed = false;
                Debug.Log("Slime wore off!");
            }
        }

        // CHECK IF PANIC SHOULD END
        float distToPlayer = Vector2.Distance(rb.position, player.position);
        bool canEndPanic = Time.time >= panicEndTime && !isSlimed && !isStunned;

        if (canEndPanic)
        {
            // Check distance or just end panic
            if (distToPlayer >= safeDistance)
            {
                Debug.Log("Panic ended - reached safe distance");
                machine.SwitchState(NPCSuperStateMachine.SuperStateType.Calm);
                return;
            }
            else if (Time.time >= panicEndTime + 2f) // Grace period
            {
                Debug.Log("Panic ended - timeout");
                machine.SwitchState(NPCSuperStateMachine.SuperStateType.Calm);
                return;
            }
        }

        // FLEE BEHAVIOR
        Vector2 fleeDirection = (rb.position - (Vector2)player.position).normalized;
        float currentSpeed = isSlimed ? panicSpeed * slimeSlowMultiplier : panicSpeed;

        rb.MovePosition(rb.position + fleeDirection * currentSpeed * Time.deltaTime);

        // Update animation
        animator?.SetAnimationParameters(fleeDirection.x, isSlimed ? 0.5f : 1f);
    }

    public void Exit()
    {
        Debug.Log("Exiting Panic State");
        // Clear effects on exit
        isSlimed = false;
        isStunned = false;
    }

    // Effect application methods
    public void ApplySlime(float duration)
    {
        isSlimed = true;
        slimeEndTime = Time.time + duration;
        ExtendPanic(duration); // Also extend panic duration
        Debug.Log($"Slimed for {duration} seconds!");
    }

    public void ApplyStun(float duration)
    {
        isStunned = true;
        stunEndTime = Time.time + duration;
        ExtendPanic(duration); // Also extend panic duration
        Debug.Log($"Stunned for {duration} seconds!");
    }

    public void ExtendPanic(float additionalTime)
    {
        // Extend panic time but don't reduce it
        float newEndTime = Time.time + additionalTime;
        panicEndTime = Mathf.Max(panicEndTime, newEndTime);
    }
}