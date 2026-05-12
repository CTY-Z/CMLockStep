using FrameSync;
using LS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LSObject : LSObjectBase
{
    public int PosX { get; protected set; }
    public int PosY { get; protected set; }

    private const int moveSpeed = 100;

    public LSObject(int playerID, int posX = 0, int posY = 0) : base(playerID)
    {
        this.PosX = posX;
        this.PosY = posY;
    }

    public override void Step(FrameSync.PlayerInput input)
    {
        base.Step(input);

        int inputX = NormalizeInput(input.InputX);
        int inputY = NormalizeInput(input.InputY);

        PosX += inputX * moveSpeed;
        PosY += inputY * moveSpeed;
    }

    private int NormalizeInput(float value)
    {
        if (value > 0.1f) return 1;
        if (value < -0.1f) return -1;
        return 0;
    }

    public override ObjectSnapshot CreateSnapshot()
    {
        //todo objectpool
        return new ObjectSnapshot { PlayerID = PlayerID, PosX = PosX, PosY = PosY, };
    }

    public void Restore(ObjectSnapshot snapshot)
    {
        PosX = snapshot.PosX;
        PosY = snapshot.PosY;
    }
}
