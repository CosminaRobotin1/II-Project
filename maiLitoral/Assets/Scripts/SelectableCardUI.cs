using UnityEngine;

public class SelectableCardUI : MonoBehaviour {
    public GameObject selectedCheck;
    public SelectableCardUI[] groupCards;

    public void ToggleThisCard() {
        if (selectedCheck == null) return;

        bool wasActive = selectedCheck.activeSelf;

        foreach (SelectableCardUI card in groupCards) {
            if (card != null && card.selectedCheck != null) {
                card.selectedCheck.SetActive(false);
            }
        }

        if (!wasActive) {
            selectedCheck.SetActive(true);
        }
    }
}