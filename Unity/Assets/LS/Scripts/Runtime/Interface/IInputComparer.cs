using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LS
{
    public interface IInputComparer
    {
        bool IsMatch(FrameSync.PlayerInput predicted, FrameSync.PlayerInput authoritative);
        bool IsFrameInputMatch(FrameSync.FrameInput predictedFrame, FrameSync.FrameInput authoritativeFrame);
    }
}
