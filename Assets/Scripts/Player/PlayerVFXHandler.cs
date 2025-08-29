using UnityEngine;

public class PlayerVFXHandler : MonoBehaviour
{
    [SerializeField] Transform playerVFXPoint;
    private void Start()
    {
        if (TryGetComponent<PlayerInteractionHandler>(out PlayerInteractionHandler interactionHandler))
            interactionHandler.OnDevourEvent += () =>
            { VFXHandler.Instance.PlayVisualEffect(VFXType.Blood, interactionHandler.itemHolder.position); };

        HealthHandler healthHandler = GameManager.Instance?.GrannyHealthHandler;
        if (healthHandler)
        {
            healthHandler.OnDeathTrigger += () =>
            { VFXHandler.Instance.PlayVisualEffect(VFXType.SoulRelease, transform.position); };
            healthHandler.OnHealthDrop += () =>
            { VFXHandler.Instance.PlayVisualEffect(VFXType.HeartBreak, playerVFXPoint.position); };
            healthHandler.OnHealthGain += () =>
            { VFXHandler.Instance.PlayVisualEffect(VFXType.HeartGain, playerVFXPoint.position); };
        }
    }
}