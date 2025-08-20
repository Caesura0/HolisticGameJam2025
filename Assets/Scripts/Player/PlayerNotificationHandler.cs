using UnityEngine;

public class PlayerNotificationHandler : MonoBehaviour
{
    [SerializeField] private float hungerNotificationTimer = 3f;
    private float hungerNotificationTime = 0f;

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
        if (hungerNotificationTime > 0f)
        {
            hungerNotificationTime -= Time.deltaTime;
            return;
        }

        if (!HungerHandler.Instance.InHungerRage())
            return;

        hungerNotificationTime = hungerNotificationTimer;
        PlayNotification(NotificationType.Hunger);
    }
}