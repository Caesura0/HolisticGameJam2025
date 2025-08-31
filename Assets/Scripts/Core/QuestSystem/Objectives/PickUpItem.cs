using System;
using UnityEngine;

[Serializable]
public class PickUpItem : QuestObjectiveStructure
{
    [SerializeField] private int questItemId;
    [SerializeField] private string description;

    private bool initialized;

    public override void UpdateProgress()
    {
        if (!initialized)
        {
            initialized = true;
            isCompleted = false;
            GameManager.Instance.PlayerInteractionHandler.OnItemPicked += HandleOnItemPicked;
            Debug.Log($"Waiting to pick up item (id: {questItemId})");
        }
    }

    private void HandleOnItemPicked(int itemId)
    {
        Debug.Log("Item Pick Up detected");
        if (questItemId != itemId)
            return;
        Debug.Log("Quest item picked up");

        GameManager.Instance.PlayerInteractionHandler.OnItemPicked -= HandleOnItemPicked;
        isCompleted = true;
    }
}
