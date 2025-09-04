using UnityEngine;

public class HitTarget : QuestObjectiveStructure
{
    [SerializeField] private StatusEffectType effectType;
    [SerializeField] private ItemData targetToHit;
    [SerializeField] private string description;

    private bool initialized;
    public HitTarget() => initialized = false;
    private HitTarget(HitTarget original)
    {
        targetToHit = original.targetToHit;
        description = original.description;
        initialized = false;
    }
    public override void UpdateProgress()
    {
        if (!initialized)
        {
            initialized = true;
            GameManager.Instance.PlayerInteractionHandler.OnHitTarget += HandleOnTargetHit;
            Debug.Log($"Waiting to hit target (id: {targetToHit.Id}) and cause status effect (effectType: {effectType})");
        }
    }
    public override QuestObjectiveStructure Clone()
    {
        return new HitTarget(this);
    }
    private void HandleOnTargetHit(InteractableItem item, InteractableItem target)
    {
        Debug.Log($"Hit a target (id: {target.id}) causing a status effect (effect: {item.EffectType})");
        if (target.id != targetToHit.Id)
            return;
        if (effectType != StatusEffectType.None && effectType != item.EffectType)
            return;

        GameManager.Instance.PlayerInteractionHandler.OnHitTarget -= HandleOnTargetHit;
        CompleteObjective();
    }
}
