using System;
using UnityEngine;

[Serializable]
public class PickableItem : InteractableStructure
{
    public event Action OnPickedUp;
    public event Action<PickableItem> OnFoundPlacement;

    private Transform holder;
    private bool beingHeld;
    [SerializeField] protected bool initialized;
    protected Collider2D collision;

    public override void Initialize() =>
        initialized = transform.TryGetComponent<Collider2D>(out collision);

    public override void Tick()
    {
        if (!initialized)
            return;
        
        if (beingHeld)
        {
            transform.position = holder.position;
            return;
        }
    }
    public void PickUp(Transform assignedHolder)
    {
        if (!initialized)
            return;

        holder = assignedHolder;
        beingHeld = true;
        DisableCollision();
        OnPickedUp?.Invoke();
    }
    public void Release(bool findPlacement = false)
    {
        if (!initialized)
            return;

        beingHeld = false;
        EnableCollision();

        if (findPlacement)
            TryFindPlacement();
        holder = null;
    }

    private void TryFindPlacement()
    {
        Debug.Log("Trying to find placement");
        float searchRange = .5f;
        Collider2D[] others = Physics2D.OverlapCircleAll(transform.position, searchRange);

        ItemHolder chosenHolder = null;

        if (others.Length == 0)
            return;

        float distance = float.MaxValue;
        foreach (Collider2D other in others)
        {
            if (!other.TryGetComponent<ItemHolder>(out ItemHolder holder))
                continue;

            if (!holder.CanHoldItem)
                continue;

            float itemDistance = Vector2.Distance(holder.transform.position, transform.position);
            if (itemDistance < distance)
            {
                distance = itemDistance;
                chosenHolder = holder;
            }
        }

        if (chosenHolder == null)
            return;
        chosenHolder.HoldItem(item);
        Debug.Log("placement found");
        OnFoundPlacement?.Invoke(this);
    }
    private void EnableCollision() => collision.enabled = true;
    private void DisableCollision() => collision.enabled = false;
}