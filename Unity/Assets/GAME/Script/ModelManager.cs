using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModelManager
{
    public LoginModel login;
    public GameModel game;

    public void Init()
    {
        login = new LoginModel();
        login.Init();

        game = new GameModel();
        game.Init();
    }
}
