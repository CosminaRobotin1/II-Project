using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;


public class AdminManager : MonoBehaviour
{

    //* Attributes *//

    private string adminName = "admin"; // Attribute for admin name
    private string adminPasswordHash = "03ac674216f3e15c761ee1a5e255f067953623c8b388b4459e13f978d7c846f4"; // Attribute for admin password hash
    [SerializeField] private GameObject loginPanel; // Attribute for login panel
    [SerializeField] private TMP_InputField adminNameInputField; // Attribute for admin name input field
    [SerializeField] private TMP_InputField adminPasswordInputField; // Attribute for admin password input field
    [SerializeField] private TMP_Dropdown zonesDropdown; // Attribute for zones dropdown
    [SerializeField] private TMP_Dropdown beachesDropdown; // Attribute for beaches dropdown
    [SerializeField] private TMP_Dropdown propertiesDropdown; // Attribute for properties dropdown

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
            Debug.Log("Invalid admin name or password.");
        }
    }

    private string HashPassword(string password) { // Converts the password text into a SHA256 hash
        using (SHA256 sha256 = SHA256.Create())
        {
            byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
            byte[] hashBytes = sha256.ComputeHash(passwordBytes);
            StringBuilder stringBuilder = new StringBuilder();

            foreach (byte hashByte in hashBytes)
            {
                stringBuilder.Append(hashByte.ToString("x2"));
            }

            return stringBuilder.ToString();
        }
    }

    private void LoadZonesDropdown() { // Loads all zones from database into dropdown
        zonesDropdown.ClearOptions();
        List<string> zoneNames = new List<string>();

        foreach (ZoneData zone in DatabaseManager.Instance.GetAllZones()) {zoneNames.Add(zone.Name);}

        zonesDropdown.AddOptions(zoneNames);
    }

    private void LoadBeachesDropdown() { // Loads all beaches from database into dropdown
        beachesDropdown.ClearOptions();
        List<string> beachNames = new List<string>();
        beachNames.Add("");

        foreach (BeachData beach in DatabaseManager.Instance.GetAllBeaches()){beachNames.Add(beach.Name);}
        beachesDropdown.AddOptions(beachNames);
    }

    private void LoadPropertiesDropdown() { // Loads all properties from database into dropdown
        propertiesDropdown.ClearOptions();
        List<string> propertyNames = new List<string>();

        foreach (PropertyData property in DatabaseManager.Instance.GetAllProperties())
        {
            propertyNames.Add(property.Name);
        }

        propertiesDropdown.AddOptions(propertyNames);
    }

    public void RefreshDropdowns() { // Refreshes all admin dropdowns with database data
        LoadZonesDropdown();
        LoadBeachesDropdown();
        LoadPropertiesDropdown();
    }

    //* Getters *//



    //* Setters *//


}