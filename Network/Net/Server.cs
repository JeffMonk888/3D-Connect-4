using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine.Networking;
using UnityEngine;
using Newtonsoft.Json;
using System.Data;

public class Server : MonoBehaviour
{
    public static Server Instance { set; get; }
    
    private void Awake()
    {
        Instance = this;
    }
    public Dictionary<int, List<int>> rooms = new Dictionary<int, List<int>>(); //{Room: Player in room (InternalId)}
    private Dictionary<int, GameRoom> gameRooms = new Dictionary<int, GameRoom>(); //{Room: type = Class GameRoom}
    public Dictionary<int, UserInfo> userInfo = new Dictionary<int, UserInfo>(); //Internalid : Class UserInfo


    private int roomCount = 1; //roomCount == SessionID //Initial room count //Retrive the largest session ID from the database 
    public NetworkDriver driver;
    private NativeList<NetworkConnection> connections;
    
    private bool isActive = false;
    private const float keepAliveTickRate = 20.0f;
    private float lastKeepAlive;
    public Action connectionDropped;

    public void Init(ushort port) // Start()
    {

        //init server
        driver = NetworkDriver.Create(); //data are sent through this driver
        NetworkEndPoint endpoint = NetworkEndPoint.AnyIpv4; //Who can connect to us
        endpoint.Port = port;


        if (driver.Bind(endpoint) != 0)
        {
            Debug.Log("unable to bind on port " + endpoint.Port);
            return;
	    }
        else 
	    {
            driver.Listen();
            Debug.Log("Currently listening on port " + endpoint.Port);
	    }
        
        //init the conenction list
        connections = new NativeList<NetworkConnection>(8, Allocator.Persistent);//Max player in the server
        isActive = true;
    }
    public void Shutdown()
    {
        if (isActive)
        {
            driver.Dispose();
            connections.Dispose();
            isActive = false;
        }
    }
    public void OnDestroy()
    {
        Shutdown();
    }

    public void Update()
    {
        if (!isActive)
            return;
        KeepAlive();

        driver.ScheduleUpdate().Complete();

        CleanupConnections();
        AcceptNewConnections();
        UpdateMessagePump();
    }

    private void KeepAlive()
    { 
        if (Time.time - lastKeepAlive > keepAliveTickRate)
        {
            lastKeepAlive = Time.time;
            foreach (var room in rooms)
            {
                int roomNumber = room.Key;
                List<int> playerIds = room.Value;
                Broadcast(new NetKeepAlive(), playerIds);
            }
            
	    }
    }

    private void CleanupConnections()
    //Cleaning up stale connections ensures you don't have any old connections lying around when you iterate through the list to check for new events.
    { 
        for (int i = 0; i < connections.Length; i++)
        { 
            if (!connections[i].IsCreated)
            {
                connections.RemoveAtSwapBack(i);
                --i;

	        }
	    }
    }
    private void AcceptNewConnections()
    {
        //Accept new connections
        NetworkConnection c;
        while ((c = driver.Accept()) != default(NetworkConnection))
        {
            connections.Add(c);
            // AssignPlayerToRoom(c);
            Debug.Log("Accept a connection");
	    }
    }

    public void AssignPlayerToRoom(NetworkConnection connection)
    {
        int playerId = connection.InternalId;

        // Check if the player's score is available in userInfo
        if (!userInfo.TryGetValue(playerId, out UserInfo joiningPlayerInfo))
        {
            Debug.LogError("Score not found for player: " + playerId);
            return; // Or handle this case as needed
        }

        int joiningPlayerScore = joiningPlayerInfo.Score;

        foreach (var room in rooms)
        {
            int roomNumber = room.Key;
            List<int> roomPlayers = room.Value;
            if (roomPlayers.Count == 1)
            {
                // Get the score of the existing player in the room
                int existingPlayerId = roomPlayers[0];
                if (userInfo.TryGetValue(existingPlayerId, out UserInfo existingPlayerInfo))
                {
                    int scoreDifference = Math.Abs(existingPlayerInfo.Score - joiningPlayerScore);
                    if (scoreDifference <= 100)
                    {

                        // Retrieve the list of player IDs for this room
                        List<int> playersInRoom = rooms[roomNumber];

                        //Create a new GameRoom with the list of player IDs
                        gameRooms[roomNumber] = new GameRoom(playersInRoom);
                        Debug.Log("Game can start in room " + roomNumber);


                        // Add player to room
                        roomPlayers.Add(playerId);
                        Debug.Log($"Player {playerId} joined room {roomNumber}");

                        
                        roomCount++;
                        return;
                    }
                }
            }
        }

        // If no suitable room is found, create a new one
        rooms[roomCount] = new List<int> { playerId };
        Debug.Log($"Player {playerId} created a new room {roomCount}");
    }


