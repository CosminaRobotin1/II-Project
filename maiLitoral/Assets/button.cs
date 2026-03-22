using UnityEngine;
using TMPro;

public class button : MonoBehaviour
{
    public TextMeshProUGUI statusText;

    public void OnButtonPressed()
    {
        statusText.text = "Button pressed!";
    }

    public void OnButtonReleased()
    {
        statusText.text = "";
    }
}

