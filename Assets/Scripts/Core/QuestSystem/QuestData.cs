using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct QuestData
{
    [field: SerializeField] public int id { get; private set; }
    [field: SerializeField] public QuestDataWrapper prerequisite { get; private set; }
    [field: SerializeField] public string questName { get; private set; }
    [field: SerializeField, TextArea(1, 5)] public string questDescription { get; private set; }
    [field: SerializeField] public List<QuestObjective> objectives { get; private set; }
    [field: SerializeField] public bool sequentialCompletion { get; private set; }
    [field: SerializeField] public QuestReward reward { get; private set; }
    [field: SerializeField] public bool isRepeatable { get; private set; }
}