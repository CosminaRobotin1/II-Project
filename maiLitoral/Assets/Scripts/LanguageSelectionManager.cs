using UnityEngine;

public class LanguageSelectionManager : MonoBehaviour {
    public GameObject romanianCheck;
    public GameObject englishCheck;

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