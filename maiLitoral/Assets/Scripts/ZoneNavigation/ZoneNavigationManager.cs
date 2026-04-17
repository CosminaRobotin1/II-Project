using UnityEngine;

public class ZoneNavigationManager : MonoBehaviour {
    [SerializeField] private GameObject zoneList;

    [SerializeField] private GameObject beachesPanelNord;
    [SerializeField] private GameObject beachesPanelMamaiaNord;
    [SerializeField] private GameObject beachesPanelMamaia;
    [SerializeField] private GameObject beachesPanelConstanta;
    [SerializeField] private GameObject beachesPanelEforie;
    [SerializeField] private GameObject beachesPanelCostinesti;
    [SerializeField] private GameObject beachesPanelSudLitoral;
    [SerializeField] private GameObject beachesPanelMangalia;
    [SerializeField] private GameObject beachesPanelVamaVeche;

    // Open Nord panel
    public void OpenNordPanel() {
        beachesPanelNord.SetActive(true);
        beachesPanelMamaiaNord.SetActive(false);
        beachesPanelMamaia.SetActive(false);
        beachesPanelConstanta.SetActive(false);
        beachesPanelEforie.SetActive(false);
        beachesPanelCostinesti.SetActive(false);
        beachesPanelSudLitoral.SetActive(false);
        beachesPanelMangalia.SetActive(false);
        beachesPanelVamaVeche.SetActive(false);
        zoneList.SetActive(false);
    }

    // Open Mamaia Nord panel
    public void OpenMamaiaNordPanel() {
        beachesPanelNord.SetActive(false);
        beachesPanelMamaiaNord.SetActive(true);
        beachesPanelMamaia.SetActive(false);
        beachesPanelConstanta.SetActive(false);
        beachesPanelEforie.SetActive(false);
        beachesPanelCostinesti.SetActive(false);
        beachesPanelSudLitoral.SetActive(false);
        beachesPanelMangalia.SetActive(false);
        beachesPanelVamaVeche.SetActive(false);
        zoneList.SetActive(false);
    }

    // Open Mamaia panel
    public void OpenMamaiaPanel() {
        beachesPanelNord.SetActive(false);
        beachesPanelMamaiaNord.SetActive(false);
        beachesPanelMamaia.SetActive(true);
        beachesPanelConstanta.SetActive(false);
        beachesPanelEforie.SetActive(false);
        beachesPanelCostinesti.SetActive(false);
        beachesPanelSudLitoral.SetActive(false);
        beachesPanelMangalia.SetActive(false);
        beachesPanelVamaVeche.SetActive(false);
        zoneList.SetActive(false);
    }

    // Open Constanta panel
    public void OpenConstantaPanel() {
        beachesPanelNord.SetActive(false);
        beachesPanelMamaiaNord.SetActive(false);
        beachesPanelMamaia.SetActive(false);
        beachesPanelConstanta.SetActive(true);
        beachesPanelEforie.SetActive(false);
        beachesPanelCostinesti.SetActive(false);
        beachesPanelSudLitoral.SetActive(false);
        beachesPanelMangalia.SetActive(false);
        beachesPanelVamaVeche.SetActive(false);
        zoneList.SetActive(false);
    }

    // Open Eforie panel
    public void OpenEforiePanel() {
        beachesPanelNord.SetActive(false);
        beachesPanelMamaiaNord.SetActive(false);
        beachesPanelMamaia.SetActive(false);
        beachesPanelConstanta.SetActive(false);
        beachesPanelEforie.SetActive(true);
        beachesPanelCostinesti.SetActive(false);
        beachesPanelSudLitoral.SetActive(false);
        beachesPanelMangalia.SetActive(false);
        beachesPanelVamaVeche.SetActive(false);
        zoneList.SetActive(false);
    }

    // Open Costinesti panel
    public void OpenCostinestiPanel() {
        beachesPanelNord.SetActive(false);
        beachesPanelMamaiaNord.SetActive(false);
        beachesPanelMamaia.SetActive(false);
        beachesPanelConstanta.SetActive(false);
        beachesPanelEforie.SetActive(false);
        beachesPanelCostinesti.SetActive(true);
        beachesPanelSudLitoral.SetActive(false);
        beachesPanelMangalia.SetActive(false);
        beachesPanelVamaVeche.SetActive(false);
        zoneList.SetActive(false);
    }

    // Open Sud Litoral panel
    public void OpenSudLitoralPanel() {
        beachesPanelNord.SetActive(false);
        beachesPanelMamaiaNord.SetActive(false);
        beachesPanelMamaia.SetActive(false);
        beachesPanelConstanta.SetActive(false);
        beachesPanelEforie.SetActive(false);
        beachesPanelCostinesti.SetActive(false);
        beachesPanelSudLitoral.SetActive(true);
        beachesPanelMangalia.SetActive(false);
        beachesPanelVamaVeche.SetActive(false);
        zoneList.SetActive(false);
    }

    // Open Mangalia panel
    public void OpenMangaliaPanel() {
        beachesPanelNord.SetActive(false);
        beachesPanelMamaiaNord.SetActive(false);
        beachesPanelMamaia.SetActive(false);
        beachesPanelConstanta.SetActive(false);
        beachesPanelEforie.SetActive(false);
        beachesPanelCostinesti.SetActive(false);
        beachesPanelSudLitoral.SetActive(false);
        beachesPanelMangalia.SetActive(true);
        beachesPanelVamaVeche.SetActive(false);
        zoneList.SetActive(false);
    }

    // Open Vama Veche panel
    public void OpenVamaVechePanel() {
        beachesPanelNord.SetActive(false);
        beachesPanelMamaiaNord.SetActive(false);
        beachesPanelMamaia.SetActive(false);
        beachesPanelConstanta.SetActive(false);
        beachesPanelEforie.SetActive(false);
        beachesPanelCostinesti.SetActive(false);
        beachesPanelSudLitoral.SetActive(false);
        beachesPanelMangalia.SetActive(false);
        beachesPanelVamaVeche.SetActive(true);
        zoneList.SetActive(false);
    }

    // Go back to zone list
    public void BackToZoneList() {
        beachesPanelNord.SetActive(false);
        beachesPanelMamaiaNord.SetActive(false);
        beachesPanelMamaia.SetActive(false);
        beachesPanelConstanta.SetActive(false);
        beachesPanelEforie.SetActive(false);
        beachesPanelCostinesti.SetActive(false);
        beachesPanelSudLitoral.SetActive(false);
        beachesPanelMangalia.SetActive(false);
        beachesPanelVamaVeche.SetActive(false);
        zoneList.SetActive(true);
    }
}
