using FrameSync;
using LS;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LSLogic : LSLogicBase
{
    public override void Step(FrameInput frameInput)
    {
        base.Step(frameInput);

        foreach (var input in frameInput.Inputs.OrderBy(input => input.PlayerId))
        {
            LSObject player = GetOrCreatePlayer(input.PlayerId);
            player.Step(input);
        }

        //Debug.Log($"[LockstepWorld] Execute frame={frameInput.FrameNumber}, inputCount={frameInput.Inputs.Count}");
    }

    private LSObject GetOrCreatePlayer(int playerID)
    {
        LSObject player;
        if (!dic_ID_Playerlogic.TryGetValue(playerID, out LSObjectBase playerBase))
        {
            player = new LSObject(playerID);
            dic_ID_Playerlogic.Add(playerID, player);
        }
        else
            player = (LSObject)playerBase;

        return player;
    }

    public override WorldSnapshot CreateSnapshot()
    {
        WorldSnapshot snapshot = new WorldSnapshot();
        snapshot.frame = CurFrame;

        foreach (int playerID in dic_ID_Playerlogic.Keys.OrderBy(id => id))
        {
            LSObject player = (LSObject)dic_ID_Playerlogic[playerID];
            snapshot.dic_ID_objectSnapshot[playerID] = player.CreateSnapshot();
        }

        return snapshot;
    }

    public override void RestoreSnapshot(WorldSnapshot snapshot)
    {
        dic_ID_Playerlogic.Clear();
        CurFrame = snapshot.frame;

        foreach (var kv in snapshot.dic_ID_objectSnapshot)
        {
            ObjectSnapshot playerSnapshot = kv.Value;
            LSObject player = new LSObject(playerSnapshot.PlayerID);
            player.Restore(playerSnapshot);
            dic_ID_Playerlogic.Add(playerSnapshot.PlayerID, player);
        }
    }


    public override int GetHash()
    {
        int hash = 17;
        hash = hash * 31 + CurFrame;

        foreach (int PlayerID in dic_ID_Playerlogic.Keys.OrderBy(id => id))
        {
            hash = hash * 31 + PlayerID;
            hash = hash * 31 + ((LSObject)dic_ID_Playerlogic[PlayerID]).PosX;
            hash = hash * 31 + ((LSObject)dic_ID_Playerlogic[PlayerID]).PosY;
        }

        return hash;
    }
}
