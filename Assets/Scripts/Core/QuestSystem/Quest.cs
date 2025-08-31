using System.Collections.Generic;
using Unity.Android.Gradle.Manifest;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class Quest
{
    public Quest(QuestData data)
    {
        Debug.Log($"Created new quest");
        this.data = data;
        objectives = new List<QuestObjective>();
        foreach(QuestObjective objective in data.objectives)
            objectives.Add(new QuestObjective(objective));
        isCompleted = false;
    }

    [SerializeField] private QuestData data;
    #region QuestData
    
    public int id => data.id;
    public string name => data.questName;
    public string description => data.questDescription;
    public List<QuestObjective> objectives { get; private set; }
    public bool sequentialCompletion => data.sequentialCompletion;
    public QuestReward reward => data.reward;
    public bool canBeRepeated => data.isRepeatable;

    #endregion
    private bool isCompleted;
    public bool IsCompleted => isCompleted;

    public void UpdateProgress()
    {
        if(isCompleted) return;

        bool incompleteObjectiveNotFound = true;
        foreach(QuestObjective objective in objectives)
        {
            //Debug.Log($"Updating {objective} progress");
            objective.UpdateProgress();
            if (objective.IsCompleted)
                continue;
            incompleteObjectiveNotFound = false;
            if (data.sequentialCompletion)
                break;
        }
        isCompleted = incompleteObjectiveNotFound;

    }
    public void TriggerReward() => reward?.TriggerReward();
}