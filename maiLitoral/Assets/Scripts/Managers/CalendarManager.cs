using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CalendarManager : MonoBehaviour {

    /* Attributes */

    [SerializeField] private GameObject beachManager; // Attribute for beach manager (reference from beach manager)
    [SerializeField] private GameObject beachCalendar; // Attribute for beach calendar
    [SerializeField] private GameObject dayPrefab; // Attribute that represents the standard form of a day
    [SerializeField] private TextMeshProUGUI currentDateText; // Attribute for current date text
    [SerializeField] private TextMeshProUGUI currentBeachText; // Attribute for current beach text
    [SerializeField] private List<GameObject> weeks; // Attribute for list of weeks in calendar
    private GameObject currentBeach; // Attribute for current beach (reference from beach manager) 
    private Color[] statusColors = new Color[] { // Attribute for 5 colors (each for status 0->4)
        Color.red,
        new Color(1f, 0.5f, 0f),
        Color.yellow,
        new Color(0.5f, 1f, 0f),
        Color.green,
    };

    /* Custom methods */

    // Returns the culture that matches the selected language
    private CultureInfo GetCurrentCulture() {
        if (SettingsOptionsManager.Instance == null) {
            return new CultureInfo("ro-RO");
        }
        if (SettingsOptionsManager.Instance.GetSelectedLanguageIndex() == 0) {
            return new CultureInfo("ro-RO");
        }
        return new CultureInfo("en-US");
    }

    public void LoadCalendar(DateTime currentDate, GameObject currentBeach) { // Loading the calendar data
        this.currentBeach = currentBeach;
        for(int i = 0; i < 5; i++) {  // Destroying already shown days (for each week)
            foreach(Transform day in weeks[i].transform) {
                Destroy(day.gameObject);
            }
        }
        currentBeachText.text = currentBeach.name;
        CultureInfo culture = GetCurrentCulture(); // Gets the culture based on the selected language
        string text = currentDate.ToString("MMMM yyyy", culture); // Formats the date using the selected language
        currentDateText.text = char.ToUpper(text[0]) + text.Substring(1); // Capitalizes the first letter

        int totalDaysLastMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.AddMonths(-1).Month); // Last month number of days
        int totalDaysThisMonth = DateTime.DaysInMonth(currentDate.Year, currentDate.Month); // Current month number of days
        DateTime firstDayInMonth = new DateTime(currentDate.Year, currentDate.Month, 1);
        int firstDayInMonthIndex = (firstDayInMonth.DayOfWeek == DayOfWeek.Sunday) ? 6 : (int)firstDayInMonth.DayOfWeek - 1; // First day in month (For example, 1 may = 4 because it's friday)

        for (int i = 0; i < (totalDaysThisMonth + firstDayInMonthIndex); i++) { // Creating first the days in current month, and the days in last month
            GameObject newDay = Instantiate(dayPrefab, weeks[i / 7].transform); // Instantiating a new day
            if (firstDayInMonthIndex > i) { // Adding the days from the last month
                newDay.name = "Day_" + (totalDaysLastMonth - firstDayInMonthIndex + i + 1) + "_Last";
                newDay.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = (totalDaysLastMonth - firstDayInMonthIndex + i + 1).ToString();
                Destroy(newDay.GetComponent<Button>()); // Disabling the buttons on last month days
                newDay.transform.GetChild(2).gameObject.SetActive(true); // Displaying unavailable
                continue;
            }
            newDay.name = "Day_" + (i - firstDayInMonthIndex + 1); // Adding the days from the current month
            newDay.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = (i - firstDayInMonthIndex + 1).ToString();
            if (firstDayInMonthIndex + currentDate.Day <= i) { // Disabling the buttons on future days in current month
                Destroy(newDay.GetComponent<Button>());
                newDay.transform.GetChild(2).gameObject.SetActive(true); // Displaying unavailable
                continue;
            }
            int dayNumber = i - firstDayInMonthIndex + 1;
            DateTime loopDate = new DateTime(currentDate.Year, currentDate.Month, dayNumber); // Getting the relevant date for each day button
            string date = loopDate.ToString("dd-MM-yyyy");

            Beach currentBeachScript = currentBeach.GetComponent<Beach>(); // Getting the beach script from the current beach
            if (currentBeachScript.GetBeachProperties().ContainsKey(date)) {
                newDay.transform.GetChild(1).GetComponent<Image>().color = statusColors[Mathf.FloorToInt(currentBeachScript.GetRank()[date] + 0.5f)]; // Setting each day status
                newDay.GetComponent<Button>().onClick.AddListener(() => SelectDay(loopDate)); // Adding the correspondent listener to day button
            } else {
                newDay.transform.GetChild(1).GetComponent<Image>().color = Color.gray; // Displaying gray if there is no data available for this day
            }
        }

        int daysLeftInCalendar = 35 - (totalDaysThisMonth + firstDayInMonthIndex); // Days left in calendar panel (if there is any)
        for(int i = 0; i < daysLeftInCalendar; i++) { // Adding the days from the next month, if needed
            GameObject newDay = Instantiate(dayPrefab, weeks[4].transform); // Instantiating a new day
            newDay.name = "Day_" + (i + 1) + "_New";
            newDay.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = (i + 1).ToString();
            Destroy(newDay.GetComponent<Button>()); // Disabling the buttons on this days
            newDay.transform.GetChild(2).gameObject.SetActive(true); // Displaying unavailable
        }
    }
    private void SelectDay(DateTime currentDate) { // Open properties for selected day
        beachManager.GetComponent<BeachManager>().LoadBeachProperties(currentDate, BeachManager.GetCurrentPressedBeach()); // Loading properties for selected day
        ButtonsManager.ToggleObject(beachCalendar); // Disabling calendar panel
    }
    public void NextPreviousMonth(int direction) { // Switch calendar month (last or next month)
        if(direction >= 0) { // Next month is only the current month
            LoadCalendar(DateTime.Now, currentBeach);
        } else { // User can see only the last month
            DateTime lastDate = DateTime.Now.AddMonths(direction);
            lastDate = lastDate.AddDays(DateTime.DaysInMonth(lastDate.Year, lastDate.Month) - lastDate.Day);
            LoadCalendar(lastDate, currentBeach);
        }
    }
}
