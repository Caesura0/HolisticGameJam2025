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
    private void Awake() =>
        rectTransform = GetComponent<RectTransform>();

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

    bool removeQuest = false;
    float removeTimer = .8f;
    private void Update()
    {
        if (!removeQuest)
            return;

        if (removeTimer > 0)
        {
            removeTimer -= Time.deltaTime;
            MoveLeft();
        }
        else
            Destroy(gameObject);
    }

    RectTransform rectTransform;
    float movementSpeed = 200f;
    private void MoveLeft() => rectTransform.anchoredPosition += Vector2.left * Time.deltaTime * movementSpeed;
    public void DeleteQuestUI() => removeQuest = true;
}