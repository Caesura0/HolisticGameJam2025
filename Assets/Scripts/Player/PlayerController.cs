using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private void Awake()
    {
        Controls.Instance.OnPlayerMove += UpdateVelocity;
        Controls.Instance.OnPlayerAttack += HandlePickUp;
    }
    private void Update()
    {
        HandleMovement();
    }

    #region MovementZone
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
    #endregion

    #region PickUp
    [SerializeField] private Transform itemHolder;
    [SerializeField] private float pickUpRange = 1f;
    [SerializeField] private LayerMask pickableItemsLayer;
    [SerializeField] private PickableItem pickedItem;
    [SerializeField] private float throwForce = 10f;
    private void HandlePickUp()
    {
        if (pickedItem)
        {
            #region
            pickedItem.Throw(velocity.normalized * throwForce);
            pickedItem = null;
            TriggerThrow();
            return;
            #endregion
        }
        else
        {
            #region If holding nothing
            Collider2D[] others =
                Physics2D.OverlapCircleAll(transform.position, pickUpRange, pickableItemsLayer);
            Debug.Log(others);
            if (others.Length == 0)
                return;
            Debug.Log("Found Items in range");
            PickableItem chosenItem = null;
            float distance = float.MaxValue;
            foreach (Collider2D other in others)
            {
                if (!other.TryGetComponent<PickableItem>(out PickableItem item))
                    continue;

                float itemDistance = Vector2.Distance(item.transform.position, transform.position);
                if (itemDistance < distance)
                {
                    distance = itemDistance;
                    chosenItem = item;
                }
            }
            pickedItem = chosenItem;
            TriggerPickUp();
            chosenItem.PickUp(itemHolder);
            #endregion
        }
    }
    #endregion
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
}
