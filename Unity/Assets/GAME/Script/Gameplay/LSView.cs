using LS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LSView : LSViewBase
{
    public LSView(GameObject playerPrefab) : base(playerPrefab)
    {

    }

    public override void Sync(LSLogicBase logic)
    {
        base.Sync(logic);

        foreach (var playerBase in logic.dic_ID_Playerlogic.Values)
        {
            LSObject player = (LSObject)playerBase;
            Transform view = GetOrCreatePlayerView(player.PlayerID);
            view.position = new Vector3(player.PosX / 1000f, 0, player.PosY / 1000f);
        }
    }

    private Transform GetOrCreatePlayerView(int playerId)
    {
        if (dic_ID_PlayerView.TryGetValue(playerId, out Transform view))
            return view;

        GameObject obj = Object.Instantiate(m_playerPrefab);
        obj.name = $"Player_{playerId}";
        dic_ID_PlayerView[playerId] = obj.transform;
        return obj.transform;
    }
}
