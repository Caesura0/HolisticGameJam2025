using System;
using UnityEngine;

public class HealthHandler : MonoBehaviour
{
    public event Action OnHealthDrop;
    public event Action OnHealthGain;
    public event Action OnDeathTrigger;

    [SerializeField] private int maxHealth = 5;
    [SerializeField] private int criticalHealthPoint = 3;
    [SerializeField] private float healthReductionTime = 15f;

    private int currentHealth = 0;
    private float timeLeftBeforeReduction;
    private bool isActive = false;

    public bool IsHealthCritical => currentHealth <= criticalHealthPoint;
    public bool IsActive => isActive;
    public int CurrentHealth => currentHealth;
    public float GetTimeBeforeHealthDropNormalized() => timeLeftBeforeReduction / healthReductionTime;

    private void Awake()
    {
        GameManager gameManager = GameManager.Instance;
        if (gameManager)
        {
            gameManager.OnFirstKill += Activate;
            gameManager.OnGamePaused += HandleGamePaused;
        }
    }
    private void Start()
    {
        currentHealth = maxHealth;
        timeLeftBeforeReduction = healthReductionTime;
    }
    private void Update()
    {
        if (!IsActive)
            return;

        if (timeLeftBeforeReduction > 0f)
            timeLeftBeforeReduction -= Time.deltaTime;
        else
            DropHealthPoint();
    }

    private void Activate() => isActive = true;
    private void Deactivate() => isActive = false;

    public void RecoverHealthPoint()
    {
        if (!IsActive)
            return;

        currentHealth = Mathf.Min(currentHealth + 1, maxHealth);
        OnHealthGain?.Invoke();
        timeLeftBeforeReduction = healthReductionTime;
    }
    private void DropHealthPoint()
    {
        if (!IsActive)
            return;

        int minHealth = 0;
        currentHealth = Mathf.Max(currentHealth - 1, minHealth);
        OnHealthDrop?.Invoke();
        timeLeftBeforeReduction = healthReductionTime;

        if (currentHealth == minHealth)
        {
            Deactivate();
            OnDeathTrigger?.Invoke();
        }
    }

    private void HandleGamePaused()
    {
        if (!IsActive)
            return;

        Deactivate();
        GameManager.Instance.OnGameResumed += HandleGameResumed;
    }
    private void HandleGameResumed()
    {
        Activate();
        GameManager.Instance.OnGameResumed -= HandleGameResumed;
    }
}