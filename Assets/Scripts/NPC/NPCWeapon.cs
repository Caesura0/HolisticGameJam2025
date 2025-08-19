using UnityEngine;

public class NPCWeapon : MonoBehaviour
{
    [Header("Visual")]
    [SerializeField] Vector2 heldOffset = new Vector2(0, 1f);
    [SerializeField] float destroyAfterDropDelay = 1f;

    Transform holder;
    bool isHeld = false;
    SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void LateUpdate()
    {
        if (isHeld && holder)
        {
            // Float above NPC's head
            transform.position = holder.position + (Vector3)heldOffset;

            // Optional: gentle bobbing animation
            float bob = Mathf.Sin(Time.time * 2f) * 0.05f;
            transform.position += Vector3.up * bob;
        }
    }

    public void Pickup(Transform newHolder)
    {
        holder = newHolder;
        isHeld = true;

        transform.SetParent(holder);
        transform.localPosition = heldOffset; // Above head position
        spriteRenderer.sortingOrder = 10;
    }

    public void Drop()
    {
        transform.SetParent(null);
        holder = null;
        isHeld = false;

        spriteRenderer.sortingOrder = 0;

        if (!TryGetComponent<Rigidbody2D>(out var rb))
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }
        rb.gravityScale = 1f;
        rb.AddForce(Random.insideUnitCircle * 2f, ForceMode2D.Impulse);

        Destroy(gameObject, destroyAfterDropDelay);
    }

    public bool IsHeld() => isHeld;
}