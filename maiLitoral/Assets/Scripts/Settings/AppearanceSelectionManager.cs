using UnityEngine;

public class AppearanceSelectionManager : MonoBehaviour {
    [SerializeField] private GameObject paletteSelection;
    [SerializeField] private GameObject themeSelection;
    [SerializeField] private GameObject appearancePanel;

    // Toggle palette panel
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

    // Toggle theme panel
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

    // Handle back navigation
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
