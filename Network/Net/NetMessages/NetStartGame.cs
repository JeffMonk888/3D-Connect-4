using System.Collections;
using System.Collections.Generic;
using Unity.Networking.Transport;
using UnityEngine;
using Unity.Collections;

public class NetStartGame : NetMessage
{
    public string usernameBlack;
    public int scoreBlack;
    public string usernameWhite;
    public int scoreWhite;
    

    public NetStartGame()
    {
        Code = OpCode.START_GAME;
    }
    public NetStartGame(DataStreamReader reader)
    {
        Code = OpCode.START_GAME;
        Deserialize(reader);
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
        writer.WriteInt(scoreBlack);
        writer.WriteInt(scoreWhite);

        // Serialize usernameBlack
        byte[] usernameBlackBytes = System.Text.Encoding.UTF8.GetBytes(usernameBlack);
        using (var nativeUsernameBlackBytes = new NativeArray<byte>(usernameBlackBytes, Allocator.Temp))
        {
            writer.WriteInt(nativeUsernameBlackBytes.Length); // Write the length of the usernameBlack
            writer.WriteBytes(nativeUsernameBlackBytes); // Then write the usernameBlack itself
        }

        // Serialize usernameWhite
        byte[] usernameWhiteBytes = System.Text.Encoding.UTF8.GetBytes(usernameWhite);
        using (var nativeUsernameWhiteBytes = new NativeArray<byte>(usernameWhiteBytes, Allocator.Temp))
        {
            writer.WriteInt(nativeUsernameWhiteBytes.Length); // Write the length of the usernameWhite
            writer.WriteBytes(nativeUsernameWhiteBytes); // Then write the usernameWhite itself
        }


        
    }
    public override void Deserialize(DataStreamReader reader)
    {

        scoreBlack = reader.ReadInt();
        scoreWhite = reader.ReadInt();

        // Deserialize usernameBlack
        int usernameBlackLength = reader.ReadInt();
        using (NativeArray<byte> usernameBlackBytes = new NativeArray<byte>(usernameBlackLength, Allocator.Temp))
        {
            reader.ReadBytes(usernameBlackBytes);
            usernameBlack = System.Text.Encoding.UTF8.GetString(usernameBlackBytes.ToArray());
        }

        // Deserialize usernameWhite
        int usernameWhiteLength = reader.ReadInt();
        using (NativeArray<byte> usernameWhiteBytes = new NativeArray<byte>(usernameWhiteLength, Allocator.Temp))
        {
            reader.ReadBytes(usernameWhiteBytes);
            usernameWhite = System.Text.Encoding.UTF8.GetString(usernameWhiteBytes.ToArray());
        }

    }
    public override void ReceivedOnClient()
    {
        NetUtility.C_START_GAME?.Invoke(this);
    }
    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_START_GAME?.Invoke(this, cnn);

    }
}




