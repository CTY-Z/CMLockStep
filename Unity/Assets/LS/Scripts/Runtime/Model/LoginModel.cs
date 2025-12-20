using Login;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LS.Model
{
    public class LoginModel
    {
        public int clientID = -1;
        public string playerName = "player_0";
        public DateTime lastSendTime = DateTime.MaxValue;
        public DateTime lastReceiveTime = DateTime.MaxValue;

        public void Init()
        {

        }

        public void SetConnectData(ConnectResponse data)
        {
            clientID = data.ClientId;
        }
    }
}

