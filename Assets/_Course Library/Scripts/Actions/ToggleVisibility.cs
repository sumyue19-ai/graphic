using UnityEngine;

public class ToggleVisibility : MonoBehaviour
{
    public GameObject labels;

    public void Toggle()
    {
        if (labels != null)
        {
            labels.SetActive(!labels.activeSelf);
        }
    }
}