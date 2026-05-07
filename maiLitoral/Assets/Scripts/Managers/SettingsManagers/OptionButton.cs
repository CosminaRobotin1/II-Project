using System;
using UnityEngine;
using UnityEngine.UI;

public enum OptionButtonType {
    Palette,
    Theme,
    Language
}

public class OptionButton : MonoBehaviour {
    /* Attributes */

    [SerializeField] private Button button; // Button component that receives the user click
    [SerializeField] private GameObject selectedCheck; // Visual check mark shown when this option is selected
    [SerializeField] private OptionButtonType buttonType; // Defines if this button controls palette, theme or language
    [SerializeField] private int optionIndex; // Index used by the manager to identify this option
    private Action<int> onSelected; // Callback assigned by the settings manager

    /* Properties */

    public OptionButtonType ButtonType => buttonType; // Allows the manager to group buttons automatically
    public int OptionIndex => optionIndex; // Allows the manager to sort buttons in the correct order

    /* Initialization method */

    // Connects this button to the manager callback in a reusable way
    public void Initialize(Action<int> callback) {
        onSelected = callback;
        if (button == null) {
            return; // Prevents errors if the Button component was not assigned
        }

        // Avoids adding the same listener multiple times after scene reloads
        button.onClick.RemoveListener(SelectOption);

        // Adds the click listener for this option
        button.onClick.AddListener(SelectOption);
    }

    /* Selection method */

    // Sends this option index to the settings manager
    private void SelectOption() {
        onSelected?.Invoke(optionIndex);
    }

    /* Visual method */

    // Updates the selected check mark for this button
    public void SetSelected(bool isSelected) {
        if (selectedCheck == null)  {
            return; // Some buttons may not have a check mark assigned yet
        }
        selectedCheck.SetActive(isSelected);
    }
}