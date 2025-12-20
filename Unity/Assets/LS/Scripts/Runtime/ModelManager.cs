using LS.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LS
{
    public class ModelManager
    {
        public LoginModel login;

        public void Init()
        {
            login = new LoginModel();
            login.Init();
        }
    }
}
