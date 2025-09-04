using System;
using UnityEngine;

[Serializable]
public class PickUpItem : QuestObjectiveStructure
{
    private enum IdentificationType
    {
        Id,
        Type
    }
    [SerializeField] private IdentificationType identifyBy = IdentificationType.Id;
    [SerializeField] private ItemData questItem;
    [SerializeField] private string description;

    private bool initialized;
    public PickUpItem() => initialized = false;
    private PickUpItem(PickUpItem original)
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
            GameManager.Instance.PlayerInteractionHandler.OnItemPicked += HandleOnItemPicked;
            Debug.Log($"Waiting to pick up item (id: {questItem.Id})");
        }
    }
    public override QuestObjectiveStructure Clone()
    {
        return new PickUpItem(this);
    }
    private void HandleOnItemPicked(PickableItem pickedItem)
    {
        Debug.Log($"Item Pick Up detected (id: {pickedItem.id} type: {pickedItem.type})");
        switch (identifyBy)
        {
            case IdentificationType.Id:
                if (questItem.Id != pickedItem.id)
                    return;
                break;
            case IdentificationType.Type:
                if (questItem.Type != pickedItem.type)
                    return;
                break;
        }

        GameManager.Instance.PlayerInteractionHandler.OnItemPicked -= HandleOnItemPicked;
        Debug.Log($"Completed {this} Objective");
        CompleteObjective();
    }
}
