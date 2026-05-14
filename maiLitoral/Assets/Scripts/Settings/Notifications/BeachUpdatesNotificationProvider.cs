using System.Collections.Generic;
using UnityEngine;

public class BeachUpdatesNotificationProvider : MonoBehaviour {
    /* Attributes */

    [SerializeField] private int maximumItems = 3; // Maximum number of new beaches or properties shown in one notification

    /* Custom Methods */

    public AppNotification CreateBeachStateNotification() { // Creates notification about newly added beaches and properties
        List<string> beaches = DatabaseManager.Instance.GetLatestBeachNames(maximumItems); // Gets latest added beach names
        List<string> properties = DatabaseManager.Instance.GetLatestPropertyNames(maximumItems); // Gets latest added property names
        if (beaches.Count == 0 && properties.Count == 0) {
            return null; // Stops if there is no database content to announce
        }
        string message = "Noutati plaje: " + string.Join(", ", beaches) + ". Proprietati noi: " + string.Join(", ", properties) + "."; // Builds the update message
        return new AppNotification(NotificationType.BeachState, "Starea plajelor", message); // Returns the beach state notification
    }
}