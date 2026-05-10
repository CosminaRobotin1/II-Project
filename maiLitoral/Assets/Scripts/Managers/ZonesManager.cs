using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ZonesManager : MonoBehaviour {

    /* Attributes */

    [SerializeField] private GameObject zonesManager; // Attribute for zones manager
    [SerializeField] private GameObject zonesContent; // Attribute for zones panel content
    [SerializeField] private GameObject zonePrefab; // Attribute that represents the standard form of a zone
    private List<GameObject> zones = new List<GameObject>(); // Attribute for zones list
    private static int currentPressedZone; // Attribute for identifying the current pressed zone

    /* Main Methods */

    private void Start() { // Loads all zones from the database and creates a button for each one
        LoadZonesFromDatabase();
    }

    /* Custom Methods */

    private void LoadZonesFromDatabase() { // Loads all zones from the database and creates a button for each one
        if (zones == null || SceneManager.GetActiveScene().name != "StartingPage") {
            return;
        }
        if (DatabaseManager.Instance == null) {
            UnityEngine.Debug.LogError("DatabaseManager instance is missing.");
            return;
        }
        List<ZoneData> zonesFromDatabase = DatabaseManager.Instance.GetAllZones();
        foreach (ZoneData zoneData in zonesFromDatabase) {
            GameObject newZone = Instantiate(zonePrefab, zonesContent.transform);
            newZone.name = zoneData.Name;
            newZone.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = zoneData.Name;
            int zoneId = zoneData.Id;
            newZone.GetComponent<Button>().onClick.AddListener(() => SelectZone(zoneId));
            zones.Add(newZone);
        }
    }
    private void SelectZone(int index) { // Saves the selected zone id and opens the beach page
        currentPressedZone = index;
        ButtonsManager.ReturnToPage("BeachPage");
    }

    /* Getters */

    public static int GetCurrentPressedZone() { // Returns the id of the currently selected zone
        return currentPressedZone;
    }
    public List<GameObject> GetZones() { // Returns the instantiated zone objects
        return zones;
    }
}