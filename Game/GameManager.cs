using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using TMPro;
using UnityEditor.VersionControl;
using Unity.PlasticSCM.Editor.WebApi;


//For Client, handling drop block and loading scene when win
public class GameManager : MonoBehaviour
{
    
    public static GameManager instance;
    private SceneManagerNavigation sceneManagerNavigation;

    // List to track all instantiated block prefabs
    private List<GameObject> instantiatedBlocks = new List<GameObject>();
    //bool activeTurn = true;

    [SerializeField] private  GameObject b_block;
    [SerializeField] private  GameObject w_block;
    [SerializeField] private  Transform startPoint;
    [SerializeField] private GameObject gameOverWindow;
    [SerializeField] private GameObject userNameDisplay;
    [SerializeField] private GameObject playerTurnWindow;

    [SerializeField] private TMP_Text blackPlayerName;
    [SerializeField] private TMP_Text blackPlayerScore;
    [SerializeField] private TMP_Text whitePlayerName;
    [SerializeField] private TMP_Text whitePlayerScore;
    [SerializeField] private TMP_Text winner; 
    public TMP_Text PlayerTurn;
    public TMP_Text win;

    public Transform rematchIndicator;

    

    bool activeTurn = true;
    int currentPlayer = 1;
    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        gameOverWindow.SetActive(false);
        userNameDisplay.SetActive(false);
        if (!DBManager.online)
        {
            PlayerTurn.text = $"Player {currentPlayer}'s Turn";
        }
        
    }

    public void SetUserDisplay(string usernameBlack, string usernameWhite, int scoreBlack, int scoreWhite)
    {
        userNameDisplay.SetActive(true);
        blackPlayerName.text = "Black: " + usernameBlack;
        blackPlayerScore.text = "Score: " + scoreBlack.ToString();

        whitePlayerName.text = "White: " + usernameWhite;
        whitePlayerScore.text = "Score: " + scoreWhite.ToString();
    }


    public void ColumnPressed(int row, int column)
    {
        if (DBManager.online)
        {
            int currentTeam = FindObjectOfType<ClientNetworkHandler>().GetCurrentTeam();
            Debug.Log(currentTeam);

            int layer = Playfield.instance.ValidMove(row, column);
            NetMakeMove mm = new NetMakeMove();
            mm.lastMoveRow = row;
            mm.lastMoveColumn = column;
            mm.lastMoveLayer = layer;
            mm.teamId = currentTeam;
            Client.Instance.SendToServer(mm);
        }
        else
        {
            if (!activeTurn)
            {
                Debug.Log("wait until turn is over");
                return;
            }

            int layer = Playfield.instance.ValidMove(row, column);
            if(layer != -1)
            {
                PlayBlock(row, column, layer, currentPlayer);
            }
        }

    }
    
    public void PlayBlock(int row, int column, int layer, int currentPlayer)
    {
        StartCoroutine(PlayBlockCourtine(row, column, layer, currentPlayer));
    }

    
    IEnumerator PlayBlockCourtine(int row, int column, int layer, int currentPlayer)
    {
        activeTurn = false;
        GameObject block = (Instantiate(currentPlayer == 1 ? b_block : w_block)) as GameObject;

        
        instantiatedBlocks.Add(block); // Track the instantiated block
        
        block.transform.position = new Vector3(startPoint.position.x + column * 2, startPoint.position.y, startPoint.position.z - row * 2);
        
        Vector3 goalPos = new Vector3(startPoint.position.x + column * 2, startPoint.position.y - layer, startPoint.position.z - row * 2);
        while (MoveToGoal(goalPos, block)) { yield return null; }
        
        Playfield.instance.DropBlock(layer, row, column, currentPlayer);

    }

    bool MoveToGoal(Vector3 goalPos, GameObject block)
    {
        return goalPos != (block.transform.position = Vector3.MoveTowards(block.transform.position, goalPos, 10f * Time.deltaTime));
    }
    
    public void WinCondition(bool winner)
    {
        if (winner)
        {
            gameOverWindow.SetActive(true);
            userNameDisplay.SetActive(false);
            playerTurnWindow.SetActive(false);
            rematchIndicator.gameObject.SetActive(true);
            rematchIndicator.GetChild(0).gameObject.SetActive(false);
            rematchIndicator.GetChild(1).gameObject.SetActive(false);

            if (!DBManager.online)
            {
                win.text = (currentPlayer == 1 ? "Black Won" : "White Won");
            }

        }
        else if(!DBManager.online)
        {
            activeTurn = true;
            SwitchPlayer();
        }
    }


    #region Options after game finshes 
    public void OnRematchButton()
    {
        if(DBManager.online)
        {
            NetRematch rm = new NetRematch();
            rm.teamId = FindObjectOfType<ClientNetworkHandler>().GetCurrentTeam();
            rm.wantRematch = 1; // 1 means want to rematch
            Client.Instance.SendToServer(rm);
        }
        else
        {
            GameReset();
            currentPlayer = 1;
            PlayerTurn.text = (currentPlayer == 1 ? "Black's Turn" : "White's Turn");
            Playfield.instance.Reset_Board();
            userNameDisplay.SetActive(false);
            currentPlayer = 1;
            activeTurn = true;
        }
        
    }

    public void OnExitButton()
    {
        if(DBManager.online)
        {
            NetRematch rm = new NetRematch();
            rm.teamId = FindObjectOfType<ClientNetworkHandler>().GetCurrentTeam();
            rm.wantRematch = 0; // 0 means dont want to rematch
            Client.Instance.SendToServer(rm);

            SceneManager.LoadScene("GameUI");
        }
        else
        {
            SceneManager.LoadScene("GameUI");
        }
        
    }

    public void OnNewGame()
    {
        if(!DBManager.online)
        {
            GameReset();
            Playfield.instance.Reset_Board();
            userNameDisplay.SetActive(false);
            currentPlayer = 1;
            activeTurn = true;

        }

        if(DBManager.online)
        {
            NetRematch rm = new NetRematch();
            rm.teamId = FindObjectOfType<ClientNetworkHandler>().GetCurrentTeam();
            rm.wantRematch = 0; // 0 means dont want to rematch
            Client.Instance.SendToServer(rm);
            SceneManager.LoadScene("OnlineMenu");

        }
       
    }


    #region restart the board

    public void GameReset()
    {
        Reset_Physical_Board();
        gameOverWindow.SetActive(false);
        userNameDisplay.SetActive(true);
        playerTurnWindow.SetActive(true);

    }

    
    private void Reset_Physical_Board()
    {
        //UI
        rematchIndicator.transform.GetChild(0).gameObject.SetActive(false);
        rematchIndicator.transform.GetChild(1).gameObject.SetActive(false);

        // clean up
        foreach (GameObject block in instantiatedBlocks)
        {
            Destroy(block);
        }
        instantiatedBlocks.Clear(); // Clear the list after destroying the objects
    }
    
    #endregion
    #endregion

    #region local game
    void SwitchPlayer()
    {
        currentPlayer = (currentPlayer == 1 ? 2 : 1);
        PlayerTurn.text = (currentPlayer == 1? "Player 1's Turn" : "Player 2's Turn");
        if (currentPlayer == 2 && DBManager.ai)
        {
            AI_Mode.instance.BestMove();
	    }
        
    }

    #endregion

}