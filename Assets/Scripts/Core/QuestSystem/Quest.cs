using System;
using System.Collections.Generic;
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

    public event Action<Quest> OnQuestCompleted;

    [SerializeField] private QuestData data;

    #region QuestData
    public int id => data.id;
    public string name => data.questName;
    public string description => data.questDescription;
    public bool sequentialCompletion => data.sequentialCompletion;
    public QuestReward reward => data.reward;
    public bool canBeRepeated => data.isRepeatable;
    #endregion

    private bool isCompleted;
    public bool IsCompleted => isCompleted;

    public List<QuestObjective> objectives { get; private set; }

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
        if (!incompleteObjectiveNotFound)
            return;

        isCompleted = incompleteObjectiveNotFound;
        OnQuestCompleted?.Invoke(this);
    }
    public void TriggerReward() => reward?.TriggerReward();
}