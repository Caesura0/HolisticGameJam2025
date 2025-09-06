using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestHandler : MonoBehaviour
{
    public event Action<Quest> OnActiveQuestAdded;
    [field: SerializeField] private List<QuestDataWrapper> questDataRegistry = new List<QuestDataWrapper>();
    private Dictionary<int, Quest> activeQuests;
    private List<int> completedQuests;
    private bool activated;

    private void Awake()
    {
        activeQuests = new Dictionary<int, Quest>();
        completedQuests = new List<int>();
    }
    private void Start()
    {
        if (activated || questDataRegistry.Count == 0)
            return;

        activated = true;
        CheckAvailableQuests();
    }
    private void Update()
    {
        if (!activated)
            return;

        UpdateQuestProgress();
    }

    private void CheckAvailableQuests()
    {
        Debug.Log("Checking Available Quests");
        foreach (QuestDataWrapper dataWrapper in questDataRegistry)
        {
            if (dataWrapper == null)
                continue;

            if (IsQuestActive(dataWrapper.GetData().id))
                continue;

            if (IsQuestArchived(dataWrapper))
                continue;

            if (dataWrapper.GetData().prerequisite == null)
            {
                Debug.Log($"Activating Data on absence of prerequisite");
                ActivateQuest(dataWrapper);
                return;
            }

            if(IsQuestCompleted(dataWrapper.GetData().prerequisite.GetData().id))
            {
                Debug.Log($"Activating Data on prerequisite completion");
                ActivateQuest(dataWrapper);
                return;
            }
        }
    }
    private void UpdateQuestProgress()
    {
        List<Quest> questsToRemove = new List<Quest>();
        foreach (Quest quest in activeQuests.Values)
        {
            quest.UpdateProgress();
            if (!quest.IsCompleted)
                continue;

            CompleteQuest(quest);
            questsToRemove.Add(quest);
        }

        if (questsToRemove.Count == 0)
            return;
        
        foreach (Quest quest in questsToRemove)
            activeQuests.Remove(quest.id);

        CheckAvailableQuests();
    }

    private void ActivateQuest(QuestDataWrapper dataWrapper)
    {
        Quest quest = new Quest(dataWrapper.GetData());
        activeQuests.Add(dataWrapper.GetData().id, quest);
        OnActiveQuestAdded?.Invoke(quest);
    }
    private void CompleteQuest(Quest quest)
    {
        if (!IsQuestActive(quest.id))
            return;

        Debug.Log($"{quest.name} quest completed");
        quest.TriggerReward();

        if (!completedQuests.Contains(quest.id))
            completedQuests.Add(quest.id);
    }

    private bool IsQuestCompleted(int questId) => completedQuests.Contains(questId);
    private bool IsQuestActive(int questId) => activeQuests.ContainsKey(questId);
    private bool IsQuestArchived(QuestDataWrapper dataWrapper) =>
        !dataWrapper.GetData().isRepeatable && IsQuestCompleted(dataWrapper.GetData().id);
}