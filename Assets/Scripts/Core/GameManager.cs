using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private enum GameOverType
    {
        Survived, Starved, Captured
    }

    public event Action OnFirstKill;
    public event Action OnStarvedToDeath;
    public event Action OnSurvivedTimer;
    public event Action OnGotCaptured;
    public event Action OnGamePaused;
    public event Action OnGameResumed;

    private static GameManager instance = null;
    public static GameManager Instance
    {
        get
        {
            if (instance == null)
                Debug.LogError("GameplayManager instance not found");

            return instance;
        }
    }
    private HealthHandler healthHandler = null;
    public HealthHandler GrannyHealthHandler => healthHandler;
    private PlayerInteractionHandler playerInteractionHandler = null;
    private void Awake()
    {
        Time.timeScale = 1.0f;
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);

        if(FindFirstObjectByType<GrannyHealthDisplay>().TryGetComponent<HealthHandler>(out healthHandler))
            healthHandler.OnDeathTrigger += HandleDeathEvent;
        playerInteractionHandler = FindFirstObjectByType<PlayerInteractionHandler>();
        if (playerInteractionHandler)
            playerInteractionHandler.OnDevourEvent += HandleFirstKillEvent;
    }

    private void HandleFirstKillEvent()
    {
        OnFirstKill?.Invoke();
        playerInteractionHandler.OnDevourEvent -= HandleFirstKillEvent;
    }

    private void Start()
    {
        if (SurvivalTimeHandler.Instance)
            SurvivalTimeHandler.Instance.OnTimerComplete += HandleSurviveEvent;

        //if(PlayerController.OnCaptured)
            //Subscribe to PlayerController.OnCaptured Event
    }

    private void HandleDeathEvent() => CallGameOver(GameOverType.Starved);
    private void HandleSurviveEvent() => CallGameOver(GameOverType.Survived);
    private void HandleCapturedEvent() => CallGameOver(GameOverType.Captured);

    private void CallGameOver(GameOverType gameOverType)
    {
        switch (gameOverType)
        {
            case GameOverType.Survived:
                OnSurvivedTimer?.Invoke();
                break;
            case GameOverType.Starved:
                OnStarvedToDeath?.Invoke();
                break;
            case GameOverType.Captured:
                OnGotCaptured?.Invoke();
                break;
        }
    }

    private bool gamePaused;
    public void PauseGame()
    {
        gamePaused = true;
        OnGamePaused?.Invoke();
    }
    public void ResumeGame()
    {
        gamePaused = false;
        OnGameResumed?.Invoke();
    }
    public bool IsGamePaused() => gamePaused;
}