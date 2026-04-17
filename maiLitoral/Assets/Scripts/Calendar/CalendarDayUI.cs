using UnityEngine;
using UnityEngine.UI;
using TMPro;

// This script controls a single calendar day UI.
// It sets the day number, status color, handles click events and turns the highlight on/off when the day is selected.
public class CalendarDayUI : MonoBehaviour
{
    [Header("Referinte UI")]
    [SerializeField] private TMP_Text dayNumberText;
    [SerializeField] private Image statusImage;
    [SerializeField] private GameObject highlightObject;
    [SerializeField] private Button dayButton;

    [Header("Datele zilei")]
    [SerializeField] private int dayNumber;
    [SerializeField] private string status;
    [SerializeField] private string reason;

    private BeachCalendarManager calendarManager;

    public int DayNumber => dayNumber;
    public string Status => status;
    public string Reason => reason;

    public void InitializeDay(
        int newDayNumber,
        string newStatus,
        string newReason,
        Color newStatusColor,
        BeachCalendarManager manager)
    {
        dayNumber = newDayNumber;
        status = newStatus;
        reason = newReason;
        calendarManager = manager;

        if (dayNumberText != null) dayNumberText.text = dayNumber.ToString();
        if (statusImage != null) statusImage.color = newStatusColor;

        SetHighlight(false);

        if (dayButton != null)
        {
            dayButton.onClick.RemoveAllListeners();
            dayButton.onClick.AddListener(OnDayClicked);
        }
    }

    private void OnDayClicked()
    {
        if (calendarManager != null)
            calendarManager.SelectDay(this);
        else
            Debug.LogWarning("Calendar manager nu este asignat pentru ziua " + dayNumber);
    }

    public void SetHighlight(bool isActive)
    {
        if (highlightObject != null)
            highlightObject.SetActive(isActive);
    }
}