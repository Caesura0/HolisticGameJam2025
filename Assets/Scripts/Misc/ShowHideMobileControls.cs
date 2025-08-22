using UnityEngine;

public class ShowHideMobileControls : MonoBehaviour
{


    private void Start()
    {
        if (Application.isMobilePlatform)
        {
            ShowControls();
        }
        else
        {
            HideControls();
        }
    }

    public void ShowControls()
    {
        gameObject.SetActive(true);
    }
    public void HideControls()
    {
        gameObject.SetActive(false);
    }
}