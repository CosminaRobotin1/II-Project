using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsOptionsManager : MonoBehaviour {
    /* Serializable classes */

    [Serializable]
    private class ColorPalette {
        [SerializeField] private string paletteName; // Name used only for Inspector readability
        [SerializeField] private Color lightColor; // Color used for main light backgrounds
        [SerializeField] private Color softColor; // Color used for panels, cards and soft containers
        [SerializeField] private Color mediumColor; // Color used for secondary UI elements
        [SerializeField] private Color accentColor; // Color used for important buttons and highlights
        [SerializeField] private Color darkColor; // Color used for titles, text and strong details

        // Returns the color that matches the requested palette role
        public Color GetColor(PaletteColorRole colorRole) {
            switch (colorRole) {
                case PaletteColorRole.Light:
                    return lightColor;
                case PaletteColorRole.Soft:
                    return softColor;
                case PaletteColorRole.Medium:
                    return mediumColor;
                case PaletteColorRole.Accent:
                    return accentColor;
                case PaletteColorRole.Dark:
                    return darkColor;
                default:
                    return lightColor; // Safe fallback for unexpected role values
            }
        }
    }

    [Serializable]
    private class ThemeMode {
        [SerializeField] private string themeName; // Name used only for Inspector readability
        [SerializeField] private float brightnessMultiplier = 1f; // Controls how bright or dark the theme appears

        // Applies the theme brightness over a palette color
        public Color ApplyTheme(Color color) {
            color.r = Mathf.Clamp01(color.r * brightnessMultiplier);
            color.g = Mathf.Clamp01(color.g * brightnessMultiplier);
            color.b = Mathf.Clamp01(color.b * brightnessMultiplier);
            return color;
        }
    }

    /* Attributes */

    private const string PaletteKey = "selected_palette"; // Key used to save the selected palette locally
    private const string ThemeKey = "selected_theme"; // Key used to save the selected theme locally
    private const string LanguageKey = "selected_language"; // Key used to save the selected language locally

    [SerializeField] private ColorPalette defaultPalette; // Default palette that matches the original team leader design
    [SerializeField] private int defaultThemeIndex = 0; // Default theme used when no theme was selected
    [SerializeField] private int defaultLanguageIndex = 0; // Default language used when no language was selected

    private List<OptionButton> paletteButtons = new List<OptionButton>(); // Palette buttons found automatically in the active scene
    private List<OptionButton> themeButtons = new List<OptionButton>(); // Theme buttons found automatically in the active scene
    private List<OptionButton> languageButtons = new List<OptionButton>(); // Language buttons found automatically in the active scene

    [SerializeField] private List<ColorPalette> colorPalettes = new List<ColorPalette>(); // All available color palettes
    [SerializeField] private List<ThemeMode> themeModes = new List<ThemeMode>(); // All available theme modes

    private PaletteTarget[] paletteTargets; // All UI elements affected by palette and theme changes
    private LocalizedText[] localizedTexts; // All UI texts affected by language changes

    private int selectedPaletteIndex; // Currently selected palette index
    private int selectedThemeIndex; // Currently selected theme index
    private int selectedLanguageIndex; // Currently selected language index

    private bool hasSavedPalette; // True if the user selected a palette before
    private bool hasSavedTheme; // True if the user selected a theme before
    private bool hasSavedLanguage; // True if the user selected a language before

    public static SettingsOptionsManager Instance { get; private set; } // Global access to the active settings manager

    /* Unity methods */

    // Initializes the manager and keeps it alive between scenes
    private void Awake() {
        if (Instance != null && Instance != this) {
            Destroy(gameObject); // Prevents duplicated settings managers after scene reloads
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Keeps settings active between StartingPage and BeachPage !!!
        LoadOptions();
        FindTargets();
        ApplySavedOptions();
    }

    // Registers the scene loading callback
    private void OnEnable() {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    // Unregisters the scene loading callback
    private void OnDisable() {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // Reconnects UI elements after a new scene was loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) {
        FindTargets();
        ApplySavedOptions();
    }

    /* Initialization methods */

    // Finds all palette, language and option targets in the active scene
    private void FindTargets() {
        paletteTargets = FindObjectsByType<PaletteTarget>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        localizedTexts = FindObjectsByType<LocalizedText>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        paletteButtons.Clear();
        themeButtons.Clear();
        languageButtons.Clear();

        OptionButton[] optionButtons = FindObjectsByType<OptionButton>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (OptionButton optionButton in optionButtons) {
            if (optionButton == null) {
                continue; // Ignores empty references safely
            }

            // Groups buttons automatically by their configured type
            if (optionButton.ButtonType == OptionButtonType.Palette) {
                paletteButtons.Add(optionButton);
            } else if (optionButton.ButtonType == OptionButtonType.Theme) {
                themeButtons.Add(optionButton);
            } else if (optionButton.ButtonType == OptionButtonType.Language) {
                languageButtons.Add(optionButton);
            }
        }
        SortOptionButtons(paletteButtons);
        SortOptionButtons(themeButtons);
        SortOptionButtons(languageButtons);
        InitializeButtons();
    }

    // Loads saved options from local device storage
    private void LoadOptions() {
        hasSavedPalette = PlayerPrefs.HasKey(PaletteKey);
        hasSavedTheme = PlayerPrefs.HasKey(ThemeKey);
        hasSavedLanguage = PlayerPrefs.HasKey(LanguageKey);

        selectedPaletteIndex = PlayerPrefs.GetInt(PaletteKey, 0);
        selectedThemeIndex = PlayerPrefs.GetInt(ThemeKey, defaultThemeIndex);
        selectedLanguageIndex = PlayerPrefs.GetInt(LanguageKey, defaultLanguageIndex);
    }

    // Sorts option buttons by their configured option index
    private void SortOptionButtons(List<OptionButton> buttons) {
        if (buttons == null) {
            return;
        }
        buttons.Sort((firstButton, secondButton) => firstButton.OptionIndex.CompareTo(secondButton.OptionIndex));
    }

    // Initializes all option button groups with their correct callbacks
    private void InitializeButtons() {
        InitializeOptionButtons(paletteButtons, SelectPalette);
        InitializeOptionButtons(themeButtons, SelectTheme);
        InitializeOptionButtons(languageButtons, SelectLanguage);
    }

    // Initializes one option button group
    private void InitializeOptionButtons(List<OptionButton> buttons, Action<int> onSelected) {
        for (int i = 0; i < buttons.Count; i++) {
            if (buttons[i] != null) {
                buttons[i].Initialize(onSelected);
            }
        }
    }

    /* Selection methods */

    // Selects a new palette and saves it locally
    private void SelectPalette(int index) {
        selectedPaletteIndex = index;
        hasSavedPalette = true;

        PlayerPrefs.SetInt(PaletteKey, selectedPaletteIndex); // Stores the selected palette index
        PlayerPrefs.Save(); // Forces Unity to save the updated preference

        ApplyVisualOptions();
        RefreshChecks();
    }

    // Selects a new theme and saves it locally
    private void SelectTheme(int index) {
        selectedThemeIndex = index;
        hasSavedTheme = true;

        PlayerPrefs.SetInt(ThemeKey, selectedThemeIndex); // Stores the selected theme index
        PlayerPrefs.Save(); // Forces Unity to save the updated preference

        ApplyVisualOptions();
        RefreshChecks();
    }

    // Selects a new language and saves it locally
    private void SelectLanguage(int index) {
        selectedLanguageIndex = index;
        hasSavedLanguage = true;

        PlayerPrefs.SetInt(LanguageKey, selectedLanguageIndex); // Stores the selected language index
        PlayerPrefs.Save(); // Forces Unity to save the updated preference

        ApplyLanguage();
        RefreshChecks();
    }

    /* Apply methods */

    // Applies only the options that were previously selected by the user
    private void ApplySavedOptions() {
        if (hasSavedPalette || hasSavedTheme) {
            ApplyVisualOptions();
        }
        if (hasSavedLanguage) {
            ApplyLanguage();
        }
        RefreshChecks();
    }

    // Applies the selected palette and selected theme to all palette targets
    private void ApplyVisualOptions() {
        if (paletteTargets == null) {
            FindTargets(); // Reconnects targets if the scene was not initialized yet
        }
        ColorPalette activePalette = GetActivePalette();
        ThemeMode activeTheme = GetActiveTheme();

        foreach (PaletteTarget paletteTarget in paletteTargets) {
            if (paletteTarget != null) {
                Color roleColor = activePalette.GetColor(paletteTarget.ColorRole); // Gets the color based on the target role
                Color themedColor = activeTheme.ApplyTheme(roleColor); // Applies the selected theme over that color
                paletteTarget.ApplyColor(themedColor);
            }
        }
    }

    // Applies the selected language to all localized texts
    private void ApplyLanguage() {
        if (localizedTexts == null) {
            FindTargets(); // Reconnects localized texts if the scene was not initialized yet
        }
        foreach (LocalizedText localizedText in localizedTexts) {
            if (localizedText != null) {
                localizedText.ApplyLanguage(selectedLanguageIndex);
            }
        }
    }

    // Applies current visual settings to one newly enabled palette target
    public void ApplyCurrentSettingsToTarget(PaletteTarget paletteTarget) {
        if (paletteTarget == null) {
            return;
        }
        if (!hasSavedPalette && !hasSavedTheme) {
            return; // Keeps default UI unchanged if the user never selected visual settings
        }

        ColorPalette activePalette = GetActivePalette();
        ThemeMode activeTheme = GetActiveTheme();
        Color roleColor = activePalette.GetColor(paletteTarget.ColorRole);
        Color themedColor = activeTheme.ApplyTheme(roleColor);
        paletteTarget.ApplyColor(themedColor);
    }

    // Applies the current language to one newly enabled localized text
    public void ApplyCurrentLanguageToText(LocalizedText localizedText) {
        if (localizedText == null) {
            return;
        }
        if (!hasSavedLanguage) {
            return;
        }
        localizedText.ApplyLanguage(selectedLanguageIndex);
    }

    // Applies the current language to all localized texts found under a specific object
    public void ApplyCurrentLanguageToChildren(GameObject rootObject) {
        if (rootObject == null) {
            return; // Prevents errors if the instantiated object is missing
        }
        if (!hasSavedLanguage) {
            return; // Keeps default texts unchanged if the user never selected a language
        }

        LocalizedText[] childTexts = rootObject.GetComponentsInChildren<LocalizedText>(true); // Finds all localized texts inside the object, including inactive ones
        foreach (LocalizedText localizedText in childTexts) {
            if (localizedText != null) {
                localizedText.ApplyLanguage(selectedLanguageIndex); // Applies the saved language to each found text
            }
        }
    }

    /* Getter methods */

    // Returns the selected palette or the default team leader palette
    private ColorPalette GetActivePalette() {
        if (hasSavedPalette && IsValidIndex(selectedPaletteIndex, colorPalettes.Count)) {
            return colorPalettes[selectedPaletteIndex];
        }
        return defaultPalette;
    }

    // Returns the selected theme or a safe default theme
    private ThemeMode GetActiveTheme() {
        if (themeModes == null || themeModes.Count == 0) {
            return new ThemeMode(); // Safe fallback if themes were not configured yet
        }
        if (IsValidIndex(selectedThemeIndex, themeModes.Count)) {
            return themeModes[selectedThemeIndex];
        }
        if (IsValidIndex(defaultThemeIndex, themeModes.Count)) {
            return themeModes[defaultThemeIndex];
        }
        return themeModes[0];
    }

    // Returns the currently selected language index
    public int GetSelectedLanguageIndex() {
        return selectedLanguageIndex;
    }

    /* Visual update methods */

    // Refreshes all selected check marks
    private void RefreshChecks() {
        RefreshButtonGroup(paletteButtons, hasSavedPalette, selectedPaletteIndex);
        RefreshButtonGroup(themeButtons, hasSavedTheme, selectedThemeIndex);
        RefreshButtonGroup(languageButtons, hasSavedLanguage, selectedLanguageIndex);
    }

    // Refreshes one option button group
    private void RefreshButtonGroup(List<OptionButton> buttons, bool hasSavedSelection, int selectedIndex) {
        if (buttons == null) {
            return;
        }

        for (int i = 0; i < buttons.Count; i++) {
            if (buttons[i] == null) {
                continue;
            }
            bool isSelected = hasSavedSelection && i == selectedIndex;
            buttons[i].SetSelected(isSelected);
        }
    }

    /* Testing method */

    // Resets all saved settings for testing from the component context menu
    [ContextMenu("Reset Saved Settings")]
    private void ResetSavedSettings() {
        PlayerPrefs.DeleteKey(PaletteKey);
        PlayerPrefs.DeleteKey(ThemeKey);
        PlayerPrefs.DeleteKey(LanguageKey);
        PlayerPrefs.Save();

        hasSavedPalette = false;
        hasSavedTheme = false;
        hasSavedLanguage = false;

        RefreshChecks();
    }

    /* Validation method */

    // Checks if an index exists in a list
    private bool IsValidIndex(int index, int count) {
        return index >= 0 && index < count;
    }
}