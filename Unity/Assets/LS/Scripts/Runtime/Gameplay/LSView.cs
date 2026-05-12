using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LS 
{
    public class LSView
    {
        private readonly Dictionary<int, Transform> dic_ID_PlayerView = new();
        private readonly GameObject m_playerPrefab;

        public LSView(GameObject playerPrefab)
        {
            this.m_playerPrefab = playerPrefab;
        }

        public void Sync(LSLogic logic)
        {
            foreach (var player in logic.dic_ID_Playerlogic.Values)
            {
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
}

