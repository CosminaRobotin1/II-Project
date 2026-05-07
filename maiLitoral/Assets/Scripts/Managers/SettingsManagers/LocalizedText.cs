using TMPro;
using UnityEngine;

public class LocalizedText : MonoBehaviour {
    /* Attributes */

    [SerializeField] private TMP_Text targetText; // Text component that will be updated when the language changes
    [SerializeField] private string romanianText; // Romanian version of this UI text
    [SerializeField] private string englishText; // English version of this UI text

    /* Unity method */

    // Applies the current language when the object becomes active
    private void OnEnable() {
        if (SettingsOptionsManager.Instance != null) {
            SettingsOptionsManager.Instance.ApplyCurrentLanguageToText(this);
        }
    }

    /* Language method */

    // Applies the selected language to this specific text element
    public void ApplyLanguage(int languageIndex) {
        if (targetText == null) {
            return;
        }
        if (languageIndex == 0) {
            targetText.text = romanianText;
        } else {
            targetText.text = englishText;
        }
    }
}