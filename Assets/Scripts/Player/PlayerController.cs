using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private bool isActive;
    private PlayerMovementHandler playerMovementHandler;
    private PlayerNotificationHandler playerNotificationHandler;
    private bool hasMovementHandler, hasNotificationHandler;

    private void Awake()
    {
        hasMovementHandler = TryGetComponent<PlayerMovementHandler>(out playerMovementHandler);
        hasNotificationHandler = TryGetComponent<PlayerNotificationHandler>(out playerNotificationHandler);

        GameManager gameManager = GameManager.Instance;
        if (gameManager)
        {
            Activate();
            gameManager.OnGamePaused += HandleGamePaused;
            gameManager.OnStarvedToDeath += Deactivate;
        }
    }

    private void Update()
    {
        if (!isActive)
            return;

        if(hasMovementHandler)
            playerMovementHandler.HandleMovement();

        if (hasNotificationHandler)
            playerNotificationHandler.HandleHungerNotification();
    }

    private void Activate() => 
        isActive = true;
    private void Deactivate() => 
        isActive = false;

    private void HandleGamePaused()
    {
        if (!isActive)
            return;

        Deactivate();
        GameManager.Instance.OnGameResumed += HandleGameResumed;
    }
    private void HandleGameResumed()
    {
        Activate();
        GameManager.Instance.OnGameResumed -= HandleGameResumed;
    }
}
