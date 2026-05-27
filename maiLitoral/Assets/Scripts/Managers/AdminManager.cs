using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Unity.Android.Gradle.Manifest;
using System.Collections;
using System;

public class AdminManager : MonoBehaviour {

    //* Attributes *//

    private string adminName = "admin"; // Attribute for admin name
    private string adminPasswordHash = "03ac674216f3e15c761ee1a5e255f067953623c8b388b4459e13f978d7c846f4"; // Attribute for admin password hash
    [SerializeField] private GameObject loginPanel; // Attribute for login panel
    [SerializeField] private GameObject addBeaches; // Attribute for add beaches panel
    [SerializeField] private GameObject modifyBeaches; // Attribute for modify beaches panel
    [SerializeField] private TMP_InputField adminNameInputField; // Attribute for admin name input field
    [SerializeField] private TMP_InputField adminPasswordInputField; // Attribute for admin password input field
    [SerializeField] private TMP_Dropdown zonesDropdown; // Attribute for zones dropdown
    [SerializeField] private TMP_Dropdown beachesDropdown; // Attribute for beaches dropdown
    [SerializeField] private TMP_Dropdown propertiesDropdown; // Attribute for properties dropdown
    [SerializeField] private TMP_Dropdown propertiesDropdown2; // Attribute for properties dropdown
    [SerializeField] private GameObject zonesInput; // Attribute for zones input field
    [SerializeField] private GameObject beachesInput; // Attribute for beaches input field
    [SerializeField] private GameObject modifyBeachesInput; // Attribute for new beaches input field
    [SerializeField] private Button createZoneButton; // Attribute for create zone button
    [SerializeField] private Button deleteZoneButton; // Attribute for delete zone button
    [SerializeField] private Button deleteBeachButton; // Attribute for delete beach button
    [SerializeField] private Button addPropertyButton; // Attribute for add property button
    private int deleteZoneTries = 0; // Attribute for making sure the user really wants to delete a zone
    private int deleteBeachTries = 0; // Attribute for making sure the user really wants to delete a beach

    //* Main Methods *//

    private void Start() { // Initializes the admin manager settings
        adminPasswordInputField.contentType = TMP_InputField.ContentType.Password;
        adminPasswordInputField.ForceLabelUpdate();
        LoadZonesDropdown();
        LoadBeachesDropdown();
        LoadPropertiesDropdown();
    }

    //* Custom Methods *//