    public class UserInfo
    {
        public string Username { get; set; }
        public int Score { get; set; }
        public int userid  { get; set; }
    }

    public void UpdateUserInfo(int id, string username, int score, int userid)
    {
        if (userInfo.ContainsKey(id))
        {
            userInfo[id].Username = username;
            userInfo[id].Score = score;
            userInfo[id].userid = userid;
        }
        else
        {
            userInfo[id] = new UserInfo { Username = username, Score = score };
            print("sucess");
        }
    }


    private void UpdateMessagePump()
    {
        DataStreamReader stream; //use to process received Data events
        for (int i = 0; i < connections.Length; i++)
        {
            NetworkEvent.Type cmd;
            while ((cmd = driver.PopEventForConnection(connections[i], out stream)) != NetworkEvent.Type.Empty)
            { 
                if (cmd == NetworkEvent.Type.Data)
                {
                    NetUtility.OnData(stream, connections[i], this);
                }
                else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Some client disconnected from server");

                    int playerId = connections[i].InternalId;
                    // Check if the player was assigned to a room
                    foreach (var room in rooms)
                    {
                        int roomNumber = room.Key;
                        List<int> roomPlayers = room.Value;
                        if (roomPlayers.Contains(playerId))
                        {
                            Debug.Log($"Player {playerId} disconnected from Room {roomNumber}");
                            roomPlayers.Remove(playerId); // Remove the player from the room
                            break;
                        }
                    }
                    connections[i] = default(NetworkConnection);
                    connectionDropped?.Invoke();
                    // Shutdown(); //Because we are in a 2player game, a shutdown is needed so if one of the player exits then the game will stop.

		        }
	        }

	    }
    }
    public void SendToClient(NetworkConnection connection, NetMessage msg)
    {
        DataStreamWriter writer;
        driver.BeginSend(connection, out writer);
        msg.Serialize(ref writer);
        driver.EndSend(writer);
    }
    public void Broadcast(NetMessage msg, List<int> playerIds)
    {

        if (msg == null)
        {
            Debug.LogWarning("trying to boardcast a null message");
            return;
	    }
        Debug.Log($"Number of connections: {connections.Length}");

        for (int i = 0; i < connections.Length; i++)
            if (connections[i].IsCreated && playerIds.Contains(connections[i].InternalId))//sending to all clients in the room even the player who send the message
            {
                Debug.Log($"Sending {msg.Code} to :{connections[i].InternalId}");
                SendToClient(connections[i], msg);
                    
            }

    }

    public GameRoom GetGameRoom(int roomId) {
            if (gameRooms.TryGetValue(roomId, out GameRoom room)) {
                return room;
            }
            return null; // Or handle this appropriately
        }

    public void StartStoringMovesCoroutine(List<int> connections, string jsonData, int winner)
    {
        StartCoroutine(StoreMove(connections, jsonData, winner));
    }

    IEnumerator StoreMove(List<int> connections, string json, int winner)
    {
        int playerConnectionId1 = connections[0];
        int playerConnectionId2 = connections[1];

        UserInfo player1Info = userInfo[playerConnectionId1];
        UserInfo player2Info = userInfo[playerConnectionId2];

        WWWForm form = new WWWForm();
        form.AddField("p1_id", player1Info.userid);
        form.AddField("p2_id", player2Info.userid);
        form.AddField("winner_id", winner == 1 ? player1Info.userid : player2Info.userid);
        form.AddField("moves", json);

        Debug.Log(winner == 1 ? player1Info.userid : player2Info.userid);

        // Add the players name into the php script
        string url = "http://localhost:8888/3D_Connect_4/savedata.php";
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
                Debug.Log("Moves stored succcessfully");
            }
            else
            {
                Debug.Log("User creation failed. Error #" + www.downloadHandler.text);
            }

        }
        
    }

    


 
    public class GameRoom
    {
        const int rows = 4;
        const int columns = 4; 
        const int layers = 4;
        const int winlength = 4;
        private List<Vector3Int> coordinateList = new List<Vector3Int>(); //<-- put into the database //[row layer column]
        public int currentPlayer = 1;
        public List<int> connectionID; //list of the conenctionid within the 

        public GameRoom(List<int> playerIDs) // Constructor to set the roomID
        {
            connectionID = playerIDs;
        }

        int[,,] board = new int[layers, rows, columns]
        {
        // Layer 0 (top)
            {
                {0, 0, 0, 0},
                {0, 0, 0, 0},
                {0, 0, 0, 0},
                {0, 0, 0, 0}

            },

        // Layer 1
            {
                {0, 0, 0, 0},
                {0, 0, 0, 0},
                {0, 0, 0, 0},
                {0, 0, 0, 0}

            },

        // Layer 2
            {
                {0, 0, 0, 0},
                {0, 0, 0, 0},
                {0, 0, 0, 0},
                {0, 0, 0, 0}

            },

        // Layer 3
            {
                {0, 0, 0, 0},
                {0, 0, 0, 0},
                {0, 0, 0, 0},
                {0, 0, 0, 0},

            }

        };

        #region WinCheck - return Bool
        
        bool WinCheck()
        {
            if (x_axis_Check() || z_axis_Check() || z_x_axis_Check() || y_axis_Check() || x_y_axis_Check() || z_y_axis_Check() || x_y_z_axis_Check())
            {
                return true;
                
            }
            return false;
        }

        #region Checks
        //2D Checks - each layer (start form the fourth to increase efficency)
        bool x_axis_Check() //horizontal 
        {
            for (int i = layers - 1; i >= 0; i--)
            {
                for (int j = 0; j < rows; j++)
                {
                   
                    if (board[i, j, 0] > 0)
                    {
                        int a = board[i, j, 0];
                        int b = board[i, j, 1];
                        int c = board[i, j, 2];
                        int d = board[i, j, 3];
                        if (a == b && a == c && a == d)
                        {
                            Debug.Log("Win" + a);
                            return true;

                        }

                    }
                    
                }
            }
            return false;

        }

        bool z_axis_Check() //Vertical
        {
            for (int i = layers - 1; i >= 0; i--)
            {
                for (int k = 0; k < columns; k++)
                {
                    
                    if (board[i, 0, k] > 0)
                    {
                        int a = board[i, 0, k];
                        int b = board[i, 1, k];
                        int c = board[i, 2, k];
                        int d = board[i, 3, k];
                        if (a == b && a == c && a == d)
                        {
                            Debug.Log("Win" + a);
                            return true;

                        }

                    }
                    
                }
            }

            return false;

        }

        bool z_x_axis_Check() //diagonal
        {
            
            for (int i = layers - 1; i >= 0; i--)
            {
                //Right to left Check
                if (board[i, 0, 0] > 0)
                {
                    int a = board[i, 0, 0];
                    int b = board[i, 1, 1];
                    int c = board[i, 2, 2];
                    int d = board[i, 3, 3];
                    if (a == b && a == c && a == d)
                    {
                        Debug.Log("Win" + a);
                        return true;

                    }
                }

                if (board[i,0,3] > 0)
                {
                    int a = board[i, 0, 3];
                    int b = board[i, 1, 2];
                    int c = board[i, 2, 1];
                    int d = board[i, 3, 0];
                    if (a == b && a == c && a == d)
                    {
                        Debug.Log("Win" + a);
                        return true;

                    }
                }
            }

            return false;
        }



        //3D multi-layer involvement

        bool y_axis_Check() //diagonal
        {
            for (int i = layers - 1; i >= 3; i--)
            {
                for (int j = 0; j < rows; j++)
                {
                    for (int k = 0; k < columns; k++)
                    {
                        if (board[i, j, k] > 0)
                        {
                            int a = board[i, j, k];
                            int b = board[i - 1, j, k];
                            int c = board[i - 2, j, k];
                            int d = board[i - 3, j, k];
                            if (a == b && a == c && a == d)
                            {
                                Debug.Log("Win" + a);
                                return true;

                            }

                        }
                    }
                }
            }
            return false;
        }

        bool x_y_axis_Check()
        {
            
            for (int j = 0; j < rows; j++)
            {
                
                if (board[3, j, 0] > 0)
                {
                    int a = board[3, j, 0];
                    int b = board[2, j, 1];
                    int c = board[1, j, 2];
                    int d = board[0, j, 3];
                    if (a == b && a == c && a == d)
                    {
                        Debug.Log("Win" + a);
                        return true;

                    }

                }

                if (board[3, j, 3] > 0)
                {
                    int a = board[3, j, 3];
                    int b = board[2, j, 2];
                    int c = board[1, j, 1];
                    int d = board[0, j, 0];
                    if (a == b && a == c && a == d)
                    {
                        Debug.Log("Win" + a);
                        return true;

                    }

                }
                    
            
            }
            return false;

        }

        bool z_y_axis_Check()
        {
            
            for (int k = 0; k < columns; k++)
            {
                if (board[3, 0, k] > 0)
                {
                    int a = board[3, 0, k];
                    int b = board[2, 1, k];
                    int c = board[1, 2, k];
                    int d = board[0, 3, k];
                    
                    if (a == b && a == c && a == d)
                    {
                        Debug.Log("Win" + a);
                        return true;

                    }
                }

                if (board[3, 3, k] > 0)
                {
                    int a = board[3, 3, k];
                    int b = board[2, 2, k];
                    int c = board[1, 1, k];
                    int d = board[0, 0, k];

                    if (a == b && a == c && a == d)
                    {
                        Debug.Log("Win" + a);
                        return true;

                    }
                }
            }
                
            
            return false;
        }


        bool x_y_z_axis_Check() //through all layers
        {
            if (board[3,0,0] > 0)
            {
                int a = board[3, 0, 0];
                int b = board[2, 1, 1];
                int c = board[1, 2, 2];
                int d = board[0, 3, 3];

                if (a == b && a == c && a == d)
                {
                    Debug.Log("Win" + a);
                    return true;

                }

            }

            if (board[3,0,3] > 0)
            {
                int a = board[3, 0, 3];
                int b = board[2, 1, 2];
                int c = board[1, 2, 1];
                int d = board[0, 3, 0];

                if (a == b && a == c && a == d)
                {
                    Debug.Log("Win" + a);
                    return true;

                }

            }

            if (board[3,3,0] > 0)
            {
                int a = board[3, 3, 0];
                int b = board[2, 2, 1];
                int c = board[1, 1, 2];
                int d = board[0, 0, 3];

                if (a == b && a == c && a == d)
                {
                    Debug.Log("Win" + a);
                    return true;

                }

            }

            if (board[3,3,3] > 0)
            {
                int a = board[3, 3, 3];
                int b = board[2, 2, 2];
                int c = board[1, 1, 1];
                int d = board[0, 0, 0];

                if (a == b && a == c && a == d)
                {
                    Debug.Log("Win" + a);
                    return true;

                }

            }


            return false;
        }

        #endregion
        #endregion

        #region Game

        public void DropBlock(int layer, int row, int column, int player)
        {
            board[layer, row, column] = player;
            
            coordinateList.Add(new Vector3Int(row, column, layer));

            //win check
            WinCondition(WinCheck());
        }

        void SwitchPlayer()
        {
            currentPlayer = (currentPlayer == 1 ? 2 : 1);
            Debug.Log($"Switched to Player {currentPlayer}");
        }

        
        #endregion 

        #region GameOver
        public void WinCondition(bool winner)
        {
            if (winner)
            {
                //save the board
                OnWinnerDetermined();
                Reset_Board();
                //BroadcastMessage that there is a winner
                
                NetEndGame endGameMsg = new NetEndGame();
                endGameMsg.teamId = currentPlayer;
                endGameMsg.EndGame = 1;
                currentPlayer = 0;
                Debug.Log("WinCondition");
                Debug.Log($"Broadcasting end game message to players in room: {String.Join(", ", connectionID)}");
                Server.Instance.Broadcast(endGameMsg, connectionID);

            }
            else
            {
                SwitchPlayer();
            }
            
        }

        public void OnWinnerDetermined()
        {
            string jsonData = SerialiseJson(coordinateList);
            Server.Instance.StartStoringMovesCoroutine(connectionID, jsonData, currentPlayer);

        }

        private string SerialiseJson(List<Vector3Int> vectorList)
        {
            // Convert the List<Vector3Int> into a List<List<int>>
            var listOfLists = new List<List<int>>();
            foreach (var vector in vectorList)
            {
                listOfLists.Add(new List<int> { vector.x, vector.y, vector.z });
            }

            // Serialize the List<List<int>> into a JSON string
            string json = JsonConvert.SerializeObject(listOfLists);
            return json;
        }

        public void Reset_Board()
        {
            for (int i = 0; i < 4; i++) //layers
            {

                for (int j = 0; j < 4; j++) //rows
                {

                    for (int k = 0; k < 4; k++) //columns
                    {
                        board[i,j,k] = 0;
                    }
                }
            }
        }

        #endregion 
        string DebugBoard()
        {
            string s = "";
            string seperator = ",";
            string boarder = "|";

            for (int i = 0; i < 4; i++) //layers
            {

                for (int j = 0; j < 4; j++) //rows
                {
                    s += boarder;

                    for (int k = 0; k < 4; k++) //columns
                    {
                        s += board[i, j, k];
                        if (k != 3)
                        {
                            s += seperator;
                        }
                        else
                        {
                            s += boarder;
                        }
                    }
                    s += "\n";

                }
                s += "\n";
            }
            return s;
        }


    }

}

