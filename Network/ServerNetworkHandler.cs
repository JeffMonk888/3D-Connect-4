using System;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using Unity.Networking.Transport;
using UnityEngine;
//Each person's networking stuff
public class ServerNetworkHandler : MonoBehaviour
{
    private SceneManagerNavigation sceneManagerNavigation;
    // private int playerCount = 0; // <--- For server

    // Reference to the Server instance
    private Server serverInstance;
    private bool[] playerRematch = { false, false };

    public void OnClickOnHost()
    {
        RegisterEvents();
        DontDestroyOnLoad(gameObject);
    }
    private void RegisterEvents()
    {
        NetUtility.S_WELCOME += OnWelcomeServer;
        NetUtility.S_MAKE_MOVE += OnMakeMoveServer;
        NetUtility.S_REMATCH += OnRematchServer;
        NetUtility.S_END_GAME += OnEndGameServer;

    }

    private void UnRegisterEvents()
    {
        NetUtility.S_WELCOME -= OnWelcomeServer;
        NetUtility.S_MAKE_MOVE -= OnMakeMoveServer;
        NetUtility.S_REMATCH -= OnRematchServer;
        NetUtility.S_END_GAME -= OnEndGameServer;
    }
    
    //Server
    private void OnWelcomeServer(NetMessage msg, NetworkConnection cnn)
    {
        //Client has connected, assign a team and return the message back to the client
        NetWelcome nw = msg as NetWelcome;

        int score = nw.score;
        string username = nw.username;
        int userid = nw.userid;
        Server.Instance.UpdateUserInfo(cnn.InternalId, username, score, userid);
        Server.Instance.AssignPlayerToRoom(cnn);
        // //Assign a team ----------------------------------------------------------------
        foreach (var room in Server.Instance.rooms)
        {
            if (room.Value.Contains(cnn.InternalId))
            {
                int playerIndex = room.Value.IndexOf(cnn.InternalId);
                // Assign team based on player's position in the room
                nw.AssignedTeam = playerIndex + 1;

                Debug.Log($"player {cnn.InternalId} joined room {room.Key} with team number {nw.AssignedTeam}");
                break;
            }
        }

        //Store it into the dictionary
        Server.Instance.SendToClient(cnn, nw);
        // Server.Instance.UpdateUsername(cnn.InternalId, username );


        //Return back to the client
        foreach (var room in Server.Instance.rooms)
        {
            if (room.Value.Count == 2 && room.Value.Contains(cnn.InternalId))
            {
                NetStartGame nsg = new NetStartGame();

                // Assuming the first player in the list is Black and the second is White
                Server.UserInfo userInfoBlack = Server.Instance.userInfo[room.Value[0]];
                Server.UserInfo userInfoWhite = Server.Instance.userInfo[room.Value[1]];


                // Populate the NetStartGame message
                nsg.scoreBlack = userInfoBlack.Score;
                nsg.scoreWhite = userInfoWhite.Score;

                nsg.usernameBlack = userInfoBlack.Username;
                nsg.usernameWhite = userInfoWhite.Username;
                
                
                // Call the BroadcastToRoom method in the Server script
                Server.Instance.Broadcast(nsg, room.Value);
                break;
            }
        }

    }


    private void OnMakeMoveServer(NetMessage msg, NetworkConnection cnn)
    {
        //Recieve the message, broadcast it back
        NetMakeMove mm = msg as NetMakeMove;

        //receive, and broadcast it back
        foreach (var room in Server.Instance.rooms)
        {
            if (room.Value.Count == 2 && room.Value.Contains(cnn.InternalId))
            {
                //Make the move in the specific room in the GameRoom Class
                Server.GameRoom Room = Server.Instance.GetGameRoom(room.Key);
                if (mm.teamId == Room.currentPlayer)
                {
                    // Call the BroadcastToRoom method in the Server script if the player who sent is the one who meant to make the move
                    Server.Instance.Broadcast(msg, room.Value);
                    Room.DropBlock(mm.lastMoveLayer,mm.lastMoveRow,mm.lastMoveColumn, mm.teamId);
                }
                break;
            }
        }


    }

    private void OnRematchServer(NetMessage msg, NetworkConnection cnn)
    {
        //receive, and broadcast it back
        foreach (var room in Server.Instance.rooms)
        {
            if (room.Value.Count == 2 && room.Value.Contains(cnn.InternalId))
            {
                // Call the BroadcastToRoom method in the Server script
                Server.Instance.Broadcast(msg, room.Value);
                NetRematch rm = msg as NetRematch;
                playerRematch[rm.teamId - 1] = rm.wantRematch == 1;

                if (playerRematch[0] && playerRematch[1])
                {
                    Server.GameRoom gameRoom = Server.Instance.GetGameRoom(room.Key);
                    gameRoom.currentPlayer = 1;
                    gameRoom.Reset_Board();

                }
                break;

            }
            
        
        }
        
    }
    
    private void OnEndGameServer(NetMessage msg, NetworkConnection cnn)
    {

    }
    
}
