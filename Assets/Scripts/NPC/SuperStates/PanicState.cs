using UnityEngine;

public class PanicState : INPCSuperState
{
    NPCSuperStateMachine machine;
    Rigidbody2D rb;
    Transform player;
    NPCAnimator animator;

    [SerializeField]
    float SafeDistance = 10f;

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
    }

    public void Tick()
    {
        if (!player) return;

        // Calculate flee target position (opposite direction from player)
        Vector2 directionAwayFromPlayer = (rb.position - (Vector2)player.position).normalized;
        Vector2 targetPosition = rb.position + directionAwayFromPlayer * SafeDistance;

        rb.MovePosition(Vector2.MoveTowards(rb.position, targetPosition, machine.GetMovementSpeedPanicked() * Time.deltaTime));

        if (animator != null)
        {
            float directionX = targetPosition.x - rb.position.x;
            animator.SetAnimationParameters(directionX, 1);
        }

        // Check if we've escaped far enough
        float distanceToPlayer = Vector2.Distance(rb.position, player.position);
        if (distanceToPlayer > SafeDistance)
        {
            machine.SwitchState(NPCSuperStateMachine.SuperStateType.Calm);
        }
    }

    public void Exit()
    {
        Debug.Log("Exiting Panic State");
    }
}