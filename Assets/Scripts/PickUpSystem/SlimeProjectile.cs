using UnityEngine;

public class SlimeProjectile : MonoBehaviour
{
    public float slowDuration = 2f;

    // Projectile doesn't care about our hit logic
    // NPC handles it via OnTriggerEnter2D
}
