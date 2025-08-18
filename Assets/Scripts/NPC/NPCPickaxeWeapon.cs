// Simplified Pickaxe Weapon System - Focus on capture prevention and slime disarming
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class NPCPickaxeWeapon : MonoBehaviour
{
    [Header("Weapon Configuration")]
    [SerializeField] float dropForce = 3f;
    [SerializeField] Vector2 heldOffset = new Vector2(0.5f, 0.3f);

    Transform holder;
    bool isHeld = false;

    Collider2D weaponCollider;
    Rigidbody2D rb;
    SpriteRenderer spriteRenderer;

    // Event for when NPC loses weapon (due to slime, etc.)
    public System.Action OnWeaponLost;

    void Awake()
    {
        weaponCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Add Rigidbody2D if not present (for dropping)
        rb = GetComponent<Rigidbody2D>();
        if (!rb)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
            rb.gravityScale = 0; // Top-down game, no gravity
        }

        // Set up trigger for player interaction
        weaponCollider.isTrigger = true;
    }

    void Update()
    {
        if (isHeld && holder)
        {
            // Follow holder with offset
            transform.position = holder.position + (Vector3)heldOffset;
        }
    }

    public void Pickup(Transform newHolder)
    {
        if (isHeld) return;

        holder = newHolder;
        isHeld = true;

        // Disable physics while held
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;

        // Set as child for easier management
        transform.SetParent(holder);
        transform.localPosition = heldOffset;

        // Adjust sorting order to appear in front of NPC
        spriteRenderer.sortingOrder = 10;

        Debug.Log("Pickaxe picked up!");
    }

    public void Drop()
    {
        if (!isHeld) return;

        // Remove from parent
        transform.SetParent(null);

        holder = null;
        isHeld = false;

        // Re-enable physics
        rb.bodyType = RigidbodyType2D.Dynamic;

        // Add a small random force when dropping
        Vector2 dropDirection = Random.insideUnitCircle.normalized;
        rb.AddForce(dropDirection * dropForce, ForceMode2D.Impulse);

        // Reset sorting order
        spriteRenderer.sortingOrder = 5;

        // Reset rotation
        transform.rotation = Quaternion.identity;

        // Notify that weapon was lost
        OnWeaponLost?.Invoke();

        Debug.Log("Pickaxe dropped!");
    }

    // Simplified swing for visual threat only
    public void StartSwing()
    {
        if (!isHeld) return;

        // Just a simple visual animation - no complex combat
        StartCoroutine(SimpleSwingAnimation());
    }

    public void EndSwing()
    {
        // Stop any ongoing swing animation
        StopAllCoroutines();
        if (isHeld)
        {
            transform.rotation = Quaternion.identity;
        }
    }

    System.Collections.IEnumerator SimpleSwingAnimation()
    {
        float startRotation = transform.rotation.eulerAngles.z;
        float swingAngle = 45f;
        float duration = 0.3f;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float angle = Mathf.Sin(t / duration * Mathf.PI) * swingAngle;
            transform.rotation = Quaternion.Euler(0, 0, startRotation + angle);
            yield return null;
        }

        transform.rotation = Quaternion.Euler(0, 0, startRotation);
    }

    public bool IsHeld()
    {
        return isHeld;
    }

    // The key method: prevents player capture while weapon is held
    public bool PreventsCapture()
    {
        return isHeld;
    }

    // Public method to disarm NPC (called by slime projectiles, etc.)
    public void DisarmNPC()
    {
        if (!isHeld) return;

        Debug.Log("NPC disarmed by slime!");
        Drop(); // This will trigger OnWeaponLost event
    }

    // Handle interaction with player - this is where the capture prevention happens!
    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isHeld)
        {
            // Player is trying to capture an armed NPC!
            // The player controller should handle:
            // 1. Drain stamina as normal
            // 2. Fail the capture attempt
            // 3. Show feedback that the NPC is armed
            Debug.Log("Player attempting to capture armed NPC - this should fail and cost stamina!");
        }
    }
}