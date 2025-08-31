using UnityEngine;

[System.Serializable]
public class QuestObjective
{
    [SerializeField] private QuestObjectiveType objectiveType;

    [SerializeReference] private QuestObjectiveStructure structure;
    private bool isCompleted = false;
    public bool IsCompleted => isCompleted;
    public void UpdateProgress()
    {
        if (structure == null)
        {
            isCompleted = true;
            return;
        }

        structure.UpdateProgress();
        isCompleted = structure.isCompleted;
    }

    public QuestObjective(QuestObjective original)
    {
        objectiveType = original.objectiveType;
        structure = original.structure;
    }
}