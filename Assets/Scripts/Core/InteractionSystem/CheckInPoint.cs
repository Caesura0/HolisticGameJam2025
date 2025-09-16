using UnityEngine;

[RequireComponent (typeof(Collider2D))]
public class CheckInPoint : MonoBehaviour
{
    [SerializeField] private ItemData pointData;
    private void Awake()
    {
        if (TryGetComponent<Collider2D>(out Collider2D collider))
            collider.isTrigger = true;

        if(!pointData || pointData.Type != ItemType.CheckInPoint)
            Destroy(gameObject);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent<PlayerController>(out _))
            GameEvents.CheckIn(pointData);
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 2f);
    }
#endif
}
