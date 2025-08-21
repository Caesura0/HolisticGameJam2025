using System.Collections;
using UnityEngine;

public sealed class CameraEffectsHandler : MonoBehaviour
{
    private static CameraEffectsHandler instance = null;
    public static CameraEffectsHandler Instance
    {
        get
        {
            if (instance == null)
                Debug.LogError("CameraEffectHandler instance not found!");
            return instance;
        }
    }

    private Vector3 originalPos;

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    void OnEnable()
    {
        originalPos = transform.localPosition;
    }

    public void Shake(float duration, float magnitude)
    {
        StopAllCoroutines();
        StartCoroutine(ShakeCoroutine(duration, magnitude));
    }

    private IEnumerator ShakeCoroutine(float duration, float magnitude)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float offsetX = Random.Range(-1f, 1f) * magnitude;
            float offsetY = Random.Range(-1f, 1f) * magnitude;

            transform.localPosition = new Vector3(originalPos.x + offsetX, originalPos.y + offsetY, originalPos.z);

            elapsed += Time.deltaTime;

            yield return null;
        }

        transform.localPosition = originalPos;
    }
}