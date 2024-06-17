using UnityEngine;
using Unity.Networking.Transport;
using Unity.Collections;
public class NetWelcome : NetMessage
{ 

    public int AssignedTeam { set; get; }
    public string username;

    public int score;

    public int userid;


    public NetWelcome()
    {
        Code = OpCode.WELCOME;
    }
    public NetWelcome(DataStreamReader reader)
    {
        Code = OpCode.WELCOME;
        Deserialize(reader);
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
        writer.WriteInt(AssignedTeam);
        writer.WriteInt(score);
        writer.WriteInt(userid);
        
        // Convert byte[] to NativeArray<byte>
        byte[] usernameBytes = System.Text.Encoding.UTF8.GetBytes(username);
        using (var nativeUsernameBytes = new NativeArray<byte>(usernameBytes, Allocator.Temp))
        {
            writer.WriteInt(nativeUsernameBytes.Length); // Write the length of the string
            writer.WriteBytes(nativeUsernameBytes); // Then write the string itself
        }
    }
    public override void Deserialize(DataStreamReader reader)
    {
        AssignedTeam = reader.ReadInt();
        score = reader.ReadInt();
        userid = reader.ReadInt();

        //Deserialize the string
        int usernameLength = reader.ReadInt(); // Read the length of the string
        using (NativeArray<byte> usernameBytes = new NativeArray<byte>(usernameLength, Allocator.Temp))
        {
            reader.ReadBytes(usernameBytes); // Read the bytes into the array
            username = System.Text.Encoding.UTF8.GetString(usernameBytes.ToArray()); // Convert bytes to string
        }
    }
    public override void ReceivedOnClient()
    {
        NetUtility.C_WELCOME?.Invoke(this);
    }
    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_WELCOME?.Invoke(this, cnn);
    
    }
}



