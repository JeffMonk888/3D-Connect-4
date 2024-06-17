using Unity.Networking.Transport;
using UnityEngine;


public class NetMessage
{
    public OpCode Code { set; get; }

    public virtual void Serialize(ref DataStreamWriter writer) //put stuff inside
    {
        writer.WriteByte((byte)Code);
    }
    public virtual void Deserialize(DataStreamReader reader) //unpacking and putting, and making sure it suits 
    {
    }
    public virtual void ReceivedOnClient()
    {
    }
    public virtual void ReceivedOnServer(NetworkConnection cnn)
    {
    }


}