using Unity.VisualScripting;
using UnityEngine;

public class DevourTarget : QuestObjectiveStructure
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
    public DevourTarget() => initialized = false;
    private DevourTarget(DevourTarget original)
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
            GameManager.Instance.PlayerInteractionHandler.OnDevourEvent += HandleOnItemPicked;
            Debug.Log($"Waiting to pick up item (id: {questItem.Id})");
        }
    }
    public override QuestObjectiveStructure Clone()
    {
        return new DevourTarget(this);
    }
    private void HandleOnItemPicked(ConsumableItem consumedItem)
    {
        Debug.Log($"Item consumption detected (id: {consumedItem.id} type: {consumedItem.type})");
        switch (identifyBy)
        {
            case IdentificationType.Id:
                if (questItem.Id != consumedItem.id)
                    return;
                break;
            case IdentificationType.Type:
                if (questItem.Type != consumedItem.type)
                    return;
                break;
        }

        GameManager.Instance.PlayerInteractionHandler.OnDevourEvent -= HandleOnItemPicked;
        Debug.Log($"Completed {this} Objective");
        CompleteObjective();
    }
}
