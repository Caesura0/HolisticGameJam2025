using UnityEngine;

[RequireComponent (typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PickableItem: MonoBehaviour
{
    Rigidbody2D rb;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    public void PickUp(Transform holder)
    {
        rb.bodyType = RigidbodyType2D.Kinematic;
        transform.parent = holder.transform;
        transform.position = holder.transform.position;
    } 

    public void Throw(Vector3 velocity)
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        transform.parent = null;
        rb.AddForce (velocity, ForceMode2D.Impulse);
    }
}
