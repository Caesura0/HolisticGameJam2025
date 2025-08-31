[System.Serializable]
public class QuestObjectiveStructure
{
    public bool isCompleted { get; protected set; }
    public virtual void UpdateProgress() { isCompleted = true; }

    public static QuestObjectiveStructure GetStructureByType(QuestObjectiveType type)
    {
        switch (type)
        {
            case QuestObjectiveType.None:
            default:
                return null;
            case QuestObjectiveType.PickUpItem:
                return new PickUpItem();
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
    CaptureTarget,
    DevourTarget
}