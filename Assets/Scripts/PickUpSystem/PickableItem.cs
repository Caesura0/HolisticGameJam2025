using UnityEngine;

[RequireComponent (typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PickableItem: MonoBehaviour
{
    Rigidbody2D rb;
    Transform holder;
    bool beingHeld = false;
    Collider2D collision;
    const float stopThreshold = 0.1f;
    bool wasMoving = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        collision = GetComponent<Collider2D>();
    }
    
    private void Update()
    {
        if(beingHeld)
            transform.position = holder.position;
    }
    
    public void PickUp(Transform assignedHolder)
    {
        rb.bodyType = RigidbodyType2D.Kinematic;
        holder = assignedHolder;
        beingHeld = true;
        collision.enabled = false; // Disable collider while being held

    } 

    public void Throw(Vector3 velocity)
    {
        Release();
        rb.AddForce (velocity, ForceMode2D.Impulse);
        //activate trigger collider
        collision.enabled = true;
    }

    public void Release()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        holder = null;
        beingHeld = false;
    }
}