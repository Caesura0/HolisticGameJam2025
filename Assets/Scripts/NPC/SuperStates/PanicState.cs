using UnityEngine;

public class PanicState : INPCSuperState
{
    NPCSuperStateMachine machine;
    Rigidbody2D rb;
    Transform player;
    NPCAnimator animator;

    [SerializeField]
    float speed = 10f; 
    [SerializeField]
    float SafeDistance = 10f;
    
    float currentSpeed;

    // Slime effect
    bool isSlimed = false;
    float slimeSlowMultiplier = 0.4f;
    float slimeTimer = 0f;

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
        currentSpeed = speed;
    }

    public void Tick()
    {
        if (!player) return;
        // Update slime timer
        if (isSlimed)
        {
            Debug.Log("slimed");
            slimeTimer -= Time.deltaTime;
            if (slimeTimer <= 0)
            {
                isSlimed = false;
                Debug.Log("Slime wore off!");
            }
        }

        // Calculate flee target position (opposite direction from player)
        Vector2 directionAwayFromPlayer = (rb.position - (Vector2)player.position).normalized;
        Vector2 targetPosition = rb.position + directionAwayFromPlayer * SafeDistance;

        // Apply speed (with slime slow if applicable)
        float actualSpeed = isSlimed ? currentSpeed * slimeSlowMultiplier : currentSpeed;

        rb.MovePosition(Vector2.MoveTowards(rb.position, targetPosition, actualSpeed * Time.deltaTime));

        if (animator != null)
        {
            float directionX = targetPosition.x - rb.position.x;
            animator.SetAnimationParameters(directionX, 1);
        }

        // Check if we've escaped far enough
        float distanceToPlayer = Vector2.Distance(rb.position, player.position);
        if (distanceToPlayer > SafeDistance && !isSlimed)
        {
            machine.SwitchState(NPCSuperStateMachine.SuperStateType.Calm);
        }
    }

    public void Exit()
    {
        Debug.Log("Exiting Panic State");
        isSlimed = false;
    }

    public void ApplySlime(float duration)
    {
        isSlimed = true;
        slimeTimer = duration;
        Debug.Log($"Slimed for {duration} seconds!");
    }
}