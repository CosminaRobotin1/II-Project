using UnityEngine;

public class NotificationsSelectionManager : MonoBehaviour {
    [SerializeField] private GameObject notificationsPanel;
    [SerializeField] private GameObject beachRecommendationsPanel;
    [SerializeField] private GameObject newsUpdatesPanel;
    [SerializeField] private GameObject seasonalEventsPanel;

    // Open recommendations panel
    public void OpenRecommendationsPanel() {
        notificationsPanel.SetActive(false);
        beachRecommendationsPanel.SetActive(true);
        newsUpdatesPanel.SetActive(false);
        seasonalEventsPanel.SetActive(false);
    }

    // Open news panel
    public void OpenNewsPanel() {
        notificationsPanel.SetActive(false);
        beachRecommendationsPanel.SetActive(false);
        newsUpdatesPanel.SetActive(true);
        seasonalEventsPanel.SetActive(false);
    }

    // Open events panel
    public void OpenEventsPanel() {
        notificationsPanel.SetActive(false);
        beachRecommendationsPanel.SetActive(false);
        newsUpdatesPanel.SetActive(false);
        seasonalEventsPanel.SetActive(true);
    }

    // Back to notifications panel
    public void BackToNotificationsPanel() {
        notificationsPanel.SetActive(true);
        beachRecommendationsPanel.SetActive(false);
        newsUpdatesPanel.SetActive(false);
        seasonalEventsPanel.SetActive(false);
    }
}