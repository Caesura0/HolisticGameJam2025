using UnityEngine;

public class PlayerVFXHandler : MonoBehaviour
{
    [SerializeField] Transform playerVFXPoint;
    private void Start()
    {
        if (TryGetComponent<PlayerInteractionHandler>(out PlayerInteractionHandler interactionHandler))
            interactionHandler.OnDevourEvent += () =>
            { VFXHandler.Instance.PlayVisualEffect(VFXType.Blood, interactionHandler.itemHolder.position); };

        if (HungerHandler.Instance)
        {
            HungerHandler.Instance.OnDeathTrigger += () =>
            { VFXHandler.Instance.PlayVisualEffect(VFXType.SoulRelease, playerVFXPoint.position); };
            HungerHandler.Instance.OnDropHeart += () =>
            { VFXHandler.Instance.PlayVisualEffect(VFXType.HeartBreak, playerVFXPoint.position); };
            HungerHandler.Instance.OnGainHeart += () =>
            { VFXHandler.Instance.PlayVisualEffect(VFXType.HeartGain, playerVFXPoint.position); };
        }
    }
}