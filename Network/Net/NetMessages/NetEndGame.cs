using UnityEngine;
using Unity.Networking.Transport;
public class NetEndGame : NetMessage
{
    public int teamId;
    public byte EndGame; //1 = winner

    public NetEndGame()
    {
        Code = OpCode.END_GAME;
    }

    public NetEndGame(DataStreamReader reader)
    {
        Code = OpCode.END_GAME;
        Deserialize(reader);
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
        writer.WriteInt(teamId); //should I do teamID or use their playerId? if it doesn't work consider that
        writer.WriteByte(EndGame); //1 = positive

    }
    public override void Deserialize(DataStreamReader reader)
    {
        teamId = reader.ReadInt();
        EndGame = reader.ReadByte();
    }
    public override void ReceivedOnClient()
    {
        NetUtility.C_END_GAME?.Invoke(this);
    }
    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_END_GAME?.Invoke(this, cnn);

    }
}
