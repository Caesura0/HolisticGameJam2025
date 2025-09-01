using UnityEngine;


public class PlaceDownItem : QuestObjectiveStructure
{
    [SerializeField] private int questItemId;
    [SerializeField] private string description;

    private bool initialized;
    public PlaceDownItem() => initialized = false;
    private PlaceDownItem(PlaceDownItem original)
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
            GameManager.Instance.PlayerInteractionHandler.OnItemPositioned += HandleOnItemPositioned;
            Debug.Log($"Starting Objective");
            Debug.Log($"Waiting to position item (id: {questItemId})");
        }
    }

    public override QuestObjectiveStructure Clone() => new PlaceDownItem(this);

    private void HandleOnItemPositioned(int itemId)
    {
        Debug.Log($"Item placement detected (id: {itemId})");
        if (questItemId != itemId)
            return;

        GameManager.Instance.PlayerInteractionHandler.OnItemPositioned -= HandleOnItemPositioned;
        CompleteObjective();
    }
}