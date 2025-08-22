using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private PlayerMovementHandler playerMovementHandler;
    private PlayerNotificationHandler playerNotificationHandler;
    private bool hasMovementHandler, hasNotificationHandler;

    private void Awake()
    {
        hasMovementHandler = TryGetComponent<PlayerMovementHandler>(out playerMovementHandler);
        hasNotificationHandler = TryGetComponent<PlayerNotificationHandler>(out playerNotificationHandler);
    }

    private void Update()
    {
        if (GameplayManager.Instance.IsGamePaused())
            return;

        if (HungerHandler.Instance.IsDead()) return;

        if(hasMovementHandler)
            playerMovementHandler.HandleMovement();

        if (hasNotificationHandler)
            playerNotificationHandler.HandleHungerNotification();
    }
}