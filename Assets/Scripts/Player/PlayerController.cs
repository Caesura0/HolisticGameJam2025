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
        if (HungerHandler.Instance.IsDead())
            return;

        HandleMovement();
        HandleHungerNotification();
    }

    #region MovementZone
    [SerializeField]
    private float movementSpeed = 2;
    private Vector3 velocity = Vector3.zero;

    private void UpdateVelocity(Vector2 input)
    {
        Vector2 direction = input.normalized;
        velocity = direction * movementSpeed;
        float errorMargin = .3f;
        UpdateDirection(directionUpId, direction.y >= errorMargin);
        UpdateDirection(directionRightId, direction.x >= errorMargin);
        UpdateDirection(directionDownId, direction.y <= -errorMargin);
        UpdateDirection(directionLeftId, direction.x <= -errorMargin);
        UpdateSpeed(GetSpeedFractionized());
    }

    private void HandleMovement()
    {
        transform.position += velocity * Time.deltaTime;
    }
    private float GetSpeedFractionized()
    {
        float magnitude = velocity.magnitude;
        return magnitude > 0 ? (1 + movementSpeed / (magnitude)) : 1;
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
        StaminaHandler staminaHandler = StaminaHandler.Instance;
        if (pickedItem)
        {
            if (pickedItem.TryGetComponent<IAttackable>(out _))
            {
                pickedItem.Release();
                //Run Eating Animation
                pickedItem.gameObject.SetActive(false);
                TriggerAttack();
                int foodValue = 1;
                HungerHandler.Instance.Feed(foodValue);
            }
            else
            {
                if (!staminaHandler || !staminaHandler.HasStamina())
                    return;

                pickedItem.Throw(velocity.normalized * throwForce);
                //TriggerThrow();
                staminaHandler.SpendStamina();
                Debug.Log($"Threw {pickedItem.name}");
            }
            pickedItem = null;
            return;
        }
        else
        {
            #region If holding nothing
            if (!staminaHandler || !staminaHandler.HasStamina())
                return;

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
            //TriggerPickUp();
            staminaHandler.SpendStamina();
            chosenItem.PickUp(itemHolder);
            notificationHandler.PlayNotification(NotificationType.Alert);
            //Debug.Log($"PickedUp {pickedItem.name}");
            if(FirstAttack && pickedItem.TryGetComponent<IAttackable>(out _))
            {
                FirstAttack = false;
                HandlePickUp();
            }
            #endregion
        }
    }
    #endregion

    #region Notification
    [SerializeField] private NotificationHandler notificationHandler;
    private void PlayNotification(NotificationType type) => 
        notificationHandler.PlayNotification(type);
    [SerializeField] private float hungerNotificationTimer = 3f;
    private float hungerNotificationTime = 0f;
    private void HandleHungerNotification()
    {
        if (hungerNotificationTime > 0f)
        {
            hungerNotificationTime -= Time.deltaTime;
            return;
        }

        if (HungerHandler.Instance.InHungerRage())
        {
            hungerNotificationTime = hungerNotificationTimer;
            PlayNotification(NotificationType.Hunger);
        }
    }
    #endregion

    #region AnimationZone
    private const string SpeedString = "Speed";
    private const string UpString = "Up";
    private const string RightString = "Right";
    private const string DownString = "Down";
    private const string LeftString = "Left";
    //private const string ThrowString = "Throw";
    private const string AttackString = "Attack";
    //private const string PickUpString = "PickUp";
    private int speedBlendId = Animator.StringToHash(SpeedString);
    private int directionUpId = Animator.StringToHash(UpString);
    private int directionRightId = Animator.StringToHash(RightString);
    private int directionDownId = Animator.StringToHash(DownString);
    private int directionLeftId = Animator.StringToHash(LeftString);
    //private int throwTriggerId = Animator.StringToHash(ThrowString);
    private int attackTriggerId = Animator.StringToHash(AttackString);
    //private int pickUpTriggerId = Animator.StringToHash(PickUpString);

    [SerializeField] private Animator animator;

    private void TriggerAttack() => animator.SetTrigger(attackTriggerId);
    //private void TriggerPickUp() => animator.SetTrigger(pickUpTriggerId);
    //private void TriggerThrow() => animator.SetTrigger(throwTriggerId);
    private void UpdateSpeed(float speed) => animator.SetFloat(speedBlendId, speed);
    private void UpdateDirection(int direction, bool value) => animator.SetBool(direction, value);
    #endregion
}