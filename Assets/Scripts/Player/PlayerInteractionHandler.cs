using System;
using UnityEngine;

[RequireComponent(typeof(PlayerNotificationHandler))]
public class PlayerInteractionHandler : MonoBehaviour
{
    public event Action OnDevourEvent;
    public event Action OnThrowEvent;
    public event Action OnTryCaptureEvent;
    public event Action OnFailedCaptureEvent;

    [field: SerializeField] public Transform itemHolder {  get; private set; }
    [SerializeField] private LayerMask pickableItemsLayer;
    [SerializeField] private float pickUpRange = .7f;
    [SerializeField] private float throwForce = 10f;

    private PlayerMovementHandler playerMovementHandler;
    private Interactable pickedItem;
    bool FirstAttack = true;

    private void Awake() => playerMovementHandler = GetComponent<PlayerMovementHandler>();
    private void Start() => Controls.Instance.OnPlayerAttack += HandlePickUp;

    private void HandlePickUp()
    {
        if (pickedItem)
        {
            if (pickedItem.IsEatable)
            {
                pickedItem.Release();
                pickedItem.gameObject.SetActive(false);
                OnDevourEvent?.Invoke();
                int foodValue = 1;
                HungerHandler.Instance.Feed(foodValue);
                AudioManager.Instance.PlayEatSound();
            }
            else
            {
                pickedItem.Throw(playerMovementHandler.Velocity.normalized * throwForce);
                OnThrowEvent?.Invoke();
                Debug.Log($"Threw {pickedItem.name}");
            }
            pickedItem = null;
            return;
        }
        else
        {
            #region If holding nothing
            if (!TryPickItemInRange(out pickedItem))
                return;

            if (pickedItem.TryGetComponent<NPCSuperStateMachine>(out NPCSuperStateMachine enemy))
            {
                if (!enemy.IsCapturable())
                {
                    Debug.Log("Shouldn't capture");
                    pickedItem = null;
                    OnTryCaptureEvent?.Invoke();
                    OnFailedCaptureEvent?.Invoke();
                    return;
                }

                // Only pick up if capturable
                pickedItem.Grab(itemHolder);

                if (FirstAttack)
                {
                    FirstAttack = false;
                    HandlePickUp();
                    GameEvents.RaiseFirstEat();
                }
                else
                    OnTryCaptureEvent?.Invoke();
            }
            else
            {
                OnTryCaptureEvent?.Invoke();
                pickedItem.Grab(itemHolder);
            }
            #endregion
        }
    }

    private bool TryPickItemInRange(out Interactable chosenItem)
    {
        Collider2D[] others =
            Physics2D.OverlapCircleAll(transform.position, pickUpRange, pickableItemsLayer);

        chosenItem = null;

        if (others.Length == 0)
            return false;

        float distance = float.MaxValue;
        foreach (Collider2D other in others)
        {
            if (!other.TryGetComponent<Interactable>(out Interactable item))
                continue;

            if (chosenItem && (chosenItem.IsEatable && !item.IsEatable))
                continue;
            else if (chosenItem && (!chosenItem.IsEatable && item.IsEatable))
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
        return true;
    }
}