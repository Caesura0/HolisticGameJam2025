using System.Collections.Generic;
using UnityEngine;

public class QuestsHandlerUI : MonoBehaviour
{
    [SerializeField] private QuestUI questUIPrefab;
    [SerializeField] private RectTransform questUIContainer;
    [SerializeField] private GameObject questBox;
    [SerializeField] private QuestHandler questHandler;
    private Dictionary<Quest, QuestUI> activeQuests = new Dictionary<Quest, QuestUI>();

    private void Awake()
    {
        if(!questHandler)
            questHandler = FindFirstObjectByType<QuestHandler>();

        Initialize();
    }

    private void Initialize()
    {
        Debug.Log($"Initializing quest handler ui");
        questHandler.OnActiveQuestAdded += AddQuest;
    }

    private void AddQuest(Quest quest)
    {
        Debug.Log($"Creating UI for {quest}");
        if (activeQuests.ContainsKey(quest))
            return;

        questBox.SetActive(true);
        QuestUI questUI = Instantiate(questUIPrefab, questUIContainer);
        activeQuests.Add(quest, questUI);
        questUI.Initialize(quest);
        quest.OnQuestCompleted += RemoveQuest;
    }

    private void RemoveQuest(Quest quest)
    {
        activeQuests[quest].DeleteQuestUI();
        activeQuests.Remove(quest);
        quest.OnQuestCompleted -= RemoveQuest;

        if (activeQuests.Count > 0)
            return;

        questBox.SetActive(false);
    }
}