using UnityEngine;

public enum NotificationType
{
    Alert,
    Hunger,
    Eat,
    KO,
    Slow,
    Attack
}
public class NotificationHandler : MonoBehaviour
{
    private const string AlertString = "Alert";
    private const string HungerString = "Hunger";
    private const string EatString = "Eat";
    private const string KOString = "KO";
    private const string SlowString = "Slow";
    private const string AttackString = "Attack";
    private int AlertTriggerId = Animator.StringToHash(AlertString);
    private int HungerTriggerId = Animator.StringToHash(HungerString);
    private int EatTriggerId = Animator.StringToHash(EatString);
    private int KOTriggerId = Animator.StringToHash(KOString);
    private int SlowTriggerId = Animator.StringToHash(SlowString);
    private int AttackTriggerId = Animator.StringToHash(AttackString);

    [SerializeField] private Animator animator;
    private bool initialized;

    private void Awake()
    {
        if(animator)
            initialized = true;
    }

    /// <summary>
    /// Displays a visual notification above the head
    /// </summary>
    /// <param name="type">The type of notification (alert, hunger, panic, etc...)</param>
    /// <param name="isBool">Specify if it is a bool type notification and not a trigger</param>
    public void PlayNotification(NotificationType type, bool enableIfBool)
    {
        if (!initialized)
            return;

        switch (type)
        {
            case NotificationType.Alert:
                TriggerAnimation(AlertTriggerId);
                break;
            case NotificationType.Hunger:
                TriggerAnimation(HungerTriggerId);
                break;
            case NotificationType.Eat:
                TriggerAnimation(EatTriggerId);
                break;
            case NotificationType.KO:
                TriggerAnimation(KOTriggerId, enableIfBool);
                break;
            case NotificationType.Slow:
                TriggerAnimation(SlowTriggerId);
                break;
            case NotificationType.Attack:
                TriggerAnimation(AttackTriggerId);
                break;
        }
    }
    public void PlayNotification(NotificationType type) => PlayNotification(type, true);

    private void TriggerAnimation(int animationId)
    {
        if (animationId == 0)
            return;
        
        animator.SetTrigger(animationId);
    }

    private void TriggerAnimation(int animationId, bool triggerIfBool)
    {
        if (animationId == 0)
            return;

        animator.SetBool(animationId, triggerIfBool);
    }

    public void ClearNotification()
    {
        TriggerAnimation(KOTriggerId, false);
        TriggerAnimation(SlowTriggerId, false);
    }
}