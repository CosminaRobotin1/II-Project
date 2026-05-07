using NUnit.Framework;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine.SceneManagement;

public class SearchManagerTest{
    private Scene GetOrCreateScene(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);

        if (!scene.IsValid() || !scene.isLoaded)
        {
            scene = SceneManager.CreateScene(sceneName);
        }

        return scene;
    }

    private void SetActiveScene(string sceneName){
        Scene scene = GetOrCreateScene(sceneName);
        SceneManager.SetActiveScene(scene);
    }

    private void SetPrivateField(object target, string fieldName, object value){
        target.GetType()
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(target, value);
    }

    private void InvokePrivateMethod(object target, string methodName, params object[] parameters){
        try
        {
            target.GetType()
                .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(target, parameters);
        }
        catch (TargetInvocationException exception)
        {
            throw exception.InnerException;
        }
    }

    private GameObject CreateZone(string name){
        GameObject zone = new GameObject(name);
        zone.SetActive(false);
        return zone;
    }

    private ZonesManager CreateZonesManagerWithZones(List<GameObject> zones){
        SetActiveScene("SafeSearchManagerTestScene");

        GameObject zonesManagerObject = new GameObject("ZonesManagerObject");
        ZonesManager zonesManager = zonesManagerObject.AddComponent<ZonesManager>();

        typeof(ZonesManager)
            .GetField("zones", BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(zonesManager, zones);

        return zonesManager;
    }

    private TMP_InputField CreateInputField(){
        GameObject inputObject = new GameObject("SearchInputField");
        return inputObject.AddComponent<TMP_InputField>();
    }

    private TMP_Text CreateAutoCompleteText(){
        GameObject textObject = new GameObject("AutoCompleteText");
        return textObject.AddComponent<TextMeshProUGUI>();
    }

    private SearchManager CreateSearchManager(
        ZonesManager zonesManager,
        TMP_InputField inputField,
        TMP_Text autoCompleteText
    )
    {
        GameObject searchManagerObject = new GameObject("SearchManagerObject");
        SearchManager searchManager = searchManagerObject.AddComponent<SearchManager>();

        SetPrivateField(searchManager, "zonesManager", zonesManager.gameObject);
        SetPrivateField(searchManager, "searchInputField", inputField);
        SetPrivateField(searchManager, "autoCompleteText", autoCompleteText);

        return searchManager;
    }

    [Test]
    public void SearchInit_ShouldClearAutocompleteAndShowAllZones(){
        List<GameObject> zones = new List<GameObject>()
        {
            CreateZone("Zone_0"),
            CreateZone("Zone_1"),
            CreateZone("Zone_2")
        };

        ZonesManager zonesManager = CreateZonesManagerWithZones(zones);
        TMP_InputField input = CreateInputField();
        TMP_Text autoText = CreateAutoCompleteText();

        autoText.text = "Old suggestion";

        SearchManager searchManager = CreateSearchManager(zonesManager, input, autoText);

        InvokePrivateMethod(searchManager, "SearchInit");

        Assert.AreEqual("", autoText.text);

        Assert.IsTrue(zones[0].activeSelf);
        Assert.IsTrue(zones[1].activeSelf);
        Assert.IsTrue(zones[2].activeSelf);
    }

    [Test]
    public void OnSearchValueChanged_WithPartialMatch_ShouldShowSuggestionAndFilterZones(){
        List<GameObject> zones = new List<GameObject>()
        {
            CreateZone("Mamaia"),
            CreateZone("VamaVeche"),
            CreateZone("Eforie")
        };

        ZonesManager zonesManager = CreateZonesManagerWithZones(zones);
        TMP_InputField input = CreateInputField();
        TMP_Text autoText = CreateAutoCompleteText();

        SearchManager searchManager = CreateSearchManager(zonesManager, input, autoText);

        InvokePrivateMethod(searchManager, "SearchInit");

        input.onValueChanged.Invoke("mam");

        Assert.AreEqual("Mamaia", autoText.text);

        Assert.IsTrue(zones[0].activeSelf);
        Assert.IsFalse(zones[1].activeSelf);
        Assert.IsFalse(zones[2].activeSelf);
    }

    [Test]
    public void OnSearchValueChanged_WithExactMatch_ShouldClearSuggestion(){
        List<GameObject> zones = new List<GameObject>()
        {
            CreateZone("Mamaia"),
            CreateZone("VamaVeche")
        };

        ZonesManager zonesManager = CreateZonesManagerWithZones(zones);
        TMP_InputField input = CreateInputField();
        TMP_Text autoText = CreateAutoCompleteText();

        SearchManager searchManager = CreateSearchManager(zonesManager, input, autoText);

        InvokePrivateMethod(searchManager, "SearchInit");

        input.onValueChanged.Invoke("Mamaia");

        Assert.AreEqual("", autoText.text);

        Assert.IsTrue(zones[0].activeSelf);
        Assert.IsFalse(zones[1].activeSelf);
    }

    [Test]
    public void OnSearchValueChanged_WithNoMatch_ShouldHideAllZonesAndClearSuggestion(){
        List<GameObject> zones = new List<GameObject>()
        {
            CreateZone("Mamaia"),
            CreateZone("VamaVeche"),
            CreateZone("Eforie")
        };

        ZonesManager zonesManager = CreateZonesManagerWithZones(zones);
        TMP_InputField input = CreateInputField();
        TMP_Text autoText = CreateAutoCompleteText();

        SearchManager searchManager = CreateSearchManager(zonesManager, input, autoText);

        InvokePrivateMethod(searchManager, "SearchInit");

        input.onValueChanged.Invoke("unknown");

        Assert.AreEqual("", autoText.text);

        Assert.IsFalse(zones[0].activeSelf);
        Assert.IsFalse(zones[1].activeSelf);
        Assert.IsFalse(zones[2].activeSelf);
    }

    [Test]
    public void OnSearchValueChanged_WithEmptyInput_ShouldShowAllZonesAndClearSuggestion(){
        List<GameObject> zones = new List<GameObject>()
        {
            CreateZone("Mamaia"),
            CreateZone("VamaVeche"),
            CreateZone("Eforie")
        };

        ZonesManager zonesManager = CreateZonesManagerWithZones(zones);
        TMP_InputField input = CreateInputField();
        TMP_Text autoText = CreateAutoCompleteText();

        SearchManager searchManager = CreateSearchManager(zonesManager, input, autoText);

        InvokePrivateMethod(searchManager, "SearchInit");

        input.onValueChanged.Invoke("mam");
        input.onValueChanged.Invoke("");

        Assert.AreEqual("", autoText.text);

        Assert.IsTrue(zones[0].activeSelf);
        Assert.IsTrue(zones[1].activeSelf);
        Assert.IsTrue(zones[2].activeSelf);
    }

    [Test]
    public void ShowZonesManager_ShouldToggleZonesManagerPanel(){
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
    public void SearchInit_WithMissingZonesManagerComponent_ShouldThrowException(){
        GameObject fakeZonesManagerObject = new GameObject("FakeZonesManagerObject");

        TMP_InputField input = CreateInputField();
        TMP_Text autoText = CreateAutoCompleteText();

        GameObject searchManagerObject = new GameObject("SearchManagerObject");
        SearchManager searchManager = searchManagerObject.AddComponent<SearchManager>();

        SetPrivateField(searchManager, "zonesManager", fakeZonesManagerObject);
        SetPrivateField(searchManager, "searchInputField", input);
        SetPrivateField(searchManager, "autoCompleteText", autoText);

        Assert.Throws<System.NullReferenceException>(() =>
        {
            InvokePrivateMethod(searchManager, "SearchInit");
        });
    }

    [Test]
    public void SearchInit_WithNullInputField_ShouldThrowException(){
        List<GameObject> zones = new List<GameObject>()
        {
            CreateZone("Mamaia")
        };

        ZonesManager zonesManager = CreateZonesManagerWithZones(zones);
        TMP_Text autoText = CreateAutoCompleteText();

        GameObject searchManagerObject = new GameObject("SearchManagerObject");
        SearchManager searchManager = searchManagerObject.AddComponent<SearchManager>();

        SetPrivateField(searchManager, "zonesManager", zonesManager.gameObject);
        SetPrivateField(searchManager, "searchInputField", null);
        SetPrivateField(searchManager, "autoCompleteText", autoText);

        Assert.Throws<System.NullReferenceException>(() =>
        {
            InvokePrivateMethod(searchManager, "SearchInit");
        });
    }

    [Test]
    public void SearchInit_WithNullAutoCompleteText_ShouldThrowException(){
        List<GameObject> zones = new List<GameObject>()
        {
            CreateZone("Mamaia")
        };

        ZonesManager zonesManager = CreateZonesManagerWithZones(zones);
        TMP_InputField input = CreateInputField();

        GameObject searchManagerObject = new GameObject("SearchManagerObject");
        SearchManager searchManager = searchManagerObject.AddComponent<SearchManager>();

        SetPrivateField(searchManager, "zonesManager", zonesManager.gameObject);
        SetPrivateField(searchManager, "searchInputField", input);
        SetPrivateField(searchManager, "autoCompleteText", null);

        Assert.Throws<System.NullReferenceException>(() =>
        {
            InvokePrivateMethod(searchManager, "SearchInit");
        });
    }

    [Test]
    public void ShowAllZones_WithNullZoneInList_ShouldThrowException(){
        GameObject searchManagerObject = new GameObject("SearchManagerObject");
        SearchManager searchManager = searchManagerObject.AddComponent<SearchManager>();

        List<GameObject> zones = new List<GameObject>()
        {
            CreateZone("Mamaia"),
            null
        };

        SetPrivateField(searchManager, "zones", zones);

        Assert.Throws<System.NullReferenceException>(() =>
        {
            InvokePrivateMethod(searchManager, "ShowAllZones");
        });
    }
}