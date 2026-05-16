using UnityEngine;
using UnityEngine.UI;

public class NotificationToggleButton : MonoBehaviour {
    /* Attributes */

    [SerializeField] private NotificationType notificationType; // Type of notification controlled by this reusable toggle button
    [SerializeField] private Button button; // Button component used to detect the user click
    [SerializeField] private GameObject selectedCheck; // Visual selected mark displayed when this notification is active

    /* Main Methods */

    private void OnEnable() { // Registers this button inside the notifications manager when the object becomes active
        if (NotificationsManager.Instance == null) {
            return; // Stops safely if the notifications manager does not exist yet
        }
        NotificationsManager.Instance.RegisterButton(this); // Adds this button to the manager reusable buttons list
    }
    private void OnDisable() { // Removes this button from the notifications manager when the object becomes inactive
        if (NotificationsManager.Instance == null) {
            return; // Stops safely if the manager was already destroyed
        }
        NotificationsManager.Instance.UnregisterButton(this); // Removes this button from the active buttons list
    }

    /* Custom Methods */

    public void Initialize() { // Connects the button click event only once
        if (button == null) {
            button = GetComponent<Button>(); // Automatically gets the button component from the same object if it was not assigned manually
            return;
        }
        button.onClick.RemoveListener(ToggleSelection); // Prevents duplicated listeners after scene reloads or object reactivation
        button.onClick.AddListener(ToggleSelection); // Connects the reusable toggle logic to this button click
    }
    private void ToggleSelection() { // Sends the selection request to the notifications manager
        if (NotificationsManager.Instance == null) {
            return; // Stops safely if the manager is missing
        }
        NotificationsManager.Instance.ToggleNotification(notificationType); // Toggles the current notification type without hardcoded logic
    }
    public void SetSelected(bool isSelected) { // Updates the selected mark visibility
        if (selectedCheck == null) {
            return; // Prevents errors if the selected check reference is missing
        }
        selectedCheck.SetActive(isSelected); // Shows or hides the selected mark depending on the current state
    }
    public NotificationType GetNotificationType() { // Returns the notification type configured inside the Inspector
        return notificationType;
    }
}