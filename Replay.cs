using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using Newtonsoft.Json;
using TMPro;
using UnityEngine.UIElements;

public class Replay : MonoBehaviour
{
    public static Replay instance;
    [SerializeField] private  GameObject b_block;
    [SerializeField] private  GameObject w_block;
    [SerializeField] private Transform startPoint;

    [SerializeField] private TMP_Text P1_User;
    [SerializeField] private TMP_Text P2_User;
    private List<GameObject> instantiatedBlocks = new List<GameObject>();
    private List<Vector3Int> moves = new List<Vector3Int>(); //List to track moves [row, column, layer]
    int currentPlayer = 1;
    [SerializeField] private TMP_Text Winner;
    private bool isKeyPressed = false;
    
    int pointer = 0; //which move through the list the client is on
    void Awake()
    {
        instance = this;
    }

    public void CallReplay(GameEntry.GameData data)
    {
        StartCoroutine(GetMove(data)); //Edit this so clients can choose from
    }
    IEnumerator GetMove(GameEntry.GameData data)
    {
        WWWForm form = new WWWForm();
        form.AddField("SessionID", data.SessionID);
        string url = "http://localhost:8888/3D_Connect_4/replay.php";
        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            string responseText = www.downloadHandler.text;
            string[] responseParts = responseText.Split('\t');
            if (responseParts[0] == "0" )
            {
                //Data in reponseParts[1]
                string movesJson = responseParts[1];
                P1_User.text = data.UserRole == "White" ? $"White: {DBManager.username} ({DBManager.score})" :  $"White: {data.OpponentUsername} ({data.OpponentScore})";
                P2_User.text = data.UserRole == "White" ? $"Black: {data.OpponentUsername} ({data.OpponentScore})" : $"Black: {DBManager.username} ({DBManager.score})" ;
                Winner.text = $"Winner : {data.WinnerUsername}";
                DeserialiseJson(movesJson);

            }
        }
    }

    private void DeserialiseJson(string json)
    {
       // Deserialize the JSON string into a List<List<int>>
        var listOfLists = JsonConvert.DeserializeObject<List<List<int>>>(json);

        if (listOfLists != null)
        {
            foreach (var coord in listOfLists)
            {
                // Assuming each inner list contains exactly 3 integers (x, y, z)
                if (coord.Count == 3)
                {
                    moves.Add(new Vector3Int(coord[0], coord[1], coord[2]));
                   
                }
            }
        }
    }
    
    void Update()
    {
        // Detect right arrow key for forward
        if (Input.GetKeyDown(KeyCode.RightArrow) && !isKeyPressed)
        {
            OnForwardArrow();
            isKeyPressed = true;
        }
        else if (Input.GetKeyUp(KeyCode.RightArrow))
        {
            isKeyPressed = false;
        }

        // Detect left arrow key for backward
        if (Input.GetKeyDown(KeyCode.LeftArrow) && !isKeyPressed)
        {
            OnBackArrow();
            isKeyPressed = true;
        }
        else if (Input.GetKeyUp(KeyCode.LeftArrow))
        {
            isKeyPressed = false;
        }
    }

    public void OnForwardArrow()
    {
        if (pointer < moves.Count)
        {
            var move = moves[pointer];
            PlayBlock(move.x, move.y,move.z);
            pointer++;
        }

    }
    public void OnBackArrow()
    {
        if (pointer > 0)
        {
            pointer--;
            var move = moves[pointer];
            RemoveBlock(move.x, move.y, move.z);
        }

    }

    #region Place Block
    public void PlayBlock(int row, int column, int layer)
    {
       
        StartCoroutine(PlayBlockCourtine(row, column, layer));

    }
    IEnumerator PlayBlockCourtine(int row, int column, int layer)
    {
        GameObject block = (Instantiate(currentPlayer == 1 ? b_block : w_block)) as GameObject;

        instantiatedBlocks.Add(block); // Track the instantiated block

        block.transform.position = new Vector3(startPoint.position.x + column * 2, startPoint.position.y, startPoint.position.z - row * 2);

        Vector3 goalPos = new Vector3(startPoint.position.x + column * 2, startPoint.position.y - layer, startPoint.position.z - row * 2);
        while (MoveToGoal(goalPos, block)) { yield return null; }
        SwitchPlayer();
    }

    
    
    #endregion
    

    #region Remove Block
    public void RemoveBlock(int row, int column, int layer)
    {
        StartCoroutine(RemoveBlockCoroutine(row, column, layer));
    }
    IEnumerator RemoveBlockCoroutine(int row, int coloumn, int layer)
    {
        var block = instantiatedBlocks[pointer];
        Vector3 goalPos = block.transform.position + Vector3.up * 2;//adjust the height as needed

        while (MoveToGoal(goalPos, block)) {yield return null;}

        Destroy(block);
        instantiatedBlocks.RemoveAt(pointer);
        SwitchPlayer();
    }

    bool MoveToGoal(Vector3 goalPos, GameObject block)
    {
        return goalPos != (block.transform.position = Vector3.MoveTowards(block.transform.position, goalPos, 10f * Time.deltaTime));
    }

    #endregion

    void SwitchPlayer()
    {
        currentPlayer = (currentPlayer == 1 ? 2 : 1);
        Debug.Log($"Switched to Player {currentPlayer}");
    }

}
