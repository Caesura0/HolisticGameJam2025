using System;
using UnityEngine;

[System.Serializable]
public class QuestObjective
{
    public event Action OnObjectiveCompleted;

    [SerializeField] private QuestObjectiveType objectiveType;
    [SerializeReference] private QuestObjectiveStructure structure;
    [SerializeField] private string description;
    private bool initialized = false;
    private bool isCompleted = false;
    public bool IsCompleted => isCompleted;
    public string Description => description;
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

    private void CompleteObjective()
    {
        isCompleted = true;
        OnObjectiveCompleted?.Invoke();
    }

    public QuestObjective(QuestObjective original)
    {
        Debug.Log("Created new quest objective");
        objectiveType = original.objectiveType;
        structure = original.structure?.Clone();
        description = original.description;
    }
}