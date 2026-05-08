using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour {

    /* Serializable classes */

    [Serializable]
    private class ColorPalette {
        [SerializeField] private string paletteName; // Name used only for Inspector readability
        [SerializeField] private Color lightColor; // Color used for main light backgrounds
        [SerializeField] private Color softColor; // Color used for panels, cards and soft containers
        [SerializeField] private Color mediumColor; // Color used for secondary UI elements
        [SerializeField] private Color accentColor; // Color used for important buttons and highlights
        [SerializeField] private Color darkColor; // Color used for titles, text and strong details
        public Color GetColor(PaletteColorRole colorRole) { // Returns the color that matches the requested palette role
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
    private class Theme {
        [SerializeField] private string themeName; // Name used only for Inspector readability
        [SerializeField] private float brightnessMultiplier = 1f; // Controls how bright or dark the theme appears
        public Color ApplyTheme(Color color) { // Applies the theme brightness over a palette color
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
    [SerializeField] private ColorPalette defaultPalette; // Default palette that matches the original design
    [SerializeField] private int defaultThemeIndex = 0; // Default theme used when no theme was selected
    [SerializeField] private int defaultLanguageIndex = 0; // Default language used when no language was selected
    private List<OptionButton> paletteButtons = new List<OptionButton>(); // Palette buttons found automatically in the active scene
    private List<OptionButton> themeButtons = new List<OptionButton>(); // Theme buttons found automatically in the active scene
    private List<OptionButton> languageButtons = new List<OptionButton>(); // Language buttons found automatically in the active scene
    [SerializeField] private List<ColorPalette> colorPalettes = new List<ColorPalette>(); // All available color palettes
    [SerializeField] private List<Theme> themes = new List<Theme>(); // All available theme modes
    private PaletteTarget[] paletteTargets; // All UI elements affected by palette and theme changes
    private LocalizedText[] localizedTexts; // All UI texts affected by language changes
    private int selectedPaletteIndex; // Currently selected palette index
    private int selectedThemeIndex; // Currently selected theme index
    private int selectedLanguageIndex; // Currently selected language index
    private bool hasSavedPalette; // True if the user selected a palette before
    private bool hasSavedTheme; // True if the user selected a theme before
    private bool hasSavedLanguage; // True if the user selected a language before
    public static SettingsManager Instance { get; private set; } // Global access to the active settings manager

    /* Custom Methods */

    private void Awake() { // Initializes the manager and keeps it alive between scenes
        // ResetSavedSettings(); // ONLY FOR NOW *TO BE DELETED WHEN FINISHING THE APP*
        if (Instance != null && Instance != this) {
            Destroy(gameObject); // Prevents duplicated settings managers after scene reloads
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // Keeps settings active between StartingPage and BeachPage
        LoadSettings();
        FindTargets();
        ApplySavedOptions();
    }
    private void OnEnable() { // Registers the scene loading callback
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void OnDisable() { // Unregisters the scene loading callback
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) { // Reconnects UI elements after a new scene was loaded
        FindTargets();
        ApplySavedOptions();
    }
    private void FindTargets() { // Finds all palette, language and option targets in the active scene
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
    private void LoadSettings() { // Loads saved options from local device storage
        hasSavedPalette = PlayerPrefs.HasKey(PaletteKey);
        hasSavedTheme = PlayerPrefs.HasKey(ThemeKey);
        hasSavedLanguage = PlayerPrefs.HasKey(LanguageKey);
        selectedPaletteIndex = PlayerPrefs.GetInt(PaletteKey, 0);
        selectedThemeIndex = PlayerPrefs.GetInt(ThemeKey, defaultThemeIndex);
        selectedLanguageIndex = PlayerPrefs.GetInt(LanguageKey, defaultLanguageIndex);
    }
    private void SortOptionButtons(List<OptionButton> buttons) { // Sorts option buttons by their configured option index
        if (buttons == null) {
            return;
        }
        buttons.Sort((firstButton, secondButton) => firstButton.OptionIndex.CompareTo(secondButton.OptionIndex));
    }
    private void InitializeButtons() { // Initializes all option button groups with their correct callbacks
        InitializeOptionButtons(paletteButtons, SelectPalette);
        InitializeOptionButtons(themeButtons, SelectTheme);
        InitializeOptionButtons(languageButtons, SelectLanguage);
    }
    private void InitializeOptionButtons(List<OptionButton> buttons, Action<int> onSelected) { // Initializes one option button group
        for (int i = 0; i < buttons.Count; i++) {
            if (buttons[i] != null) {
                buttons[i].Initialize(onSelected);
            }
        }
    }
    private void SelectPalette(int index) { // Selects a new palette and saves it locally
        if (index < 0) { // Checks if the received index represents the default palette reset 
            hasSavedPalette = false; // Marks that there is no custom palette selected
            PlayerPrefs.DeleteKey(PaletteKey); // Removes the saved palette from local storage  
            PlayerPrefs.Save(); // Forces unity to save the updated preferences
            ApplyVisualOptions(); // Applies the default palette visually
            RefreshChecks(); // Refreshes all palette check marks
            return;
        }
        if (!IsValidIndex(index, colorPalettes.Count)) { // Stops invalid indexes safely
            return;
        }
        selectedPaletteIndex = index;
        hasSavedPalette = true;
        PlayerPrefs.SetInt(PaletteKey, selectedPaletteIndex); // Stores the selected palette index
        PlayerPrefs.Save(); // Forces Unity to save the updated preference
        ApplyVisualOptions();
        RefreshChecks();
    }
    private void SelectTheme(int index) { // Selects a new theme and saves it locally
        selectedThemeIndex = index;
        hasSavedTheme = true;

        PlayerPrefs.SetInt(ThemeKey, selectedThemeIndex); // Stores the selected theme index
        PlayerPrefs.Save(); // Forces Unity to save the updated preference

        ApplyVisualOptions();
        RefreshChecks();
    }
    private void SelectLanguage(int index) { // Selects a new language and saves it locally
        selectedLanguageIndex = index;
        hasSavedLanguage = true;

        PlayerPrefs.SetInt(LanguageKey, selectedLanguageIndex); // Stores the selected language index
        PlayerPrefs.Save(); // Forces Unity to save the updated preference

        ApplyLanguage();
        RefreshChecks();
    }
    private void ApplySavedOptions() { // Applies current saved settings or default settings
        // Applies visual settings every time; this ensures that the default palette is restored after scene changes or play mode restarts
        ApplyVisualOptions();
        // Applies the saved language only if
        // The user selected a language before
        if (hasSavedLanguage) {
            ApplyLanguage();
        }
        // Refreshes all selected check marks
        // After visuals and language are updated
        RefreshChecks();
    }
    private void ApplyVisualOptions() { // Applies the selected palette and selected theme to all palette targets
        if (paletteTargets == null) {
            FindTargets(); // Reconnects targets if the scene was not initialized yet
        }
        ColorPalette activePalette = GetActivePalette();
        Theme activeTheme = GetActiveTheme();
        foreach (PaletteTarget paletteTarget in paletteTargets) {
            if (paletteTarget != null) {
                Color roleColor = activePalette.GetColor(paletteTarget.ColorRole); // Gets the color based on the target role
                Color themedColor = activeTheme.ApplyTheme(roleColor); // Applies the selected theme over that color
                paletteTarget.ApplyColor(themedColor);
            }
        }
    }
    private void ApplyLanguage() { // Applies the selected language to all localized texts
        if (localizedTexts == null) {
            FindTargets(); // Reconnects localized texts if the scene was not initialized yet
        }
        foreach (LocalizedText localizedText in localizedTexts) {
            if (localizedText != null) {
                localizedText.ApplyLanguage(selectedLanguageIndex);
            }
        }
    }
    public void ApplyCurrentSettingsToTarget(PaletteTarget paletteTarget) { // Applies current visual settings to one newly enabled palette target
        if (paletteTarget == null) {
            return;
        }
        if (!hasSavedPalette && !hasSavedTheme) {
            return; // Keeps default UI unchanged if the user never selected visual settings
        }

        ColorPalette activePalette = GetActivePalette();
        Theme activeTheme = GetActiveTheme();
        Color roleColor = activePalette.GetColor(paletteTarget.ColorRole);
        Color themedColor = activeTheme.ApplyTheme(roleColor);
        paletteTarget.ApplyColor(themedColor);
    }
    public void ApplyCurrentLanguageToText(LocalizedText localizedText) { // Applies the current language to one newly enabled localized text
        if (localizedText == null) {
            return;
        }
        if (!hasSavedLanguage) {
            return;
        }
        localizedText.ApplyLanguage(selectedLanguageIndex);
    }
    public void ApplyCurrentLanguageToChildren(GameObject rootObject) { // Applies the current language to all localized texts found under a specific object
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
    private void RefreshChecks() { // Refreshes all selected check marks
        RefreshButtonGroup(paletteButtons, hasSavedPalette, selectedPaletteIndex);
        RefreshButtonGroup(themeButtons, hasSavedTheme, selectedThemeIndex);
        RefreshButtonGroup(languageButtons, hasSavedLanguage, selectedLanguageIndex);
    }
    private void RefreshButtonGroup(List<OptionButton> buttons, bool hasSavedSelection, int selectedIndex) { // Refreshes one option button group
        if (buttons == null) {
            return; // Stops safely if the button list is missing
        }
        for (int i = 0; i < buttons.Count; i++) {
            if (buttons[i] == null) {
                continue; // Ignores missing button references
            }
            // Compares the saved index with the real option index from the button, not with the position of the button inside the sorted list.
            bool isSelected = hasSavedSelection && buttons[i].OptionIndex == selectedIndex;
            // Activates the selected check only on the correct option button.
            buttons[i].SetSelected(isSelected);
        }
    }
    [ContextMenu("Reset Saved Settings")] // Resets all saved settings for testing from the component context menu
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
    private bool IsValidIndex(int index, int count) { // Checks if an index exists in a list
        return index >= 0 && index < count;
    }

    /* Getter methods */

    private ColorPalette GetActivePalette() { // Returns the selected palette or the default palette
        if (hasSavedPalette && IsValidIndex(selectedPaletteIndex, colorPalettes.Count)) {
            return colorPalettes[selectedPaletteIndex];
        }
        return defaultPalette;
    }
    private Theme GetActiveTheme() {
        if (themes == null || themes.Count == 0) {
            return new Theme(); // Safe fallback if themes were not configured yet
        }
        if (IsValidIndex(selectedThemeIndex, themes.Count)) {
            return themes[selectedThemeIndex];
        }
        if (IsValidIndex(defaultThemeIndex, themes.Count)) {
            return themes[defaultThemeIndex];
        }
        return themes[0];
    }
    public int GetSelectedLanguageIndex() { // Returns the currently selected language index
        return selectedLanguageIndex;
    }    
}