using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LS
{
    public class LSLogic
    {
        public readonly Dictionary<int, LockstepPlayer> dic_ID_Playerlogic = new();
        public int CurFrame { get; private set; }

        public IReadOnlyDictionary<int, LockstepPlayer> Dic_ID_PlayerLogic => dic_ID_Playerlogic;

        public void Step(FrameSync.FrameInput frameInput)
        {
            CurFrame = frameInput.FrameNumber;
            
            foreach (var input in frameInput.Inputs.OrderBy(input => input.PlayerId))
            {
                LockstepPlayer player = GetOrCreatePlayer(input.PlayerId);
                player.Step(input);
            }

            //Debug.Log($"[LockstepWorld] Execute frame={frameInput.FrameNumber}, inputCount={frameInput.Inputs.Count}");
        }

        private LockstepPlayer GetOrCreatePlayer(int playerID)
        {
            if (!dic_ID_Playerlogic.TryGetValue(playerID, out LockstepPlayer player))
                dic_ID_Playerlogic.Add(playerID, new LockstepPlayer(playerID));
            
            return player;
        }

        public int GetHash()
        {
            int hash = 17;
            hash = hash * 31 + CurFrame;

            foreach (int PlayerID in dic_ID_Playerlogic.Keys.OrderBy(id => id))
            {
                hash = hash * 31 + PlayerID;
                hash = hash * 31 + dic_ID_Playerlogic[PlayerID].PosX;
                hash = hash * 31 + dic_ID_Playerlogic[PlayerID].PosY;
            }

            return hash;
        }
    }
}



