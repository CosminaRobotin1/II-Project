using UnityEngine;

public class LanguageSelectionManager : MonoBehaviour {
    [SerializeField] private GameObject romanianCheck;
    [SerializeField] private GameObject englishCheck;

    // Select Romanian language
    public void SelectRomanian() {
        if (romanianCheck == null || englishCheck == null) {
            return;
        }

        bool isActive = romanianCheck.activeSelf;

        if (isActive) {
            romanianCheck.SetActive(false);
        } else {
            romanianCheck.SetActive(true);
            englishCheck.SetActive(false);
        }
    }

    // Select English language
    public void SelectEnglish() {
        if (romanianCheck == null || englishCheck == null) {
            return;
        }

        bool isActive = englishCheck.activeSelf;

        if (isActive) {
            englishCheck.SetActive(false);
        } else {
            romanianCheck.SetActive(false);
            englishCheck.SetActive(true);
        }
    }
}