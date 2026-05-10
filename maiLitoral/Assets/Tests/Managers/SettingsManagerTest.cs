using NUnit.Framework;
using UnityEngine;
using System;
using System.Collections;
using System.Reflection;

public class SettingsManagerTest {

    private const string PaletteKey = "selected_palette"; // Local storage key used for palette selection.
    private const string ThemeKey = "selected_theme"; // Local storage key used for theme selection.
    private const string LanguageKey = "selected_language"; // Local storage key used for language selection.

    [SetUp]
    public void SetUp() { // Clears saved settings and old managers before each test.
        ClearPlayerPrefsKeys();
        DestroyExistingSettingsManagers();
        ResetSettingsManagerInstance();
    }

    [TearDown]
    public void TearDown() { // Cleans test objects and saved settings after each test.
        ClearPlayerPrefsKeys();
        DestroyExistingSettingsManagers();
        ResetSettingsManagerInstance();
    }

    private void ClearPlayerPrefsKeys() { // Removes settings keys from local storage.
        PlayerPrefs.DeleteKey(PaletteKey);
        PlayerPrefs.DeleteKey(ThemeKey);
        PlayerPrefs.DeleteKey(LanguageKey);
        PlayerPrefs.Save();
    }

    private void DestroyExistingSettingsManagers() { // Removes old SettingsManager objects from previous tests.
        SettingsManager[] managers = UnityEngine.Object.FindObjectsByType<SettingsManager>(
            FindObjectsInactive.Include,
            FindObjectsSortMode.None
        );

        foreach (SettingsManager manager in managers) {
            if (manager != null) {
                UnityEngine.Object.DestroyImmediate(manager.gameObject);
            }
        }
    }

    private void ResetSettingsManagerInstance() { // Resets the singleton Instance field.
        PropertyInfo instanceProperty = typeof(SettingsManager).GetProperty(
            "Instance",
            BindingFlags.Public | BindingFlags.Static
        );

        MethodInfo setter = instanceProperty.GetSetMethod(true);

        if (setter != null) {
            setter.Invoke(null, new object[] { null });
        }
    }

    private SettingsManager CreateManager() { // Creates an inactive SettingsManager for controlled testing.
        GameObject obj = new GameObject("SettingsManager_TestObject");
        obj.SetActive(false); // Prevents Unity lifecycle methods from running automatically.

        SettingsManager manager = obj.AddComponent<SettingsManager>();

        SetPrivateField(manager, "paletteTargets", new PaletteTarget[0]); // Prevents real scene UI from affecting tests.
        SetPrivateField(manager, "localizedTexts", new LocalizedText[0]); // Prevents real scene text from affecting tests.

        return manager;
    }

    private void SetPrivateField(object target, string fieldName, object value) { // Sets a private field using reflection.
        target.GetType()
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            .SetValue(target, value);
    }

