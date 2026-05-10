using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using System.Reflection;

public class BeachManagerTest {
    private DatabaseManager databaseManager; // Stores the test database manager.

    [SetUp]
    public void SetUp() { // Creates a database manager before each test.
        DestroyExistingDatabaseManagers();
        DatabaseManager.Instance = null;

        GameObject databaseObject = new GameObject("DatabaseManager_TestObject");
        databaseManager = databaseObject.AddComponent<DatabaseManager>(); // Awake runs here and initializes the database.
    }

    [TearDown]
    public void TearDown() { // Cleans database manager objects after each test.
        DestroyExistingDatabaseManagers();
        DatabaseManager.Instance = null;
    }

    private void DestroyExistingDatabaseManagers() { // Removes old database managers from previous tests.
        DatabaseManager[] managers = UnityEngine.Object.FindObjectsByType<DatabaseManager>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (DatabaseManager manager in managers) {
            if (manager != null) {
                UnityEngine.Object.DestroyImmediate(manager.gameObject);
            }
        }
    }

    private Scene GetOrCreateScene(string sceneName) { // Gets or creates a scene used for safe testing.
        Scene scene = SceneManager.GetSceneByName(sceneName);

        if (!scene.IsValid() || !scene.isLoaded) {
            scene = SceneManager.CreateScene(sceneName);
        }

        return scene;
    }

    private void SetActiveScene(string sceneName) { // Sets the active scene before creating test objects.
        Scene scene = GetOrCreateScene(sceneName);
        SceneManager.SetActiveScene(scene);
    }

    private void SetPrivateField(object target, string fieldName, object value) { // Sets a private field using reflection.
        target.GetType()
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(target, value);
    }

    private T GetPrivateField<T>(object target, string fieldName) { // Reads a private field using reflection.
        return (T)target.GetType()
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(target);
    }

    private void InvokePrivateMethod(object target, string methodName, params object[] parameters) { // Calls private methods and unwraps reflection exceptions.
        try {
            target.GetType()
                .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(target, parameters);
        } catch (TargetInvocationException exception) {
            throw exception.InnerException;
        }
    }

    private void SetCurrentPressedZone(int zoneId) { // Sets the selected zone used by BeachManager.
        typeof(ZonesManager)
            .GetField("currentPressedZone", BindingFlags.NonPublic | BindingFlags.Static)
            .SetValue(null, zoneId);
    }

    private void SetCurrentPressedBeach(int beachIndex) { // Sets the selected beach used by BeachManager.
        typeof(BeachManager)
            .GetField("currentPressedBeach", BindingFlags.NonPublic | BindingFlags.Static)
            .SetValue(null, beachIndex);
    }

    private BeachManager CreateManager() { // Creates a BeachManager with controlled test references.
        GameObject managerObject = new GameObject("BeachManager_TestObject");
        BeachManager manager = managerObject.AddComponent<BeachManager>();

        SetPrivateField(manager, "beachCalendar", new GameObject("BeachCalendar")); // Calendar panel reference.
        SetPrivateField(manager, "calendarManager", new GameObject("CalendarManager")); // Calendar manager reference.
        SetPrivateField(manager, "beachesContent", new GameObject("BeachesContent")); // Parent for loaded beaches.
        SetPrivateField(manager, "propertiesContent", new GameObject("PropertiesContent")); // Parent for loaded properties.
        SetPrivateField(manager, "beachPrefab", CreateBeachPrefab()); // Valid beach prefab.
        SetPrivateField(manager, "propertyPrefab", CreatePropertyPrefab()); // Valid property prefab.
        SetPrivateField(manager, "reviewCalendarPrefab", CreateReviewCalendarPrefab()); // Valid review/calendar prefab.
        SetPrivateField(manager, "propertiesText", CreateTextObject("PropertiesText")); // Text used for title.

        List<GameObject> scrollViews = new List<GameObject>() {
            CreateScrollView("BeachListScrollView"),
            CreateScrollView("PropertiesScrollView")
        };

        SetPrivateField(manager, "scrollViews", scrollViews); // Scroll views used by SelectBeach and ReviewButton.

        return manager;
    }

    private TextMeshProUGUI CreateTextObject(string name) { // Creates a TextMeshProUGUI object.
        GameObject textObject = new GameObject(name);
        return textObject.AddComponent<TextMeshProUGUI>();
    }

