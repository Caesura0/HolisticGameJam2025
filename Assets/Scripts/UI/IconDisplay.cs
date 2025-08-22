using UnityEngine;
using UnityEngine.UI;

public class IconDisplay : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private float timeBetweenBlinks = .5f;
    private float blinkTimer;
    private bool blinkActive;
    private bool initialized;

    private void Awake()
    {
        initialized = iconImage;
    }

    private void Update()
    {
        if (!initialized || GameplayManager.Instance.IsGamePaused())
            return;

        if (blinkTimer > 0)
        {
            blinkTimer -= Time.deltaTime;
            return;
        }

        if (blinkActive)
        {
            iconImage.enabled = !iconImage.enabled;

            blinkTimer = timeBetweenBlinks;
        }
    }
    
    public void HideIcon()
    {
        if (!initialized)
            return;

        iconImage.gameObject.SetActive(false);
    }

    public void ShowIcon()
    {
        if(!initialized)
            return;

        iconImage.gameObject.SetActive(true);
    }

    public void IsBlinkActive(bool assignedValue) => blinkActive = assignedValue;
}