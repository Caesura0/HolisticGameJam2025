using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionButton : MonoBehaviour
{
    [SerializeField] private Image display;
    [SerializeField] private TextMeshProUGUI interactionTextbox;
    private const string pickUpText = "Pick Up";
    private const string placeDownText = "Place Down";
    private const string throwText = "Throw";
    private const string captureText = "Capture";
    private const string devourText = "Devour";

    private PlayerMovementHandler playerMovementHandler;
    bool holdingItem;
    bool itemIsNPC;
    InteractableItem selectedItem;
    private void Start()
    {
        PlayerInteractionHandler playerInteraction = GameManager.Instance.PlayerInteractionHandler;
        playerInteraction.OnSelectionUpdated += HandleSelectionUpdate;
        if (playerInteraction.TryGetComponent<PlayerMovementHandler>(out playerMovementHandler))
            playerInteraction.OnItemPicked += HandleItemPicked;

    }

    private void Update()
    {
        if (holdingItem && !itemIsNPC)
            UpdateHoldingItemInteractionText();
    }
    private void UpdateHoldingItemInteractionText()
    {
        if (playerMovementHandler.Velocity.magnitude > .1f)
            interactionTextbox.text = throwText;
        else
            interactionTextbox.text = placeDownText;
    }
    private void HandleItemPicked(PickableItem newSelection)
    {
        holdingItem = true;
        if (!itemIsNPC)
            interactionTextbox.text = devourText;
    }

    private void HandleSelectionUpdate(InteractableItem newSelection)
    {
        holdingItem = false;
        itemIsNPC = false;
        selectedItem = newSelection;
        if (!newSelection)
        {
            display.enabled = false;
            interactionTextbox.text = "";
            return;
        }
        else if (newSelection.Is<ConsumableItem>())
        {
            interactionTextbox.text = captureText;
            itemIsNPC = true;
        }
        else if (newSelection.Is<PickableItem>())
            interactionTextbox.text = pickUpText;
        display.enabled = true;
    }
}
