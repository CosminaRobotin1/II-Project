using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CalendarDayUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text dayText;
    public Image statusImage;
    public GameObject highlightObject;
    public Button button;

    [Header("Data")]
    public int dayNumber;
    public string status;
    public string reason;

    private BeachCalendarManager calendarManager;

    public void Setup(int newDayNumber, string newStatus, string newReason, Color newColor, BeachCalendarManager manager)
    {
        dayNumber = newDayNumber;
        status = newStatus;
        reason = newReason;
        calendarManager = manager;

        dayText.text = dayNumber.ToString();
        statusImage.color = newColor;

        if (highlightObject != null)
            highlightObject.SetActive(false);

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnDayClicked);
    }

    private void OnDayClicked()
    {
        if (calendarManager != null)
        {
            calendarManager.SelectDay(this);
        }
    }

    public void SetSelected(bool isSelected)
    {
        if (highlightObject != null)
            highlightObject.SetActive(isSelected);
    }
}