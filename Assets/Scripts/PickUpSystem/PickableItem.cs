using UnityEngine;

public enum StatusEffectType
{
    None,
    Slowed,
    Stunned
}

public enum SoundType
{
    None,
    Table,
    Log,
    Slime
}

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PickableItem : MonoBehaviour
{
    [Range(1, 5)]
    [SerializeField] private int health = 1;
    [SerializeField] private VFXType OnHitVFX = VFXType.Boom;
    [SerializeField] private VFXType OnDeathVFX = VFXType.None;
    [SerializeField] private float onHitShakeDuration = .3f;
    [SerializeField] private float onHitShakePower = .1f;
    Rigidbody2D rb;
    Transform holder;
    Collider2D collision;
    const float stopThreshold = 0.1f;
    bool beingHeld, isMoving, wasMoving, isStatic, isEatable;


    [SerializeField] private SoundType impactSound = SoundType.Table;

    [field: SerializeField] public StatusEffectType effectType { get; private set; } = StatusEffectType.None;
    [field: SerializeField] public float effectDuration { get; private set; } = 5f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        collision = GetComponent<Collider2D>();

        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        collision.enabled = true;

        isEatable = GetType() == typeof(EatableItem);
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

    void CheckIfStopped()
    {
        if (isStatic)
            return;

        isMoving = rb.linearVelocity.magnitude > stopThreshold;

        if (wasMoving && !isMoving)
            OnStoppedMoving();

        wasMoving = isMoving;
    }

    void OnStoppedMoving()
    {
        if (isEatable)
            return;

        DisableCollisionTrigger();
        DisableMovement();
    }

    public bool BeingHeld => beingHeld;
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

    private void OnHitEnemy()
    {
        rb.linearVelocity /= 2;
        PlayImpactSound();
        VFXHandler.Instance.PlayVisualEffect(OnHitVFX, transform.position);
        CameraEffectsHandler.Instance.Shake(onHitShakeDuration, onHitShakePower);
    }

    private void OnHealthDown()
    {
        VFXHandler.Instance.PlayVisualEffect(OnDeathVFX, transform.position);
        Destroy(gameObject);
    }

    private void PlayImpactSound()
    {
        switch(impactSound)
        {
            case SoundType.None:
                return;
            case SoundType.Table:
                AudioManager.Instance.PlayTableSound();
                break;
            case SoundType.Log:
                AudioManager.Instance.PlayLogSound();
                break;
            case SoundType.Slime:
                AudioManager.Instance.PlaySlimeSound();
                break;
        }
    }
}