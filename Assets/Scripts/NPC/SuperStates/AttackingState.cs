using UnityEngine;

public class AttackingState : INPCSuperState
{
    public enum AttackBehaviorType
    {
        Hunter,     // Aggressive pursuit
        Guardian    // Area denial
    }

    enum AttackPhase
    {
        Pursuing,      // Actively chasing
        Circling,      // Close-range intimidation
        Recovering,    // Brief cooldown
        Searching,     // Lost sight
        Guarding       // Guardian-specific patrol
    }

    NPCSuperStateMachine machine;
    Rigidbody2D rb;
    Transform player;
    NPCAnimator animator;
    AttackBehaviorType behaviorType;

    // Current state
    AttackPhase currentPhase;
    float phaseEndTime;
    bool playerDetected = false;

    // Hunter configuration
    float hunterSpeed = 2.1f;
    float hunterPursuitDuration = 6f;
    float hunterCircleDuration = 2f;
    float hunterRecoveryDuration = 1.5f;
    float hunterSearchDuration = 3f;
    float hunterDetectRange = 10f;
    float hunterLoseRange = 12f;

    // Guardian configuration  
    float guardianSpeed = 1.8f;
    float guardianPatrolDuration = 4f;
    float guardianInterceptDuration = 3f;
    float guardianDetectRange = 8f;
    float guardianPatrolRadius = 6f;
    Vector2 guardianHomeBase;
    Vector2 guardianPatrolTarget;
    float patrolAngle = 0f;

    // Shared configuration
    float circleDistance = 3f;
    float circleAngle = 0f;

    // Path recalculation
    private Vector2 currentMoveDirection;

    public AttackingState(NPCSuperStateMachine machine, Rigidbody2D rb, Transform player,
        NPCAnimator animator, AttackBehaviorType behaviorType)
    {
        this.machine = machine;
        this.rb = rb;
        this.player = player;
        this.animator = animator;
        this.behaviorType = behaviorType;
    }

    public void Enter()
    {
        Debug.Log($"Entering Attacking State as {behaviorType}!");

        // Weapon check
        if (!machine.currentWeapon)
        {
            Debug.Log("No weapon! Switching to panic!");
            machine.SwitchState(NPCSuperStateMachine.SuperStateType.Panic);
            return;
        }

        // Initialize based on type
        if (behaviorType == AttackBehaviorType.Guardian)
        {
            guardianHomeBase = rb.position;
            SetNewPatrolTarget();
            StartPhase(AttackPhase.Guarding);
        }
        else
        {
            StartPhase(AttackPhase.Searching);
        }
    }

    public void Tick()
    {
        if (!player) return;

        // Lost weapon check
        if (!machine.currentWeapon)
        {
            Debug.Log("Weapon lost! Panicking!");
            machine.SwitchState(NPCSuperStateMachine.SuperStateType.Panic);
            return;
        }

        float distToPlayer = Vector2.Distance(rb.position, player.position);

        // Behavior-specific logic
        if (behaviorType == AttackBehaviorType.Hunter)
        {
            HandleHunterBehavior(distToPlayer);
        }
        else
        {
            HandleGuardianBehavior(distToPlayer);
        }
    }

