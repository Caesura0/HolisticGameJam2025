using Unity.VisualScripting;
using UnityEngine;

public class DisarmTarget : QuestObjectiveStructure
{
    [SerializeField] private ItemData targetToDisarm;
    [SerializeField] private string description;

    private bool initialized;
    public DisarmTarget() => initialized = false;
    private DisarmTarget(DisarmTarget original)
    {
        targetToDisarm = original.targetToDisarm;
        description = original.description;
        initialized = false;
    }
    public override void UpdateProgress()
    {
        if (!initialized)
        {
            initialized = true;
            GameEvents.OnDisarmed += HandleOnTargetDisarmed;
            Debug.Log($"Waiting to disarm target (id: {targetToDisarm.Id})");
        }
    }
    public override QuestObjectiveStructure Clone()
    {
        return new DisarmTarget(this);
    }
    private void HandleOnTargetDisarmed(InteractableItem target)
    {
        Debug.Log($"Disarmed a target (id: {target.id})");
        if (target.id != targetToDisarm.Id)
            return;

        GameEvents.OnDisarmed -= HandleOnTargetDisarmed;
        CompleteObjective();
    }
}
