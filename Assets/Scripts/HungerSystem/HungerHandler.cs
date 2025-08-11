using NUnit.Framework;
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
    [SerializeField] private GameObject hungerIconPrefab;
    [SerializeField] private GameObject hungerIconHolder;
    [SerializeField] private int hungerLimit;
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

        currentHunger = hungerLimit;
        ResetTimer();
        InitializeDisplay();
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

    private Image[] displayIcons;
    private void InitializeDisplay()
    {
        List<Image> icons = new List<Image>();
        for (int i = 0; i < hungerLimit; i++)
        {
            Image display =
                Instantiate(hungerIconPrefab, hungerIconHolder.transform).GetComponent<Image>();
            icons.Add(display);
        }
        displayIcons = icons.ToArray();

        UpdateDisplay();
    }

    public void Feed(int foodValue)
    {
        foodValue = Mathf.Clamp(foodValue, 0, hungerLimit);
        currentHunger += foodValue;
        UpdateDisplay();
        ResetTimer();
    }

    private void UpdateDisplay()
    {
        for (int i = 0; i < hungerLimit; i++)
        {
            if (i > currentHunger - 1)
                displayIcons[i].gameObject.SetActive(false);
            else
                displayIcons[i].gameObject.SetActive(true);

            if (currentHunger <= hungerRagePoint)
                displayIcons[i].color = Color.red;
            else
                displayIcons[i].color = Color.white;
        }
    }
    public void Feed(int foodValue)
    {
        foodValue = Mathf.Clamp(foodValue, 0, hungerLimit);
        currentHunger += foodValue;
        UpdateDisplay();
        ResetTimer();
    }
}