using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SearchAutoCompleteController : MonoBehaviour {
    [System.Serializable]
    private class SearchItem {
        public string label;
        public GameObject targetObject;
    }

    [SerializeField] private TMP_InputField searchInputField;
    [SerializeField] private TMP_Text autoCompleteText;
    [SerializeField] private List<SearchItem> searchItems = new List<SearchItem>();

    private string currentSuggestion = "";

    private void Start() {
        searchInputField.onValueChanged.AddListener(OnSearchValueChanged);
        autoCompleteText.text = "";
        ShowAllItems();
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Tab)) {
            ApplySuggestion();
        }
    }

    // Update search state
     private void OnSearchValueChanged(string currentText) {
       UpdateSuggestion(currentText);
       FilterItems(currentText);
    }

    // Update ghost suggestion
    private void UpdateSuggestion(string currentText) {
        currentSuggestion = "";

        if (string.IsNullOrWhiteSpace(currentText)) {
            autoCompleteText.text = "";
            return;
        }

        string lowerText = currentText.ToLower();

        for (int i = 0; i < searchItems.Count; i++) {
            string label = searchItems[i].label;
            if (label.ToLower().StartsWith(lowerText)) {
                currentSuggestion = label;
                break;
            }
        }

        if (string.IsNullOrEmpty(currentSuggestion)) {
            autoCompleteText.text = "";
            return;
        }

        if (currentSuggestion.Length == currentText.Length) {
            autoCompleteText.text = "";
            return;
        }

        autoCompleteText.text = currentSuggestion;
    }

    // Filter visible items
    private void FilterItems(string currentText) {
        if (string.IsNullOrWhiteSpace(currentText)) {
            ShowAllItems();
            return;
        }

        string lowerText = currentText.ToLower();

        for (int i = 0; i < searchItems.Count; i++) {
            bool matches = searchItems[i].label.ToLower().Contains(lowerText);
            searchItems[i].targetObject.SetActive(matches);
        }
    }

    // Show all items
    private void ShowAllItems() {
        for (int i = 0; i < searchItems.Count; i++) {
            searchItems[i].targetObject.SetActive(true);
        }
    }

    // Apply current suggestion
    private void ApplySuggestion() {
        if (string.IsNullOrEmpty(currentSuggestion)) {
            return;
        }

        if (searchInputField.text == currentSuggestion) {
            return;
        }

        searchInputField.text = currentSuggestion;
        searchInputField.caretPosition = searchInputField.text.Length;
        autoCompleteText.text = "";
        FilterItems(currentSuggestion);
    }
}