using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelManager
{
    public LoginModel login;

    public void Init()
    {
        login = new LoginModel();
        login.Init();
    }
}

