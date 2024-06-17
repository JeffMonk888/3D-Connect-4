using UnityEngine;
using Unity.Networking.Transport;
public class NetMakeMove : NetMessage
{
    public int lastMoveLayer;// <-- potentially could be remov, this msg is never used
    public int lastMoveRow;
    public int lastMoveColumn;
    public int teamId;

    public NetMakeMove()
    {
        Code = OpCode.MAKE_MOVE;
    }
    public NetMakeMove(DataStreamReader reader)
    {
        Code = OpCode.MAKE_MOVE ;
        Deserialize(reader);
    }

    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
        writer.WriteInt(lastMoveRow);
        writer.WriteInt(lastMoveColumn);
        writer.WriteInt(lastMoveLayer);
        writer.WriteInt(teamId);

        
    }
    public override void Deserialize(DataStreamReader reader)
    {
        lastMoveRow = reader.ReadInt();
        lastMoveColumn = reader.ReadInt();
        lastMoveLayer = reader.ReadInt();
        teamId = reader.ReadInt();
        
    }
    public override void ReceivedOnClient()
    {
        NetUtility.C_MAKE_MOVE?.Invoke(this);
    }
    public override void ReceivedOnServer(NetworkConnection cnn)
    {
        NetUtility.S_MAKE_MOVE?.Invoke(this, cnn);

    }
}



