using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Globalization;

// Controls the calendar, day selection, info panel and month navigation.
public class BeachCalendarManager : MonoBehaviour
{
    [Header("All Days")]
    public List<CalendarDayUI> allDays = new List<CalendarDayUI>();

    [Header("Navigation Buttons")]
    public Button leftButton;
    public Button rightButton;
    public TMP_Text monthTitleText;

    private CalendarDayUI currentlySelectedDay;
    private DateTime currentMonth;

    void Start()
    {
        DateTime today = DateTime.Today;
        currentMonth = new DateTime(today.Year, today.Month, 1);

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
                day.SetEmpty();

        if (monthTitleText != null)
        {
            CultureInfo culture = new CultureInfo("ro-RO");
            string text = currentMonth.ToString("MMMM yyyy", culture);
            monthTitleText.text = char.ToUpper(text[0]) + text.Substring(1);
        }

        DateTime firstDay = new DateTime(currentMonth.Year, currentMonth.Month, 1);
        int daysInMonth = DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month);
        int startIndex = GetMondayIndex(firstDay.DayOfWeek);

        for (int d = 1; d <= daysInMonth; d++)
        {
            int index = startIndex + (d - 1);
            if (index < 0 || index >= allDays.Count) continue;

            string status = GetStatusForDay(d);
            string reason = GetReasonForStatus(status);
            Color color = GetColorForStatus(status);

            allDays[index].InitializeDay(d, status, reason, color, this);
        }
    }

    int GetMondayIndex(DayOfWeek day)
    {
        return day == DayOfWeek.Sunday ? 6 : (int)day - 1;
    }

    void GoToPreviousMonth()
    {
        DateTime limit = DateTime.Today.AddMonths(-1);

        DateTime prevMonth = currentMonth.AddMonths(-1);

        if (prevMonth < new DateTime(limit.Year, limit.Month, 1))
            return;

        currentMonth = prevMonth;
        ClearSelection();
        GenerateCalendar();
        UpdateNavigationButtons();
    }

    void GoToNextMonth()
    {
        currentMonth = currentMonth.AddMonths(1);
        ClearSelection();
        GenerateCalendar();
        UpdateNavigationButtons();
    }

    void UpdateNavigationButtons()
    {
        if (leftButton != null)
            SetButtonAlpha(leftButton, 1f);

        if (rightButton != null)
            SetButtonAlpha(rightButton, 1f);
    }

    void SetButtonAlpha(Button button, float alpha)
    {
        Image img = button.GetComponent<Image>();

        if (img != null)
        {
            Color c = img.color;
            c.a = alpha;
            img.color = c;
        }
    }

    void ClearSelection()
    {
        if (currentlySelectedDay != null)
            currentlySelectedDay.SetHighlight(false);

        currentlySelectedDay = null;
    }

    public void SelectDay(CalendarDayUI day)
    {
        if (currentlySelectedDay != null)
            currentlySelectedDay.SetHighlight(false);

        currentlySelectedDay = day;
        currentlySelectedDay.SetHighlight(true);

        SelectedDayData.dayNumber = day.DayNumber;
        SelectedDayData.status = day.Status;
        SelectedDayData.reason = day.Reason;

        SceneManager.LoadScene("DayDetailsScene");
    }

    string GetStatusForDay(int d)
    {
        if (d % 3 == 1) return "Bun pentru baie";
        if (d % 3 == 2) return "Atentie";
        return "Risc ridicat";
    }

    string GetReasonForStatus(string s)
    {
        switch (s)
        {
            case "Risc ridicat": return "Valuri mari.";
            case "Atentie": return "Conditii instabile.";
            case "Bun pentru baie": return "Apa calma.";
            default: return "-";
        }
    }

    Color GetColorForStatus(string s)
    {
        switch (s)
        {
            case "Risc ridicat": return Color.red;
            case "Atentie": return Color.yellow;
            case "Bun pentru baie": return Color.green;
            default: return Color.gray;
        }
    }
}