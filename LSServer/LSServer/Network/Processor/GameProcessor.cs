using LSServer;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using static System.Runtime.InteropServices.JavaScript.JSType;

public class GameProcessor : BaseProcessor
{
    public GameProcessor() : base()
    {

    }

    // 3-1 server -> client
    public static void S_C_RoomSnapshot(IPEndPoint endPoint, Room.RoomSnapshot data)
    {
        ProtoHandler.OnSendMsg(EventDefine.S_C_RoomSnapshot, endPoint, data);
    }

    // 3-2 server -> client
    public static void S_C_RoomPlayerJoined(IPEndPoint endPoint, Room.RoomPlayerJoined data)
    {
        ProtoHandler.OnSendMsg(EventDefine.S_C_RoomPlayerJoined, endPoint, data);
    }

    // 3-3 server -> client
    public static void S_C_RoomPlayerLeft(IPEndPoint endPoint, Room.RoomPlayerLeft data)
    {
        ProtoHandler.OnSendMsg(EventDefine.S_C_RoomPlayerLeft, endPoint, data);
    }
}
