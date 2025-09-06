using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestObjectivesUI : MonoBehaviour
{
    [SerializeField] private Toggle toggle;
    [SerializeField] private TextMeshProUGUI description;
    private QuestObjective assignedObjective;
    
    public void Initialize(QuestObjective objective)
    {
        Debug.Log($"Initializing Objective UI for {objective.Description}");
        toggle.isOn = false;
        assignedObjective = objective;
        description.text = assignedObjective.Description;
        assignedObjective.OnObjectiveCompleted += HandleObjectiveCompleted;
    }

    private void HandleObjectiveCompleted()
    {
        toggle.isOn = true;
    }
}