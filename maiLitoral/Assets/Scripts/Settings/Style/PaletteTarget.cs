using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum PaletteColorRole {
    Light = 0,
    Soft = 1,
    Medium = 2,
    Accent = 3,
    Dark = 4
}

public class PaletteTarget : MonoBehaviour {

    /* Attributes */

    [SerializeField] private Image targetImage; // Image component affected by palette and theme changes
    [SerializeField] private TMP_Text targetText; // Text component affected by palette and theme changes
    [SerializeField] private PaletteColorRole colorRole; // Role that decides which palette color this object uses
    [SerializeField] private bool preserveAlpha = true; // Keeps the original transparency of this UI element

    /* Properties */

    public PaletteColorRole ColorRole => colorRole; // Gives the manager access to the selected color role

    /* Custom Methods */

    private void OnEnable() { // Applies the current settings when this object becomes active
        if (SettingsManager.Instance != null) {
            SettingsManager.Instance.ApplyCurrentSettingsToTarget(this);
        }
    }
    public void ApplyColor(Color newColor) { // Applies the received color to the assigned Image and TMP_Text component
        if (targetImage != null) {
            if (preserveAlpha) {
                newColor.a = targetImage.color.a; // Keeps the original image transparency
            }
            targetImage.color = newColor;
        }
        if (targetText != null) {
            if (preserveAlpha) {
                newColor.a = targetText.color.a; // Keeps the original text transparency
            }
            targetText.color = newColor;
        }
    }
}