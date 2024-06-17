using UnityEngine;
using Unity.Networking.Transport;
public class NetRematch : NetMessage
{
    public int teamId;
    public byte wantRematch; //would this client want a rematch

    public NetRematch()
    {
        Code = OpCode.REMATCH;
    }
    public NetRematch(DataStreamReader reader)
    {
        Code = OpCode.REMATCH;
        Deserialize(reader);
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
        writer.WriteInt(teamId); //should I do teamID or use their playerId? if it doesn't work consider that
        writer.WriteByte(wantRematch); //1 = positive

    }
    public override void Deserialize(DataStreamReader reader)
    {
        teamId = reader.ReadInt();
        wantRematch = reader.ReadByte();
    }
    public override void ReceivedOnClient()
    {
        NetUtility.C_REMATCH?.Invoke(this);
    }
    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_REMATCH?.Invoke(this, cnn);

    }
}

