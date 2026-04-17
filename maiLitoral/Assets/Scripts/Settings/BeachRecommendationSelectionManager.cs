using UnityEngine;

public class BeachRecommendationSelectionManager : MonoBehaviour {
    [SerializeField] private GameObject popularCheck;
    [SerializeField] private GameObject quietCheck;
    [SerializeField] private GameObject familyCheck;

    [SerializeField] private string selectedRecommendation;

    private const string RecommendationKey = "BeachRecommendationType";

    private void Start() {
        selectedRecommendation = PlayerPrefs.GetString(RecommendationKey, "");
        UpdateChecks();
    }

    // Select popular beaches
    public void SelectPopular() {
        bool isAlreadySelected = popularCheck.activeSelf;
        if (isAlreadySelected) {
            selectedRecommendation = "";
            SaveSelection();
            UpdateChecks();
            return;
        }
        selectedRecommendation = "Popular";
        SaveSelection();
        UpdateChecks();
    }

    // Select quiet beaches
    public void SelectQuiet() {
        bool isAlreadySelected = quietCheck.activeSelf;
        if (isAlreadySelected) {
            selectedRecommendation = "";
            SaveSelection();
            UpdateChecks();
            return;
        }
        selectedRecommendation = "Quiet";
        SaveSelection();
        UpdateChecks();
    }

    // Select family beaches
    public void SelectFamily() {
        bool isAlreadySelected = familyCheck.activeSelf;
        if (isAlreadySelected) {
            selectedRecommendation = "";
            SaveSelection();
            UpdateChecks();
            return;
        }
        selectedRecommendation = "Family";
        SaveSelection();
        UpdateChecks();
    }

    // Save current selection
    private void SaveSelection() {
        PlayerPrefs.SetString(RecommendationKey, selectedRecommendation);
        PlayerPrefs.Save();
    }

    // Update check visibility
    private void UpdateChecks() {
        popularCheck.SetActive(selectedRecommendation == "Popular");
        quietCheck.SetActive(selectedRecommendation == "Quiet");
        familyCheck.SetActive(selectedRecommendation == "Family");
    }

    // Get current selection
    public string GetSelectedRecommendation() {
        return selectedRecommendation;
    }

    // Check if notifications are active
    public bool AreNotificationsEnabled() {
        return !string.IsNullOrEmpty(selectedRecommendation);
    }
}