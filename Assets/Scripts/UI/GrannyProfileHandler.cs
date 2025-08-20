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
    private bool initialized, isHungry;


    private void Start()
    {
        if (!TryGetComponent<Image>(out profileFrame) || !defaultProfile)
            return;

        initialized = true;

        HungerHandler.Instance.OnDropHeart += HandleHungerState;
        HungerHandler.Instance.OnGainHeart += HandleHungerState;

        PlayerInteractionHandler player = FindFirstObjectByType<PlayerInteractionHandler>();
        player.OnDevourEvent += HandleDevourEvent;
    }

    private void HandleDevourEvent() => sinCount++;

    private void HandleHungerState()
    {
        if(HungerHandler.Instance.InHungerRage())
            isHungry = true;
        else
            isHungry= false;
    }

    private void Update()
    {
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