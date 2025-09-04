using UnityEngine;

public class ItemHolder : MonoBehaviour
{
    [SerializeField] private Transform holderTransform;
    private InteractableItem item;
    private bool holdingItem;
    public int Id
    {
        get
        {
            if (item)
                return item.id;
            else
                return -1;
        }
    }

    private void Start()
    {
        if (TryGetComponent<InteractableItem>(out item))
            if(item.Is<ThrowableItem>())
            item.ConvertTo<ThrowableItem>().OnPickedUp += OnItemPicked;
    }

    public bool CanHoldItem => !holdingItem;
    public void HoldItem(InteractableItem assignedItem)
    {
        if (holdingItem)
            return;

        if(assignedItem.Is<PickableItem>())
            assignedItem.ConvertTo<PickableItem>().OnPickedUp += OnItemPicked;

        assignedItem.transform.position = holderTransform.position;
        DisableSelf();
    }

    private void OnItemPicked() => EnableSelf();
    private void EnableSelf()
    {
        holdingItem = false;
        item?.Enable();
    }
    private void DisableSelf()
    {
        holdingItem = true;
        item?.Disable();
    }
}