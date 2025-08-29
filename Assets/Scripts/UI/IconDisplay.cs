using UnityEngine;
using UnityEngine.UI;

public class IconDisplay : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private float timeBetweenBlinkSteps = .5f;

    private bool isActive;
    private bool isBlinkActive, hasBlinkStarted;
    private float timeBeforeNextStep;

    private void Awake()
    {
        isActive = iconImage;

        GameManager gameManager = GameManager.Instance;
        if (gameManager)
            gameManager.OnGamePaused += HandleGamePaused;
    }
    private void Update()
    {
        if (!isActive)
            return;

        if (timeBeforeNextStep > 0)
            timeBeforeNextStep -= Time.deltaTime;
        else
            HandleBlink();
    }
    
    public void ShowIcon()
    {
        if(!isActive)
            return;

        iconImage.gameObject.SetActive(true);
    }
    public void HideIcon()
    {
        if (!isActive)
            return;

        iconImage.gameObject.SetActive(false);
    }

    public void SetBlinkActive(bool value) => isBlinkActive = value;

    private void HandleBlink()
    {
        if (hasBlinkStarted)
            BlinkStepTrigger();
        else if (isBlinkActive)
            BlinkStepTrigger();
    }
    private void BlinkStepTrigger()
    {
        iconImage.enabled = hasBlinkStarted;
        timeBeforeNextStep = timeBetweenBlinkSteps;
        hasBlinkStarted = !hasBlinkStarted;
    }

    private void HandleGamePaused()
    {
        if (!isActive)
            return;

        isActive = false;
        GameManager.Instance.OnGameResumed += HandleGameResumed;
    }
    private void HandleGameResumed()
    {
        isActive = true;
        GameManager.Instance.OnGameResumed -= HandleGameResumed;
    }
}