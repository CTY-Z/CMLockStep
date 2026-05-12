using FrameSync;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace LS
{
    public class LockstepPlayer
    {
        public int PlayerID { get; private set; }
        public int PosX { get; private set; }
        public int PosY { get; private set; }

        private const int moveSpeed = 100;

        public LockstepPlayer(int playerID, int posX = 0, int posY = 0)
        {
            this.PlayerID = playerID;
            this.PosX = posX;
            this.PosY = posY;
        }

        public void Step(FrameSync.PlayerInput input)
        {
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
    }
}
