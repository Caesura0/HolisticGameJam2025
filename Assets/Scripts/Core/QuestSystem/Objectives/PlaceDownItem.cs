using UnityEngine;


public class PlaceDownItem : QuestObjectiveStructure
{
    [SerializeField] private ItemData itemHolder;
    [SerializeField] private ItemData questItem;
    [SerializeField] private string description;

    private bool initialized;
    public PlaceDownItem() => initialized = false;
    private PlaceDownItem(PlaceDownItem original)
    {
        questItem = original.questItem;
        description = original.description;
        initialized = false;
    }

    public override void UpdateProgress()
    {
        if (!initialized)
        {
            initialized = true;
            GameManager.Instance.PlayerInteractionHandler.OnItemPositioned += HandleOnItemPositioned;
            Debug.Log($"Starting Objective");
            Debug.Log($"Waiting to position item (id: {questItem?.Id}) on item holder (id: {itemHolder?.Id})");
        }
    }

    public override QuestObjectiveStructure Clone() => new PlaceDownItem(this);

    private void HandleOnItemPositioned(ItemHolder holder, int positionedItemId)
    {
        Debug.Log($"Item placement detected (id: {questItem?.Id})");
        if (questItem.Id != positionedItemId )
            return;
        if (itemHolder && itemHolder.Id != holder.Id)
            return;

        GameManager.Instance.PlayerInteractionHandler.OnItemPositioned -= HandleOnItemPositioned;
        Debug.Log($"Completed {this} Objective");
        CompleteObjective();
    }
}