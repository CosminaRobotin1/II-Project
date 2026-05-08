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

    private void Start() { LoadZonesFromDatabase(); }

    /* Custom Methods */

    // Loads all zones from the database and creates a button for each one.
    private void LoadZonesFromDatabase() {
        if (zones == null || SceneManager.GetActiveScene().name != "StartingPage") { return; }

        if (DatabaseManager.Instance == null){
            UnityEngine.Debug.LogError("DatabaseManager instance is missing.");
            return;
        }

        List<ZoneData> zonesFromDatabase = DatabaseManager.Instance.GetAllZones();

        foreach (ZoneData zoneData in zonesFromDatabase)
        {
            GameObject newZone = Instantiate(zonePrefab, zonesContent.transform);
            newZone.name = "Zone_" + zoneData.Id;
            newZone.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = zoneData.Name;

            int zoneId = zoneData.Id;
            newZone.GetComponent<Button>().onClick.AddListener(() => SelectZone(zoneId));

            zones.Add(newZone);
        }
    }

    // Saves the selected zone id and opens the beach page.
    private void SelectZone(int index) {
        currentPressedZone = index;
        ButtonsManager.ReturnToPage("BeachPage");
    }

    /* Getters */

    // Returns the id of the currently selected zone.
    public static int GetCurrentPressedZone() { return currentPressedZone; }

    // Returns the instantiated zone objects.
    public List<GameObject> GetZones() { return zones; }
}