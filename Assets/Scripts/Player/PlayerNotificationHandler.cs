using UnityEngine;

public class PlayerNotificationHandler : MonoBehaviour
{
    [SerializeField] private float waitBetweenNotifications = 3f;
    private float timeBeforeNextNotification = 0f;

    private NotificationHandler notificationHandler;

    private void Awake()
    {
        notificationHandler = GetComponentInChildren<NotificationHandler>();

        if (TryGetComponent<PlayerInteractionHandler>(out PlayerInteractionHandler playerInteractionHandler))
        {
            playerInteractionHandler.OnDevourEvent += HandleDevourNotification;
            playerInteractionHandler.OnTryCaptureEvent += HandleCaptureNotification;
        }
    }

    private void PlayNotification(NotificationType type) => 
        notificationHandler.PlayNotification(type);
    private void HandleCaptureNotification() => PlayNotification(NotificationType.Capture);
    private void HandleDevourNotification() => PlayNotification(NotificationType.Eat);

    public void HandleHungerNotification()
    {
        if (timeBeforeNextNotification > 0f)
            timeBeforeNextNotification -= Time.deltaTime;
        else if (GameManager.Instance.GrannyHealthHandler.IsHealthCritical)
        {
            PlayNotification(NotificationType.Hunger);
            timeBeforeNextNotification = waitBetweenNotifications;
        }
    }
}