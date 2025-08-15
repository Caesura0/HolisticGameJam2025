using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HungerHandler : MonoBehaviour
{
    private static HungerHandler instance;
    public static HungerHandler Instance
    {
        get
        {
            if (instance)
                return instance;
            return instance = new GameObject("_HungerHandler").AddComponent<HungerHandler>();
        }
    }

    public event Action<int> OnHungerUpdate;

    [SerializeField] private TextMeshProUGUI timerUI;
    [SerializeField] private RectTransform hungerIconDisplayContainer;
    private IconDisplay[] displayIcons;
    [SerializeField] private int hungerRagePoint;
    [SerializeField] private float hungerResetTime;
    public int currentHunger { get; private set; }
    public float remainingResetTime { get; private set; }
    public bool InHungerRage() => currentHunger <= hungerRagePoint;

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeHandler();
    }

    private void Update()
    {
        if (remainingResetTime > 0)
            remainingResetTime -= Time.deltaTime;
        else
        {
            ResetTimer();
            currentHunger--;
            UpdateDisplay();
            OnHungerUpdate?.Invoke(currentHunger);
        }
        timerUI.text = TimeSpan.FromSeconds(remainingResetTime).ToString(@"mm\:ss");
    }

    private void ResetTimer() => remainingResetTime = hungerResetTime;

    private void InitializeHandler()
    {
        displayIcons = hungerIconDisplayContainer.GetComponentsInChildren<IconDisplay>(true);
        currentHunger = displayIcons.Length;
        UpdateDisplay();
        ResetTimer();
    }

    private void UpdateDisplay()
    {
        for (int i = 0; i < displayIcons.Length; i++)
        {
            if (i > currentHunger - 1)
                displayIcons[i].HideIcon();
            else
                displayIcons[i].ShowIcon();

            displayIcons[i].IsBlinkActive(currentHunger <= hungerRagePoint);
        }
    }

    public void Feed(int foodValue)
    {
        foodValue = Mathf.Clamp(foodValue, 0, displayIcons.Length);
        currentHunger += foodValue;
        UpdateDisplay();
        ResetTimer();
    }
}