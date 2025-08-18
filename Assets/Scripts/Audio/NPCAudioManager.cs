using UnityEngine;

[RequireComponent(typeof(NPCSuperStateMachine))]
public class NPCAudioHandler : MonoBehaviour
{
    NPCSuperStateMachine stateMachine;

    void Awake()
    {
        stateMachine = GetComponent<NPCSuperStateMachine>();

        // Subscribe to all NPC events
        stateMachine.onSlimeHit.AddListener(OnSlimeHit);
        stateMachine.onStunned.AddListener(OnStunned);
        // Subscribe to the duration versions if you want different sounds based on duration
        stateMachine.onSlimeHitWithDuration.AddListener(OnSlimeHitWithDuration);
    }

    public void OnSlimeHit()
    {
        Debug.Log("Slime sound should play if exists");
        // Use existing hit sound as placeholder
        AudioManager.Instance?.PlayHitSound();
        // Or play a specific sound when you add it
        // AudioManager.Instance?.PlaySlimeHitSound();
    }

    void OnStunned()
    {
        // Could use boulder sound for heavy impact
        AudioManager.Instance?.PlayRockSounds();
        // Or AudioManager.Instance?.PlayStunSound();
    }

    void OnSlimeHitWithDuration(float duration)
    {
        // Play different sounds based on duration
        if (duration > 3f)
        {
            // Long slime effect - play a "heavy slime" sound
            AudioManager.Instance?.PlayHitSound();
        }
        else
        {
            // Quick slime - play a "splash" sound
            AudioManager.Instance?.PlayPickupPointsSound(); // as placeholder
        }
    }

    void OnDestroy()
    {
        // Clean up listeners
        if (stateMachine != null)
        {
            stateMachine.onSlimeHit.RemoveListener(OnSlimeHit);
            stateMachine.onStunned.RemoveListener(OnStunned);
            stateMachine.onSlimeHitWithDuration.RemoveListener(OnSlimeHitWithDuration);
        }
    }
}