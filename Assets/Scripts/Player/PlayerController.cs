using System;
using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private void Awake()
    {
        Controls.Instance.OnPlayerMove += UpdateVelocity;
        Controls.Instance.OnPlayerAttack += HandlePickUp;
        FirstAttack = true;
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

    #region PickUp & Throw or Eat
    [SerializeField] private Transform itemHolder;
    [SerializeField] private float pickUpRange = .7f;
    [SerializeField] private LayerMask pickableItemsLayer;
    [SerializeField] private PickableItem pickedItem;
    [SerializeField] private float throwForce = 10f;
    bool FirstAttack;

    private void HandlePickUp()
    {
        if (pickedItem)
        {
            if (pickedItem.TryGetComponent<IAttackable>(out _))
            {
                pickedItem.Release();
                //Run Eating Animation
                pickedItem.gameObject.SetActive(false);
                Debug.Log($"Ate {pickedItem}");
            }
            else
            {
                pickedItem.Throw(velocity.normalized * throwForce);
                TriggerThrow();
                Debug.Log($"Threw {pickedItem}");
            }
            pickedItem = null;
            return;
        }
        else
        {
            #region If holding nothing
            Collider2D[] others =
                Physics2D.OverlapCircleAll(transform.position, pickUpRange, pickableItemsLayer);
            
            if (others.Length == 0)
                return;
            //Debug.Log($"Found {others} in range");
            PickableItem chosenItem = null;
            float distance = float.MaxValue;
            foreach (Collider2D other in others)
            {
                if (!other.TryGetComponent<PickableItem>(out PickableItem item))
                    continue;

                if (chosenItem
                    && (chosenItem.TryGetComponent<IAttackable>(out _)
                    && !other.TryGetComponent<IAttackable>(out _)))
                    continue;
                else if (chosenItem
                    && (!chosenItem.TryGetComponent<IAttackable>(out _)
                    && other.TryGetComponent<IAttackable>(out _)))
                {
                    chosenItem = item;
                    continue;
                }
                
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
            Debug.Log($"PickedUp {pickedItem}");

            if(FirstAttack && pickedItem.TryGetComponent<IAttackable>(out _))
            {
                FirstAttack = false;
                HandlePickUp();
            }
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