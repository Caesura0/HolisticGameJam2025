using UnityEngine;

public class ShowHideMobileControls : MonoBehaviour
{
    



    public void ShowControls()
    {
        gameObject.SetActive(true);
    }
    public void HideControls()
    {
        gameObject.SetActive(false);
    }
}
