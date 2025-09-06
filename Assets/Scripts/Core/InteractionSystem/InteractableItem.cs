using System;
using UnityEngine;
public enum StatusEffectType
{
    None,
    Slowed,
    Stunned
}
public sealed class InteractableItem : MonoBehaviour
{
    [SerializeField] private ItemData item;
    [SerializeField] private InteractableType selectedType;
    [SerializeField] private StatusEffectType effectType = StatusEffectType.None;
    [SerializeField] private float effectDuration = 5f;
    private InteractableStructure structure;
    private bool interactionEnabled;

    public int id => item.Id;
    public new string name => item.Name;
    public ItemType type => item.Type;
    public StatusEffectType EffectType => effectType;
    public float EffectDuration => effectDuration;

    private void Awake()
    {
        SelectStructure();
        Enable();
    }
    private void Start() => structure?.AssignData(transform, this);
    private void Update() => structure?.Tick();

    private void OnTriggerEnter2D(Collider2D collision) => structure?.OnTriggerEnter2D(collision);

    public bool Is<T>() where T : InteractableStructure => interactionEnabled && structure is T;
    public T ConvertTo<T>() where T : InteractableStructure => structure as T;
    
    public bool Disable() => interactionEnabled = false;
    public void Enable() => interactionEnabled = true;
    private void SelectStructure()
    {
        switch (selectedType)
        {
            case InteractableType.ThrowableItem:
            default:
                structure = new ThrowableItem();
                break;
            case InteractableType.ConsumableItem:
                structure = new ConsumableItem();
                break;
        }
    }
}