using UnityEngine;

public enum NotificationType
{
    Alert,
    Hunger
}
public class NotificationHandler : MonoBehaviour
{
    private const string AlertString = "Alert";
    private const string HungerString = "Hunger";
    private int AlertTriggerId = Animator.StringToHash(AlertString);
    private int HungerTriggerId = Animator.StringToHash(HungerString);

    [SerializeField] private Animator animator;
    private bool initialized;

    private void Awake()
    {
        if(animator)
            initialized = true;
    }

    public void PlayNotification(NotificationType type)
    {
        if (!initialized)
            return;

        int chosenId = 0;
        switch (type)
        {
            case NotificationType.Alert:
                chosenId = AlertTriggerId;
                break;
            case NotificationType.Hunger:
                chosenId = HungerTriggerId;
                break;
        }

        if(chosenId != 0)
            animator.SetTrigger(chosenId);
    }
}