    void HandleHunterBehavior(float distToPlayer)
    {
        // Detection check
        if (distToPlayer <= hunterDetectRange && !playerDetected)
        {
            playerDetected = true;
            StartPhase(AttackPhase.Pursuing);
        }

        switch (currentPhase)
        {
            case AttackPhase.Pursuing:
                if (distToPlayer <= circleDistance)
                {
                    StartPhase(AttackPhase.Circling);
                }
                else if (distToPlayer > hunterLoseRange)
                {
                    StartPhase(AttackPhase.Searching);
                }
                else if (Time.time >= phaseEndTime)
                {
                    StartPhase(AttackPhase.Recovering);
                }
                else
                {
                    // Chase with obstacle avoidance
                    Vector2 idealChaseDir = ((Vector2)player.position - rb.position).normalized;
                    currentMoveDirection = machine.GetObstacleAvoidedDirection(idealChaseDir, 2f);

                    // Check for dynamic obstacle prediction (where player might be)
                    Vector2 predictedPlayerPos = (Vector2)player.position + (player.GetComponent<Rigidbody2D>()?.linearVelocity ?? Vector2.zero) * 0.5f;
                    Vector2 predictiveDir = (predictedPlayerPos - rb.position).normalized;

                    // Blend predictive and current
                    currentMoveDirection = Vector2.Lerp(currentMoveDirection,
                        machine.GetObstacleAvoidedDirection(predictiveDir), 0.3f).normalized;

                    //rb.MovePosition(rb.position + currentMoveDirection * hunterSpeed * Time.deltaTime);
                    rb.linearVelocity = currentMoveDirection * hunterSpeed;

                    animator?.SetAnimationParameters(currentMoveDirection.x, 1f);
                }
                break;

            case AttackPhase.Circling:
                if (Time.time >= phaseEndTime || distToPlayer > circleDistance * 2)
                {
                    StartPhase(AttackPhase.Pursuing);
                }
                else
                {
                    CircleStrafeWithAvoidance(distToPlayer, hunterSpeed);
                }
                break;

            case AttackPhase.Recovering:
                if (Time.time >= phaseEndTime)
                {
                    StartPhase(distToPlayer <= hunterDetectRange ? AttackPhase.Pursuing : AttackPhase.Searching);
                }
                else
                {
                    // Stand still or back away slowly
                    animator?.SetAnimationParameters(0, 0);
                }
                break;

            case AttackPhase.Searching:
                if (distToPlayer <= hunterDetectRange)
                {
                    playerDetected = true;
                    StartPhase(AttackPhase.Pursuing);
                }
                else if (Time.time >= phaseEndTime)
                {
                    Debug.Log("Lost player, returning to calm");
                    machine.SwitchState(NPCSuperStateMachine.SuperStateType.Calm);
                }
                else
                {
                    // Random search movement
                    SearchPattern();
                }
                break;
        }
    }

    void HandleGuardianBehavior(float distToPlayer)
    {
        float distToHome = Vector2.Distance(rb.position, guardianHomeBase);

        // Detection within guardian range
        if (distToPlayer <= guardianDetectRange && !playerDetected)
        {
            playerDetected = true;
            StartPhase(AttackPhase.Pursuing);
        }

        switch (currentPhase)
        {
            case AttackPhase.Guarding:
                if (distToPlayer <= guardianDetectRange)
                {
                    StartPhase(AttackPhase.Pursuing);
                }
                else
                {
                    // Patrol around home base
                    PatrolAroundBase();
                }
                break;

            case AttackPhase.Pursuing:
                // Guardian pursuit is more about interception
                if (Time.time >= phaseEndTime || distToPlayer > guardianDetectRange * 1.5f)
                {
                    playerDetected = false;
                    StartPhase(AttackPhase.Guarding);
                }
                else if (distToHome > guardianPatrolRadius * 2f)
                {
                    // Don't chase too far from home
                    StartPhase(AttackPhase.Guarding);
                }
                else
                {
                    // Intercept player
                    Vector2 interceptDir = ((Vector2)player.position - guardianHomeBase).normalized;
                    Vector2 interceptPos = guardianHomeBase + interceptDir * (guardianPatrolRadius * 0.8f);

                    Vector2 moveDir = (interceptPos - rb.position).normalized;
                    //rb.MovePosition(moveDir * guardianSpeed * Time.deltaTime);
                    rb.linearVelocity = moveDir * guardianSpeed;
                    animator?.SetAnimationParameters(moveDir.x, 0.8f);
                }
                break;
        }
    }

