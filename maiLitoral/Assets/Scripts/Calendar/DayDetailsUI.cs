using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DayDetailsUI : MonoBehaviour
{
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text reasonText;
    [SerializeField] private Button backButton;

    void Start()
    {
        if (dayText != null)
            dayText.text = "Ziua selectata: " + SelectedDayData.dayNumber;

        if (statusText != null)
            statusText.text = "Status: " + SelectedDayData.status;

        if (reasonText != null)
            reasonText.text = "Motiv: " + SelectedDayData.reason;

        if (backButton != null)
            backButton.onClick.AddListener(GoBack);
    }

    void GoBack()
    {
        SceneManager.LoadScene("FirstPage");
    }
}