using UnityEngine;

public class SettingsNavigationManager : MonoBehaviour {
    public GameObject settingsPanel;
    public GameObject optionsContainer;

    public GameObject appearancePanel;
    public GameObject popupColors;
    public GameObject popupThemes;
    public GameObject appearanceOptionsContainer;

    public GameObject languagePanel;
    public GameObject notificationsPanel;
    public GameObject aboutPanel;

    public void OpenSettingsPanel() {
        if (settingsPanel != null) {
            settingsPanel.SetActive(true);
        }

        if (optionsContainer != null) { 
            optionsContainer.SetActive(true);
        }

        if (appearancePanel != null) { 
            appearancePanel.SetActive(false);
        }

        if (popupColors != null) {
            popupColors.SetActive(false);
        }

        if (popupThemes != null) {
            popupThemes.SetActive(false);
        }

        if (languagePanel != null) { 
            languagePanel.SetActive(false);
        }

        if (notificationsPanel != null) { 
            notificationsPanel.SetActive(false);
        }

        if (aboutPanel != null) {  
            aboutPanel.SetActive(false);
        }
    }

    public void CloseSettingsPanel() {
        if (settingsPanel != null) {
            settingsPanel.SetActive(false);
        }
    }

    public void OpenAppearancePanel() {
        if (optionsContainer != null) {
            optionsContainer.SetActive(false);
        }

        if (appearancePanel != null) {
            appearancePanel.SetActive(true);
        }

        if (appearanceOptionsContainer != null) {
            appearanceOptionsContainer.SetActive(true);
        }

        if (popupColors != null) {
            popupColors.SetActive(false);
        }

        if (popupThemes != null) {
            popupThemes.SetActive(false);
        }

        if (languagePanel != null) { 
            languagePanel.SetActive(false);
        }

        if (notificationsPanel != null) { 
            notificationsPanel.SetActive(false);
        }

        if (aboutPanel != null) { 
            aboutPanel.SetActive(false);
        }
    }

    public void OpenPopupColors() {
        if (appearanceOptionsContainer != null) {
            appearanceOptionsContainer.SetActive(false);
        }

        if (popupColors != null) {
            popupColors.SetActive(true);
        }

        if (popupThemes != null) {
            popupThemes.SetActive(false);
        }
    }

    public void OpenPopupThemes() {
        if (appearanceOptionsContainer != null) {
            appearanceOptionsContainer.SetActive(false);
        }

        if (popupThemes != null) {
            popupThemes.SetActive(true);
        }

        if (popupColors != null) {
            popupColors.SetActive(false);
        }
    }

    public void OpenLanguagePanel() {
        if (optionsContainer != null) {
            optionsContainer.SetActive(false);
        }

        if (languagePanel != null) {
            languagePanel.SetActive(true);
        }

        if (appearancePanel != null) {
            appearancePanel.SetActive(false);
        }

        if (notificationsPanel != null) {
            notificationsPanel.SetActive(false);
        }

        if (aboutPanel != null) {
            aboutPanel.SetActive(false);
        }
    }

    public void OpenNotificationsPanel() {
        if (optionsContainer != null) {
            optionsContainer.SetActive(false);
        }

        if (notificationsPanel != null) {
            notificationsPanel.SetActive(true);
        }

        if (appearancePanel != null) {
            appearancePanel.SetActive(false);
        }

        if (languagePanel != null) {
            languagePanel.SetActive(false);
        }

        if (aboutPanel != null) {
            aboutPanel.SetActive(false);
        }
    }

    public void OpenAboutPanel() {
        if (optionsContainer != null) {
            optionsContainer.SetActive(false);
        }

        if (aboutPanel != null) {
            aboutPanel.SetActive(true);
        }

        if (appearancePanel != null) {
            appearancePanel.SetActive(false);
        }

        if (languagePanel != null) {
            languagePanel.SetActive(false);
        }

        if (notificationsPanel != null) {
            notificationsPanel.SetActive(false);
        }
    }

    public void BackFromAppearance() {
        if (popupColors != null) {
            popupColors.SetActive(false);
        }

        if (popupThemes != null) {
            popupThemes.SetActive(false);
        }

        if (appearanceOptionsContainer != null) {
            appearanceOptionsContainer.SetActive(true);
        }

        if (appearancePanel != null) {
            appearancePanel.SetActive(false);
        }

        if (optionsContainer != null) {
            optionsContainer.SetActive(true);
        }
    }

    public void BackFromPopupColors() {
        if (popupColors != null) {
            popupColors.SetActive(false);
        }

        if (appearanceOptionsContainer != null) {
            appearanceOptionsContainer.SetActive(true);
        }
    }

    public void BackFromPopupThemes() {
        if (popupThemes != null) {
            popupThemes.SetActive(false);
        }

        if (appearanceOptionsContainer != null) {
            appearanceOptionsContainer.SetActive(true);
        }
    }

    public void BackFromLanguage() {
        if (languagePanel != null) {
            languagePanel.SetActive(false);
        }

        if (optionsContainer != null) {
            optionsContainer.SetActive(true);
        }
    }

    public void BackFromNotifications() {
        if (notificationsPanel != null) {
            notificationsPanel.SetActive(false);
        }

        if (optionsContainer != null) {
            optionsContainer.SetActive(true);
        }
    }

    public void BackFromAbout() {
        if (aboutPanel != null) {
            aboutPanel.SetActive(false);
        }

        if (optionsContainer != null) {
            optionsContainer.SetActive(true);
        }
    }
}
