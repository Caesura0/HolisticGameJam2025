using System;
using UnityEngine;

[Serializable]
public class PickUpItem : QuestObjectiveStructure
{
    [SerializeField] private int questItemId;
    [SerializeField] private string description;

    private bool initialized;
    public PickUpItem() { initialized = false; }
    public PickUpItem(PickUpItem original)
    {
        questItemId = original.questItemId;
        description = original.description;
        initialized = false;
    }
    public override void UpdateProgress()
    {
        if (!initialized)
        {
            initialized = true;
            GameManager.Instance.PlayerInteractionHandler.OnItemPicked += HandleOnItemPicked;
            Debug.Log($"Waiting to pick up item (id: {questItemId})");
        }
    }
    public override QuestObjectiveStructure Clone()
    {
        return new PickUpItem(this);
    }
    private void HandleOnItemPicked(int itemId)
    {
        Debug.Log($"Item Pick Up detected (id: {itemId})");
        if (questItemId != itemId)
            return;

        GameManager.Instance.PlayerInteractionHandler.OnItemPicked -= HandleOnItemPicked;
        CompleteObjective();
    }
}
