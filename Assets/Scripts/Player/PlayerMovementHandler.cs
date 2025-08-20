using System;
using UnityEngine;

[RequireComponent (typeof(Rigidbody2D))]
public class PlayerMovementHandler : MonoBehaviour
{
    public event Action<float> OnUpdateSpeed;

    [SerializeField] private float movementSpeed = 2;

    private Vector3 velocity = Vector3.zero;
    private Rigidbody2D rb;
    private bool up, right, down, left;

    public Vector3 Velocity => velocity;
    public bool movingUp => up;
    public bool movingDown => down;
    public bool movingLeft => left;
    public bool movingRight => right;

    private void Awake() => rb = GetComponent<Rigidbody2D>();
    private void Start() => Controls.Instance.OnPlayerMove += UpdateVelocity;

    public void HandleMovement() => rb.linearVelocity = velocity;

    private void UpdateVelocity(Vector2 input)
    {
        Vector2 direction = input.normalized;
        velocity = direction * movementSpeed;
        float errorMargin = .3f;

        up = direction.y >= errorMargin;
        right = direction.x >= errorMargin;
        down = direction.y <= -errorMargin;
        left = direction.x <= -errorMargin;
        OnUpdateSpeed?.Invoke(GetSpeedFractionized());
    }

    private float GetSpeedFractionized()
    {
        float magnitude = velocity.magnitude;
        return magnitude > 0 ? (1 + movementSpeed / (magnitude)) : 1;
    }
}