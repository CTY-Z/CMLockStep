using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameModel
{
    private Dictionary<int, Room.PlayerData> dic_playerID_data;

    public void Init()
    {
        dic_playerID_data = new();

        GameEntry.Instance.eventPool.Register<Room.RoomPlayerJoined>(EventDefine.S_C_RoomPlayerJoined, OnPlayerJoined);
        GameEntry.Instance.eventPool.Register<Room.RoomPlayerLeft>(EventDefine.S_C_RoomPlayerLeft, OnPlayerLeft);
        GameEntry.Instance.eventPool.Register<Room.RoomSnapshot>(EventDefine.S_C_RoomSnapshot, OnRoom);
    }

    #region Event
    private void OnPlayerJoined(Room.RoomPlayerJoined joinedData)
    {
        dic_playerID_data[joinedData.Player.PlayerId] = joinedData.Player;
        Debug.Log($"PlayerJoined - {joinedData.Player.PlayerId}");
    }

    private void OnPlayerLeft(Room.RoomPlayerLeft leftData)
    {
        Debug.Log($"PlayerLeft - {leftData.PlayerId}");

        if (dic_playerID_data.ContainsKey(leftData.PlayerId))
            dic_playerID_data.Remove(leftData.PlayerId);
    }

    private void OnRoom(Room.RoomSnapshot roomData)
    {
        dic_playerID_data.Clear();

        foreach (var playerData in roomData.Players)
        {
            dic_playerID_data[playerData.PlayerId] = playerData;
            Debug.Log($" - - - - PlayerData - {playerData.PlayerId}");
        }
    }
    #endregion

    public IEnumerable<int> GetAllPlayer()
    {
        return dic_playerID_data.Keys.OrderBy(id => id);
    }
}
