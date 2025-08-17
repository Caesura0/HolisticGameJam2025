using UnityEngine;

public enum NotificationType
{
    Alert,
    Hunger,
    Eat
}
public class NotificationHandler : MonoBehaviour
{
    private const string AlertString = "Alert";
    private const string HungerString = "Hunger";
    private const string EatString = "Eat";
    private int AlertTriggerId = Animator.StringToHash(AlertString);
    private int HungerTriggerId = Animator.StringToHash(HungerString);
    private int EatTriggerId = Animator.StringToHash(EatString);

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
            case NotificationType.Eat:
                chosenId = EatTriggerId;
                break;
        }

        if(chosenId != 0)
            animator.SetTrigger(chosenId);
    }
}