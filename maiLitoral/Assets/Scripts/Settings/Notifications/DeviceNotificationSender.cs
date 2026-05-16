using System;
using UnityEngine;

#if UNITY_ANDROID
    using Unity.Notifications.Android;
#endif

public class DeviceNotificationSender : MonoBehaviour {
    /* Attributes */

    private const string ChannelId = "mailitoral_notifications"; // Unique Android notification channel id used by the app
    private const string ChannelName = "maiLitoral Notifications"; // Visible Android channel name for app notifications
    private const string ChannelDescription = "Notifications for beach recommendations, beach updates and weather news."; // Android channel description

    /* Main Methods */

    private void Awake() { // Prepares the device notification system when the manager is created
        InitializeNotificationChannel(); // Creates the Android notification channel used by all app notifications
    }

    /* Custom Methods */

    private void InitializeNotificationChannel() { // Initializes platform specific notification settings
        #if UNITY_ANDROID
            AndroidNotificationChannel channel = new AndroidNotificationChannel(); // Creates a new Android notification channel structure
            channel.Id = ChannelId; // Sets the internal channel id
            channel.Name = ChannelName; // Sets the channel name visible in Android settings
            channel.Importance = Importance.Default; // Sets normal notification importance
            channel.Description = ChannelDescription; // Sets the channel description visible in Android settings
            AndroidNotificationCenter.RegisterNotificationChannel(channel); // Registers the channel before any notification is sent
        #endif
    }

    public void SendNotification(AppNotification appNotification) { // Sends one prepared notification to the user device
        if (appNotification == null) {
            return; // Prevents sending an invalid notification
        }
        #if UNITY_ANDROID
            AndroidNotification androidNotification = new AndroidNotification(); // Creates the Android notification content
            androidNotification.Title = appNotification.Title; // Sets the notification title
            androidNotification.Text = appNotification.Message; // Sets the notification message
            androidNotification.FireTime = DateTime.Now.AddSeconds(5); // Sends the notification shortly after it is created
            AndroidNotificationCenter.SendNotification(androidNotification, ChannelId); // Sends the notification through the registered channel
        #else
            Debug.Log("Notification: " + appNotification.Title + " - " + appNotification.Message); // Keeps testing possible inside the Unity Editor
        #endif
    }
}