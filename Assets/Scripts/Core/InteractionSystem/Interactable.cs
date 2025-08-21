using UnityEngine;

public enum StatusEffectType
{
    None,
    Slowed,
    Stunned
}

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Interactable : MonoBehaviour
{
    const float stopThreshold = 0.1f;

    [SerializeField, Range(1, 5)] private int health = 1;
    [SerializeField] private VFXType OnHitVFX = VFXType.Boom;
    [SerializeField] private VFXType OnDeathVFX = VFXType.None;
    [SerializeField] private float onHitShakeDuration = .3f;
    [SerializeField] private float onHitShakePower = .1f;

    Rigidbody2D rb;
    Transform holder;
    Collider2D collision;
    bool beingHeld, isMoving, wasMoving, isStatic, isEatable;

    [field: SerializeField] public StatusEffectType effectType { get; private set; } = StatusEffectType.None;
    [field: SerializeField] public float effectDuration { get; private set; } = 5f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        collision = GetComponent<Collider2D>();

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        collision.enabled = true;

        isEatable = GetType() == typeof(Eatable);
        OnStoppedMoving();
    }
    private void Update()
    {
        if (beingHeld)
        {
            transform.position = holder.position;
            return;
        }

        if (isMoving)
            CheckIfStopped();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isMoving)
            return;

        if (!collision.TryGetComponent<NPCSuperStateMachine>(out _))
            return;

        health--;
        OnHitEnemy();

        if (health <= 0)
            OnHealthDown();
    }

    public bool BeingHeld => beingHeld;
    public bool IsEatable => isEatable;

    public void Grab(Transform assignedHolder)
    {
        holder = assignedHolder;
        beingHeld = true;
        DisableCollision();
    }
    public void Throw(Vector3 velocity)
    {
        if (isEatable)
            return;

        EnableMovement();
        Release();
        EnableCollisionTrigger();

        if (velocity.magnitude > stopThreshold)
            rb.AddForce(velocity, ForceMode2D.Impulse);

        isMoving = true;
        wasMoving = true;
    }
    public void Release()
    {
        holder = null;
        beingHeld = false;
        EnableCollision();
    }

    private void CheckIfStopped()
    {
        if (isStatic)
            return;

        isMoving = rb.linearVelocity.magnitude > stopThreshold;

        if (wasMoving && !isMoving)
            OnStoppedMoving();

        wasMoving = isMoving;
    }

    private void OnStoppedMoving()
    {
        if (isEatable)
            return;

        DisableCollisionTrigger();
        DisableMovement();
    }
    private void OnHitEnemy()
    {
        rb.linearVelocity /= 2;
        VFXHandler.Instance.PlayVisualEffect(OnHitVFX, transform.position);
        CameraEffectsHandler.Instance.Shake(onHitShakeDuration, onHitShakePower);
    }
    private void OnHealthDown()
    {
        VFXHandler.Instance.PlayVisualEffect(OnDeathVFX, transform.position);
        Destroy(gameObject);
    }

    private void EnableMovement()
    {
        isStatic = false;
        rb.bodyType = RigidbodyType2D.Dynamic;
    }
    private void DisableMovement()
    {
        isStatic = true;
        rb.bodyType = RigidbodyType2D.Static;
    }
    private void EnableCollision() => collision.enabled = true;
    private void DisableCollision() => collision.enabled = false;
    private void EnableCollisionTrigger() => collision.isTrigger = true;
    private void DisableCollisionTrigger() => collision.isTrigger = false;
}