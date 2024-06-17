using System;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class Client : MonoBehaviour
{

    public static Client Instance { set; get; }

    private void Awake()
    {
        Instance = this;
    }

    public NetworkDriver driver;
    private NetworkConnection connection;

    private bool isActive = false;
    public Action connectionDropped;

    public void Init(string ip, ushort port)
    {
        driver = NetworkDriver.Create();
        NetworkEndPoint endpoint = NetworkEndPoint.Parse(ip, port);
        endpoint.Port = port;

        connection = driver.Connect(endpoint);
        Debug.Log("Attempting to connect to Server on " + endpoint.Address);

        isActive = true;

        RegisterToEvent();
    }

    public void Shutdown()
    {
        if (isActive)
        {
            UnregisterToEvent();
            driver.Dispose();
            isActive = false;
            connection = default(NetworkConnection);
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

        driver.ScheduleUpdate().Complete();

        CheckAlive();
        UpdateMessagePump();
    }

    private void CheckAlive()
    {
        if (!connection.IsCreated && isActive)
        {
            Debug.Log("Lost connection to server");
            connectionDropped?.Invoke();
            Shutdown();
        }
    }

    private void UpdateMessagePump() //Add the username into here
    {
        DataStreamReader stream;
        NetworkEvent.Type cmd;
        while ((cmd = connection.PopEvent(driver, out stream)) != NetworkEvent.Type.Empty)
        {
            if (cmd == NetworkEvent.Type.Connect)
            {
                NetWelcome welcomeMsg = new NetWelcome();
                welcomeMsg.username = DBManager.username;
                welcomeMsg.score = DBManager.score;
                welcomeMsg.userid = DBManager.userid;

                SendToServer(welcomeMsg);
                // SendToServer()//Sending the server the Username
                Debug.Log("We are Connect");
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                NetUtility.OnData(stream, default(NetworkConnection));
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                Debug.Log("Client disconnected from server");
                connectionDropped?.Invoke();
                Shutdown();
            }
        }
    }

    public void SendToServer(NetMessage msg)
    {
        DataStreamWriter writer;
        driver.BeginSend(connection, out writer);
        msg.Serialize(ref writer);
        driver.EndSend(writer);
    }

    // Event parsing
    private void RegisterToEvent()
    {
        NetUtility.C_KEEP_ALIVE += OnKeepAlive;
    }

    private void UnregisterToEvent()
    {
        NetUtility.C_KEEP_ALIVE -= OnKeepAlive;
    }

    private void OnKeepAlive(NetMessage nm)
    {
        SendToServer(nm);
    }
}
