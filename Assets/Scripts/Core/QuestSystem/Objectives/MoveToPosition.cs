using UnityEngine;

public class MoveToPosition : QuestObjectiveStructure
{
    [SerializeField] private ItemData checkInPoint;
    [SerializeField] private string description;

    private bool initialized;
    public MoveToPosition() => initialized = false;
    private MoveToPosition(MoveToPosition original)
    {
        checkInPoint = original.checkInPoint;
        description = original.description;
        initialized = false;
    }
    public override void UpdateProgress()
    {
        if (!initialized)
        {
            initialized = true;
            GameEvents.OnCheckedIn += HandleOnCheckedIn;
            Debug.Log($"Waiting for check-in at point (id: {checkInPoint.Id})");
        }
    }
    public override QuestObjectiveStructure Clone()
    {
        return new MoveToPosition(this);
    }
    private void HandleOnCheckedIn(ItemData checkedInPoint)
    {
        Debug.Log($"Entered a check-in point (id: {checkedInPoint.Id})");
        if (checkedInPoint.Type != ItemType.CheckInPoint || checkInPoint.Id != checkedInPoint.Id)
            return;

        GameEvents.OnCheckedIn -= HandleOnCheckedIn;
        Debug.Log($"Completed {this} Objective");
        CompleteObjective();
    }
}
