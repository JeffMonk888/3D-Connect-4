using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;
//Each person's networking stuff
public class ClientNetworkHandler : MonoBehaviour
{
    public static ClientNetworkHandler Instance { set; get; }
    private int currentTeam = 1; // <-- Different for each client
    private SceneManagerNavigation sceneManagerNavigation;
    private bool[] playerRematch = { false, false };

    public void OnClickOnHost()
    {
        RegisterEvents();
        DontDestroyOnLoad(gameObject);
        Instance = this;
    }
    private void RegisterEvents()
    {
        NetUtility.C_WELCOME += OnWelcomeClient;
        NetUtility.C_START_GAME += OnStartGameClient;
        NetUtility.C_MAKE_MOVE += OnMakeMoveClient;
        NetUtility.C_REMATCH += OnRematchClient;
        NetUtility.C_END_GAME += OnEndGameClient;
    }
    private void UnRegisterEvents()
    {

        NetUtility.C_WELCOME -= OnWelcomeClient;
        NetUtility.C_START_GAME -= OnStartGameClient;
        NetUtility.C_REMATCH -= OnRematchClient;
        NetUtility.C_END_GAME -= OnEndGameClient;
    }

    public void CallUnRegisterEvents()
    {
        UnRegisterEvents();
    }
    public int GetCurrentTeam() //used in game GameManager
    {
        return currentTeam;
    }

    private void OnWelcomeClient(NetMessage msg)
    {
        //recieve the connection message
        NetWelcome nw = msg as NetWelcome;

        //assign team
        currentTeam = nw.AssignedTeam;

        Debug.Log($"My assigned team is {nw.AssignedTeam}");

    }

    private void OnStartGameClient(NetMessage msg)
    {
        
        sceneManagerNavigation = FindObjectOfType<SceneManagerNavigation>();
        sceneManagerNavigation.GoToOnlineGame();

        StartCoroutine(SetupUI(msg as NetStartGame));
    
    }
    private IEnumerator SetupUI(NetStartGame msg)
    {
        // Wait until the new scene has started (you might need a frame delay)
        yield return null; // Optional: yield return new WaitUntil(() => some condition);
        
        if (GameManager.instance != null)
        {
            GameManager.instance.SetUserDisplay(msg.usernameBlack, msg.usernameWhite,msg.scoreBlack, msg.scoreWhite);
            GameManager.instance.PlayerTurn.text = (currentTeam == 1? "Your Turn" : "Opponent's Turn");
            DBManager.online = true;
        }
        else
        {
            Debug.LogError("GameManager instance is not set after scene load.");
        }
    
    }
    
    private void OnMakeMoveClient(NetMessage msg)
    {
        NetMakeMove mm = msg as NetMakeMove;

        Debug.Log($"MM : CurrentPlayer - {mm.teamId},Row - {mm.lastMoveRow}, Column - {mm.lastMoveColumn}, Layer - {mm.lastMoveLayer}");

        GameManager.instance.PlayBlock(mm.lastMoveRow, mm.lastMoveColumn, mm.lastMoveLayer, mm.teamId);
        if (GameManager.instance.PlayerTurn.text == "Your Turn")
        {
            GameManager.instance.PlayerTurn.text = "Opponent's Turn";
        }
        else
        {
            GameManager.instance.PlayerTurn.text = "Your Turn";
        }
    }
    
    
    private void OnRematchClient(NetMessage msg)
    {

        NetRematch rm = msg as NetRematch;
        playerRematch[rm.teamId - 1] = rm.wantRematch == 1; //Return Bool if == 1 -> true

        //Activate the rematchIndicator
        if (rm.teamId != currentTeam)
        {
            GameManager.instance.rematchIndicator.GetChild((rm.wantRematch == 1) ? 0 : 1).gameObject.SetActive(true);
            
            //Set the rematchButton to be false
        }
        
        if (playerRematch[0] && playerRematch[1])
        {

            //ResetBoard physically
            GameManager.instance.GameReset();

            //Change who goes first
            currentTeam = (currentTeam == 1 ? 2 : 1);

            GameManager.instance.PlayerTurn.text = (currentTeam == 1 ? "Your Turn" : "Opponent's Turn");
            //Reset the clients' board
            Playfield.instance.Reset_Board();

            //Reset the server's board which is storing in the database
        }
    }

    private void OnEndGameClient(NetMessage msg)
    {
        NetEndGame em = msg as NetEndGame;
        if (em != null && em.EndGame == 1)
        {
            Debug.Log("Game ended. Winning team: " + em.teamId);
            GameManager.instance.win.text = (em.teamId == 1 ? "Black Won" : "White Won");
            GameManager.instance.WinCondition(true);
        }
        
    }
}
