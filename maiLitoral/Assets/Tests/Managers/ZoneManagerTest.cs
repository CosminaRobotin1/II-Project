using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Reflection;
using System.Collections.Generic;

public class ZonesManagerTest {
    private DatabaseManager databaseManager; // Stores the test database manager

    [SetUp]
    public void SetUp() { // Creates a database manager before each test
        DestroyExistingDatabaseManagers();
        DatabaseManager.Instance = null;
        GameObject databaseObject = new GameObject("DatabaseManager_TestObject");
        databaseManager = databaseObject.AddComponent<DatabaseManager>(); // Awake runs here and initializes the database
    }
    [TearDown]
    public void TearDown() { // Cleans database manager objects after each test
        DestroyExistingDatabaseManagers();
        DatabaseManager.Instance = null;
    }
    private void DestroyExistingDatabaseManagers() { // Removes old database managers from previous tests
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
    private Scene GetOrCreateScene(string sceneName) { // Gets or creates a scene used for safe testing
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.IsValid() || !scene.isLoaded) {
            scene = SceneManager.CreateScene(sceneName);
        }
        return scene;
    }
    private void SetActiveScene(string sceneName) { // Sets the active scene before creating test objects
        Scene scene = GetOrCreateScene(sceneName);
        SceneManager.SetActiveScene(scene);
    }
    private void SetPrivateField(object target, string fieldName, object value) { // Sets a private field using reflection
        target.GetType()
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(target, value);
    }
    private void InvokeLoadZonesFromDatabase(ZonesManager manager) { // Calls the private LoadZonesFromDatabase method
        typeof(ZonesManager)
            .GetMethod("LoadZonesFromDatabase", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(manager, null);
    }
    private ZonesManager CreateManager(GameObject zonesContent, GameObject zonePrefab) { // Creates a ZonesManager with test references assigned
        SetActiveScene("SafeTestScene"); // Prevents Awake from loading zones too early
        GameObject obj = new GameObject("ZonesManager_TestObject");
        ZonesManager manager = obj.AddComponent<ZonesManager>();
        SetPrivateField(manager, "zonesContent", zonesContent); // Assign private content object
        SetPrivateField(manager, "zonePrefab", zonePrefab); // Assign private prefab object
        return manager;
    }
    private GameObject CreateValidZonePrefab() { // Creates a valid zone prefab with a button and text child
        GameObject prefab = new GameObject("ZonePrefab");
        prefab.AddComponent<Button>(); // Production code expects a Button component
        GameObject textChild = new GameObject("ZoneText");
        textChild.transform.SetParent(prefab.transform);
        textChild.AddComponent<TextMeshProUGUI>(); // Production code expects text on child index 0
        return prefab;
    }
    private GameObject CreatePrefabWithoutButton() { // Creates an invalid prefab without a Button component
        GameObject prefab = new GameObject("ZonePrefab_NoButton");
        GameObject textChild = new GameObject("ZoneText");
        textChild.transform.SetParent(prefab.transform);
        textChild.AddComponent<TextMeshProUGUI>();
        return prefab;
    }
    [Test]
    public void AwakeSceneIsNotStartingPageNotLoadZones() { // Tests that zones are not loaded outside the StartingPage scene
        SetActiveScene("SafeTestScene");
        GameObject obj = new GameObject("ZonesManager_TestObject");
        ZonesManager manager = obj.AddComponent<ZonesManager>();
        Assert.AreEqual(0, manager.GetZones().Count); // Loading should be skipped outside StartingPage
    }
    [Test]
    public void LoadZonesFromDatabaseSceneIsStartingPage() { // Tests that database zones are created in the StartingPage scene
        GameObject zonesContent = new GameObject("ZonesContent");
        GameObject zonePrefab = CreateValidZonePrefab();
        ZonesManager manager = CreateManager(zonesContent, zonePrefab);
        SetActiveScene("StartingPage"); // This scene name allows loading to continue
        InvokeLoadZonesFromDatabase(manager);
        Assert.AreEqual(databaseManager.GetAllZones().Count, manager.GetZones().Count); // Zone count should match database count
    }
    [Test]
    public void LoadZonesFromDatabaseCreateZonesCorrectly() { // Tests that created zones match database names, text, parent, and button
        GameObject zonesContent = new GameObject("ZonesContent");
        GameObject zonePrefab = CreateValidZonePrefab();
        ZonesManager manager = CreateManager(zonesContent, zonePrefab);
        SetActiveScene("StartingPage");
        InvokeLoadZonesFromDatabase(manager);
        List<ZoneData> databaseZones = databaseManager.GetAllZones(); // Expected zone data comes from the database
        for (int i = 0; i < manager.GetZones().Count; i++) {
            GameObject zone = manager.GetZones()[i];
            Assert.AreEqual(databaseZones[i].Name, zone.name); // Zone object name should match database name
            Assert.AreEqual(zonesContent.transform, zone.transform.parent); // Zone should be under content object
            TextMeshProUGUI text = zone.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            Assert.AreEqual(databaseZones[i].Name, text.text); // Visible text should match database name
            Assert.IsNotNull(zone.GetComponent<Button>()); // Zone should have a Button
        }
    }
    [Test]
    public void LoadZonesFromDatabaseCalledTwiceCreateDuplicateZones() { // Tests that calling the loader twice creates duplicate zones
        GameObject zonesContent = new GameObject("ZonesContent");
        GameObject zonePrefab = CreateValidZonePrefab();
        ZonesManager manager = CreateManager(zonesContent, zonePrefab);
        SetActiveScene("StartingPage");
        int databaseZoneCount = databaseManager.GetAllZones().Count; // Used because zones now come from the database
        InvokeLoadZonesFromDatabase(manager);
        InvokeLoadZonesFromDatabase(manager); // Second call creates duplicates
        Assert.AreEqual(
            databaseZoneCount * 2,
            manager.GetZones().Count,
            "BUG: Calling LoadZonesFromDatabase twice creates duplicate zones."
        );
    }
    [Test]
    public void LoadZonesFromDatabaseNullPrefab() { // Tests that a missing prefab causes an exception
        GameObject zonesContent = new GameObject("ZonesContent");
        ZonesManager manager = CreateManager(zonesContent, null);
        SetActiveScene("StartingPage");
        Assert.Throws<TargetInvocationException>(() => {
            InvokeLoadZonesFromDatabase(manager); // Instantiate fails because prefab is null
        });
    }
    [Test]
    public void LoadZonesFromDatabasePrefabMissingButton() { // Tests that a prefab without a Button causes an exception
        GameObject zonesContent = new GameObject("ZonesContent");
        GameObject zonePrefab = CreatePrefabWithoutButton();
        ZonesManager manager = CreateManager(zonesContent, zonePrefab);
        SetActiveScene("StartingPage");
        Assert.Throws<TargetInvocationException>(() => {
            InvokeLoadZonesFromDatabase(manager); // GetComponent<Button>() returns null
        });
    }
    [Test]
    public void GetZonesExposeInternalListReference() { // Tests that GetZones exposes the internal zones list
        GameObject zonesContent = new GameObject("ZonesContent");
        GameObject zonePrefab = CreateValidZonePrefab();
        ZonesManager manager = CreateManager(zonesContent, zonePrefab);
        SetActiveScene("StartingPage");
        InvokeLoadZonesFromDatabase(manager);
        var exposedZonesList = manager.GetZones();
        exposedZonesList.Clear(); // Simulates another script clearing the internal list
        Assert.AreEqual(
            0,
            manager.GetZones().Count,
            "BUG: GetZones exposes the internal list, allowing external scripts to erase zone data."
        );
    }
}