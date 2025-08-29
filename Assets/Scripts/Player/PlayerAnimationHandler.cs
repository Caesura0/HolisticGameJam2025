using UnityEngine;

public class PlayerAnimationHandler : MonoBehaviour
{
    private const string SpeedString = "Speed";
    private const string UpString = "Up";
    private const string RightString = "Right";
    private const string DownString = "Down";
    private const string LeftString = "Left";
    //private const string ThrowString = "Throw";
    private const string AttackString = "Attack";
    private const string DieString = "Die";
    //private const string PickUpString = "PickUp";
    private int speedBlendId = Animator.StringToHash(SpeedString);
    private int directionUpId = Animator.StringToHash(UpString);
    private int directionRightId = Animator.StringToHash(RightString);
    private int directionDownId = Animator.StringToHash(DownString);
    private int directionLeftId = Animator.StringToHash(LeftString);
    //private int throwTriggerId = Animator.StringToHash(ThrowString);
    private int attackTriggerId = Animator.StringToHash(AttackString);
    private int deathTriggerId = Animator.StringToHash(DieString);
    //private int pickUpTriggerId = Animator.StringToHash(PickUpString);

    [SerializeField] private Animator animator;

    private void TriggerAttack() => animator.SetTrigger(attackTriggerId);
    private void TriggerDeath() => animator.SetTrigger(deathTriggerId);
    //private void TriggerPickUp() => animator.SetTrigger(pickUpTriggerId);
    //private void TriggerThrow() => animator.SetTrigger(throwTriggerId);
    private void UpdateSpeed(float speed) => animator.SetFloat(speedBlendId, speed);
    private void UpdateDirection(int direction, bool value) => animator.SetBool(direction, value);

    PlayerMovementHandler movementHandler;
    private void Awake()
    {
        if(TryGetComponent<PlayerMovementHandler>(out movementHandler))
        {
            movementHandler.OnUpdateSpeed += UpdateSpeed;
            movementHandler.OnUpdateSpeed += HandleMovementDirectionUpdate;
        }
        if (TryGetComponent<PlayerInteractionHandler>(out PlayerInteractionHandler interactionHandler))
            interactionHandler.OnDevourEvent += TriggerAttack;
        GameManager gameManager = GameManager.Instance;
        if (gameManager)
            gameManager.OnStarvedToDeath += HandleDeathTrigger;
    }

    private void HandleMovementDirectionUpdate(float _)
    {
        UpdateDirection(directionUpId, movementHandler.movingUp);
        UpdateDirection(directionRightId, movementHandler.movingRight);
        UpdateDirection(directionDownId, movementHandler.movingDown);
        UpdateDirection(directionLeftId, movementHandler.movingLeft);
    }
    private void HandleDeathTrigger() => TriggerDeath();
}