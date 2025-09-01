using System;
using UnityEngine;

public sealed class InteractableItem : MonoBehaviour
{
    [field: SerializeField] public int itemId { get; private set; } = -1;
    [SerializeField] private InteractableType selectedType;
    private InteractableStructure structure;
    private bool interactionEnabled;

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