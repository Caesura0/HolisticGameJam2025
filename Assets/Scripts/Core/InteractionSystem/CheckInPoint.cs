using UnityEngine;

[RequireComponent (typeof(Collider2D))]
public class CheckInPoint : MonoBehaviour
{
    [SerializeField] private ItemData pointData;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<PlayerController>(out _))
            GameEvents.CheckIn(pointData);
    }
}
