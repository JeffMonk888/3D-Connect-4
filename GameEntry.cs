using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using UnityEngine.SceneManagement;
using Unity.VisualScripting;


public class GameEntry : MonoBehaviour
{
    public static GameEntry instance;
    public GameObject gameEntryPrefab; // Assign this in the Inspector
    public Transform contentPanel; // Assign the ScrollView content panel


    [System.Serializable]
    public class GameDataList
    {
        public List<GameData> gameList;
    }

    [System.Serializable] 
    public struct GameData
    {
        public string SessionID;
        public string OpponentUsername;
        public string WinnerUsername;
        public string OpponentScore;
        public string UserRole;
        public int TotalMoves; // Use the appropriate type for moves

    }

    void Start()
    {
        StartCoroutine(FetchGameHistory(DBManager.userid));
    }
    void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    IEnumerator FetchGameHistory(int userid)
    {
        WWWForm form = new WWWForm();
        form.AddField("userid", userid);

        string url = "http://localhost:8888/3D_Connect_4/retreivedata.php";
        using (UnityWebRequest www = UnityWebRequest.Post(url, form))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log(www.downloadHandler.text);
                GameDataList gameDataList = JsonUtility.FromJson<GameDataList>("{\"gameList\":" + www.downloadHandler.text + "}");
                PopulateGameHistoryUI(gameDataList.gameList);
            }
            else
            {
                Debug.LogError("Error in fetching game history: " + www.error);
            }
	   
        }

        
    }

    private void PopulateGameHistoryUI(List<GameData> gameHistory)
    {
        foreach (var child in contentPanel.GetComponentsInChildren<GameEntry>())
        {
            Destroy(child.gameObject); // Clear existing entries
        }

        foreach (GameData data in gameHistory)
        {
            GameObject entryObject = Instantiate(gameEntryPrefab, contentPanel);
            TMP_Text[] texts = entryObject.GetComponentsInChildren<TMP_Text>();
            UnityEngine.UI.Button entryButton = entryObject.GetComponentInChildren<UnityEngine.UI.Button>();
            texts[0].text = data.UserRole == "White" ? $"{DBManager.username} ({DBManager.score})" :  $"{data.OpponentUsername} ({data.OpponentScore})";
            texts[1].text = data.UserRole == "White" ? $"{data.OpponentUsername} ({data.OpponentScore})" : $"{DBManager.username} ({DBManager.score})" ;
            texts[2].text = data.WinnerUsername == DBManager.username ? "Win" : "Lose";
            texts[3].text = data.TotalMoves.ToString();


            // Add an event listener to the button that passes the specific GameData instance
            entryButton.onClick.AddListener(() => OnGameEntryClick(data));
        }
    }
    private void OnGameEntryClick(GameData data)
    {
        // Now you have the data for the specific game entry that was clicked
        // You can use this data to transition to the replay scene or do something else
        SceneManager.LoadScene("Replay");
        Debug.Log($"Game entry clicked with Session ID: {data.SessionID}");
        
        // For example, if you want to start a replay:
        StartCoroutine(CallReplay(data));
       
    }

    private IEnumerator CallReplay(GameData data)
    {
        
        yield return null; 
        Debug.Log("Replay");
        Replay.instance.CallReplay(data);
    
    }
}
