using System;
using TMPro;
using UnityEngine;

public class HungerHandler : MonoBehaviour
{
    private static HungerHandler instance;
    public static HungerHandler Instance
    {
        get { if(instance)
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
    public int currentHunger {  get; private set; }
    public float remainingResetTime {  get; private set; }
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
    }

    private void Update()
    {
        if (remainingResetTime > 0)
            remainingResetTime -= Time.deltaTime;
        else
        {
            ResetTimer();
            currentHunger--;
            OnHungerUpdate?.Invoke(currentHunger);
        }
        timerUI.text = TimeSpan.FromSeconds(remainingResetTime).ToString(@"mm\:ss");
    }

    private void ResetTimer() => remainingResetTime = hungerResetTime;
}