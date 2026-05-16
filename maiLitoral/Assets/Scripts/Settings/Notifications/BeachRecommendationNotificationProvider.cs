using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BeachRecommendationNotificationProvider : MonoBehaviour {
    /* Attributes */

    [SerializeField] private string quietPropertyName = "Quiet"; // Database property name used for quiet beach recommendations
    [SerializeField] private string familyPropertyName = "Family"; // Database property name used for family beach recommendations

    /* Custom Methods */

    public List<AppNotification> CreateRecommendationNotifications(List<NotificationType> selectedTypes) { // Creates recommendation notifications only for selected options
        List<AppNotification> notifications = new List<AppNotification>(); // Stores all generated notifications
        string today = DateTime.Now.ToString("dd-MM-yyyy"); // Uses today's date to read daily beach data
        Dictionary<NotificationType, Func<AppNotification>> notificationCreators = new Dictionary<NotificationType, Func<AppNotification>> {
            { NotificationType.PopularBeaches, () => CreatePopularBeachesNotification(today) }, // Maps popular beaches option to its creator
            { NotificationType.QuietBeaches, () => CreatePropertyBasedNotification(NotificationType.QuietBeaches, today, quietPropertyName, "Plaje linistite", "Iti recomandam aceste plaje linistite: ") }, // Maps quiet beaches option to its creator
            { NotificationType.FamilyBeaches, () => CreatePropertyBasedNotification(NotificationType.FamilyBeaches, today, familyPropertyName, "Plaje pentru familii", "Iti recomandam aceste plaje potrivite pentru familii: ") } // Maps family beaches option to its creator
        }; // Keeps logic extensible without if chains
        foreach (NotificationType selectedType in selectedTypes) { // Goes through all options selected by the user
            if (!notificationCreators.ContainsKey(selectedType)) {
                continue; // Skips selected types that are not recommendation notifications
            }
            AppNotification notification = notificationCreators[selectedType].Invoke(); // Creates the notification for the selected type
            if (notification == null) {
                continue; // Skips empty notifications when no matching beaches exist
            }
            notifications.Add(notification); // Adds the valid notification to the final list
        }
        return notifications; // Returns all generated recommendation notifications
    }
    private AppNotification CreatePopularBeachesNotification(string date) { // Creates a notification based on beach rank
        List<(string name, float rank)> beaches = DatabaseManager.Instance.GetTop3BeachesByRank(date); // Gets the top ranked beaches from the database
        if (beaches.Count == 0) {
            return null; // Stops if there are no beaches in the database
        }
        string beachNames = string.Join(", ", beaches.Select(beach => beach.name)); // Builds a readable beach list
        return new AppNotification(NotificationType.PopularBeaches, "Plaje populare", "Cele mai populare plaje azi: " + beachNames); // Returns the popular beaches notification
    }
    private AppNotification CreatePropertyBasedNotification(NotificationType notificationType, string date, string propertyName, string title, string messagePrefix) { // Creates a notification based on a boolean beach property
        List<string> beaches = DatabaseManager.Instance.GetBeachesByActiveBoolProperty(date, propertyName); // Gets beaches where the property is active
        if (beaches.Count == 0) {
            return null; // Stops if no beaches match the selected property
        }
        string beachNames = string.Join(", ", beaches.Take(3)); // Keeps the notification short and readable
        return new AppNotification(notificationType, title, messagePrefix + beachNames); // Returns the property based notification
    }
}