    void CircleStrafeWithAvoidance(float currentDistance, float speed)
    {
        circleAngle += Time.deltaTime * 1.2f;

        Vector2 offset = new Vector2(Mathf.Cos(circleAngle), Mathf.Sin(circleAngle)) * circleDistance;
        Vector2 targetPos = (Vector2)player.position + offset;

        Vector2 idealMoveDir = (targetPos - rb.position).normalized;
        Vector2 actualMoveDir = machine.GetObstacleAvoidedDirection(idealMoveDir, 1f);

        // If we can't circle, adjust the angle
        if (Vector2.Dot(actualMoveDir, idealMoveDir) < 0.5f)
        {
            // Reverse circle direction if blocked
            circleAngle -= Time.deltaTime * 2.4f;
        }

        //rb.MovePosition(rb.position + actualMoveDir * speed * Time.deltaTime);
        rb.linearVelocity = actualMoveDir * speed;
        float faceDir = player.position.x > rb.position.x ? 1 : -1;
        animator?.SetAnimationParameters(faceDir, 0.7f);
    }

    public void RecalculatePath()
    {
        // Called when stuck - add some variation
        if (currentPhase == AttackPhase.Circling)
        {
            // Reverse circling direction
            circleAngle += Mathf.PI;
        }
    }

    void PatrolAroundBase()
    {
        Vector2 toTarget = guardianPatrolTarget - rb.position;

        if (toTarget.magnitude < 1f)
        {
            SetNewPatrolTarget();
        }
        else
        {
            Vector2 moveDir = toTarget.normalized;
            //rb.MovePosition(rb.position + moveDir * guardianSpeed * 0.5f * Time.deltaTime);

            rb.linearVelocity = moveDir * guardianSpeed * 0.5f;
            animator?.SetAnimationParameters(moveDir.x, 0.5f);
        }
    }

    void SearchPattern()
    {
        // Simple wandering search
        float searchSpeed = behaviorType == AttackBehaviorType.Hunter ? hunterSpeed * 0.5f : guardianSpeed * 0.5f;
        Vector2 searchDir = new Vector2(Mathf.Sin(Time.time * 2f), Mathf.Cos(Time.time * 2f));
        //rb.MovePosition(rb.position + searchDir * searchSpeed * Time.deltaTime);
        rb.linearVelocity = searchDir * searchSpeed;

        animator?.SetAnimationParameters(searchDir.x, 0.3f);
    }

    void SetNewPatrolTarget()
    {
        patrolAngle += Random.Range(1f, 2f);
        float radius = Random.Range(guardianPatrolRadius * 0.5f, guardianPatrolRadius);
        guardianPatrolTarget = guardianHomeBase + new Vector2(
            Mathf.Cos(patrolAngle) * radius,
            Mathf.Sin(patrolAngle) * radius
        );
    }

    void StartPhase(AttackPhase newPhase)
    {
        currentPhase = newPhase;

        switch (newPhase)
        {
            case AttackPhase.Pursuing:
                phaseEndTime = Time.time + (behaviorType == AttackBehaviorType.Hunter ?
                    hunterPursuitDuration : guardianInterceptDuration);
                Debug.Log("Starting pursuit!");
                break;
            case AttackPhase.Circling:
                phaseEndTime = Time.time + hunterCircleDuration;
                circleAngle = Mathf.Atan2(rb.position.y - player.position.y,
                                          rb.position.x - player.position.x);
                Debug.Log("Circling player!");
                break;
            case AttackPhase.Recovering:
                phaseEndTime = Time.time + hunterRecoveryDuration;
                Debug.Log("Cooldown Recovery...");
                break;
            case AttackPhase.Searching:
                phaseEndTime = Time.time + hunterSearchDuration;
                Debug.Log("Searching for player...");
                break;
            case AttackPhase.Guarding:
                phaseEndTime = Time.time + guardianPatrolDuration;
                Debug.Log("Returning to guard duty");
                break;
        }
    }

    public void Exit()
    {
        Debug.Log($"Exiting Attacking State ({behaviorType})");
        playerDetected = false;
    }
}