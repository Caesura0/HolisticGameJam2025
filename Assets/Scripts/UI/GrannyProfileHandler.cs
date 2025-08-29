using UnityEngine;
using UnityEngine.UI;

public class GrannyProfileHandler : MonoBehaviour
{
    public int sinCount { get; private set; } = 0;

    [SerializeField] private Sprite defaultProfile = null;
    [SerializeField] private Sprite evilProfile = null;
    [SerializeField] private Sprite sadProfile = null;
    [SerializeField] private int bearableSinLimit = 5;
    [SerializeField, Range(.5f, 5f)] private float profileRefreshTime = 2f;

    private Image profileFrame; 
    private float remainingTimer = 0;
    private bool 
        initialized, 
        isHungry;

    private void Start()
    {
        if (!TryGetComponent<Image>(out profileFrame) || !defaultProfile)
            return;

        initialized = true;

        HealthHandler healthHandler = GameManager.Instance?.GrannyHealthHandler;
        if (healthHandler)
        {
            healthHandler.OnHealthDrop += HandleHungerState;
            healthHandler.OnHealthGain += HandleHungerState;
        }

        PlayerInteractionHandler player = FindFirstObjectByType<PlayerInteractionHandler>();
        player.OnDevourEvent += HandleDevourEvent;
    }

    private void HandleDevourEvent() => sinCount++;

    private void HandleHungerState() =>
        isHungry = GameManager.Instance.GrannyHealthHandler.IsHealthCritical;

    private void Update()
    {
        if (!initialized || GameManager.Instance.IsGamePaused())
            return;

        if (remainingTimer > 0)
        {
            remainingTimer -= Time.deltaTime;
            return;
        }
        else
            remainingTimer = profileRefreshTime;

        Sprite selectedSprite = isHungry ? evilProfile : defaultProfile;
        
        float randNum = Random.value;
        
        if (sinCount > bearableSinLimit && randNum >= .5f)
            profileFrame.sprite = sadProfile;
        else
            profileFrame.sprite = selectedSprite;
    }
}