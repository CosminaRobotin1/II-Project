using NUnit.Framework;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.SceneManagement;

public class SearchManagerTest {
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
    private Scene GetOrCreateScene(string sceneName) { // Gets or creates a scene used during testing
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.IsValid() || !scene.isLoaded) {
            scene = SceneManager.CreateScene(sceneName);
        }
        return scene;
    }
    private void SetActiveScene(string sceneName) { // Changes the active scene for safe test setup
        Scene scene = GetOrCreateScene(sceneName);
        SceneManager.SetActiveScene(scene);
    }
    private void SetPrivateField(object target, string fieldName, object value) { // Sets private fields inside the SearchManager
        target.GetType()
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(target, value);
    }
    private void InvokePrivateMethod(object target, string methodName, params object[] parameters) { // Calls private methods and unwraps reflection exceptions
        try {
            target.GetType()
                .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(target, parameters);
        } catch (TargetInvocationException exception) {
            throw exception.InnerException;
        }
    }
    private GameObject CreateZone(string name) { // Creates a test zone GameObject with a given name
        GameObject zone = new GameObject(name);
        zone.SetActive(false); // Start hidden so tests can check if the script makes it visible
        return zone;
    }
    private ZonesManager CreateZonesManagerWithZones(List<GameObject> zones) { // Creates a ZonesManager with a custom zones list
        SetActiveScene("SafeSearchManagerTestScene"); // Prevents StartingPage-only loading during setup
        GameObject zonesManagerObject = new GameObject("ZonesManagerObject");
        zonesManagerObject.SetActive(false); // Prevents unwanted lifecycle behavior
        ZonesManager zonesManager = zonesManagerObject.AddComponent<ZonesManager>();
        typeof(ZonesManager)
            .GetField("zones", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(zonesManager, zones); // Replace private zone list with controlled test data
        return zonesManager;
    }
    private TMP_InputField CreateInputField() { // Creates a test TMP input field
        GameObject inputObject = new GameObject("SearchInputField");
        return inputObject.AddComponent<TMP_InputField>();
    }
    private TMP_Text CreateAutoCompleteText() { // Creates a test TMP autocomplete text object
        GameObject textObject = new GameObject("AutoCompleteText");
        return textObject.AddComponent<TextMeshProUGUI>();
    }
    private SearchManager CreateSearchManager() { // Creates an inactive SearchManager so Start does not run automatically
        GameObject searchManagerObject = new GameObject("SearchManagerObject");
        searchManagerObject.SetActive(false); // Important: prevents Start from running before test fields are assigned
        return searchManagerObject.AddComponent<SearchManager>();
    }
    private SearchManager CreateSearchManagerWithReferences(ZonesManager zonesManager, TMP_InputField inputField, TMP_Text autoCompleteText) { // Creates a SearchManager with all required references assigned
        SearchManager searchManager = CreateSearchManager();
        SetPrivateField(searchManager, "zonesManager", zonesManager.gameObject); // Assign private zones manager reference.
        SetPrivateField(searchManager, "searchInputField", inputField); // Assign private input field reference.
        SetPrivateField(searchManager, "autoCompleteText", autoCompleteText); // Assign private autocomplete text reference.
        return searchManager;
    }
    private SearchManager CreateSearchManagerWithZonesOnly(List<GameObject> zones, TMP_Text autoCompleteText) { // Creates a SearchManager for direct search logic tests
        SearchManager searchManager = CreateSearchManager();
        SetPrivateField(searchManager, "zones", zones); // Inject zones directly to avoid SearchInit dependency.
        SetPrivateField(searchManager, "autoCompleteText", autoCompleteText); // Required by UpdateSuggestion.
        return searchManager;
    }
    [Test]
    public void SearchInitClearAutocompleteShowAllZones() { // Tests that search initialization clears autocomplete and shows all zones
        List<GameObject> zones = new List<GameObject>() {
            CreateZone("Zone_0"),
            CreateZone("Zone_1"),
            CreateZone("Zone_2")
        };
        TMP_Text autoText = CreateAutoCompleteText();
        autoText.text = "Old suggestion"; // Start with old text to verify it gets cleared
        SearchManager searchManager = CreateSearchManager();
        SetPrivateField(searchManager, "zones", zones); // Inject zones directly to avoid ZonesManager dependency
        SetPrivateField(searchManager, "autoCompleteText", autoText); // Required for suggestion clearing
        InvokePrivateMethod(searchManager, "OnSearchValueChanged", ""); // Empty input clears autocomplete and shows all zones
        Assert.AreEqual("", autoText.text); // Autocomplete should be empty
        Assert.IsTrue(zones[0].activeSelf);
        Assert.IsTrue(zones[1].activeSelf);
        Assert.IsTrue(zones[2].activeSelf);
    }
    [Test]
    public void OnSearchValueChangedPartialMatchShowSuggestionFilterZones() { // Tests that partial input shows a suggestion and filters zones
        List<GameObject> zones = new List<GameObject>() {
            CreateZone("Mamaia"),
            CreateZone("VamaVeche"),
            CreateZone("Eforie")
        };
        TMP_Text autoText = CreateAutoCompleteText();
        SearchManager searchManager = CreateSearchManagerWithZonesOnly(zones, autoText);
        InvokePrivateMethod(searchManager, "OnSearchValueChanged", "mam"); // This should only match Mamaia
        Assert.AreEqual("Mamaia", autoText.text);
        Assert.IsTrue(zones[0].activeSelf);
        Assert.IsFalse(zones[1].activeSelf);
        Assert.IsFalse(zones[2].activeSelf);
    }
    [Test]
    public void OnSearchValueChangedExactMatchClearSuggestion() { // Tests that exact matches clear the autocomplete suggestion
        List<GameObject> zones = new List<GameObject>() {
            CreateZone("Mamaia"),
            CreateZone("VamaVeche")
        };
        TMP_Text autoText = CreateAutoCompleteText();
        SearchManager searchManager = CreateSearchManagerWithZonesOnly(zones, autoText);
        InvokePrivateMethod(searchManager, "OnSearchValueChanged", "Mamaia"); // Exact match should not show suggestion text
        Assert.AreEqual("", autoText.text);
        Assert.IsTrue(zones[0].activeSelf);
        Assert.IsFalse(zones[1].activeSelf);
    }
    [Test]
    public void OnSearchValueChangedNoMatchHideAllZonesClearSuggestion() { // Tests that unknown input hides all zones and clears the suggestion
        List<GameObject> zones = new List<GameObject>() {
            CreateZone("Mamaia"),
            CreateZone("VamaVeche"),
            CreateZone("Eforie")
        };
        TMP_Text autoText = CreateAutoCompleteText();
        SearchManager searchManager = CreateSearchManagerWithZonesOnly(zones, autoText);
        InvokePrivateMethod(searchManager, "OnSearchValueChanged", "unknown"); // No zone should match this input
        Assert.AreEqual("", autoText.text);
        Assert.IsFalse(zones[0].activeSelf);
        Assert.IsFalse(zones[1].activeSelf);
        Assert.IsFalse(zones[2].activeSelf);
    }
    [Test]
    public void OnSearchValueChangedEmptyInputShowAllZonesClearSuggestion() { // Tests that empty input resets the search and shows all zones
        List<GameObject> zones = new List<GameObject>() {
            CreateZone("Mamaia"),
            CreateZone("VamaVeche"),
            CreateZone("Eforie")
        };
        TMP_Text autoText = CreateAutoCompleteText();
        SearchManager searchManager = CreateSearchManagerWithZonesOnly(zones, autoText);
        InvokePrivateMethod(searchManager, "OnSearchValueChanged", "mam"); // First filter the list
        InvokePrivateMethod(searchManager, "OnSearchValueChanged", ""); // Then clear the input
        Assert.AreEqual("", autoText.text);
        Assert.IsTrue(zones[0].activeSelf);
        Assert.IsTrue(zones[1].activeSelf);
        Assert.IsTrue(zones[2].activeSelf);
    }
    [Test]
    public void ShowZonesManagerToggleZonesManagerPanel() { // Tests that the zones manager panel can be shown and hidden
        GameObject zonesPanel = new GameObject("ZonesPanel");
        SearchManager searchManager = CreateSearchManager();
        SetPrivateField(searchManager, "zonesManager", zonesPanel); // ShowZonesManager only needs this object
        zonesPanel.SetActive(false);
        searchManager.ShowZonesManager(true);
        Assert.IsTrue(zonesPanel.activeSelf);
        searchManager.ShowZonesManager(false);
        Assert.IsFalse(zonesPanel.activeSelf);
    }
    [Test]
    public void SearchInitWithMissingZonesManagerComponent() { // Tests that a missing ZonesManager component causes an exception
        GameObject fakeZonesManagerObject = new GameObject("FakeZonesManagerObject");
        TMP_InputField input = CreateInputField();
        TMP_Text autoText = CreateAutoCompleteText();
        SearchManager searchManager = CreateSearchManager();
        SetPrivateField(searchManager, "zonesManager", fakeZonesManagerObject); // Object has no ZonesManager component
        SetPrivateField(searchManager, "searchInputField", input);
        SetPrivateField(searchManager, "autoCompleteText", autoText);
        Assert.Throws<System.NullReferenceException>(() => {
            InvokePrivateMethod(searchManager, "SearchInit");
        });
    }
    [Test]
    public void SearchInitWithNullInputField() { // Tests that a missing input field causes an exception
        List<GameObject> zones = new List<GameObject>() {
            CreateZone("Mamaia")
        };
        ZonesManager zonesManager = CreateZonesManagerWithZones(zones);
        TMP_Text autoText = CreateAutoCompleteText();
        SearchManager searchManager = CreateSearchManager();
        SetPrivateField(searchManager, "zonesManager", zonesManager.gameObject);
        SetPrivateField(searchManager, "searchInputField", null); // Missing input field
        SetPrivateField(searchManager, "autoCompleteText", autoText);
        Assert.Throws<System.NullReferenceException>(() => {
            InvokePrivateMethod(searchManager, "SearchInit");
        });
    }
    [Test]
    public void SearchInitWithNullAutoCompleteText() { // Tests that a missing autocomplete text object causes an exception
        List<GameObject> zones = new List<GameObject>() {
            CreateZone("Mamaia")
        };
        ZonesManager zonesManager = CreateZonesManagerWithZones(zones);
        TMP_InputField input = CreateInputField();
        SearchManager searchManager = CreateSearchManager();
        SetPrivateField(searchManager, "zonesManager", zonesManager.gameObject);
        SetPrivateField(searchManager, "searchInputField", input);
        SetPrivateField(searchManager, "autoCompleteText", null); // Missing autocomplete text object
        Assert.Throws<System.NullReferenceException>(() => {
            InvokePrivateMethod(searchManager, "SearchInit");
        });
    }
    [Test]
    public void ShowAllZonesWithNullZoneInList() { // Tests that a null zone in the list causes an exception
        SearchManager searchManager = CreateSearchManager();
        List<GameObject> zones = new List<GameObject>() {
            CreateZone("Mamaia"),
            null
        };
        SetPrivateField(searchManager, "zones", zones); // Inject corrupted list containing a null zone
        Assert.Throws<System.NullReferenceException>(() => {
            InvokePrivateMethod(searchManager, "ShowAllZones");
        });
    }
}