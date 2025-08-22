using TMPro;
using UnityEngine;

public class KillCountHandler : MonoBehaviour
{
    int killCount = 0;
    [SerializeField] private TextMeshProUGUI textMeshPro;
    private void Start()
    {
        PlayerInteractionHandler player = FindFirstObjectByType<PlayerInteractionHandler>();
        if(player != null)
            player.OnDevourEvent += IncreaseKillCount;

        UpdateVisual();
    }

    private void IncreaseKillCount()
    {
        killCount++;
        UpdateVisual();
    }

    private void UpdateVisual() => textMeshPro.text = killCount.ToString();
}
