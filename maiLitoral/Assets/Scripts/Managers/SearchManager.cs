using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class SearchManager : MonoBehaviour {

    /* Attributes */

    [SerializeField] private GameObject zonesManager; // Attribute for zones manager
    [SerializeField] private GameObject zonesContent; // Attribute for zones content
    [SerializeField] private GameObject zoneUnavailable; // Attribute for zone unavailable
    [SerializeField] private GameObject searchButton; // Attribute for search button
    [SerializeField] private TMP_InputField searchInputField; // Attribute for search input field
    [SerializeField] private TMP_Text autoCompleteText; // Attribute for auto complete text
    private List<GameObject> zones = new List<GameObject>(); // Attribute for zones list
    private string currentSuggestion = ""; // Attribute for current zone suggestion

    /* Main Methods */

    private void Start() {
        SearchInit(); // Setup the search suggestion
    }

    /* Custom methods */

    private void SearchInit() { // Setup the search suggestion
        zones = zonesManager.GetComponent<ZonesManager>().GetZones(); // Getting the available zones
        searchButton.GetComponent<Button>().onClick.AddListener(() => SearchButton()); // Added the listener for the search button
        searchInputField.onValueChanged.AddListener(OnSearchValueChanged); // Added the listener for input field
        autoCompleteText.text = ""; // Making sure the autocomplete is empty first
        ShowAllZones(); // Showing all zones in the start (because none was searched)
    }
    private void OnSearchValueChanged(string currentText) { // Listener for updating the panels when typing
        UpdateSuggestion(currentText); // Updating the panels when typing
        FilterZones(currentText); // Showing only the matched searched zones
    }

    private void UpdateSuggestion(string currentText) { // Updating the panels when typing
        currentSuggestion = "";
        if (string.IsNullOrWhiteSpace(currentText)) { // What happends if there is no text
            autoCompleteText.text = "";
            return;
        }
        string lowerText = currentText.ToLower(); // Not case sensitive search
        foreach (GameObject zone in zones) {
            string label = zone.name;
            if (label.ToLower().StartsWith(lowerText)) {
                currentSuggestion = label;
                break;
            }
        }
        if (string.IsNullOrEmpty(currentSuggestion)) { // What happends if there is no suggestion
            autoCompleteText.text = "";
            return;
        }
        if (currentSuggestion.Length == currentText.Length) { // Do not autocomplete if the suggestion mathes the input field
            autoCompleteText.text = "";
            return;
        }
        autoCompleteText.text = currentSuggestion; // Filling the autocomplete text with the suggestion found
    }
    private void SearchButton() { // Opens the right zone based on autocompleted text
        if (autoCompleteText.text == "" || autoCompleteText.text == null) {
            if (zoneUnavailable.activeSelf == false) {
                ButtonsManager.ToggleObject(zoneUnavailable);
            }
            return;
        }
        for (int i = 0; i < zonesContent.transform.childCount; i++) { // Calls the zone button method for the available zone name in the autocomplete text
            GameObject zone = zonesContent.transform.GetChild(i).gameObject;
            if (zone.name == autoCompleteText.text) {
                zone.GetComponent<Button>().onClick.Invoke(); // Invoking the right zone button method
                break;
            }
        }
    }
    private void FilterZones(string currentText) { // Showing only the matched searched zones
        if (string.IsNullOrWhiteSpace(currentText)) {
            ShowAllZones();
            return;
        }
        string lowerText = currentText.ToLower();
        foreach (GameObject zone in zones) { // Showing only the relevant zone
            bool matches = zone.name.ToLower().Contains(lowerText);
            zone.SetActive(matches);
        }
    }
    private void ShowAllZones() { // Showing all zones
        foreach (GameObject zone in zones) {
            zone.SetActive(true);
        }
    }
    public void ShowZonesManager(bool mode) { // Activate / Deactivate the zones panel
        zonesManager.SetActive(mode);
    }
    public void HideZoneUnavailable() { // Deactivate the zone unavailable panel
        zoneUnavailable.SetActive(false);
    }
}