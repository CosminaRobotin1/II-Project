public class AppNotification {
    /* Attributes */

    public NotificationType NotificationType { get; private set; } // Type of notification represented by this message
    public string Title { get; private set; } // Notification title sent to the device
    public string Message { get; private set; } // Notification body sent to the device

    /* Constructors */

    public AppNotification(NotificationType notificationType, string title, string message) { // Creates one reusable notification message
        NotificationType = notificationType; // Stores the notification type
        Title = title; // Stores the notification title
        Message = message; // Stores the notification body
    }
}