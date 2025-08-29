using UnityEngine;
using UnityEngine.UI;

public class GrannyHealthTimerDisplay : MonoBehaviour
{
    [SerializeField] private Image timerDisplay;
    [SerializeField] private HealthHandler healthHandler;

    private bool isActive;

    private void Awake()
    {
        isActive = timerDisplay && healthHandler;

        GameManager gameManager = GameManager.Instance;
        if (gameManager)
            gameManager.OnGamePaused += HandleGamePaused;
    }
    private void Update() => UpdateDisplay();
    private void UpdateDisplay() => timerDisplay.fillAmount = healthHandler.GetTimeBeforeHealthDropNormalized();

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