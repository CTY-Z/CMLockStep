using LS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LSObjectBase
{
    public int PlayerID { get; private set; }

    public LSObjectBase(int playerID)
    {
        this.PlayerID = playerID;
    }

    public virtual void Step(FrameSync.PlayerInput input)
    {

    }

    public abstract ObjectSnapshot CreateSnapshot();
}
