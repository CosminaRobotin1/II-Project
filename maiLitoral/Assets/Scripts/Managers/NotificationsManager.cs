using System;
using System.Collections.Generic;
using UnityEngine;

public class NotificationsManager : MonoBehaviour {
    /* Attributes */

    private const string NotificationPrefix = "notification_"; // Prefix used for all locally saved notification keys
    private readonly List<NotificationToggleButton> notificationButtons = new List<NotificationToggleButton>(); // List with all notification toggle buttons found in the active scene
    private readonly Dictionary<NotificationType, bool> selectedNotifications = new Dictionary<NotificationType, bool>(); // Dictionary that stores the selected state for each notification type
    [SerializeField] private DeviceNotificationSender notificationSender; // Component responsible for sending notifications to the user device
    [SerializeField] private BeachRecommendationNotificationProvider recommendationProvider; // Component responsible for creating beach recommendation notifications
    [SerializeField] private BeachUpdatesNotificationProvider updatesProvider; // Component responsible for creating beach updates notifications
    [SerializeField] private WeatherNotificationProvider weatherProvider; // Component responsible for creating weather notifications from the weather API
    public static NotificationsManager Instance { get; private set; } // Global access to the active notifications manager

    /* Main Methods */

    private void Awake() { // Initializes the notifications manager only once
        if (Instance != null && Instance != this) {
            Destroy(gameObject); // Prevents duplicated managers after scene reloads
            return; // Stops the duplicated manager initialization
        }
        Instance = this; // Stores the active manager instance
        DontDestroyOnLoad(gameObject); // Keeps the manager active between scene changes
        LoadSavedNotifications(); // Loads saved notification options from local device storage
    }
    private void Start() { // Refreshes all registered buttons after the scene is initialized
        RefreshAllButtons(); // Updates the selected marks based on saved data
    }

    /* Custom Methods */

    public void RegisterButton(NotificationToggleButton notificationButton) { // Registers one reusable notification toggle button
        if (notificationButton == null) {
            return; // Prevents invalid button references
        }
        if (!notificationButtons.Contains(notificationButton)) {
            notificationButtons.Add(notificationButton); // Adds the button only once
        }
        notificationButton.Initialize(); // Connects the button click to its toggle logic
        notificationButton.SetSelected(IsNotificationSelected(notificationButton.GetNotificationType())); // Applies the saved selected state visually
    }
    public void UnregisterButton(NotificationToggleButton notificationButton) { // Removes one reusable button from the active list
        if (notificationButton == null) {
            return; // Prevents invalid button references
        }
        notificationButtons.Remove(notificationButton); // Removes the button when it becomes inactive
    }
    public void ToggleNotification(NotificationType notificationType) { // Toggles one notification type without hardcoding specific options
        bool newSelectedState = !IsNotificationSelected(notificationType); // Reverses the current selected state
        selectedNotifications[notificationType] = newSelectedState; // Updates the internal state dictionary
        PlayerPrefs.SetInt(GetStorageKey(notificationType), Convert.ToInt32(newSelectedState)); // Saves the selected state locally as 1 or 0
        PlayerPrefs.Save(); // Forces Unity to save the updated preferences
        RefreshAllButtons(); // Updates all selected marks after the change
    }
    public bool IsSelected(NotificationType notificationType) { // Checks if one notification type is selected by the user
        return IsNotificationSelected(notificationType); // Reuses the internal saved state check
    }
    public List<NotificationType> GetSelectedNotificationTypes() { // Returns all notification types selected by the user
        List<NotificationType> selectedTypes = new List<NotificationType>(); // Stores only the selected notification types
        foreach (KeyValuePair<NotificationType, bool> notificationState in selectedNotifications) {
            if (!notificationState.Value) {
                continue; // Skips notification types that are not selected
            }
            selectedTypes.Add(notificationState.Key); // Adds the selected notification type to the result list
        }
        return selectedTypes; // Returns all selected notification types
    }
    public void SendSelectedNotifications() { // Sends only the notifications selected by the user
        List<NotificationType> selectedTypes = GetSelectedNotificationTypes(); // Gets the currently selected notification options
        if (recommendationProvider != null) {
            List<AppNotification> recommendationNotifications = recommendationProvider.CreateRecommendationNotifications(selectedTypes); // Creates selected recommendation notifications
            foreach (AppNotification notification in recommendationNotifications) {
                SendNotification(notification); // Sends each created recommendation notification
            }
        }
        if (IsSelected(NotificationType.BeachState) && updatesProvider != null) {
            SendNotification(updatesProvider.CreateBeachStateNotification()); // Sends beach state notification only if selected
        }
        if (IsSelected(NotificationType.WeatherNews) && weatherProvider != null) {
            StartCoroutine(weatherProvider.CreateWeatherNotification(SendNotification)); // Creates and sends weather notification only if selected
        }
    }
    public void SendNotification(AppNotification appNotification) { // Sends one prepared notification through the device sender
        if (appNotification == null) {
            return; // Stops safely if no notification was created
        }
        if (notificationSender == null) {
            return; // Stops safely if the sender component was not assigned
        }
        notificationSender.SendNotification(appNotification); // Sends the notification to the device
    }
    private void LoadSavedNotifications() { // Loads all notification options from local device storage
        foreach (NotificationType notificationType in Enum.GetValues(typeof(NotificationType))) {
            selectedNotifications[notificationType] = PlayerPrefs.GetInt(GetStorageKey(notificationType), 0) == 1; // Loads each option as selected or not selected
        }
    }
    private void RefreshAllButtons() { // Updates the selected mark for every registered toggle button
        foreach (NotificationToggleButton notificationButton in notificationButtons) {
            if (notificationButton == null) {
                continue; // Ignores missing or destroyed button references
            }
            notificationButton.SetSelected(IsNotificationSelected(notificationButton.GetNotificationType())); // Applies the saved selected state visually
        }
    }
    private bool IsNotificationSelected(NotificationType notificationType) { // Checks the saved selected state for one notification type
        return selectedNotifications.ContainsKey(notificationType) && selectedNotifications[notificationType]; // Returns true only if the type exists and is selected
    }
    private string GetStorageKey(NotificationType notificationType) { // Builds a local storage key for one notification type
        return NotificationPrefix + notificationType; // Example: notification_PopularBeaches
    }

    [ContextMenu("Test Selected Notifications")] // Allows testing selected notifications from the Inspector
    private void TestSelectedNotifications() {
        SendSelectedNotifications(); // Sends all currently selected notifications for testing
    }
}