    private GameObject CreateBeachPrefab() { // Creates a valid beach prefab.
        GameObject prefab = new GameObject("BeachPrefab");

        prefab.AddComponent<Button>(); // BeachManager expects a Button on the beach object.

        GameObject nameChild = new GameObject("BeachNameText");
        nameChild.transform.SetParent(prefab.transform);
        nameChild.AddComponent<TextMeshProUGUI>(); // Child 0 stores the beach name text.

        GameObject rankChild = new GameObject("BeachRankImage");
        rankChild.transform.SetParent(prefab.transform);
        rankChild.AddComponent<Image>(); // Child 1 stores the rank color image.

        return prefab;
    }

    private GameObject CreatePropertyPrefab() { // Creates a valid property prefab.
        GameObject prefab = new GameObject("PropertyPrefab");

        GameObject textChild = new GameObject("PropertyText");
        textChild.transform.SetParent(prefab.transform);
        textChild.AddComponent<TextMeshProUGUI>(); // Child 0 stores the property name.

        GameObject trueImage = new GameObject("TrueImage");
        trueImage.transform.SetParent(prefab.transform);
        trueImage.AddComponent<Image>(); // Child 1 stores the true/green status.

        GameObject falseImage = new GameObject("FalseImage");
        falseImage.transform.SetParent(prefab.transform);
        falseImage.AddComponent<Image>(); // Child 2 stores the false/red status.

        return prefab;
    }

    private GameObject CreateReviewCalendarPrefab() { // Creates a valid review/calendar prefab.
        GameObject prefab = new GameObject("ReviewCalendarPrefab");

        GameObject calendarButton = new GameObject("CalendarButton");
        calendarButton.transform.SetParent(prefab.transform);
        calendarButton.AddComponent<Button>(); // Child 0 opens the calendar.

        GameObject reviewButton = new GameObject("ReviewButton");
        reviewButton.transform.SetParent(prefab.transform);
        reviewButton.AddComponent<Button>(); // Child 1 enters review mode.

        return prefab;
    }

    private GameObject CreateScrollView(string name) { // Creates a scroll view object with a scrollbar child.
        GameObject scrollView = new GameObject(name);

        GameObject unusedChild = new GameObject("UnusedChild");
        unusedChild.transform.SetParent(scrollView.transform);

        GameObject scrollbarChild = new GameObject("ScrollbarChild");
        scrollbarChild.transform.SetParent(scrollView.transform);
        scrollbarChild.AddComponent<Scrollbar>(); // ReviewButton expects child index 1 to have a Scrollbar.

        return scrollView;
    }

    private GameObject CreateTopBeachesObject() { // Creates the TopBeaches object used on the StartingPage.
        GameObject topBeaches = new GameObject("TopBeaches");

        for (int i = 0; i < 3; i++) {
            GameObject topSlot = new GameObject("TopBeach_" + i);
            topSlot.transform.SetParent(topBeaches.transform);

            GameObject nameText = new GameObject("NameText");
            nameText.transform.SetParent(topSlot.transform);
            nameText.AddComponent<TextMeshProUGUI>(); // Child 0 stores the top beach name.

            GameObject rankImage = new GameObject("RankImage");
            rankImage.transform.SetParent(topSlot.transform);
            rankImage.AddComponent<Image>(); // Child 1 stores the rank color.
        }

        return topBeaches;
    }

    private GameObject CreateBeachWithProperties(string date) { // Creates a beach object with test properties.
        GameObject beachObject = new GameObject("TestBeachObject");
        Beach beach = beachObject.AddComponent<Beach>();

        beach.LoadPropertyFromDatabase(date, "Clean water", true); // Adds a property without writing to database.
        beach.LoadPropertyFromDatabase(date, "Has lifeguard", false); // Adds a second property without writing to database.

        return beachObject;
    }

    [Test]
    public void LoadBeachesFromDatabaseStartingPageUpdatesTopBeaches() { // Tests that StartingPage top beach UI is updated from database rankings.
        SetActiveScene("StartingPage");

        BeachManager manager = CreateManager();

        GameObject topBeaches = CreateTopBeachesObject();
        string today = DateTime.Now.ToString("dd-MM-yyyy");
        List<(string name, float rank)> topRanks = databaseManager.GetTop3BeachesByRank(today); // Expected data comes from database.

        InvokePrivateMethod(manager, "LoadBeachesFromDatabase");

        for (int i = 0; i < topRanks.Count && i < 3; i++) {
            TextMeshProUGUI nameText = topBeaches.transform.GetChild(i).GetChild(0).GetComponent<TextMeshProUGUI>();

            Assert.AreEqual(topRanks[i].name, nameText.text); // UI text should match database top beach name.
        }
    }

