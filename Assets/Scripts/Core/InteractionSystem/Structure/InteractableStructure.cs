using System;
using UnityEngine;

[Serializable]
public class InteractableStructure
{
    public void AssignData(Transform transform, InteractableItem item)
    {
        this.transform = transform;
        this.item = item;
        Initialize();
    }
    public int id => item.id;
    public ItemType type => item.type;
    protected Transform transform;
    protected InteractableItem item;
    public virtual void Initialize() => throw new NotImplementedException();
    public virtual void Tick() => throw new NotImplementedException();
    public virtual void OnTriggerEnter2D(Collider2D other) { }
}

public enum InteractableType
{
    ThrowableItem,
    ConsumableItem
}