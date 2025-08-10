using UnityEngine;

[RequireComponent (typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PickableItem: MonoBehaviour
{
    Rigidbody2D rb;
    Transform holder;
    bool beingHeld = false;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
    } 

    public void Throw(Vector3 velocity)
    {
        Release();
        rb.AddForce (velocity, ForceMode2D.Impulse);
    }

    public void Release()
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        holder = null;
        beingHeld = false;
    }
}
