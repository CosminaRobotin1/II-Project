using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdminManager : MonoBehaviour {

    //* Attributes *//

    private string adminName = "admin"; // Attribute for admin name
    private string adminPassword = "1234"; // Attribute for admin password (*needs to be encrypted*)
    [SerializeField] private GameObject loginPanel; // Attribute for login panel
    [SerializeField] private TMP_InputField adminNameInputField; // Attribute for admin name input field
    [SerializeField] private TMP_InputField adminPasswordInputField; // Attribute for admin password input field

    //* Main Methods *//



    //* Custom Methods *//

    public void Login() { // Logins the admin to the admin page
        if(adminName == adminNameInputField.text && adminPassword == adminPasswordInputField.text) { // * Needs to be encrypted so check the encryption key, not the text!*
            ButtonsManager.ToggleObject(loginPanel);
        }
    }

    //* Getters *//



    //* Setters *//

    
}
