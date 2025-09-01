using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerNotificationHandler))]
public class PlayerInteractionHandler : MonoBehaviour
{
    public event Action OnDevourEvent;
    public event Action OnThrowEvent;
    public event Action OnTryCaptureEvent;
    public event Action<int> OnItemPicked;
    public event Action<int> OnItemPositioned;
    public event Action<int> OnHitTarget;

    bool FirstAttack = true;
    [field: SerializeField] public Transform itemHolder {  get; private set; }
    [SerializeField] private float throwPower = 10f;
    [SerializeField] private PlayerMovementHandler movementHandler;

    private BoxCollider2D triggerCollider;
    private List<InteractableItem> inRangeTargets = new List<InteractableItem>();
    private InteractableItem selectedTarget = null;
    private InteractableItem pickedUpItem = null;

    private void Start() => Controls.Instance.OnPlayerAttack += HandleInteraction;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.TryGetComponent<InteractableItem>(out InteractableItem target))
            return;

        if (inRangeTargets.Contains(target))
            return;

        inRangeTargets.Add(target);
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.TryGetComponent<InteractableItem>(out InteractableItem target))
            return;

        inRangeTargets.Remove(target);
    }

    private void Update() => UpdateSelection();

    private void UpdateSelection()
    {
        if (pickedUpItem)
            return;

        if (inRangeTargets.Count == 0)
        {
            selectedTarget = null;
            return;
        }

        float distance = float.MaxValue;
        InteractableItem[] targetsList = inRangeTargets.ToArray();
        foreach (InteractableItem target in targetsList)
        {
            //Select only pickable items
            if (!target.Is<PickableItem>())
                continue;

            if (selectedTarget && (selectedTarget.Is<ConsumableItem>() && !target.Is<ConsumableItem>()))
                continue;

            else if (selectedTarget && (!selectedTarget.Is<ConsumableItem>() && target.Is<ConsumableItem>()))
            {
                selectedTarget = target;
                continue;
            }

            float itemDistance = Vector2.Distance(target.transform.position, transform.position);
            if (itemDistance < distance)
            {
                distance = itemDistance;
                selectedTarget = target;
            }
        }
    }

    private void HandleInteraction()
    {
        if (pickedUpItem)
        {
            if (pickedUpItem.Is<ConsumableItem>())
            {
                pickedUpItem.ConvertTo<ConsumableItem>().Consume();
                OnDevourEvent?.Invoke();
                GameManager.Instance?.GrannyHealthHandler?.RecoverHealthPoint();
            }
            else
            {
                Vector2 throwForce = movementHandler.Velocity.normalized * throwPower;
                ThrowableItem item = pickedUpItem.ConvertTo<ThrowableItem>();
                if (throwForce.magnitude > .1f)
                {
                    item.OnTargetHit -= HandleHitTarget;
                    item.OnTargetHit += HandleHitTarget;
                    item.Throw(throwForce);
                    OnThrowEvent?.Invoke();
                }
                else
                {
                    item.OnFoundPlacement += HandleItemPlacement;
                    item.Release(!pickedUpItem.TryGetComponent<ItemHolder>(out _));
                }
            }
            pickedUpItem = null;
            return;
        }
        else
        {
            //If nothing is selected, then stop.
            if (!selectedTarget)
                return;

            //Since something is selected, start to pick it up
            pickedUpItem = selectedTarget;

            //
            if (pickedUpItem.TryGetComponent<NPCSuperStateMachine>(out NPCSuperStateMachine enemy))
            {
                //If the target is an armed NPC, cancel action
                if (!enemy.TryCapture())
                {
                    pickedUpItem = null;
                    OnTryCaptureEvent?.Invoke();
                    return;
                }

                //Since it is an unarmed NPC, capture it
                pickedUpItem.ConvertTo<PickableItem>().PickUp(itemHolder);

                if (FirstAttack)
                    HandleFirstCaptureEvent();
                else
                    OnTryCaptureEvent?.Invoke();
            }
            else
            {
                //Since the target is not an NPC, directly pick it up
                pickedUpItem.ConvertTo<PickableItem>().PickUp(itemHolder);
                OnItemPicked?.Invoke(pickedUpItem.itemId);
            }
        }
    }
    private void HandleItemPlacement(PickableItem item)
    {
        Debug.Log("Triggering OnItemPositioned Event");
        item.OnFoundPlacement -= HandleItemPlacement;
        OnItemPositioned?.Invoke(item.id);
    }
    private void HandleHitTarget(int targetId) => OnHitTarget?.Invoke(targetId);
    private void HandleFirstCaptureEvent()
    {
        FirstAttack = false;
        HandleInteraction();
        GameEvents.RaiseFirstEat();
    }
}