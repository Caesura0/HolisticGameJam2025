using System;
using UnityEngine;
using UnityEngine.UI;

public class SurvivalTimeHandler : MonoBehaviour
{
    private static SurvivalTimeHandler instance = null;
    public static SurvivalTimeHandler Instance
    {
        get
        {
            if (instance == null)
                Debug.LogError("SurvivalTimeHandler not found");
            return instance;
        }
    }

    public event Action OnTimerComplete;

    [SerializeField] private Image display;
    [SerializeField, Tooltip("Units in Seconds")] 
    private float requiredSurvivalTime = 300;
    private float remainingTime = 0;
    private bool
        initialized,
        paused,
        completed;

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    private void FixedUpdate()
    {
        if (paused || completed || GameplayManager.Instance.IsGamePaused())
            return;

        if (!initialized)
        {
            remainingTime = requiredSurvivalTime;
            initialized = true;
        }

        UpdateDisplay();

        if(IsCountdownCompleted())
        {
            EndTimer();
            OnTimerComplete?.Invoke();
            return;
        }

        remainingTime -= Time.fixedDeltaTime;
    }

    private void UpdateDisplay() =>
        display.fillAmount = remainingTime / requiredSurvivalTime;

    private void EndTimer() => completed = true;
    public void PauseTimer() => paused = true;
    public void ResumeTimer() => paused = false;
    public bool IsCountdownCompleted() => remainingTime <= 0;
}