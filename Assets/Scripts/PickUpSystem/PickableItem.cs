using UnityEngine;

public enum StatusEffectType
{
    None,
    Slowed,
    Stunned
}
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
    [field: SerializeField] public bool DestroyOnHitNPC {  get; private set; }
    [field: SerializeField] public StatusEffectType effectType {  get; private set; } = StatusEffectType.None;
    [field: SerializeField] public float effectDuration {  get; private set; } = 5f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        collision = GetComponent<Collider2D>();
        OnStopMoving();
    }
    
    private void Update()
    {
        if(beingHeld)
            transform.position = holder.position;
        CheckIfStopped();
    }
    
    public void PickUp(Transform assignedHolder)
    {
        rb.bodyType = RigidbodyType2D.Dynamic;
        holder = assignedHolder;
        beingHeld = true;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        collision.isTrigger = true; // Disable collider while being held
    } 

    public void Throw(Vector3 velocity)
    {
        Release();
        if (velocity.magnitude > stopThreshold)
            rb.AddForce(velocity, ForceMode2D.Impulse);
        else
            OnStopMoving();
        //activate trigger collider
    }

    public void Release()
    {
        holder = null;
        beingHeld = false;
    }

    void CheckIfStopped()
    {
        // Only check if Rigidbody2D is dynamic
        if (rb.bodyType == RigidbodyType2D.Kinematic)
        {
            if (rb.linearVelocity.magnitude > stopThreshold)
            {
                wasMoving = true;
            }
            else if (wasMoving)
            {
                wasMoving = false;
                OnStopMoving();
            }
        }
    }

    void OnStopMoving()
    {
        Debug.Log("Item has stopped moving.");
        collision.isTrigger = false;
        if (GetType() != typeof(EatableItem))
            rb.bodyType = RigidbodyType2D.Static;
    }

    public bool BeingHeld => beingHeld;
}