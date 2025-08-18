using UnityEngine;

public class ImpactBehavior : MonoBehaviour
{
    Collider2D collision;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(TryGetComponent<NPCSuperStateMachine>( out var npc))
        {
            //trigger particle effect
            //destroy gameObject
            //stun NPC

        }
    }
}
