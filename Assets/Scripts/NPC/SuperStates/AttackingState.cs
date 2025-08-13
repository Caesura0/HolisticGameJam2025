using UnityEngine;

public class AttackingState : INPCSuperState
{
    NPCSuperStateMachine machine;
    Rigidbody2D rb;
    Transform player;

    float chaseSpeed = 1.5f;
    float attackRange = 2f;

    public AttackingState(NPCSuperStateMachine machine, Rigidbody2D rb, Transform player, NPCAnimator animator)
    {
        this.machine = machine;
        this.rb = rb;
        this.player = player;
    }

    public void Enter()
    {
        Debug.Log("Entering Attacking State");
        // TODO: Draw weapon, attack animation
    }

    public void Tick()
    {
        float dist = Vector2.Distance(rb.position, player.position);
        if (dist > attackRange)
        {
            var dir = ((Vector2)player.position - rb.position).normalized;
            rb.MovePosition(rb.position + dir * chaseSpeed * Time.deltaTime);
        }
        else
        {
            // TODO: Trigger attack logic
            Debug.Log("Attacking player!");
        }
    }

    public void Exit()
    {
        Debug.Log("Exiting Attacking State");
    }
}
