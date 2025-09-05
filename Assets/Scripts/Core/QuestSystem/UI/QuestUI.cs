using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI questName;
    [SerializeField] private QuestObjectivesUI objectiveUIPrefab;
    [SerializeField] private RectTransform objectiveUIContainer;
    private Quest assignedQuest;
    private List<QuestObjectivesUI> objectives = new List<QuestObjectivesUI>();
    public void Initialize(Quest quest)
    {
        Debug.Log($"Initializing quest ui for {quest.name}");
        assignedQuest = quest;
        questName.text = quest.name;
        InitializeObjectives();
    }

    public void InitializeObjectives()
    {
        foreach(QuestObjective objective in assignedQuest.objectives)
        {
            QuestObjectivesUI objectiveUI = Instantiate(objectiveUIPrefab, objectiveUIContainer);
            objectives.Add(objectiveUI);
            objectiveUI.Initialize(objective);
        }
    }
}