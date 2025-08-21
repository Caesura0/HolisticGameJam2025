using UnityEngine;

public enum VFXType
{
    None,
    Skull,
    HeartBreak,
    HeartGain,
    SoulRelease,
    Blood,
    Boom,
    Pow,
    Smoke
}
public class VFXHandler : MonoBehaviour
{
    private static VFXHandler _instance;
    public static VFXHandler Instance
    {
        get
        {
            if (_instance == null)
                Debug.LogError("VFXHandler Instance not present in the scene");
            return _instance;
        }
    }

    [SerializeField] private GameObject SkullVFXPrefab;
    [SerializeField] private GameObject HeartBreakVFXPrefab;
    [SerializeField] private GameObject HeartGainVFXPrefab;
    [SerializeField] private GameObject SoulReleaseVFXPrefab;
    [SerializeField] private GameObject BloodVFXPrefab;
    [SerializeField] private GameObject BoomVFXPrefab;
    [SerializeField] private GameObject PowVFXPrefab;
    [SerializeField] private GameObject SmokeVFXPrefab;

    private void Awake()
    {
        if (_instance == null)
            _instance = this;
        else
            Destroy(gameObject);
    }

    public void PlayVisualEffect(VFXType type, Vector3 position)
    {
        GameObject selectedObject = null;

        switch (type)
        {
            case VFXType.None:
                return;
            case VFXType.Skull:
                selectedObject = SkullVFXPrefab;
                break;
            case VFXType.HeartBreak:
                selectedObject = HeartBreakVFXPrefab;
                break;
            case VFXType.HeartGain:
                selectedObject= HeartGainVFXPrefab;
                break;
            case VFXType.SoulRelease:
                selectedObject = SoulReleaseVFXPrefab;
                break;
            case VFXType.Blood:
                selectedObject = BloodVFXPrefab;
                break;
            case VFXType.Boom:
                selectedObject = BoomVFXPrefab;
                break;
            case VFXType.Pow:
                selectedObject = PowVFXPrefab;
                break;
            case VFXType.Smoke:
                selectedObject = SmokeVFXPrefab;
                break;
        }

        if (selectedObject == null)
            return;

        var effect = GameObject.Instantiate(selectedObject, position, Quaternion.identity, transform);
        //To be safe, destroy object after 10 seconds
        Destroy(effect, 10);
    }
}
