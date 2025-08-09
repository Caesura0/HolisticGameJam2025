using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region AnimationZone
    private const string SpeedString = "Speed";
    private const string ThrowString = "Throw";
    private const string AttackString = "Attack";
    private const string PickUpString = "PickUp";
    private int speedBlendId = Animator.StringToHash(SpeedString);
    private int throwTriggerId = Animator.StringToHash(ThrowString);
    private int attackTriggerId = Animator.StringToHash(AttackString);
    private int pickUpTriggerId = Animator.StringToHash(PickUpString);

    [SerializeField] private Animator animator;

    private void TriggerAttack() => animator.SetTrigger(attackTriggerId);
    private void TriggerPickUp() => animator.SetTrigger(pickUpTriggerId);
    private void TriggerThrow() => animator.SetTrigger(throwTriggerId);
    private void UpdateSpeed(float speed) => animator.SetFloat(speedBlendId, speed);
    #endregion

    private void Awake()
    {
        Controls.Instance.OnPlayerMove += UpdateVelocity;
    }
    private void Update()
    {
        HandleMovement();
    }


    [SerializeField]
    private float movementSpeed = 2;
    private Vector3 velocity = Vector3.zero;
    private void UpdateVelocity(Vector2 input)
    {
        Vector2 direction = input.normalized;
        velocity = direction * movementSpeed;
    }

    private void HandleMovement()
    {
        transform.position += velocity * Time.deltaTime;
        UpdateSpeed(GetSpeedNormalized());
    }
    private float GetSpeedNormalized()
    {
        float magnitude = velocity.magnitude;
        return magnitude > 0 ? movementSpeed / magnitude : magnitude;
    }
}
