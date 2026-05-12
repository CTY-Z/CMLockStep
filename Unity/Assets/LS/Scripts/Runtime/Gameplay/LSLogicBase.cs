using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LS
{
    public abstract class LSLogicBase
    {
        public readonly Dictionary<int, LSObjectBase> dic_ID_Playerlogic = new();
        public int CurFrame { get; protected set; }
        public IReadOnlyDictionary<int, LSObjectBase> Dic_ID_PlayerLogic => dic_ID_Playerlogic;

        public virtual void Step(FrameSync.FrameInput frameInput)
        {
            CurFrame = frameInput.FrameNumber;
        }

        public abstract int GetHash();

        public abstract WorldSnapshot CreateSnapshot();
        public abstract void RestoreSnapshot(WorldSnapshot snapshot);
    }
}



