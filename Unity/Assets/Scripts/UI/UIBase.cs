using UnityEngine;
//using Code.Shared;
//using LiteNetLib;
//using LiteNetLib.Utils;

public class UIBase : MonoBehaviour
{
    public virtual void Pop()
    {
        UIManager.Get().Pop(this);
    }
    //public virtual void OnNetCallback(PacketType eventID, INetSerializable reader, NetPeer peer) { }
    //public virtual void OnUserCallback(PlayerStatus status) { }
    public virtual void ApplyLanguage() { }
}