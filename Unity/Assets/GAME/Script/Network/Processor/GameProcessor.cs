using LS;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;

public class GameProcessor : BaseProcessor
{
    public GameProcessor() : base()
    {
        Add(1, S_C_RoomSnapshot);
        Add(2, S_C_RoomPlayerJoined);
        Add(3, S_C_RoomPlayerLeft);
    }

    // 3-1 server -> client
    public static void S_C_RoomSnapshot(byte[] dataByte)
    {
        var result = ProtobufHelper.DecodeData<Room.RoomSnapshot>(dataByte);
        GameEntry.Instance.eventPool.Fire(EventDefine.S_C_RoomSnapshot, result);
    }

    // 3-2 server -> client
    public static void S_C_RoomPlayerJoined(byte[] dataByte)
    {
        var result = ProtobufHelper.DecodeData<Room.RoomPlayerJoined>(dataByte);
        GameEntry.Instance.eventPool.Fire(EventDefine.S_C_RoomPlayerJoined, result);
    }

    // 3-3 server -> client
    public static void S_C_RoomPlayerLeft(byte[] dataByte)
    {
        var result = ProtobufHelper.DecodeData<Room.RoomPlayerLeft>(dataByte);
        GameEntry.Instance.eventPool.Fire(EventDefine.S_C_RoomPlayerLeft, result);
    }
}
