using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
public class Registeration : MonoBehaviour
{
    public TMP_InputField nameField;
    public TMP_InputField passwordField;
    public Button submitButton;
    
    public TMP_Text errorMessage;

    public void CallResgister()
    {
        StartCoroutine(Register());
    }

    IEnumerator Register()
    {
        WWWForm form = new WWWForm();
        form.AddField("name", nameField.text);
        form.AddField("password", passwordField.text);

        string url = "http://localhost:8888/3D_Connect_4/register.php";
        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Network error:" + www.error);
                yield break;
            }

            string reponseText = www.downloadHandler.text.Trim();

            if (reponseText == "0")
            {
                Debug.Log("user created succcessfully");
                UnityEngine.SceneManagement.SceneManager.LoadScene("GameUI");

            }
            else
            {
                errorMessage.text = "Username already exist";
            }

        }
    }

    public void VerifyInputs()
    {
        submitButton.interactable = (nameField.text.Length >= 8 && passwordField.text.Length >= 8);

    }
}