    private T GetPrivateField<T>(object target, string fieldName) { // Reads a private field using reflection.
        return (T)target.GetType()
            .GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)
            .GetValue(target);
    }

    private void InvokePrivateMethod(object target, string methodName, params object[] parameters) { // Calls private methods and unwraps reflection exceptions.
        try {
            target.GetType()
                .GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance)
                .Invoke(target, parameters);
        } catch (TargetInvocationException exception) {
            throw exception.InnerException;
        }
    }

    private IList CreatePaletteListWithOnePalette() { // Creates one private ColorPalette entry for palette tests.
        FieldInfo palettesField = typeof(SettingsManager).GetField(
            "colorPalettes",
            BindingFlags.NonPublic | BindingFlags.Instance
        );

        Type paletteType = typeof(SettingsManager).GetNestedType(
            "ColorPalette",
            BindingFlags.NonPublic
        );

        IList palettes = (IList)Activator.CreateInstance(palettesField.FieldType);

        palettes.Add(Activator.CreateInstance(paletteType)); // Adds a valid palette object to the private list.

        return palettes;
    }

    private PaletteTarget CreatePaletteTarget() { // Creates a PaletteTarget used to test missing palette behavior.
        GameObject targetObject = new GameObject("PaletteTarget_TestObject");
        targetObject.SetActive(false);

        return targetObject.AddComponent<PaletteTarget>();
    }

    [Test]
    public void ResetSavedSettings_ShouldDeleteSavedPlayerPrefsKeys() { // Tests that saved settings are deleted from local storage.
        SettingsManager manager = CreateManager();

        PlayerPrefs.SetInt(PaletteKey, 1);
        PlayerPrefs.SetInt(ThemeKey, 1);
        PlayerPrefs.SetInt(LanguageKey, 1);
        PlayerPrefs.Save();

        InvokePrivateMethod(manager, "ResetSavedSettings");

        Assert.IsFalse(PlayerPrefs.HasKey(PaletteKey)); // Palette save should be deleted.
        Assert.IsFalse(PlayerPrefs.HasKey(ThemeKey)); // Theme save should be deleted.
        Assert.IsFalse(PlayerPrefs.HasKey(LanguageKey)); // Language save should be deleted.
    }

    [Test]
    public void LoadSettings_ShouldReadSavedValuesFromPlayerPrefs() { // Tests that saved settings are loaded from local storage.
        SettingsManager manager = CreateManager();

        PlayerPrefs.SetInt(PaletteKey, 2);
        PlayerPrefs.SetInt(ThemeKey, 1);
        PlayerPrefs.SetInt(LanguageKey, 1);
        PlayerPrefs.Save();

        InvokePrivateMethod(manager, "LoadSettings");

        Assert.IsTrue(GetPrivateField<bool>(manager, "hasSavedPalette")); // Palette save should be detected.
        Assert.IsTrue(GetPrivateField<bool>(manager, "hasSavedTheme")); // Theme save should be detected.
        Assert.IsTrue(GetPrivateField<bool>(manager, "hasSavedLanguage")); // Language save should be detected.

        Assert.AreEqual(2, GetPrivateField<int>(manager, "selectedPaletteIndex"));
        Assert.AreEqual(1, GetPrivateField<int>(manager, "selectedThemeIndex"));
        Assert.AreEqual(1, GetPrivateField<int>(manager, "selectedLanguageIndex"));
    }

    [Test]
    public void SelectPalette_WithValidIndex_ShouldSavePaletteIndex() { // Tests that a valid palette index is saved locally.
        SettingsManager manager = CreateManager();

        SetPrivateField(manager, "colorPalettes", CreatePaletteListWithOnePalette()); // Gives the manager one valid palette.
        SetPrivateField(manager, "paletteTargets", new PaletteTarget[0]); // Keeps visual application isolated from scene UI.

        InvokePrivateMethod(manager, "SelectPalette", 0);

        Assert.IsTrue(PlayerPrefs.HasKey(PaletteKey)); // Valid palette should create a saved key.
        Assert.AreEqual(0, PlayerPrefs.GetInt(PaletteKey));
        Assert.IsTrue(GetPrivateField<bool>(manager, "hasSavedPalette"));
    }

    [Test]
    public void SelectPalette_WithInvalidIndex_ShouldNotSavePaletteIndex() { // Tests that an invalid palette index is ignored.
        SettingsManager manager = CreateManager();

        SetPrivateField(manager, "colorPalettes", CreatePaletteListWithOnePalette());

        InvokePrivateMethod(manager, "SelectPalette", 99); // Index 99 does not exist.

        Assert.IsFalse(PlayerPrefs.HasKey(PaletteKey)); // Invalid index should not be saved.
        Assert.IsFalse(GetPrivateField<bool>(manager, "hasSavedPalette"));
    }

    [Test]
    public void SelectPalette_WithNegativeIndex_ShouldResetSavedPalette() { // Tests that negative palette index resets the palette setting.
        SettingsManager manager = CreateManager();

        PlayerPrefs.SetInt(PaletteKey, 0);
        PlayerPrefs.Save();

        SetPrivateField(manager, "hasSavedPalette", true);

        InvokePrivateMethod(manager, "SelectPalette", -1); // Negative index is used as a reset command.

        Assert.IsFalse(PlayerPrefs.HasKey(PaletteKey)); // Palette key should be removed.
        Assert.IsFalse(GetPrivateField<bool>(manager, "hasSavedPalette"));
    }

    [Test]
    public void SelectTheme_WithInvalidIndex_ShouldSaveInvalidIndex_CurrentBug() { // Tests that theme selection currently saves invalid indexes.
        SettingsManager manager = CreateManager();

        SetPrivateField(manager, "paletteTargets", new PaletteTarget[0]); // Avoids applying visuals to real UI targets.

        InvokePrivateMethod(manager, "SelectTheme", 99); // Invalid index is still accepted by the method.

        Assert.IsTrue(PlayerPrefs.HasKey(ThemeKey));
        Assert.AreEqual(99, PlayerPrefs.GetInt(ThemeKey)); // This confirms the risky behavior.
        Assert.AreEqual(99, GetPrivateField<int>(manager, "selectedThemeIndex"));
    }

    [Test]
    public void SelectLanguage_WithInvalidIndex_ShouldSaveInvalidIndex_CurrentBug() { // Tests that language selection currently saves invalid indexes.
        SettingsManager manager = CreateManager();

        SetPrivateField(manager, "localizedTexts", new LocalizedText[0]); // Avoids applying language to real scene text.

        InvokePrivateMethod(manager, "SelectLanguage", 99); // Invalid language index is still accepted.

        Assert.IsTrue(PlayerPrefs.HasKey(LanguageKey));
        Assert.AreEqual(99, PlayerPrefs.GetInt(LanguageKey)); // This confirms the risky behavior.
        Assert.AreEqual(99, GetPrivateField<int>(manager, "selectedLanguageIndex"));
    }

    [Test]
    public void GetSelectedLanguageIndex_AfterSelectingLanguage_ShouldReturnSelectedIndex() { // Tests that the selected language index can be read back.
        SettingsManager manager = CreateManager();

        SetPrivateField(manager, "localizedTexts", new LocalizedText[0]);

        InvokePrivateMethod(manager, "SelectLanguage", 1);

        Assert.AreEqual(1, manager.GetSelectedLanguageIndex());
    }

    [Test]
    public void ApplyCurrentSettingsToTarget_WithNullTarget_ShouldNotThrowException() { // Tests that null palette targets are ignored safely.
        SettingsManager manager = CreateManager();

        Assert.DoesNotThrow(() => {
            manager.ApplyCurrentSettingsToTarget(null); // Null target should return safely.
        });
    }

    [Test]
    public void ApplyCurrentLanguageToText_WithNullText_ShouldNotThrowException() { // Tests that null localized text is ignored safely.
        SettingsManager manager = CreateManager();

        Assert.DoesNotThrow(() => {
            manager.ApplyCurrentLanguageToText(null); // Null text should return safely.
        });
    }

    [Test]
    public void ApplyCurrentLanguageToChildren_WithNullRoot_ShouldNotThrowException() { // Tests that null root objects are ignored safely.
        SettingsManager manager = CreateManager();

        Assert.DoesNotThrow(() => {
            manager.ApplyCurrentLanguageToChildren(null); // Null root should return safely.
        });
    }

    [Test]
    public void ApplyVisualOptions_WithMissingDefaultPaletteAndPaletteTarget_ShouldThrowException() { // Tests that missing default palette can crash visual application.
        SettingsManager manager = CreateManager();

        PaletteTarget target = CreatePaletteTarget();

        SetPrivateField(manager, "paletteTargets", new PaletteTarget[] { target }); // Forces ApplyVisualOptions to process a target.
        SetPrivateField(manager, "defaultPalette", null); // Simulates missing Inspector assignment.
        SetPrivateField(manager, "hasSavedPalette", false); // Forces manager to use defaultPalette.

        Assert.Throws<NullReferenceException>(() => {
            InvokePrivateMethod(manager, "ApplyVisualOptions");
        });
    }
}