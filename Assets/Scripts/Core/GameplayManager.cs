using System;
using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    private enum GameOverType
    {
        Survived, Starved, Captured
    }

    public event Action OnStarvedToDeath;
    public event Action OnSurvivedTimer;
    public event Action OnGotCaptured;

    private static GameplayManager instance = null;
    public static GameplayManager Instance
    {
        get
        {
            if (instance == null)
                Debug.LogError("GameplayManager instance not found");

            return instance;
        }
    }

    private void Awake()
    {
        Time.timeScale = 1.0f;
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
    private void Start()
    {
        if(HungerHandler.Instance)
            HungerHandler.Instance.OnDeathTrigger += HandleDeathEvent;

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
}