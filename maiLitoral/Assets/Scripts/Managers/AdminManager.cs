using UnityEngine;

public class AdminManager : MonoBehaviour
{

    /* Attributes */

    [SerializeField] private GameObject adminLoginPanel; // Attribute for admin login panel
    [SerializeField] private GameObject adminMainPanel; // Attribute for admin main panel
    [SerializeField] private GameObject beachesPanel; // Attribute for beaches panel
    [SerializeField] private GameObject parametersPanel; // Attribute for parameters panel
    [SerializeField] private GameObject standardOptions; // Attribute for standard settings options

    private void Start()
    {
        SetupAdminPanels(); // Setting up admin panels at start
    }


    private void SetupAdminPanels()
    { // Setting up the default admin panels states
        adminLoginPanel.SetActive(false); // Hiding login panel first
        adminMainPanel.SetActive(false); // Hiding main admin panel first
        beachesPanel.SetActive(false); // Hiding beaches panel first
        parametersPanel.SetActive(false); // Hiding parameters panel first
    }

    public void OpenAdminLoginPanel()
    { // Opening admin login panel from settings
        adminLoginPanel.SetActive(true); // Showing admin login panel
        adminMainPanel.SetActive(false); // Hiding admin main panel
        beachesPanel.SetActive(false); // Hiding beaches panel
        parametersPanel.SetActive(false); // Hiding parameters panel
    }

    public void OpenAdminPanel()
    { // Opening admin main panel after login
        adminLoginPanel.SetActive(false); // Hiding login panel
        adminMainPanel.SetActive(true); // Showing admin main panel
        beachesPanel.SetActive(false); // Hiding beaches panel first
        parametersPanel.SetActive(false); // Hiding parameters panel first
    }

    public void OpenBeachesPanel()
    { // Showing beaches management panel
        beachesPanel.SetActive(true); // Showing beaches panel
        parametersPanel.SetActive(false); // Hiding parameters panel
    }

    public void OpenParametersPanel()
    { // Showing parameters management panel
        beachesPanel.SetActive(false); // Hiding beaches panel
        parametersPanel.SetActive(true); // Showing parameters panel
    }

    public void CloseAdminPanel()
    { // Closing admin main panel and returning to login panel
        adminMainPanel.SetActive(false); // Hiding admin main panel
        beachesPanel.SetActive(false); // Hiding beaches panel
        parametersPanel.SetActive(false); // Hiding parameters panel
        adminLoginPanel.SetActive(true); // Showing login panel again
    }

    public void CloseAdminLoginPanel()
    { // Closing admin login panel and returning to settings options
        adminLoginPanel.SetActive(false); // Hiding admin login panel
        standardOptions.SetActive(true); // Showing standard settings options
    }

    public void ReturnToAdminTabs()
    { // Returning from admin sub panels to admin tabs
        beachesPanel.SetActive(false); // Hiding beaches panel
        parametersPanel.SetActive(false); // Hiding parameters panel
    }
}