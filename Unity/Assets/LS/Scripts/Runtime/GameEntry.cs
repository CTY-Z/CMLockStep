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
        public NetworkManager network;
        public ModelManager model;

        public bool isCreate = false;

        public UDPClient client;

        private void Awake()
        {
            eventPool = new EventPool();
            network = new NetworkManager();
            model = new ModelManager();
        }

        private void Start()
        {
            network.Init();
            model.Init();

            if (isCreate)
                Instantiate(client, this.transform);

            //Heartbeat hb = new Heartbeat();
            //hb.Str = "ping";
            //byte[] dataByte = ProtobufHelper.Encode(1, 1, hb);
            //
            //Debug.Log(dataByte);
            //
            //var meta = ProtobufHelper.DecodeHeader(dataByte);
            //Debug.Log(meta);
            //
            //Debug.Log(ProtobufHelper.DecodeData<Heartbeat>(dataByte).Str);
        }

    }
}

