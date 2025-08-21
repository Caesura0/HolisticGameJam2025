using UnityEngine;

public class GamePhaseManager : MonoBehaviour
{
    [SerializeField] int currentPhase = 1; // 1 Party, 2 Defense, 3 Hunters (harder)



    private void Start()
    {
        AudioManager.Instance.PlayGameplayMusic();
    }

    public void AdvancePhase()
    {
        currentPhase = Mathf.Clamp(currentPhase + 1, 1, 3);
        GameEvents.RaisePhaseChanged(currentPhase);
        Debug.Log($"[PhaseDirector] Phase -> {currentPhase}");
    }

    // Optional: quick test keys
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1)) AdvancePhase();
    }
}
