using UnityEngine;

public class CalmState : INPCSuperState
{
    enum CalmSubState
    {
        Idle,
        WalkAround
    }

    NPCSuperStateMachine machine;
    Rigidbody2D rb;
    Transform player;
    CalmSubState currentSubState;
    float speed = 4;
    float secondsToWaitInIdle = 0;
    Vector2 locationToMoveTo;
    float movementRadius = 4;
    NPCAnimator Animator;

    public CalmState(NPCSuperStateMachine machine, Rigidbody2D rb, Transform player, NPCAnimator animator)
    {
        this.machine = machine;
        this.rb = rb;
        this.player = player;
        this.Animator = animator;
    }

    public void Enter()
    {
        Debug.Log("Entering Calm State");
        SwitchState(CalmSubState.Idle);
    }

    void SwitchState(CalmSubState state)
    {
        Debug.Log($"Switching to Calm SubState: {state}");
        switch (state)
        {
            case CalmSubState.Idle:
                GetRandomIdleSeconds();
                currentSubState = CalmSubState.Idle;
                break;
            case CalmSubState.WalkAround:
                GetRandomLocation();
                currentSubState = CalmSubState.WalkAround;
                break;
        }
    }

    public void Tick()
    {
        // Check if we have a weapon and player is nearby - time to attack!
        if (machine.currentWeapon && player)
        {
            float distToPlayer = Vector2.Distance(rb.position, player.position);
            if (distToPlayer < 8f) // Detection range for armed NPCs
            {
                Debug.Log("Armed and player detected! Switching to attack!");
                machine.SwitchState(NPCSuperStateMachine.SuperStateType.Attacking);
                return;
            }
        }

        switch (currentSubState)
        {
            case CalmSubState.Idle:
                Idle();
                break;
            case CalmSubState.WalkAround:
                WalkAround();
                break;
        }
    }

    public void Exit()
    {
        Debug.Log("Exiting Calm State");
    }

    void GetRandomIdleSeconds()
    {
        float randomSeconds = Random.Range(2f, 5f);
        secondsToWaitInIdle = randomSeconds;
    }

    void GetRandomLocation()
    {
        float randomX = Random.Range(-movementRadius, movementRadius);
        float randomY = Random.Range(-movementRadius, movementRadius);
        locationToMoveTo = new Vector2(rb.position.x + randomX, rb.position.y + randomY);
    }

    private void WalkAround()
    {
        // Find direction
        float directionX = locationToMoveTo.x - rb.position.x;
        Animator.SetAnimationParameters(directionX, 1); // Set walk animation parameters
        rb.MovePosition(Vector2.MoveTowards(rb.position, locationToMoveTo, speed * Time.deltaTime));

        if (Vector2.Distance(rb.position, locationToMoveTo) < 0.1f)
        {
            SwitchState(CalmSubState.Idle);
        }
    }

    private void Idle()
    {
        secondsToWaitInIdle -= Time.deltaTime;
        Animator.SetAnimationParameters(0, 0); // Set idle animation parameters

        if (secondsToWaitInIdle <= 0)
        {
            SwitchState(CalmSubState.WalkAround);
        }
    }
}