using System;

[System.Serializable]
public class QuestObjectiveStructure
{
    public event Action OnObjectiveAccomplished;
    protected void CompleteObjective() => OnObjectiveAccomplished?.Invoke();
    public virtual void UpdateProgress() => CompleteObjective();
    public virtual QuestObjectiveStructure Clone() => throw new NotImplementedException("Override absent!");
    public static QuestObjectiveStructure GetStructureByType(QuestObjectiveType type)
    {
        switch (type)
        {
            case QuestObjectiveType.None:
            default:
                return null;
            case QuestObjectiveType.PickUpItem:
                return new PickUpItem();
            case QuestObjectiveType.PlaceItemDown:
                return new PlaceDownItem();
            case QuestObjectiveType.ThrowItem:
                return new ThrowItem();
            case QuestObjectiveType.DisarmTarget:
                return new DisarmTarget();
            case QuestObjectiveType.HitTarget:
                return new HitTarget();
            case QuestObjectiveType.DevourTarget:
                return new DevourTarget();
        }
    }
}

public enum QuestObjectiveType
{
    None = 0,
    MoveToPosition,
    PickUpItem,
    PlaceItemDown,
    ThrowItem,
    DisarmTarget,
    HitTarget,
    DevourTarget
}