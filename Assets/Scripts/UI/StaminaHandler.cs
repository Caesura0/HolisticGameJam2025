using UnityEngine;
using UnityEngine.UI;

public class StaminaHandler : MonoBehaviour
{
    private static StaminaHandler instance;
    public static StaminaHandler Instance
    {
        get
        {
            if (instance)
                return instance;
            else
            {
                Debug.Log("Stamina Handler Instance not found");
                return instance = null;
            }
        }
    }

    [SerializeField] private Image displayImage;
    [SerializeField] private int maxStamina = 5;
    [SerializeField] private float staminaRecoveryTime = 5f;
    private float recoveryTimer;

    private int currentStamina;
    bool initialized;

    private void Awake()
    {
        if (!instance)
            instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        Initialize();

        if (!initialized)
            return;

        displayImage.fillMethod = Image.FillMethod.Horizontal;
    }

    private void Update()
    {
        if (IsStaminaFull())
            return;

        if(recoveryTimer > 0)
        {
            recoveryTimer -= Time.deltaTime;
            return;
        }

        RecoverStamina();
        recoveryTimer = staminaRecoveryTime;
    }

    private void Initialize()
    {
        currentStamina = maxStamina;
        recoveryTimer = staminaRecoveryTime;
        initialized = displayImage && maxStamina > 0;
        UpdateDisplay();
    }
    private void UpdateDisplay()
    {
        if (!initialized)
            return;

        displayImage.fillAmount = 1f * currentStamina / maxStamina;
    }

    public bool HasStamina() => currentStamina > 0;
    public bool IsStaminaFull() => currentStamina == maxStamina;
    public void SpendStamina()
    {
        currentStamina = HasStamina() ? currentStamina - 1 : currentStamina;
        UpdateDisplay();
    }
    public void RecoverStamina()
    {
        currentStamina = IsStaminaFull() ? currentStamina : currentStamina + 1;
        UpdateDisplay();
    }
}