    public void Login() { // Logins the admin to the admin page
        if (adminName == adminNameInputField.text && adminPasswordHash == HashPassword(adminPasswordInputField.text)){
            ButtonsManager.ToggleObject(loginPanel);
        } else {
            Debug.LogWarning("Invalid admin name or password!");
        }
    }
    private string HashPassword(string password) { // Converts the password text into a SHA256 hash
        using (SHA256 sha256 = SHA256.Create()) {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] hashBytes = sha256.ComputeHash(passwordBytes);
            StringBuilder stringBuilder = new StringBuilder();
            foreach (byte hashByte in hashBytes) {
                stringBuilder.Append(hashByte.ToString("x2"));
            }
            return stringBuilder.ToString();
        }
    }
    private void LoadZonesDropdown() { // Loads all zones from database into dropdown
        zonesDropdown.ClearOptions();
        List<string> zoneNames = new List<string>();
        foreach (ZoneData zone in DatabaseManager.Instance.GetAllZones()) {
            zoneNames.Add(zone.Name);
        }
        zonesDropdown.AddOptions(zoneNames);
    }
    private void LoadBeachesDropdown() { // Loads all beaches from database into dropdown
        beachesDropdown.ClearOptions();
        List<string> beachNames = new List<string>();
        beachNames.Add(SettingsManager.Instance.GetSelectedLanguageIndex() == 0 ? "Creează plajă" : "Create beach");
        foreach (BeachData beach in DatabaseManager.Instance.GetAllBeaches()) {
            beachNames.Add(beach.Name);
        }
        beachesDropdown.AddOptions(beachNames);
    }
    private void LoadPropertiesDropdown() { // Loads all properties from database into dropdown
        propertiesDropdown.ClearOptions(); propertiesDropdown2.ClearOptions();
        List<string> propertyNames = new List<string>();
        foreach (PropertyData property in DatabaseManager.Instance.GetAllProperties()) {
            propertyNames.Add(SettingsManager.Instance.GetSelectedLanguageIndex() == 0 ? property.NameRo : property.NameEn);
        }
        propertiesDropdown.AddOptions(propertyNames);
        propertiesDropdown2.AddOptions(propertyNames);
    }
    public void RefreshDropdowns() { // Refreshes all admin dropdowns with database data
        LoadZonesDropdown();
        LoadBeachesDropdown();
        LoadPropertiesDropdown();
    }
    public void CreateNewZone() { // Create new zone button
        string zoneName = zonesInput.GetComponent<TMP_InputField>().text;
        string outputText;
        switch(DatabaseManager.Instance.AddZone(zoneName)) {
            case -1: outputText = "Invalid!"; break;
            case 0: outputText = SettingsManager.Instance.GetSelectedLanguageIndex() == 0 ? "Zona există!" : "Zone exists!"; break;
            case 1: outputText = "Succes!"; RefreshDropdowns(); break;
            default: outputText = SettingsManager.Instance.GetSelectedLanguageIndex() == 0 ? "Eroare" : "Error"; break;
        }
        zonesInput.GetComponent<TMP_InputField>().text = outputText;
    }
    public void DeleteZone() {  // Delete zone button
        int valueIndex = zonesDropdown.value;
        string zoneName = zonesDropdown.options[valueIndex].text;
        string outputText = SettingsManager.Instance.GetSelectedLanguageIndex() == 0 ? "Sunteți sigur?" : "Are you sure?";
        deleteZoneTries++;
        if (deleteZoneTries >= 2) {
            DatabaseManager.Instance.DeleteZone(zoneName);
            outputText = "Succes!";
            RefreshDropdowns();
            deleteZoneTries = 0;
        }
        deleteZoneButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = outputText;
        StartCoroutine(WaitingBeforeWritingText(deleteZoneButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>(), SettingsManager.Instance.GetSelectedLanguageIndex() == 0 ? "Șterge" : "Delete", 3));
    }
    public void ManageBeaches() { // Manage beach button
        int valueIndex = beachesDropdown.value;
        string beachName = beachesDropdown.options[valueIndex].text;
        if(beachName == "Creează plajă" || beachName == "Create beach") {
            ButtonsManager.ToggleObject(addBeaches);
        } else {
            ButtonsManager.ToggleObject(modifyBeaches);
        }
    }
    public void CreateNewBeach() { // Create new beach button
        int valueIndex = zonesDropdown.value;
        string zoneName = zonesDropdown.options[valueIndex].text;
        ZoneData zone = DatabaseManager.Instance.GetZoneByName(zoneName);
        string beachName = beachesInput.GetComponent<TMP_InputField>().text;
        string outputText;
        switch(DatabaseManager.Instance.AddBeach(beachName, zone.Id)) {
            case -2: outputText = "Invalid!"; break;
            case -1: outputText = SettingsManager.Instance.GetSelectedLanguageIndex() == 0 ? "Zona nu există!" : "Zone inexistent!"; break;
            case 0: outputText = SettingsManager.Instance.GetSelectedLanguageIndex() == 0 ? "Plaja există!" : "Beach exists!"; break;
            case 1: outputText = "Succes!"; RefreshDropdowns(); break;
            default: outputText = SettingsManager.Instance.GetSelectedLanguageIndex() == 0 ? "Eroare" : "Error"; break;
        }
        beachesInput.GetComponent<TMP_InputField>().text = outputText;
    }
    public void DeleteBeach() { // Delete beach button
        int valueIndex = beachesDropdown.value;
        string beachName = beachesDropdown.options[valueIndex].text;
        string outputText = SettingsManager.Instance.GetSelectedLanguageIndex() == 0 ? "Sunteți sigur?" : "Are you sure?";
        BeachData beach = DatabaseManager.Instance.GetBeachDataByName(beachName);
        deleteBeachTries++;
        if (deleteBeachTries >= 2) {
            DatabaseManager.Instance.DeleteBeach(beach.Id);
            outputText = "Succes!";
            RefreshDropdowns();
            deleteBeachTries = 0;
        }
        deleteBeachButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = outputText;
        StartCoroutine(WaitingBeforeWritingText(deleteBeachButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>(), SettingsManager.Instance.GetSelectedLanguageIndex() == 0 ? "Șterge" : "Delete", 3));
    }
    public void ModifyBeachName() { // Modify beach button
        string newBeachName = modifyBeachesInput.GetComponent<TMP_InputField>().text;
        int valueIndex = beachesDropdown.value;
        string beachName = beachesDropdown.options[valueIndex].text;
        string outputText;
        switch(DatabaseManager.Instance.ModifyBeach(beachName, newBeachName)) {
            case -2: outputText = "Invalid!"; break;
            case -1: outputText = SettingsManager.Instance.GetSelectedLanguageIndex() == 0 ? "Plaja nu există!" : "Beach inexistent!"; break;
            case 0: outputText = SettingsManager.Instance.GetSelectedLanguageIndex() == 0 ? "Nume folosit!" : "Used name!"; break;
            case 1: outputText = "Succes!"; RefreshDropdowns(); break;
            default: outputText = SettingsManager.Instance.GetSelectedLanguageIndex() == 0 ? "Eroare" : "Error"; break;
        }
        modifyBeachesInput.GetComponent<TMP_InputField>().text = outputText;
    }
    public void AddPropertyToBeach() {
        int valueIndexForBeach = beachesDropdown.value;
        string beachName = beachesDropdown.options[valueIndexForBeach].text;
        if (beachName == "Creează plajă" || beachName == "Create beach") {
            addPropertyButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = SettingsManager.Instance.GetSelectedLanguageIndex() == 0 ? "Alege o plajă!" : "Select a beach!";
            StartCoroutine(WaitingBeforeWritingText(addPropertyButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>(), SettingsManager.Instance.GetSelectedLanguageIndex() == 0 ? "Adaugă" : "Add", 3));
            return;
        }
        int valueIndexForProperty = propertiesDropdown2.value;
        List<PropertyData> allProperties = DatabaseManager.Instance.GetAllProperties();
        PropertyData property = allProperties[valueIndexForProperty];
        BeachData beach = DatabaseManager.Instance.GetBeachDataByName(beachName);
        string defaultValue = "";
        switch (property.Type.ToLower()) {
            case "bool": defaultValue = "false"; break;
            case "int": defaultValue = "0"; break;
            case "string": defaultValue = "-"; break;
        }
        DatabaseManager.Instance.AddOrUpdateProperty(property, DateTime.Now.ToString("dd-MM-yyyy"), beach.Id, defaultValue);
        addPropertyButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "Succes!";
        StartCoroutine(WaitingBeforeWritingText(addPropertyButton.transform.GetChild(0).GetComponent<TextMeshProUGUI>(), SettingsManager.Instance.GetSelectedLanguageIndex() == 0 ? "Adaugă" : "Add", 3));
        //RefreshDropdowns();
    }

    /* Coroutines */

    private IEnumerator WaitingBeforeWritingText(TextMeshProUGUI textToModify, string text, float seconds) { // Coroutine for delaying some text
        yield return new WaitForSeconds(seconds);
        textToModify.text = text;
    }
}