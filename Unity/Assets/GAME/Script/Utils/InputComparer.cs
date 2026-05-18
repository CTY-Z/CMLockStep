using FrameSync;
using LS;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InputComparer : IInputComparer
{
    public bool IsMatch(PlayerInput predicted, PlayerInput authoritative)
    {
        if (predicted == null || authoritative == null)
            return false;

        return predicted.InputX == authoritative.InputX &&
            predicted.InputY == authoritative.InputY &&
            predicted.Jump == authoritative.Jump;
    }

    public bool IsFrameInputMatch(FrameInput predicted, FrameInput authoritative)
    {
        if (predicted == null || authoritative == null)
            return false;

        //todo:暂时只检测玩家一人的输入
        int localPlayerID = GameEntry.Instance.model.login.clientID;
        var authoritativeInput = authoritative.Inputs.FirstOrDefault(a => a.PlayerId == localPlayerID);
        var predictedInput = predicted.Inputs.FirstOrDefault(a => a.PlayerId == localPlayerID);

        if (predictedInput == null || authoritativeInput == null)
            return false;

        return predictedInput.InputX == authoritativeInput.InputX &&
            predictedInput.InputY == authoritativeInput.InputY &&
            predictedInput.Jump == authoritativeInput.Jump;
    }
}
