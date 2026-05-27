using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

public class CalendarManagerTest {
    private void SetPrivateField(object target, string fieldName, object value) { // Sets a private field using reflection
        target.GetType()
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(target, value);
    }
    private T GetPrivateField<T>(object target, string fieldName) { // Reads a private field using reflection
        return (T)target.GetType()
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(target);
    }
    private CalendarManager CreateManager() { // Creates a CalendarManager with controlled test references
        GameObject managerObject = new GameObject("CalendarManager_TestObject");
        CalendarManager manager = managerObject.AddComponent<CalendarManager>();
        SetPrivateField(manager, "beachManager", new GameObject("BeachManager")); // Reference used when selecting a day
        SetPrivateField(manager, "beachCalendar", new GameObject("BeachCalendar")); // Calendar panel toggled when selecting a day
        SetPrivateField(manager, "dayPrefab", CreateDayPrefab()); // Valid day prefab
        SetPrivateField(manager, "currentDateText", CreateTextObject("CurrentDateText")); // Month/year title text
        SetPrivateField(manager, "currentBeachText", CreateTextObject("CurrentBeachText")); // Current beach title text
        SetPrivateField(manager, "weeks", CreateWeeks()); // Five week containers used by the calendar
        return manager;
    }
    private TextMeshProUGUI CreateTextObject(string name) { // Creates a TextMeshProUGUI object
        GameObject textObject = new GameObject(name);
        return textObject.AddComponent<TextMeshProUGUI>();
    }
    private List<GameObject> CreateWeeks() { // Creates the five week containers required by the calendar
        List<GameObject> weeks = new List<GameObject>();
        for (int i = 0; i < 5; i++) {
            weeks.Add(new GameObject("Week_" + i));
        }
        return weeks;
    }
    private GameObject CreateDayPrefab() { // Creates a valid day prefab
        GameObject prefab = new GameObject("DayPrefab");
        prefab.AddComponent<Button>(); // CalendarManager expects each day to start with a Button
        GameObject dayNumberText = new GameObject("DayNumberText");
        dayNumberText.transform.SetParent(prefab.transform);
        dayNumberText.AddComponent<TextMeshProUGUI>(); // Child 0 stores the visible day number
        GameObject statusImage = new GameObject("StatusImage");
        statusImage.transform.SetParent(prefab.transform);
        statusImage.AddComponent<Image>(); // Child 1 stores the rank/status color
        GameObject unavailableMarker = new GameObject("UnavailableMarker");
        unavailableMarker.transform.SetParent(prefab.transform);
        unavailableMarker.SetActive(false); // Child 2 is activated for unavailable days
        return prefab;
    }
    private GameObject CreateBeachObject(string beachName) { // Creates a beach object with a Beach component
        GameObject beachObject = new GameObject(beachName);
        beachObject.AddComponent<Beach>();
        return beachObject;
    }
    private GameObject CreateBeachWithData(string beachName, string date, float rank) { // Creates a beach object with calendar data
        GameObject beachObject = CreateBeachObject(beachName);
        Beach beach = beachObject.GetComponent<Beach>();
        beach.LoadPropertiesFromDatabase(date, new List<string> {"Clean water", "Apa curata"}, "bool", "true"); // Adds data without writing to the database
        beach.LoadRankFromDatabase(date, rank); // Adds rank without writing to the database
        return beachObject;
    }
    private int CountCalendarDays(List<GameObject> weeks) { // Counts all day objects created inside all week containers
        int total = 0;
        foreach (GameObject week in weeks) {
            total += week.transform.childCount;
        }
        return total;
    }
    private GameObject FindDay(List<GameObject> weeks, string dayName) { // Finds a generated day object by name
        foreach (GameObject week in weeks) {
            foreach (Transform day in week.transform) {
                if (day.name == dayName) {
                    return day.gameObject;
                }
            }
        }
        return null;
    }
    private string GetExpectedMonthTitle(DateTime date) { // Creates the expected month title using the same language rule as CalendarManager
        CultureInfo culture;
        if (SettingsManager.Instance == null || SettingsManager.Instance.GetSelectedLanguageIndex() == 0) {
            culture = new CultureInfo("ro-RO");
        } else {
            culture = new CultureInfo("en-US");
        }
        string text = date.ToString("MMMM yyyy", culture);
        return char.ToUpper(text[0]) + text.Substring(1);
    }
    [Test]
    public void LoadCalendarSetsBeachNameCurrentDateText() { // Tests that calendar title and beach title are updated
        CalendarManager manager = CreateManager();
        DateTime date = new DateTime(2025, 5, 15);
        GameObject beachObject = CreateBeachObject("Mamaia Beach");
        manager.LoadCalendar(date, beachObject);
        TextMeshProUGUI currentDateText = GetPrivateField<TextMeshProUGUI>(manager, "currentDateText");
        TextMeshProUGUI currentBeachText = GetPrivateField<TextMeshProUGUI>(manager, "currentBeachText");
        Assert.AreEqual(GetExpectedMonthTitle(date), currentDateText.text); // Month title should match selected language
        Assert.AreEqual("Mamaia Beach", currentBeachText.text); // Beach title should match selected beach name
    }
    [Test]
    public void LoadCalendarCreatesThirtyFiveDayObjects() { // Tests that a valid month creates exactly 35 calendar day objects
        CalendarManager manager = CreateManager();
        DateTime date = new DateTime(2025, 5, 15);
        GameObject beachObject = CreateBeachObject("Mamaia Beach");
        manager.LoadCalendar(date, beachObject);
        List<GameObject> weeks = GetPrivateField<List<GameObject>>(manager, "weeks");
        Assert.AreEqual(35, CountCalendarDays(weeks)); // Calendar has five weeks with seven days each
    }
    [Test]
    public void LoadCalendarMarksLastMonthFutureDaysUnavailable() { // Tests that unavailable days are marked correctly
        CalendarManager manager = CreateManager();
        DateTime date = new DateTime(2025, 5, 15);
        GameObject beachObject = CreateBeachObject("Mamaia Beach");
        manager.LoadCalendar(date, beachObject);
        List<GameObject> weeks = GetPrivateField<List<GameObject>>(manager, "weeks");
        GameObject lastMonthDay = FindDay(weeks, "Day_28_Last"); // May 2025 calendar starts with April 28
        GameObject futureDay = FindDay(weeks, "Day_16"); // Day 16 is after the selected day 15
        Assert.IsNotNull(lastMonthDay);
        Assert.IsNotNull(futureDay);
        Assert.IsTrue(lastMonthDay.transform.GetChild(2).gameObject.activeSelf); // Previous month days should be unavailable
        Assert.IsTrue(futureDay.transform.GetChild(2).gameObject.activeSelf); // Future days should be unavailable
    }
    [Test]
    public void LoadCalendarUsesBeachRankColorForDayWithData() { // Tests that a day with beach data receives the correct rank color
        CalendarManager manager = CreateManager();
        DateTime date = new DateTime(2025, 5, 15);
        GameObject beachObject = CreateBeachWithData("Mamaia Beach", "05-05-2025", 4f);
        manager.LoadCalendar(date, beachObject);
        List<GameObject> weeks = GetPrivateField<List<GameObject>>(manager, "weeks");
        GameObject day = FindDay(weeks, "Day_5");
        Assert.IsNotNull(day);
        Image statusImage = day.transform.GetChild(1).GetComponent<Image>();
        Assert.AreEqual(Color.green, statusImage.color); // Rank 4 should map to green
    }
    [Test]
    public void LoadCalendarDayWithoutDataKeepsGrayStatus() { // Tests that days without beach data are shown with gray status
        CalendarManager manager = CreateManager();
        DateTime date = new DateTime(2025, 5, 15);
        GameObject beachObject = CreateBeachObject("Mamaia Beach");
        manager.LoadCalendar(date, beachObject);
        List<GameObject> weeks = GetPrivateField<List<GameObject>>(manager, "weeks");
        GameObject day = FindDay(weeks, "Day_5");
        Assert.IsNotNull(day);
        Image statusImage = day.transform.GetChild(1).GetComponent<Image>();
        Assert.AreEqual(Color.gray, statusImage.color); // Days without data should be gray
        Assert.IsNotNull(day.GetComponent<Button>()); // Current behavior leaves the button on days without data
    }
    [Test]
    public void LoadCalendarWithNullBeach() { // Tests that a missing current beach causes an exception
        CalendarManager manager = CreateManager();
        DateTime date = new DateTime(2025, 5, 15);
        Assert.Throws<NullReferenceException>(() => {
            manager.LoadCalendar(date, null); // currentBeach.name is accessed without a null check
        });
    }
    [Test]
    public void LoadCalendarWithMonthNeedingSixWeeks() { // Tests that months needing six weeks break the current five-week setup
        CalendarManager manager = CreateManager();
        DateTime date = new DateTime(2025, 3, 15); // March 2025 needs six calendar rows
        GameObject beachObject = CreateBeachObject("Mamaia Beach");
        Assert.Throws<ArgumentOutOfRangeException>(() => {
            manager.LoadCalendar(date, beachObject); // weeks[5] is accessed, but only five weeks exist
        });
    }
}