using UnityEngine;

public class CalmState : INPCSuperState
{
    NPCSuperStateMachine machine;
    Rigidbody2D rb;
    Transform player;

    float speed = 0.8f;

    public CalmState(NPCSuperStateMachine machine, Rigidbody2D rb, Transform player)
    {
        this.machine = machine;
        this.rb = rb;
        this.player = player;
    }

    public void Enter()
    {
        Debug.Log("Entering Calm State");
        // TODO: Play idle animation
    }

    public void Tick()
    {
        // TODO: Wander or idle here
        // Example: step slightly away if player too close
        float dist = Vector2.Distance(rb.position, player.position);
        if (dist < 3f)
        {
            var dir = (rb.position - (Vector2)player.position).normalized;
            rb.MovePosition(rb.position + dir * speed * Time.deltaTime);
        }
    }

    public void Exit()
    {
        Debug.Log("Exiting Calm State");
    }
}
