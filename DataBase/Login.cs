using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using UnityEditor.PackageManager;

public class Login : MonoBehaviour
{
    public TMP_InputField nameField;
    public TMP_InputField passwordField;
    public Button submitButton;
    public TMP_Text errorMessage;

    public void CallLogin()
    {
        StartCoroutine(LoginPlayer());
    }

    IEnumerator LoginPlayer()
    {
        WWWForm form = new WWWForm();
        form.AddField("name", nameField.text);
        form.AddField("password", passwordField.text);

        string url = "http://localhost:8888/3D_Connect_4/login.php";
        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();
            string responseText = www.downloadHandler.text;
            string[] responseParts = responseText.Split('\t');
            if (responseParts[0] == "0")
            {
                DBManager.username = nameField.text;
                DBManager.score = int.Parse(responseParts[1]); //force it to be a string with int.Parse
                DBManager.userid = int.Parse(responseParts[2]);
                UnityEngine.SceneManagement.SceneManager.LoadScene("GameUI");
            }
            else
            {
                errorMessage.text = "Incorrect Username or Password";
            }
            
	   
        }
    }

    public void VerifyInputs()
    {
        submitButton.interactable = (nameField.text.Length >= 8 && passwordField.text.Length >= 8);

    }
}
