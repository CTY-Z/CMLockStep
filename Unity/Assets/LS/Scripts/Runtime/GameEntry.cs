using Login;
using LS.Utils;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LS
{
    public class GameEntry : Singleton<GameEntry>
    {
        public EventPool eventPool;

        public bool isCreate = false;

        public UDPClient client;

        private void Awake()
        {
            eventPool = new EventPool();
        }

        private void Start()
        {
            if (isCreate)
                Instantiate(client, this.transform);

            Heartbeat hb = new Heartbeat();
            hb.Str = "ping";
            byte[] dataByte = ProtobufHelper.Encode(1, 1, hb);

            Debug.Log(dataByte);

            Heartbeat hbReturn = ProtobufHelper.Decode<Heartbeat>(dataByte);
            Debug.Log(hbReturn);
        }

    }
}