    [Test]
    public void LoadBeachesFromDatabaseBeachPageCreatesBeachButtons() { // Tests that BeachPage creates beach buttons from database beaches.
        SetActiveScene("BeachPage");

        BeachManager manager = CreateManager();

        List<ZoneData> zones = databaseManager.GetAllZones();
        int selectedZoneId = zones[0].Id;

        SetCurrentPressedZone(selectedZoneId); // BeachManager reads the selected zone from ZonesManager.

        InvokePrivateMethod(manager, "LoadBeachesFromDatabase");

        List<BeachData> expectedBeaches = databaseManager.GetBeachesByZone(selectedZoneId);
        List<GameObject> loadedBeaches = GetPrivateField<List<GameObject>>(manager, "beaches");

        Assert.AreEqual(expectedBeaches.Count, loadedBeaches.Count); // Loaded beach count should match database count.

        if (expectedBeaches.Count > 0) {
            GameObject firstBeach = loadedBeaches[0];

            Assert.AreEqual(expectedBeaches[0].Name, firstBeach.name); // Beach object name should match database name.
            Assert.AreEqual(expectedBeaches[0].Name, firstBeach.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text); // Visible name should match database name.
            Assert.IsNotNull(firstBeach.GetComponent<Button>()); // Beach object should have a Button.
            Assert.IsNotNull(firstBeach.GetComponent<Beach>()); // Beach object should receive a Beach component.
        }
    }

    [Test]
    public void LoadBeachPropertiesCreatesPropertiesReviewCalendar() { // Tests that properties and review calendar are created for a selected beach.
        SetActiveScene("BeachPage");

        BeachManager manager = CreateManager();

        DateTime currentDate = DateTime.Now.Date;
        string date = currentDate.ToString("dd-MM-yyyy");

        GameObject beachObject = CreateBeachWithProperties(date);
        List<GameObject> beaches = new List<GameObject>() { beachObject };

        SetPrivateField(manager, "beaches", beaches); // Injects a controlled beach list.

        manager.LoadBeachProperties(currentDate, 0);

        GameObject propertiesContent = GetPrivateField<GameObject>(manager, "propertiesContent");
        TextMeshProUGUI propertiesText = GetPrivateField<TextMeshProUGUI>(manager, "propertiesText");

        Assert.AreEqual(3, propertiesContent.transform.childCount); // Two properties plus one review/calendar object.

        GameObject firstProperty = propertiesContent.transform.GetChild(0).gameObject;

        Assert.AreEqual("Clean water", firstProperty.name); // Property object should use the property description as name.
        Assert.AreEqual("Clean water", firstProperty.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text); // Visible text should match property name.
        Assert.AreEqual("Facilities from " + date, propertiesText.text); // Title should include the selected date.
    }

    [Test]
    public void LoadBeachPropertiesUnavailableDateException() { // Tests that loading properties for a missing date causes an exception.
        SetActiveScene("BeachPage");

        BeachManager manager = CreateManager();

        GameObject beachObject = CreateBeachWithProperties(DateTime.Now.ToString("dd-MM-yyyy"));
        List<GameObject> beaches = new List<GameObject>() { beachObject };

        SetPrivateField(manager, "beaches", beaches); // Injects a beach that does not contain the requested date.

        Assert.Throws<KeyNotFoundException>(() => {
            manager.LoadBeachProperties(new DateTime(2099, 9, 9), 0); // Missing date causes dictionary lookup failure.
        });
    }

    [Test]
    public void LoadBeachPropertiesNotBeachPageDoesNothing() { // Tests that properties are not loaded outside BeachPage.
        SetActiveScene("StartingPage");

        BeachManager manager = CreateManager();

        DateTime currentDate = DateTime.Now.Date;
        string date = currentDate.ToString("dd-MM-yyyy");

        GameObject beachObject = CreateBeachWithProperties(date);
        List<GameObject> beaches = new List<GameObject>() { beachObject };

        SetPrivateField(manager, "beaches", beaches);

        manager.LoadBeachProperties(currentDate, 0);

        GameObject propertiesContent = GetPrivateField<GameObject>(manager, "propertiesContent");

        Assert.AreEqual(0, propertiesContent.transform.childCount); // Nothing should be created outside BeachPage.
    }

    [Test]
    public void BackToBeachesDisablesReviewMode() { // Tests that going back disables review mode.
        BeachManager manager = CreateManager();

        SetPrivateField(manager, "reviewMode", true); // Simulates being in review mode.

        manager.BackToBeaches();

        Assert.IsFalse(GetPrivateField<bool>(manager, "reviewMode")); // Review mode should be disabled.
    }

    [Test]
    public void GetCurrentPressedBeachReturnsSelectedIndex() { // Tests that the current pressed beach getter returns the stored index.
        SetCurrentPressedBeach(2);

        Assert.AreEqual(2, BeachManager.GetCurrentPressedBeach());
    }
}