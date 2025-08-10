using UnityEngine;

public class PanicState : INPCSuperState
{
    NPCSuperStateMachine machine;
    Rigidbody2D rb;
    Transform player;

    float runSpeed = 2.0f;

    public PanicState(NPCSuperStateMachine machine, Rigidbody2D rb, Transform player)
    {
        this.machine = machine;
        this.rb = rb;
        this.player = player;
    }

    public void Enter()
    {
        Debug.Log("Entering Panic State");
        // TODO: Play panic animation, scream SFX
    }

    public void Tick()
    {
        var dir = (rb.position - (Vector2)player.position).normalized;
        rb.MovePosition(rb.position + dir * runSpeed * Time.deltaTime);
    }

    public void Exit()
    {
        Debug.Log("Exiting Panic State");
    }
}
