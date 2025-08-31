using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Custom/Quest System/Quest Data")]
public class QuestDataWrapper : ScriptableObject
{
    [SerializeField] private QuestData questData;
    public QuestData GetData() => questData;
}