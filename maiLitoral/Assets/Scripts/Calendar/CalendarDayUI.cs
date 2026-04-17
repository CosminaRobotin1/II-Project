using UnityEngine;
using UnityEngine.UI;
using TMPro;

// This script controls a single calendar day UI.
// It sets the day number, status color, handles click events and turns the highlight on/off when the day is selected.
public class CalendarDayUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text dayNumberText;
    [SerializeField] private Image statusImage;
    [SerializeField] private GameObject highlightObject;
    [SerializeField] private Button dayButton;

    [Header("Day Data")]
    private int dayNumber;
    private string status;
    private string reason;

    private BeachCalendarManager calendarManager;

    public int DayNumber => dayNumber;
    public string Status => status;
    public string Reason => reason;

    public void InitializeDay(int newDayNumber, string newStatus, string newReason, Color newColor, BeachCalendarManager manager)
    {
        dayNumber = newDayNumber;
        status = newStatus;
        reason = newReason;
        calendarManager = manager;

        if (dayNumberText != null) dayNumberText.text = dayNumber.ToString();
        if (statusImage != null) statusImage.color = newColor;

        SetHighlight(false);

        if (dayButton != null)
        {
            dayButton.interactable = true;
            dayButton.onClick.RemoveAllListeners();
            dayButton.onClick.AddListener(OnDayClicked);
        }
    }

    public void SetEmpty()
    {
        dayNumber = 0;
        status = "";
        reason = "";

        if (dayNumberText != null) dayNumberText.text = "";
        if (statusImage != null) statusImage.color = new Color(1f, 1f, 1f, 0f);

        SetHighlight(false);

        if (dayButton != null)
            dayButton.interactable = false;
    }

    private void OnDayClicked()
    {
        if (calendarManager != null)
            calendarManager.SelectDay(this);
    }

    public void SetHighlight(bool isActive)
    {
        if (highlightObject != null)
            highlightObject.SetActive(isActive);
    }
}