using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class WeatherNotificationProvider : MonoBehaviour {
    /* Attributes */

    private const string WeatherUrl = "https://api.open-meteo.com/v1/forecast?latitude=44.18&longitude=28.65&current=temperature_2m,wind_speed_10m,precipitation&timezone=auto"; // Open-Meteo forecast API URL for Constanta/Romanian seaside area

    /* Serializable classes */

    [System.Serializable]
    private class WeatherResponse {
        public CurrentWeather current; // Current weather object returned by the API
    }
    [System.Serializable]
    private class CurrentWeather {
        public float temperature_2m; // Current temperature in Celsius
        public float wind_speed_10m; // Current wind speed in km/h
        public float precipitation; // Current precipitation value
    }

    /* Custom Methods */

    private bool IsEnglishSelected() { // Checks if the selected app language is English
        return PlayerPrefs.GetInt("selected_language", 0) == 1; // English uses index 1 in the language settings
    }
    public IEnumerator CreateWeatherNotification(System.Action<AppNotification> onNotificationCreated) { // Creates a weather notification using real API data
        UnityWebRequest request = UnityWebRequest.Get(WeatherUrl); // Creates a GET request to the weather API
        yield return request.SendWebRequest(); // Waits until the API request finishes
        if (request.result != UnityWebRequest.Result.Success) {
            yield break; // Stops safely if the weather API request failed
        }
        WeatherResponse weatherResponse = JsonUtility.FromJson<WeatherResponse>(request.downloadHandler.text); // Converts the JSON response into C# objects
        if (weatherResponse == null || weatherResponse.current == null) {
            yield break; // Stops safely if the response could not be parsed
        }
        string title = IsEnglishSelected() ? "Seaside weather update" : "Actualizare meteo litoral"; // Chooses the title based on the selected language
        string message = IsEnglishSelected()
            ? "The seaside temperature is " +
              weatherResponse.current.temperature_2m +
              "°C, the wind speed is " +
              weatherResponse.current.wind_speed_10m +
              " km/h, and precipitation is " +
              weatherResponse.current.precipitation +
              " mm."
            : "Temperatura pe litoral este de " +
              weatherResponse.current.temperature_2m +
              "°C, vantul are " +
              weatherResponse.current.wind_speed_10m +
              " km/h, iar precipitatii: " +
              weatherResponse.current.precipitation +
              " mm."; // Builds the weather notification message based on the selected language
        AppNotification notification = new AppNotification(NotificationType.WeatherNews, title, message); // Creates the weather notification object
        onNotificationCreated?.Invoke(notification); // Sends the created notification back to the manager
    }
}