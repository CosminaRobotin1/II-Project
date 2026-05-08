using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BeachManager : MonoBehaviour {

    /* Attributes */

    [SerializeField] private GameObject calendarManager; // Attribute for calendar manager (reference from calendar manager)_
    [SerializeField] private GameObject beachCalendar; // Attribute for calendar object (reference from calendar manager)
    [SerializeField] private GameObject beachesContent; // Attribute for beaches panel content
    [SerializeField] private GameObject propertiesContent; // Attribute for properties panel content
    [SerializeField] private GameObject beachPrefab; // Attribute that represents the standard form of a beach
    [SerializeField] private GameObject propertyPrefab; // Attribute that represents the standard form of a property
    [SerializeField] private GameObject reviewCalendarPrefab; // Attribute that represents the standard form of review and calendar preset
    [SerializeField] private TextMeshProUGUI propertiesText; // Attribute for properties text
    [SerializeField] private List<GameObject> scrollViews; // Attribute for beach manager scroll views
    private List<GameObject> beaches = new List<GameObject>(); // Attribute for beaches list
    private List<(string name, float rank)> top3Ranks = new List<(string name, float rank)>() { // Attribute for top 3 beaches *delete values after database implementation*
        ("Mamaia", 0.4f),
        ("Mangalia", 2.6f),
        ("Vama Veche", 3.5f),
    };
    private Color[] statusColors = new Color[] { // Attribute for 5 colors (each for status 0 -> 4)
        Color.red,
        new Color(1f, 0.5f, 0f),
        Color.yellow,
        new Color(0.5f, 1f, 0f),
        Color.green,
    };
    private static int currentPressedBeach; // Attribute for identifying the current pressed beach
    private bool reviewMode = false; // Attribute for review mode

    /* Main Methods */

    private void Start() {
        LoadBeachesFromDatabase(); // Loading beaches from data base *needs to be implemented*
    }

    /* Custom Methods */

    private void LoadBeachesFromDatabase() { // Loading beaches from data base *needs to be implemented*
        // For each beach in database, add a new object inside beach list
        if (beaches == null || SceneManager.GetActiveScene().name == "StartingPage") { // What happend on starting page with the top beaches *needs to be implemented*
            for (int i = 0; i < 3; i++) {
                GameObject topBeaches = GameObject.Find("TopBeaches"); // Finding in scene the needed object by it's name (redundant, needs to be changed)
                topBeaches.transform.GetChild(i).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = top3Ranks[i].name;
                topBeaches.transform.GetChild(i).transform.GetChild(1).GetComponent<Image>().color = statusColors[Mathf.FloorToInt(top3Ranks[i].rank + 0.5f)]; // Adding status color
                // topBeaches.transform.GetChild(i).transform.GetChild(2).GetComponent<Image>().sprite = something; // Need to add image from database (most probably from database)
            }
            return;
        }

        DateTime firstDayOfThisMonth = new DateTime(DateTime.Now.Date.Year, DateTime.Now.Date.Month, 1);
        DateTime firstDayOfLastMonth = firstDayOfThisMonth.AddMonths(-1);

        for (int i = 0; i < ZonesManager.GetCurrentPressedZone() * UnityEngine.Random.Range(1, 5) + 1; i++) { // Example of implementation *needs database implementation*
            GameObject newBeach = Instantiate(beachPrefab, beachesContent.transform); // Instantiating a new beach
            newBeach.name = "Beach_" + i; // Take the name from the database *needs database implementation*
            newBeach.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = newBeach.name; // Setting the zone name in it's text field
            newBeach.AddComponent<Beach>(); // Adding beach script for each beach
            Beach currentBeachScript = newBeach.GetComponent<Beach>(); // Getting the previous script aded

            for (DateTime indexDate = firstDayOfLastMonth; indexDate <= DateTime.Now.Date; indexDate = indexDate.AddDays(1)) { // Example of implementation *needs database implementation*
                string date = indexDate.ToString("dd-MM-yyyy");
                currentBeachScript.AddRank(date, UnityEngine.Random.Range(1f, 8f) / 2); // Example of adding rank *needs database implementation*
                for(int j = 0; j < 5 * UnityEngine.Random.Range(4, 8); j++) { // Example of adding properties *needs database implementation*
                    currentBeachScript.AddProperty(date, "Property_" + j, UnityEngine.Random.Range(1, 25) % 2 == 0);
                }
                newBeach.transform.GetChild(1).GetComponent<Image>().color = statusColors[Mathf.FloorToInt(currentBeachScript.GetRank()[date] + 0.5f)]; // Adding status color for each beach
            }
            
            int index = i; // Referencing index (so that it can be transmitted as parameter)
            newBeach.GetComponent<Button>().onClick.AddListener(() => SelectBeach(index)); // Adding the correspondent listener to beach button
            // newBeach.transform.GetChild(2).GetComponent<Image>().sprite = something; // Need to add image from database (most probably from database)
            beaches.Add(newBeach); // Adding the beach in the list
        }
    }
    public void LoadBeachProperties(DateTime currentDate, int beachIndex) { // Loading properties for each beach
        if (beaches == null || SceneManager.GetActiveScene().name != "BeachPage") { // Making sure there are beaches in list and the scene is the proper one
            return;
        }
        foreach(Transform property in propertiesContent.transform) { // Destroying already shown properties
            Destroy(property.gameObject);
        }

        string date = currentDate.ToString("dd-MM-yyyy"); // Setting a date format
        List<(string description, bool status)> beachProperties = beaches[beachIndex].GetComponent<Beach>().GetBeachProperties()[date]; // Getting the beach properties for the current beach index
        List<bool> propertiesModified = beaches[beachIndex].GetComponent<Beach>().GetPropertiesModified()[date]; // Getting the beach properties modified for the current beach index

        for (int i = 0; i < beachProperties.Count; i++) {
            GameObject newProperty = Instantiate(propertyPrefab, propertiesContent.transform); // Instantiating a new property
            newProperty.name = beachProperties[i].description;
            newProperty.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = newProperty.name;

            if(reviewMode == true) { // If user is in review mode there can be buttons on the property statuses
                newProperty.transform.GetChild(1).GetComponent<Image>().color = Color.gray;
                newProperty.transform.GetChild(2).GetComponent<Image>().color = Color.gray;
                newProperty.transform.GetChild(1).AddComponent<Button>(); newProperty.transform.GetChild(2).AddComponent<Button>(); // Adding buttons on statuses
                int propertyIndex = i; // Referencing index (so that it can be transmitted as parameter)
                newProperty.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => ReviewProperty(date, beachIndex, propertyIndex, newProperty, true)); // Adding the correspondent listener to positive status button
                newProperty.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() => ReviewProperty(date, beachIndex, propertyIndex, newProperty, false)); // Adding the correspondent listener to negative status button
                continue;
            }
            if (propertiesModified[i] == false) { // If there was no edit in database or user is in review mode, property statuses become gray
                newProperty.transform.GetChild(1).GetComponent<Image>().color = Color.gray;
                newProperty.transform.GetChild(2).GetComponent<Image>().color = Color.gray;
                continue;
            }
            if(beachProperties[i].status == true) { // Based on property status, the status indicator gets a color
                newProperty.transform.GetChild(1).GetComponent<Image>().color = Color.green;
                newProperty.transform.GetChild(2).GetComponent<Image>().color = Color.gray;
            } else {
                newProperty.transform.GetChild(2).GetComponent<Image>().color = Color.red;
                newProperty.transform.GetChild(1).GetComponent<Image>().color = Color.gray;
            }
        }
        if(reviewMode == false) { // If user is not in review mode already he can review the beach or search in calendar
            GameObject reviewCalendar = Instantiate(reviewCalendarPrefab, propertiesContent.transform); // Instantiating a new review&calendar panel
            if (SettingsOptionsManager.Instance != null) {
                SettingsOptionsManager.Instance.ApplyCurrentLanguageToChildren(reviewCalendar); // Applies the selected language to the newly instantiated review calendar
            }
            reviewCalendar.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => ReviewButton(currentDate)); // Adding the correspondent listener to review button
            reviewCalendar.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => CalendarButton(beaches[beachIndex])); // Adding the correspondent listener to calendar button
            if (currentDate.Date == DateTime.Now.Date || currentDate.Date == DateTime.Now.AddDays(-1).Date) { // User can add review only today or yesterday
                reviewCalendar.transform.GetChild(1).gameObject.SetActive(true);
            }
            propertiesText.text = GetPropertiesTitle(date);
        }
    }
    private void SelectBeach(int index) { // Open selected beach panel
        currentPressedBeach = index;
        ButtonsManager.ToggleObject(scrollViews[1]); // Showing the properties panel
        ButtonsManager.ToggleObject(scrollViews[0]); // hiding the beaches panel
        LoadBeachProperties(DateTime.Now, index); // Showing the properties for today 
    }
    private void ReviewProperty(string date, int beachIndex, int propertyIndex, GameObject property, bool mode) { // Based on type of button pressed in review mode, the property change
        beaches[beachIndex].GetComponent<Beach>().ModifyProperty(date, property.name, mode, propertyIndex); // Modifying selected property
        if(mode == true) { // Toggle between status indicator colors
            property.transform.GetChild(1).GetComponent<Image>().color = Color.green;
            property.transform.GetChild(2).GetComponent<Image>().color = Color.gray;
        } else {
            property.transform.GetChild(2).GetComponent<Image>().color = Color.red;
            property.transform.GetChild(1).GetComponent<Image>().color = Color.gray;
        }
    }
    private void ReviewButton(DateTime currentDate) { // Enters review mode
        reviewMode = true;
        propertiesText.text = GetFinishReviewText();
        scrollViews[1].transform.GetChild(1).GetComponent<Scrollbar>().value = 1; // Resetting the slider
        LoadBeachProperties(currentDate, currentPressedBeach); // Loading properties in review mode
    }
    private void CalendarButton(GameObject currentBeach) { // Enters calendar
        ButtonsManager.ToggleObject(beachCalendar);
        calendarManager.GetComponent<CalendarManager>().LoadCalendar(DateTime.Now, currentBeach);
    }
    public void BackToBeaches() { // Returning to beach panel and setting review mode false
        reviewMode = false;
    }

    /* Localization methods */

    // Checks if the selected application language is English
    private bool IsEnglishSelected() {
        return SettingsOptionsManager.Instance != null
            && SettingsOptionsManager.Instance.GetSelectedLanguageIndex() == 1;
    }

    /* Getters */

    // Returns the localized title used for the properties panel
    private string GetPropertiesTitle(string date) {
        if (IsEnglishSelected()) {
            return "Facilities     " + date; // English version of the dynamic properties title
        }
        return "Facilitățile din " + date; // Romanian version of the dynamic properties title
    }

    // Returns the localized text used while the user is reviewing properties
    private string GetFinishReviewText() {
        if (IsEnglishSelected()) {
            return "Finish"; // English version of the review completion text
        }
        return "Finalizează"; // Romanian version of the review completion text
    }

    public static int GetCurrentPressedBeach() { // Getter for the current beach pressed
        return currentPressedBeach;
    }
}
