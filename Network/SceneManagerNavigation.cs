using System;
using System.Collections;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.SceneManagement;
public class SceneManagerNavigation : MonoBehaviour
{
    public void GoToRegister()
    {
        SceneManager.LoadScene("RegisterMenu");
    }

    public void GoToLogin()
    {
        SceneManager.LoadScene("LoginMenu");
    }

    public void GoToMenu()
    {
        SceneManager.LoadScene("GameUI");
    }

    //public void GoToLocalMenu() //1) create an AI option and a local play against your friend option
    //{ 
    //    //SceneManager - go to LocalMenu 
    //} 

    public void GoToOnlineMenu()
    {
        SceneManager.LoadScene("OnlineMenu");
    }

    public void GoToOnlineFromHost()
    {
        NetworkManager onlineInstance = FindObjectOfType<NetworkManager>();
        // Check if the button has been clicked
        if (onlineInstance != null)
        {
            onlineInstance.backButtonClicked = true;
            if (onlineInstance.BackButtonClicked)
            {
                onlineInstance.OnHostBackButton();

                // Set the flag to true to indicate that the back button has been handled
                onlineInstance.backButtonClicked = false;
            }
            Destroy(onlineInstance.gameObject);
            SceneManager.LoadScene("OnlineMenu");

        }
    }

    public void ExitGame()
    {
        if (DBManager.online)
        {
            Client.Instance.Shutdown();
            ClientNetworkHandler.Instance.CallUnRegisterEvents();
            SceneManager.LoadScene("GameUI");
        }
        else
        {
            SceneManager.LoadScene("GameUI");
        }

    }
    public void GoToReplayMenu()
    {
        SceneManager.LoadScene("ReplayMenu");
    }
    public void GoToReplay()
    {
        SceneManager.LoadScene("Replay");

    }
    public void GoToOnlineGame()
    {
        SceneManager.LoadScene("Game");
    }

    #region AI Difficulity
    public void AI()
    {
        SceneManager.LoadScene("AIDifficulty");
    }
    public void EasyMode()
    {
        DBManager.ai = true;
        DBManager.difficulty = 1;
        SceneManager.LoadScene("Game");
        
    }
    public void MediumMode()
    {
        DBManager.ai = true;
        DBManager.difficulty = 2;
        SceneManager.LoadScene("Game");
       
    }
    public void HardMode()
    {
        DBManager.ai = true;
        DBManager.difficulty = 5;
        SceneManager.LoadScene("Game");
    }

    #endregion

    public void GoToLocalGame()
    {
        SceneManager.LoadScene("Game");
    }

    public void LogOut()
    {
        DBManager.LogOut();
        GoToMenu();
    }

    public void GoToLogOut()
    {
        SceneManager.LoadScene("LogOut");
    }
}
