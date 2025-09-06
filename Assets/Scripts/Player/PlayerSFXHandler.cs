using UnityEngine;

public class PlayerSFXHandler : MonoBehaviour
{
    private void Start()
    {
        AudioManager sfxManager = AudioManager.Instance;
        if (TryGetComponent<PlayerInteractionHandler>(out PlayerInteractionHandler interactionHandler))
            interactionHandler.OnDevourEvent += (ConsumableItem _) =>
            { sfxManager.PlayEatSound(); };
    }
}
