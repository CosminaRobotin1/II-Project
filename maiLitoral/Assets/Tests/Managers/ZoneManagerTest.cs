using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System.Reflection;

public class ZonesManagerTest
{
    private Scene GetOrCreateScene(string sceneName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);

        if (!scene.IsValid() || !scene.isLoaded)
        {
            scene = SceneManager.CreateScene(sceneName);
        }

        return scene;
    }

    private void SetActiveScene(string sceneName)
    {
        Scene scene = GetOrCreateScene(sceneName);
        SceneManager.SetActiveScene(scene);
    }

    private void SetPrivateField(object target, string fieldName, object value)
    {
        target.GetType()
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(target, value);
    }

    private void InvokeLoadZonesFromDatabase(ZonesManager manager)
    {
        typeof(ZonesManager)
            .GetMethod("LoadZonesFromDatabase", BindingFlags.NonPublic | BindingFlags.Instance)
            .Invoke(manager, null);
    }

    private ZonesManager CreateManager(GameObject zonesContent, GameObject zonePrefab)
    {
        // Important: keep the scene NOT StartingPage while adding the component,
        // because Awake() runs immediately when AddComponent is called.
        SetActiveScene("SafeTestScene");

        GameObject obj = new GameObject("ZonesManager_TestObject");
        ZonesManager manager = obj.AddComponent<ZonesManager>();

        SetPrivateField(manager, "zonesContent", zonesContent);
        SetPrivateField(manager, "zonePrefab", zonePrefab);

        return manager;
    }

    private GameObject CreateValidZonePrefab()
    {
        GameObject prefab = new GameObject("ZonePrefab");

        prefab.AddComponent<Button>();

        GameObject textChild = new GameObject("ZoneText");
        textChild.transform.SetParent(prefab.transform);

        textChild.AddComponent<TextMeshProUGUI>();

        return prefab;
    }

    private GameObject CreatePrefabWithoutButton()
    {
        GameObject prefab = new GameObject("ZonePrefab_NoButton");

        GameObject textChild = new GameObject("ZoneText");
        textChild.transform.SetParent(prefab.transform);

        textChild.AddComponent<TextMeshProUGUI>();

        return prefab;
    }

    [Test]
    public void Awake_WhenSceneIsNotStartingPage_ShouldNotLoadZones()
    {
        SetActiveScene("SafeTestScene");

        GameObject obj = new GameObject("ZonesManager_TestObject");
        ZonesManager manager = obj.AddComponent<ZonesManager>();

        Assert.AreEqual(0, manager.GetZones().Count);
    }

    [Test]
    public void LoadZonesFromDatabase_WhenSceneIsStartingPage_ShouldCreateTenZones()
    {
        GameObject zonesContent = new GameObject("ZonesContent");
        GameObject zonePrefab = CreateValidZonePrefab();

        ZonesManager manager = CreateManager(zonesContent, zonePrefab);

        SetActiveScene("StartingPage");

        InvokeLoadZonesFromDatabase(manager);

        Assert.AreEqual(10, manager.GetZones().Count);
    }

    [Test]
    public void LoadZonesFromDatabase_ShouldCreateZonesWithCorrectNamesAndText()
    {
        GameObject zonesContent = new GameObject("ZonesContent");
        GameObject zonePrefab = CreateValidZonePrefab();

        ZonesManager manager = CreateManager(zonesContent, zonePrefab);

        SetActiveScene("StartingPage");

        InvokeLoadZonesFromDatabase(manager);

        for (int i = 0; i < manager.GetZones().Count; i++)
        {
            GameObject zone = manager.GetZones()[i];

            Assert.AreEqual("Zone_" + i, zone.name);
            Assert.AreEqual(zonesContent.transform, zone.transform.parent);

            TextMeshProUGUI text = zone.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            Assert.AreEqual("Zone_" + i, text.text);

            Assert.IsNotNull(zone.GetComponent<Button>());
        }
    }

    [Test]
    public void LoadZonesFromDatabase_WhenCalledTwice_ShouldCreateDuplicateZones_CurrentBug()
    {
        GameObject zonesContent = new GameObject("ZonesContent");
        GameObject zonePrefab = CreateValidZonePrefab();

        ZonesManager manager = CreateManager(zonesContent, zonePrefab);

        SetActiveScene("StartingPage");

        InvokeLoadZonesFromDatabase(manager);
        InvokeLoadZonesFromDatabase(manager);

        Assert.AreEqual(
            20,
            manager.GetZones().Count,
            "BUG: Calling LoadZonesFromDatabase twice creates duplicate zones."
        );
    }

    [Test]
    public void LoadZonesFromDatabase_WithNullPrefab_ShouldThrowException()
    {
        GameObject zonesContent = new GameObject("ZonesContent");

        ZonesManager manager = CreateManager(zonesContent, null);

        SetActiveScene("StartingPage");

        Assert.Throws<TargetInvocationException>(() =>
        {
            InvokeLoadZonesFromDatabase(manager);
        });
    }

    [Test]
    public void LoadZonesFromDatabase_WithPrefabMissingButton_ShouldThrowException()
    {
        GameObject zonesContent = new GameObject("ZonesContent");
        GameObject zonePrefab = CreatePrefabWithoutButton();

        ZonesManager manager = CreateManager(zonesContent, zonePrefab);

        SetActiveScene("StartingPage");

        Assert.Throws<TargetInvocationException>(() =>
        {
            InvokeLoadZonesFromDatabase(manager);
        });
    }

    [Test]
    public void GetZones_ShouldExposeInternalListReference()
    {
        GameObject zonesContent = new GameObject("ZonesContent");
        GameObject zonePrefab = CreateValidZonePrefab();

        ZonesManager manager = CreateManager(zonesContent, zonePrefab);

        SetActiveScene("StartingPage");

        InvokeLoadZonesFromDatabase(manager);

        var exposedZonesList = manager.GetZones();

        exposedZonesList.Clear();

        Assert.AreEqual(
            0,
            manager.GetZones().Count,
            "BUG: GetZones exposes the internal list, allowing external scripts to erase zone data."
        );
    }
}