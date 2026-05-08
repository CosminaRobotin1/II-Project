using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LocalizedText : MonoBehaviour {

    /* Attributes */

    [SerializeField] private TMP_Text targetText; // Text component that will be updated when the language changes
    [SerializeField] private List<string> languages; // App languages
    [SerializeField] private string romanianText; // Romanian version of this UI text
    [SerializeField] private string englishText; // English version of this UI text

    /* Custom method */

    private void OnEnable() { // Applies the current language when the object becomes active
        if (SettingsManager.Instance != null) {
            SettingsManager.Instance.ApplyCurrentLanguageToText(this);
        }
    }
    public void ApplyLanguage(int languageIndex) { // Applies the selected language to this specific text element
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