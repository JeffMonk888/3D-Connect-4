using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class GameUI : MonoBehaviour
{
    [SerializeField] private GameObject Auth;
    [SerializeField] private GameObject LogOut;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button OnlineButton;
    [SerializeField] private Button LocalButton;
    [SerializeField] private Button ReplayButton;
    [SerializeField] private TMP_Text userDisplay;


    private void Start()
    {
        if (DBManager.LoggedIn)
        {
            Auth.gameObject.SetActive(false);
            userDisplay.text = $"{DBManager.username} ({DBManager.score.ToString()})";
            LogOut.gameObject.SetActive(true);
        }
        else
        {
            LogOut.gameObject.SetActive(false);
        }
        
        registerButton.interactable = !DBManager.LoggedIn;
        loginButton.interactable = !DBManager.LoggedIn;
        LocalButton.interactable = true;
        OnlineButton.interactable = DBManager.LoggedIn;
        ReplayButton.interactable = DBManager.LoggedIn;


    }
}
