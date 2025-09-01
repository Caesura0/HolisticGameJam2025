using System;
using UnityEngine;

[Serializable]
public class ThrowableItem : PickableItem
{
    public event Action<int> OnTargetHit;
    public event Action OnMovementStopped;

    const float stopThreshold = 0.1f;
    bool isStatic, isMoving, wasMoving;
    Rigidbody2D rb;
    public int testInt;

    public override void Initialize()
    {
        base.Initialize();
        initialized = transform.TryGetComponent<Rigidbody2D>(out rb);
    }
    public override void Tick()
    {
        base.Tick();

        if (!initialized)
            return;

        if (isMoving)
            CheckIfStopped();
    }
    public override void OnTriggerEnter2D(Collider2D collision)
    {
        if (!initialized)
            return;

        if (!isMoving)
            return;

        if (!collision.TryGetComponent<InteractableItem>(out InteractableItem target))
            return;

        rb.linearVelocity /= 2;
        OnTargetHit?.Invoke(target.itemId);
    }

    public void Throw(Vector2 velocity)
    {
        if (!initialized)
            return;

        EnableMovement();
        Release();
        EnableCollisionTrigger();

        if (velocity.magnitude > stopThreshold)
            rb.AddForce(velocity, ForceMode2D.Impulse);

        isMoving = true;
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
        DisableCollisionTrigger();
        DisableMovement();
        OnMovementStopped?.Invoke();
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

    private void EnableCollisionTrigger() => collision.isTrigger = true;
    private void DisableCollisionTrigger() => collision.isTrigger = false;
}
