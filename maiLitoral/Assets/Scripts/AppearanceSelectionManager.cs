using UnityEngine;

public class AppearanceSelectionManager : MonoBehaviour {
    public GameObject paletteSelection;
    public GameObject themeSelection;
    public GameObject appearancePanel;

    public void TogglePaletteSelection() {
        if (paletteSelection == null || themeSelection == null) {
            return;
        }

        bool isPaletteActive = paletteSelection.activeSelf;

        if (isPaletteActive) {
            paletteSelection.SetActive(false);
        } else {
            paletteSelection.SetActive(true);
            themeSelection.SetActive(false);
        }
    }

    public void ToggleThemeSelection() {
        if (paletteSelection == null || themeSelection == null) {
            return;
        }

        bool isThemeActive = themeSelection.activeSelf;

        if (isThemeActive) {
            themeSelection.SetActive(false);
        } else {
            themeSelection.SetActive(true);
            paletteSelection.SetActive(false);
        }
    }

    public void HandleBackButton() {
        if (paletteSelection != null && paletteSelection.activeSelf) {
            paletteSelection.SetActive(false);
            return;
        }

        if (themeSelection != null && themeSelection.activeSelf) {
            themeSelection.SetActive(false);
            return;
        }

        if (appearancePanel != null) { 
            appearancePanel.SetActive(false);
        }
    }
}
