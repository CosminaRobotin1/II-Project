using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Controls the calendar, day selection, info panel and month navigation.
public class BeachCalendarManager : MonoBehaviour
{
    [Header("All days from the calendar grid")]
    [SerializeField] private List<CalendarDayUI> allDays = new List<CalendarDayUI>();

    [Header("Info panel")]
    [SerializeField] private GameObject infoPanel;
    [SerializeField] private TMP_Text selectedDateText;
    [SerializeField] private TMP_Text selectedStatusText;
    [SerializeField] private TMP_Text selectedReasonText;

    [Header("Navigation")]
    [SerializeField] private Button leftButton;
    [SerializeField] private Button rightButton;
    [SerializeField] private TMP_Text monthTitleText;

    private CalendarDayUI currentlySelectedDay;

    private DateTime today;
    private DateTime minAllowedDate;
    private DateTime maxAllowedDate;
    private DateTime currentMonth;

    void Start()
    {
        today = DateTime.Today;
        maxAllowedDate = today;
        minAllowedDate = today.AddDays(-30);
        currentMonth = new DateTime(today.Year, today.Month, 1);

        ResetTopPanel();
        SetupNavigationButtons();
        GenerateCalendar();
        UpdateNavigationButtons();
    }

    void SetupNavigationButtons()
    {
        if (leftButton != null)
        {
            leftButton.onClick.RemoveAllListeners();
            leftButton.onClick.AddListener(GoToPreviousMonth);
        }

        if (rightButton != null)
        {
            rightButton.onClick.RemoveAllListeners();
            rightButton.onClick.AddListener(GoToNextMonth);
        }
    }

    void GenerateCalendar()
    {
        foreach (CalendarDayUI day in allDays)
            if (day != null)
                day.gameObject.SetActive(false);

        if (monthTitleText != null)
            monthTitleText.text = currentMonth.ToString("MMMM yyyy");

        DateTime firstDayOfMonth = new DateTime(currentMonth.Year, currentMonth.Month, 1);
        int daysInMonth = DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month);
        int startIndex = GetMondayBasedIndex(firstDayOfMonth.DayOfWeek);

        for (int dayNumber = 1; dayNumber <= daysInMonth; dayNumber++)
        {
            int index = startIndex + (dayNumber - 1);
            if (index < 0 || index >= allDays.Count)
                continue;

            DateTime fullDate = new DateTime(currentMonth.Year, currentMonth.Month, dayNumber);
            if (fullDate < minAllowedDate || fullDate > maxAllowedDate)
                continue;

            string status = GetStatusForDay(dayNumber);
            string reason = GetReasonForStatus(status);
            Color statusColor = GetColorForStatus(status);

            allDays[index].gameObject.SetActive(true);
            allDays[index].InitializeDay(dayNumber, status, reason, statusColor, this);
        }
    }

    int GetMondayBasedIndex(DayOfWeek dayOfWeek)
    {
        return dayOfWeek == DayOfWeek.Sunday ? 6 : (int)dayOfWeek - 1;
    }

    void GoToPreviousMonth()
    {
        DateTime previousMonth = currentMonth.AddMonths(-1);
        DateTime lastDayOfPreviousMonth = new DateTime(
            previousMonth.Year,
            previousMonth.Month,
            DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month)
        );

        if (lastDayOfPreviousMonth < minAllowedDate)
            return;

        currentMonth = previousMonth;
        ClearSelection();
        GenerateCalendar();
        UpdateNavigationButtons();
    }

    void GoToNextMonth()
    {
        DateTime nextMonth = currentMonth.AddMonths(1);
        DateTime firstDayOfNextMonth = new DateTime(nextMonth.Year, nextMonth.Month, 1);

        if (firstDayOfNextMonth > maxAllowedDate)
            return;

        currentMonth = nextMonth;
        ClearSelection();
        GenerateCalendar();
        UpdateNavigationButtons();
    }

    void UpdateNavigationButtons()
    {
        if (leftButton != null)
        {
            DateTime previousMonth = currentMonth.AddMonths(-1);
            DateTime lastDayOfPreviousMonth = new DateTime(
                previousMonth.Year,
                previousMonth.Month,
                DateTime.DaysInMonth(previousMonth.Year, previousMonth.Month)
            );

            bool canGoLeft = lastDayOfPreviousMonth >= minAllowedDate;
            leftButton.interactable = canGoLeft;
            SetButtonAlpha(leftButton, canGoLeft ? 1f : 0.3f);
        }

        if (rightButton != null)
        {
            DateTime nextMonth = currentMonth.AddMonths(1);
            DateTime firstDayOfNextMonth = new DateTime(nextMonth.Year, nextMonth.Month, 1);

            bool canGoRight = firstDayOfNextMonth <= maxAllowedDate;
            rightButton.interactable = canGoRight;
            SetButtonAlpha(rightButton, canGoRight ? 1f : 0.3f);
        }
    }

    void SetButtonAlpha(Button button, float alpha)
    {
        Image image = button.GetComponent<Image>();
        if (image == null) return;

        Color color = image.color;
        color.a = alpha;
        image.color = color;
    }

    void ClearSelection()
    {
        if (currentlySelectedDay != null)
            currentlySelectedDay.SetHighlight(false);

        currentlySelectedDay = null;
        ResetTopPanel();
    }

    public void SelectDay(CalendarDayUI day)
    {
        if (currentlySelectedDay != null)
            currentlySelectedDay.SetHighlight(false);

        currentlySelectedDay = day;
        currentlySelectedDay.SetHighlight(true);

        if (infoPanel != null)
            infoPanel.SetActive(true);

        if (selectedDateText != null)
            selectedDateText.text = "Ziua selectata: " + day.DayNumber;

        if (selectedStatusText != null)
            selectedStatusText.text = "Status: " + day.Status;

        if (selectedReasonText != null)
            selectedReasonText.text = "Motiv: " + day.Reason;
    }

    void ResetTopPanel()
    {
        if (infoPanel != null)
            infoPanel.SetActive(false);

        if (selectedDateText != null)
            selectedDateText.text = "Ziua selectata:";

        if (selectedStatusText != null)
            selectedStatusText.text = "Status:";

        if (selectedReasonText != null)
            selectedReasonText.text = "Motiv:";
    }

    string GetStatusForDay(int dayNumber)
    {
        if (dayNumber % 3 == 1) return "Bun pentru baie";
        if (dayNumber % 3 == 2) return "Atentie";
        return "Risc ridicat";
    }

    string GetReasonForStatus(string status)
    {
        switch (status)
        {
            case "Risc ridicat": return "Valuri mari.";
            case "Atentie": return "Conditii instabile.";
            case "Bun pentru baie": return "Apa calma.";
            default: return "-";
        }
    }

    Color GetColorForStatus(string status)
    {
        switch (status)
        {
            case "Risc ridicat": return Color.red;
            case "Atentie": return Color.yellow;
            case "Bun pentru baie": return Color.green;
            default: return Color.gray;
        }
    }
}