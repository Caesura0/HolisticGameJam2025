using Unity.VisualScripting;
using UnityEngine;

public class ThrowItem : QuestObjectiveStructure
{
    [SerializeField] private ItemData questItem;
    private bool initialized;
    public ThrowItem() => initialized = false;
    private ThrowItem(ThrowItem original)
    {
        questItem = original.questItem;
        initialized = false;
    }
    public override void UpdateProgress()
    {
        if (!initialized)
        {
            initialized = true;
            GameManager.Instance.PlayerInteractionHandler.OnThrowEvent += HandleOnItemThrown;
            Debug.Log($"Waiting to throw item (id: {questItem.Id})");
        }
    }
    public override QuestObjectiveStructure Clone()
    {
        return new ThrowItem(this);
    }
    private void HandleOnItemThrown(int thrownItemId)
    {
        Debug.Log($"Item Pick Up detected (id: {thrownItemId})");
        if (questItem.Id != thrownItemId)
            return;

        GameManager.Instance.PlayerInteractionHandler.OnThrowEvent -= HandleOnItemThrown;
        CompleteObjective();
    }
}
