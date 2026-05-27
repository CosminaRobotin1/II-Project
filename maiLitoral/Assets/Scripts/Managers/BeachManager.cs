using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BeachManager : MonoBehaviour {

    /* Attributes */

    [SerializeField] private GameObject calendarManager; // Attribute for calendar manager
    [SerializeField] private GameObject beachCalendar; // Attribute for calendar object
    [SerializeField] private GameObject beachesContent; // Attribute for beaches panel content
    [SerializeField] private GameObject propertiesContent; // Attribute for properties panel content
    [SerializeField] private GameObject beachPrefab; // Attribute that represents the standard form of a beach
    [SerializeField] private List<GameObject> propertyPrefabs; // Attribute that represents the standard form of a properties
    [SerializeField] private GameObject reviewCalendarPrefab; // Attribute that represents the standard form of review and calendar preset
    [SerializeField] private TextMeshProUGUI propertiesText; // Attribute for properties text
    [SerializeField] private List<GameObject> scrollViews; // Attribute for beach manager scroll views
    private bool propertiesTexting = false; // Attribute for checking if the properties text was changed
    private List<GameObject> beaches = new List<GameObject>(); // Attribute for beaches list
    private Color[] statusColors = new Color[] { // Attribute for 5 colors, each for status 0 -> 4
        Color.green,
        new Color(0.5f, 1f, 0f),
        Color.yellow,
        new Color(1f, 0.5f, 0f),
        Color.red
    };
    private static int currentPressedBeach; // Attribute for identifying the current pressed beach
    private bool reviewMode = false; // Attribute for review mode

    /* Main Methods */

    private void Start() {
        LoadBeachesFromDatabase();
    }

    /* Custom Methods */

    private void LoadBeachesFromDatabase() { // Loads top beaches, beach buttons, ranks and properties from the database
        if (beaches == null) {
            return;
        }
        string today = DateTime.Now.ToString("dd-MM-yyyy");
        if (SceneManager.GetActiveScene().name == "StartingPage") {
            List<(string name, float rank)> top3Ranks = DatabaseManager.Instance.GetTop3BeachesByRank(today);
            GameObject topBeaches = GameObject.Find("TopBeaches");
            if (topBeaches == null) {
                return;
            }
            for (int i = 0; i < top3Ranks.Count && i < 3; i++) {
                topBeaches.transform.GetChild(i).transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = top3Ranks[i].name;
                ChangeStatusColor(topBeaches.transform.GetChild(i).transform.GetChild(1).GetComponent<Image>(), top3Ranks[i].rank);
                BeachData beach = DatabaseManager.Instance.GetBeachDataByName(top3Ranks[i].name);
                StartCoroutine(DownloadImage(beach.ImageUrl, topBeaches.transform.GetChild(i).transform.GetChild(2).transform.GetChild(0).transform.GetChild(0).GetComponent<Image>())); // Adding the beach image on the designated square
            }
            return;
        }
        int selectedZoneId = ZonesManager.GetCurrentPressedZone();
        List<BeachData> beachesFromDatabase = DatabaseManager.Instance.GetBeachesByZone(selectedZoneId);
        DateTime firstDayOfThisMonth = new DateTime(DateTime.Now.Date.Year, DateTime.Now.Date.Month, 1);
        DateTime firstDayOfLastMonth = firstDayOfThisMonth.AddMonths(-1);
        for (int i = 0; i < beachesFromDatabase.Count; i++) {
            BeachData beachData = beachesFromDatabase[i];
            GameObject newBeach = Instantiate(beachPrefab, beachesContent.transform);
            newBeach.name = beachData.Name;
            StartCoroutine(DownloadImage(beachData.ImageUrl, newBeach.transform.GetChild(2).transform.GetChild(0).GetChild(0).GetComponent<Image>())); // Adding the beach image on the designated square
            newBeach.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = beachData.Name; // Adding the beach name text
            newBeach.AddComponent<Beach>();
            Beach currentBeachScript = newBeach.GetComponent<Beach>();
            currentBeachScript.SetBeachId(beachData.Id);
            for (DateTime indexDate = firstDayOfLastMonth; indexDate <= DateTime.Now.Date; indexDate = indexDate.AddDays(1)) {
                string date = indexDate.ToString("dd-MM-yyyy");
                float beachRank = DatabaseManager.Instance.GetRankForBeachDay(beachData.Id, date);
                currentBeachScript.LoadRankFromDatabase(date, beachRank);
                List<(List<string> descriptions, string type, bool? boolValue, int? intValue, string stringValue)> properties = DatabaseManager.Instance.GetPropertiesForBeachDay(beachData.Id, date);
                foreach (var property in properties) {
                    string value;
                    switch (property.type) {
                        case "bool": value = property.boolValue.ToString(); break;
                        case "int": value = property.intValue.ToString(); break;
                        case "string": value = property.stringValue.ToString(); break;
                        default: value = ""; return;
                    }
                    currentBeachScript.LoadPropertiesFromDatabase(date, property.descriptions, property.type, value);
                }
                ChangeStatusColor(newBeach.transform.GetChild(1).GetComponent<Image>(), beachRank); // Adding the beach status color
            }
            int index = i;
            newBeach.GetComponent<Button>().onClick.AddListener(() => SelectBeach(index));
            beaches.Add(newBeach);
        }
    }
    public void LoadBeachProperties(DateTime currentDate, int beachIndex) { // Loads properties for the selected beach and date
        if (beaches == null || SceneManager.GetActiveScene().name != "BeachPage") {
            return;
        }
        foreach (Transform property in propertiesContent.transform) {
            Destroy(property.gameObject);
        }
        string date = currentDate.ToString("dd-MM-yyyy");
        if(beaches[beachIndex].GetComponent<Beach>().GetBeachProperties()[date] == null) {
            Debug.LogError("Current date is invalid!");
            return;
        }
        List<(List<string> descriptions, string type, bool? boolValue, int? intValue, string stringValue)> beachProperties = beaches[beachIndex].GetComponent<Beach>().GetBeachProperties()[date];
        List<bool> propertiesModified = beaches[beachIndex].GetComponent<Beach>().GetPropertiesModified()[date];
        for (int i = 0; i < beachProperties.Count; i++) {
            GameObject propertyPrefab;
            switch(beachProperties[i].type) {
                case "bool": propertyPrefab = propertyPrefabs[0]; break;
                case "int": propertyPrefab = propertyPrefabs[1]; break;
                case "string": propertyPrefab = propertyPrefabs[2]; break;
                default: propertyPrefab = null; return;
            }
            GameObject newProperty = Instantiate(propertyPrefab, propertiesContent.transform);
            newProperty.name = SettingsManager.Instance.GetSelectedLanguageIndex() == 0 ? beachProperties[i].descriptions[1] : beachProperties[i].descriptions[0];
            newProperty.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = newProperty.name;
            if (reviewMode == true) {
                int propertyIndex = i;
                if (beachProperties[i].type == "bool") {
                    newProperty.transform.GetChild(1).GetComponent<Image>().color = Color.gray;
                    newProperty.transform.GetChild(2).GetComponent<Image>().color = Color.gray;
                    newProperty.transform.GetChild(1).gameObject.AddComponent<Button>();
                    newProperty.transform.GetChild(2).gameObject.AddComponent<Button>();
                    newProperty.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => ReviewProperty(date, beachIndex, propertyIndex, newProperty, "true"));
                    newProperty.transform.GetChild(2).GetComponent<Button>().onClick.AddListener(() => ReviewProperty(date, beachIndex, propertyIndex, newProperty, "false"));
                    continue;
                } else if(beachProperties[i].type == "int" || beachProperties[i].type == "string") {
                    newProperty.transform.GetChild(1).transform.GetChild(0).gameObject.SetActive(true);
                    newProperty.transform.GetChild(1).GetComponent<TMP_InputField>().readOnly = false;
                    newProperty.transform.GetChild(1).GetComponent<TMP_InputField>().onEndEdit.AddListener((value) => ReviewProperty(date, beachIndex, propertyIndex, newProperty, value));
                    continue;
                }
            } 
            if(beachProperties[i].type == "bool") {
                if (propertiesModified[i] == false) {
                    newProperty.transform.GetChild(1).GetComponent<Image>().color = Color.gray;
                    newProperty.transform.GetChild(2).GetComponent<Image>().color = Color.gray;
                    continue;
                }
                if (beachProperties[i].boolValue == true) {
                    newProperty.transform.GetChild(1).GetComponent<Image>().color = Color.green;
                    newProperty.transform.GetChild(2).GetComponent<Image>().color = Color.gray;
                } else {
                    newProperty.transform.GetChild(2).GetComponent<Image>().color = Color.red;
                    newProperty.transform.GetChild(1).GetComponent<Image>().color = Color.gray;
                }
            } else if(beachProperties[i].type == "int") {
                newProperty.transform.GetChild(1).transform.GetChild(0).gameObject.SetActive(false);
                TextMeshProUGUI textPlaceholder = newProperty.transform.GetChild(1).transform.GetChild(1).transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                if (textPlaceholder == null) return;
                textPlaceholder.text = beachProperties[i].intValue.ToString();
            } else if(beachProperties[i].type == "string") {
                newProperty.transform.GetChild(1).transform.GetChild(0).gameObject.SetActive(false);
                TextMeshProUGUI textPlaceholder = newProperty.transform.GetChild(1).transform.GetChild(1).transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                if (textPlaceholder == null) return;
                textPlaceholder.text = beachProperties[i].stringValue;
            }
        }
        if (reviewMode == false) {
            GameObject reviewCalendar = Instantiate(reviewCalendarPrefab, propertiesContent.transform);
            reviewCalendar.transform.GetChild(1).GetComponent<Button>().onClick.AddListener(() => ReviewButton(currentDate));
            reviewCalendar.transform.GetChild(0).GetComponent<Button>().onClick.AddListener(() => CalendarButton(beaches[beachIndex]));
            if (currentDate.Date == DateTime.Now.Date || currentDate.Date == DateTime.Now.AddDays(-1).Date) {
                reviewCalendar.transform.GetChild(1).gameObject.SetActive(true); // Letting the user review the beach only today and yersterday
            }
            if(propertiesTexting == false) {
                propertiesText.text = propertiesText.text + " " + date;
                propertiesTexting = true;
            }
        }
    }
    private void SelectBeach(int index) { // Opens the selected beach panel
        currentPressedBeach = index;
        ButtonsManager.ToggleObject(scrollViews[1]);
        ButtonsManager.ToggleObject(scrollViews[0]);
        LoadBeachProperties(DateTime.Now, index);
    }
    private void ReviewProperty(string date, int beachIndex, int propertyIndex, GameObject property, string value) { // Updates a selected property based on the user review
        if(beachIndex < 0 || beachIndex >= beaches.Count) {
            Debug.LogError("Beach index is invalid!");
            return;
        }
        if (propertyIndex < 0 || propertyIndex >= beaches[beachIndex].GetComponent<Beach>().GetBeachProperties()[date].Count) {
            Debug.LogError("Property index is invalid!");
            return;
        }
        List<(List<string> descriptions, string type, bool? boolValue, int? intValue, string stringValue)> beachProperties = beaches[beachIndex].GetComponent<Beach>().GetBeachProperties()[date];
        beaches[beachIndex].GetComponent<Beach>().AddOrUpdateProperty(date, beachProperties[propertyIndex].descriptions, beachProperties[propertyIndex].type, value);
        if(beachProperties[propertyIndex].type == "bool") {
            if (value == "true") {
                property.transform.GetChild(1).GetComponent<UnityEngine.UI.Image>().color = Color.green;
                property.transform.GetChild(2).GetComponent<UnityEngine.UI.Image>().color = Color.gray;

            } else {
                property.transform.GetChild(2).GetComponent<UnityEngine.UI.Image>().color = Color.red;
                property.transform.GetChild(1).GetComponent<UnityEngine.UI.Image>().color = Color.gray;
            }
        } else if(beachProperties[propertyIndex].type == "int") {
            if (!int.TryParse(value, out int parsedInt)) {
                Debug.LogError("De facut un mini pop-up");
                return;
            }
        }
    }
    private void ReviewButton(DateTime currentDate) { // Enters review mode for the selected beach
        StartCoroutine(CheckLocationForReview(currentDate));
    }
    private void CalendarButton(GameObject currentBeach) { // Opens the calendar for the selected beach
        ButtonsManager.ToggleObject(beachCalendar);
        calendarManager.GetComponent<CalendarManager>().LoadCalendar(DateTime.Now, currentBeach);
    }
    private void ChangeStatusColor(Image statusImage, float? rank) {
        if (rank == null) {
            statusImage.color = Color.gray;
        }
        statusImage.color = statusColors[Mathf.Clamp(Mathf.FloorToInt((float)rank + 0.5f), 0, 4)];
    }
    public void BackToBeaches() { // Returns to the beach panel and disables review mode
        reviewMode = false;
        string today = DateTime.Now.ToString("dd-MM-yyyy");
        foreach (GameObject beach in beaches) {
            ChangeStatusColor(beach.transform.GetChild(1).GetComponent<Image>(), (float)beach.GetComponent<Beach>().GetRank()[today]);
        }
    }

    /* Getters */

    public static int GetCurrentPressedBeach() { // Returns the index of the currently selected beach
        return currentPressedBeach;
    }

    //* Coroutines *//

    private IEnumerator DownloadImage(string url, Image targetImage) {
        if (string.IsNullOrEmpty(url)) yield break;
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();
        if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError) {
            Debug.LogWarning("Can't download image because: " + request.error);
        } else {
            Texture2D texture = DownloadHandlerTexture.GetContent(request);
            targetImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f)); 
        }
    }
    private IEnumerator CheckLocationForReview(DateTime currentDate) {
        #if UNITY_EDITOR
            reviewMode = true;
            scrollViews[1].transform.GetChild(1).GetComponent<Scrollbar>().value = 1; // Setting the scrollbar back to top
            LoadBeachProperties(currentDate, currentPressedBeach);
            yield break;
        #endif
        if (!Input.location.isEnabledByUser) {
            yield break;
        }
        Input.location.Start(500f, 500f);
        int maxWait = 5;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0) {
            yield return new WaitForSeconds(1);
            maxWait--;
        }
        if (maxWait < 1 || Input.location.status == LocationServiceStatus.Failed) {
            Debug.LogError("No gps found!");
            Input.location.Stop();
            yield break;
        }
        float userLat = Input.location.lastData.latitude;
        float userLon = Input.location.lastData.longitude;
        Input.location.Stop();
        float minLat = 46.4000f; // Cluj coordinates
        float maxLat = 47.4000f;
        float minLon = 22.7000f;
        float maxLon = 24.3000f;
        if (userLat >= minLat && userLat <= maxLat && userLon >= minLon && userLon <= maxLon) {
            reviewMode = true;
            scrollViews[1].transform.GetChild(1).GetComponent<Scrollbar>().value = 1;
            LoadBeachProperties(currentDate, currentPressedBeach);
            
        } else {
            Debug.LogWarning("User is not in the litoral location!");
        }
    }
}