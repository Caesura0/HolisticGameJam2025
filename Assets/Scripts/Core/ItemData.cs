using UnityEngine;

[CreateAssetMenu(menuName ="Custom/new Item")]
public sealed class ItemData : ScriptableObject
{
    [SerializeField] private int itemId;
    [SerializeField] private string itemName;
    [SerializeField] private ItemType itemType;

    public int Id => itemId;
    public string Name => itemName;
    public ItemType Type => itemType;
}

public enum ItemType
{
    Collectable,
    Table,
    NPC,
    CheckInPoint
}