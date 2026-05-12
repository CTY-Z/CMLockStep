using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LS
{
    public class ObjectSnapshot
    {
        public int PlayerID;
        public int PosX;
        public int PosY;
    }

    public class WorldSnapshot
    {
        public int frame;
        public Dictionary<int, ObjectSnapshot> dic_ID_objectSnapshot = new();
    }
}

