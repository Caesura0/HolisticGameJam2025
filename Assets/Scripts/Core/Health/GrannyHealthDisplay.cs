using UnityEngine;

public class GrannyHealthDisplay : MonoBehaviour
{
    [SerializeField] private HealthHandler healthHandler;
    [SerializeField] private Transform healthIconsContainer;
    [SerializeField] private GameObject displayWrapper;

    private bool initialized;
    private IconDisplay[] healthDisplayIcons;

    private void Awake()
    {
        SetHidden(true);

        if(healthHandler)
        {
            healthHandler.OnHealthGain += UpdateDisplay;
            healthHandler.OnHealthDrop += UpdateDisplay;
        }

        bool includeInactiveIcons = true;
        healthDisplayIcons = healthIconsContainer.GetComponentsInChildren<IconDisplay>(includeInactiveIcons);
    }

    private void Initialize()
    {
        SetHidden(false);
        initialized = true;
    }

    private void UpdateDisplay()
    {
        if (!initialized)
            Initialize();

        int healthToIndex = healthHandler.CurrentHealth - 1;
        for (int i = 0; i < healthDisplayIcons.Length; i++)
        {
            if (i > healthToIndex)
                healthDisplayIcons[i].HideIcon();
            else
                healthDisplayIcons[i].ShowIcon();

            healthDisplayIcons[i].SetBlinkActive(healthHandler.IsHealthCritical);
        }
    }

    private void SetHidden(bool value) => displayWrapper.SetActive(!value);
}