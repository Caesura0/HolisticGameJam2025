using UnityEngine;

[System.Serializable]
public class QuestObjective
{
    [SerializeField] private QuestObjectiveType objectiveType;

    [SerializeReference] private QuestObjectiveStructure structure;
    private bool initialized = false, isCompleted = false;
    public bool IsCompleted => isCompleted;
    public void UpdateProgress()
    {
        if (structure == null)
        {
            isCompleted = true;
            return;
        }

        if(!initialized)
        {
            initialized = true;
            isCompleted = false;
            structure.OnObjectiveAccomplished += CompleteObjective;
        }
        structure.UpdateProgress();
    }

    private void CompleteObjective() => isCompleted = true;

    public QuestObjective(QuestObjective original)
    {
        Debug.Log("Created new quest objective");
        objectiveType = original.objectiveType;
        structure = original.structure?.Clone();
    }
}