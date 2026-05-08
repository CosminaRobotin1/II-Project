using NUnit.Framework;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.SceneManagement;

public class SearchManagerTest {

    private Scene GetOrCreateScene(string sceneName) { // Gets or creates a scene used during testing.
        Scene scene = SceneManager.GetSceneByName(sceneName);

        if (!scene.IsValid() || !scene.isLoaded) {
            scene = SceneManager.CreateScene(sceneName);
        }

        return scene;
    }

    private void SetActiveScene(string sceneName) { // Changes the active scene for safe test setup.
        Scene scene = GetOrCreateScene(sceneName);
        SceneManager.SetActiveScene(scene);
    }

    private void SetPrivateField(object target, string fieldName, object value) { // Sets private fields inside the SearchManager.
        target.GetType()
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(target, value);
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

    private GameObject CreateZone(string name) { // Creates a test zone GameObject with a given name.
        GameObject zone = new GameObject(name);
        zone.SetActive(false); // Start hidden so tests can check if the script makes it visible.
        return zone;
    }

    private ZonesManager CreateZonesManagerWithZones(List<GameObject> zones) { // Creates a ZonesManager with a custom zones list.
        SetActiveScene("SafeSearchManagerTestScene"); // Prevents automatic zone loading during setup.

        GameObject zonesManagerObject = new GameObject("ZonesManagerObject");
        ZonesManager zonesManager = zonesManagerObject.AddComponent<ZonesManager>();

        typeof(ZonesManager)
            .GetField("zones", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(zonesManager, zones); // Replace private zone list with controlled test data.

        return zonesManager;
    }

    private TMP_InputField CreateInputField() { // Creates a test TMP input field.
        GameObject inputObject = new GameObject("SearchInputField");
        return inputObject.AddComponent<TMP_InputField>();
    }

    private TMP_Text CreateAutoCompleteText() { // Creates a test TMP autocomplete text object.
        GameObject textObject = new GameObject("AutoCompleteText");
        return textObject.AddComponent<TextMeshProUGUI>();
    }

    private SearchManager CreateSearchManager(ZonesManager zonesManager, TMP_InputField inputField, TMP_Text autoCompleteText) { // Creates a SearchManager with all required references assigned.
        GameObject searchManagerObject = new GameObject("SearchManagerObject");
        SearchManager searchManager = searchManagerObject.AddComponent<SearchManager>();

        SetPrivateField(searchManager, "zonesManager", zonesManager.gameObject); // Assign private zones manager reference.
        SetPrivateField(searchManager, "searchInputField", inputField); // Assign private input field reference.
        SetPrivateField(searchManager, "autoCompleteText", autoCompleteText); // Assign private autocomplete text reference.

        return searchManager;
    }

    [Test]
    public void SearchInitClearAutocompleteAllZones() { // Tests that search initialization clears autocomplete and shows all zones.
        List<GameObject> zones = new List<GameObject>() {
            CreateZone("Zone_0"),
            CreateZone("Zone_1"),
            CreateZone("Zone_2")
        };

        ZonesManager zonesManager = CreateZonesManagerWithZones(zones);
        TMP_InputField input = CreateInputField();
        TMP_Text autoText = CreateAutoCompleteText();

        autoText.text = "Old suggestion"; // Start with old text to verify it gets cleared.

        SearchManager searchManager = CreateSearchManager(zonesManager, input, autoText);

        InvokePrivateMethod(searchManager, "SearchInit");

        Assert.AreEqual("", autoText.text); // Autocomplete should be empty after init.

        Assert.IsTrue(zones[0].activeSelf);
        Assert.IsTrue(zones[1].activeSelf);
        Assert.IsTrue(zones[2].activeSelf);
    }

    [Test]
    public void OnSearchValueChangedPartialMatch() { // Tests that partial input shows a suggestion and filters zones.
        List<GameObject> zones = new List<GameObject>() {
            CreateZone("Mamaia"),
            CreateZone("VamaVeche"),
            CreateZone("Eforie")
        };

        ZonesManager zonesManager = CreateZonesManagerWithZones(zones);
        TMP_InputField input = CreateInputField();
        TMP_Text autoText = CreateAutoCompleteText();

        SearchManager searchManager = CreateSearchManager(zonesManager, input, autoText);

        InvokePrivateMethod(searchManager, "SearchInit");

        input.onValueChanged.Invoke("mam"); // This should only match Mamaia.

        Assert.AreEqual("Mamaia", autoText.text);

        Assert.IsTrue(zones[0].activeSelf);
        Assert.IsFalse(zones[1].activeSelf);
        Assert.IsFalse(zones[2].activeSelf);
    }

    [Test]
    public void OnSearchValueChangedExactMatchClearS() { // Tests that exact matches clear the autocomplete suggestion.
        List<GameObject> zones = new List<GameObject>() {
            CreateZone("Mamaia"),
            CreateZone("VamaVeche")
        };

        ZonesManager zonesManager = CreateZonesManagerWithZones(zones);
        TMP_InputField input = CreateInputField();
        TMP_Text autoText = CreateAutoCompleteText();

        SearchManager searchManager = CreateSearchManager(zonesManager, input, autoText);

        InvokePrivateMethod(searchManager, "SearchInit");

        input.onValueChanged.Invoke("Mamaia"); // Exact match should not show suggestion text.

        Assert.AreEqual("", autoText.text);

        Assert.IsTrue(zones[0].activeSelf);
        Assert.IsFalse(zones[1].activeSelf);
    }

    [Test]
    public void OnSearchValueChangedNoMatchHideZones() { // Tests that unknown input hides all zones and clears the suggestion.
        List<GameObject> zones = new List<GameObject>() {
            CreateZone("Mamaia"),
            CreateZone("VamaVeche"),
            CreateZone("Eforie")
        };

        ZonesManager zonesManager = CreateZonesManagerWithZones(zones);
        TMP_InputField input = CreateInputField();
        TMP_Text autoText = CreateAutoCompleteText();

        SearchManager searchManager = CreateSearchManager(zonesManager, input, autoText);

        InvokePrivateMethod(searchManager, "SearchInit");

        input.onValueChanged.Invoke("unknown"); // No zone should match this input.

        Assert.AreEqual("", autoText.text);

        Assert.IsFalse(zones[0].activeSelf);
        Assert.IsFalse(zones[1].activeSelf);
        Assert.IsFalse(zones[2].activeSelf);
    }

    [Test]
    public void OnSearchValueChangedEmptyInput() { // Tests that empty input resets the search and shows all zones.
        List<GameObject> zones = new List<GameObject>() {
            CreateZone("Mamaia"),
            CreateZone("VamaVeche"),
            CreateZone("Eforie")
        };

        ZonesManager zonesManager = CreateZonesManagerWithZones(zones);
        TMP_InputField input = CreateInputField();
        TMP_Text autoText = CreateAutoCompleteText();

        SearchManager searchManager = CreateSearchManager(zonesManager, input, autoText);

        InvokePrivateMethod(searchManager, "SearchInit");

        input.onValueChanged.Invoke("mam"); // First filter the list.
        input.onValueChanged.Invoke(""); // Then clear the input.

        Assert.AreEqual("", autoText.text);

        Assert.IsTrue(zones[0].activeSelf);
        Assert.IsTrue(zones[1].activeSelf);
        Assert.IsTrue(zones[2].activeSelf);
    }

    [Test]
    public void ShowZonesManagerToggleZonesManager() { // Tests that the zones manager panel can be shown and hidden.
        List<GameObject> zones = new List<GameObject>();

        ZonesManager zonesManager = CreateZonesManagerWithZones(zones);
        TMP_InputField input = CreateInputField();
        TMP_Text autoText = CreateAutoCompleteText();

        SearchManager searchManager = CreateSearchManager(zonesManager, input, autoText);

        zonesManager.gameObject.SetActive(false);

        searchManager.ShowZonesManager(true);

        Assert.IsTrue(zonesManager.gameObject.activeSelf);

        searchManager.ShowZonesManager(false);

        Assert.IsFalse(zonesManager.gameObject.activeSelf);
    }

    [Test]
    public void SearchInitMissingZonesManagerComponent() { // Tests that a missing ZonesManager component causes an exception.
        GameObject fakeZonesManagerObject = new GameObject("FakeZonesManagerObject");

        TMP_InputField input = CreateInputField();
        TMP_Text autoText = CreateAutoCompleteText();

        GameObject searchManagerObject = new GameObject("SearchManagerObject");
        SearchManager searchManager = searchManagerObject.AddComponent<SearchManager>();

        SetPrivateField(searchManager, "zonesManager", fakeZonesManagerObject); // Object has no ZonesManager component.
        SetPrivateField(searchManager, "searchInputField", input);
        SetPrivateField(searchManager, "autoCompleteText", autoText);

        Assert.Throws<System.NullReferenceException>(() => {
            InvokePrivateMethod(searchManager, "SearchInit");
        });
    }

    [Test]
    public void SearchInitNullInputField() { // Tests that a missing input field causes an exception.
        List<GameObject> zones = new List<GameObject>() {
            CreateZone("Mamaia")
        };

        ZonesManager zonesManager = CreateZonesManagerWithZones(zones);
        TMP_Text autoText = CreateAutoCompleteText();

        GameObject searchManagerObject = new GameObject("SearchManagerObject");
        SearchManager searchManager = searchManagerObject.AddComponent<SearchManager>();

        SetPrivateField(searchManager, "zonesManager", zonesManager.gameObject);
        SetPrivateField(searchManager, "searchInputField", null); // Missing input field.
        SetPrivateField(searchManager, "autoCompleteText", autoText);

        Assert.Throws<System.NullReferenceException>(() => {
            InvokePrivateMethod(searchManager, "SearchInit");
        });
    }

    [Test]
    public void SearchInitNullAutoCompleteText() { // Tests that a missing autocomplete text object causes an exception.
        List<GameObject> zones = new List<GameObject>() {
            CreateZone("Mamaia")
        };

        ZonesManager zonesManager = CreateZonesManagerWithZones(zones);
        TMP_InputField input = CreateInputField();

        GameObject searchManagerObject = new GameObject("SearchManagerObject");
        SearchManager searchManager = searchManagerObject.AddComponent<SearchManager>();

        SetPrivateField(searchManager, "zonesManager", zonesManager.gameObject);
        SetPrivateField(searchManager, "searchInputField", input);
        SetPrivateField(searchManager, "autoCompleteText", null); // Missing autocomplete text object.

        Assert.Throws<System.NullReferenceException>(() => {
            InvokePrivateMethod(searchManager, "SearchInit");
        });
    }

    [Test]
    public void ShowAllZonesNullZoneInList() { // Tests that a null zone in the list causes an exception.
        GameObject searchManagerObject = new GameObject("SearchManagerObject");
        SearchManager searchManager = searchManagerObject.AddComponent<SearchManager>();

        List<GameObject> zones = new List<GameObject>() {
            CreateZone("Mamaia"),
            null
        };

        SetPrivateField(searchManager, "zones", zones); // Inject corrupted list containing a null zone.

        Assert.Throws<System.NullReferenceException>(() => {
            InvokePrivateMethod(searchManager, "ShowAllZones");
        });
    }
}