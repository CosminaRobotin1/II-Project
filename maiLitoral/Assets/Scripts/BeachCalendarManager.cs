using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
//cod care permite sa apara pop ul cu status+motiv cand dau click pe o zi + butoanele left,right devin
//pale cand trecem peste limita de 30 de zile
public class BeachCalendarManager : MonoBehaviour
{
    [Header("All Days")]
    public List<CalendarDayUI> allDays = new List<CalendarDayUI>();

    [Header("Top Panel")]
    public GameObject infoPanel;

    [Header("Top Panel Texts")]
    public TMP_Text selectedDateText;
    public TMP_Text selectedStatusText;
    public TMP_Text selectedReasonText;

    [Header("Navigation Buttons")]
    public Button leftButton;
    public Button rightButton;
    public TMP_Text monthTitleText;

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
        foreach (var day in allDays)
        {
            if (day != null)
                day.gameObject.SetActive(false);
        }

        if (monthTitleText != null)
            monthTitleText.text = currentMonth.ToString("MMMM yyyy");

        DateTime firstDay = new DateTime(currentMonth.Year, currentMonth.Month, 1);
        int daysInMonth = DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month);

        int startIndex = GetMondayIndex(firstDay.DayOfWeek);

        for (int d = 1; d <= daysInMonth; d++)
        {
            int index = startIndex + (d - 1);
            if (index < 0 || index >= allDays.Count) continue;

            DateTime date = new DateTime(currentMonth.Year, currentMonth.Month, d);

            if (date < minAllowedDate || date > maxAllowedDate)
                continue;

            string status = GetStatusForDay(d);
            string reason = GetReasonForStatus(status);
            Color color = GetColorForStatus(status);

            allDays[index].gameObject.SetActive(true);
            allDays[index].Setup(d, status, reason, color, this);
        }
    }

    int GetMondayIndex(DayOfWeek day)
    {
        return day == DayOfWeek.Sunday ? 6 : ((int)day - 1);
    }

    void GoToPreviousMonth()
    {
        DateTime prev = currentMonth.AddMonths(-1);
        DateTime lastDayPrev = new DateTime(prev.Year, prev.Month, DateTime.DaysInMonth(prev.Year, prev.Month));

        if (lastDayPrev < minAllowedDate)
            return;

        currentMonth = prev;
        ClearSelection();
        GenerateCalendar();
        UpdateNavigationButtons();
    }

    void GoToNextMonth()
    {
        DateTime next = currentMonth.AddMonths(1);
        DateTime firstDayNext = new DateTime(next.Year, next.Month, 1);

        if (firstDayNext > maxAllowedDate)
            return;

        currentMonth = next;
        ClearSelection();
        GenerateCalendar();
        UpdateNavigationButtons();
    }

    void UpdateNavigationButtons()
    {
        // LEFT BUTTON
        if (leftButton != null)
        {
            DateTime prev = currentMonth.AddMonths(-1);
            DateTime lastDayPrev = new DateTime(prev.Year, prev.Month, DateTime.DaysInMonth(prev.Year, prev.Month));

            bool canGoLeft = lastDayPrev >= minAllowedDate;

            leftButton.interactable = canGoLeft;

            // 👇 AICI facem sa para mai pal
            SetButtonAlpha(leftButton, canGoLeft ? 1f : 0.3f);
        }

        // RIGHT BUTTON
        if (rightButton != null)
        {
            DateTime next = currentMonth.AddMonths(1);
            DateTime firstDayNext = new DateTime(next.Year, next.Month, 1);

            bool canGoRight = firstDayNext <= maxAllowedDate;

            rightButton.interactable = canGoRight;

            // 👇 AICI facem sa para mai pal
            SetButtonAlpha(rightButton, canGoRight ? 1f : 0.3f);
        }
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
            currentlySelectedDay.SetSelected(false);

        currentlySelectedDay = null;
        ResetTopPanel();
    }

    public void SelectDay(CalendarDayUI day)
    {
        if (currentlySelectedDay != null)
            currentlySelectedDay.SetSelected(false);

        currentlySelectedDay = day;
        currentlySelectedDay.SetSelected(true);

        infoPanel.SetActive(true);

        selectedDateText.text = "Ziua selectata: " + day.dayNumber;
        selectedStatusText.text = "Status: " + day.status;
        selectedReasonText.text = "Motiv: " + day.reason;
    }

    void ResetTopPanel()
    {
        infoPanel.SetActive(false);

        selectedDateText.text = "Ziua selectata:";
        selectedStatusText.text = "Status:";
        selectedReasonText.text = "Motiv:";
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