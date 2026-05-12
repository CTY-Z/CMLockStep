using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LS 
{
    public class LSViewBase
    {
        protected readonly Dictionary<int, Transform> dic_ID_PlayerView = new();
        protected readonly GameObject m_playerPrefab;

        public LSViewBase(GameObject playerPrefab)
        {
            this.m_playerPrefab = playerPrefab;
        }

        public virtual void Sync(LSLogicBase logic)
        {

        }


    }
}

