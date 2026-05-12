using FrameSync;
using LS;
using System.Collections.Generic;
using UnityEngine;

public class LSWorld
{
    private const int InputDelayFrames = 3;
    private const int BufferFrames = 3;

    private LSTimer m_timer;

    private int m_latestServerFrame;
    private int m_localExecutedFrame;
    private bool m_hasReceivedServerFrame;

    private readonly Dictionary<int, FrameSync.FrameInput> m_serverInputFrames = new();
    private readonly Dictionary<int, FrameSync.PlayerInput> m_localInputHistory = new();
    private readonly Dictionary<int, FrameSync.PlayerInput> m_currentPlayerInputs = new();

    public int LatestServerFrame => m_latestServerFrame;
    public int LocalExecutedFrame => m_localExecutedFrame;

    public void Init()
    {
        m_latestServerFrame = 0;
        m_localExecutedFrame = 0;
        m_hasReceivedServerFrame = false;

        GameEntry.Instance.eventPool.Register<FrameSync.FrameInput>(EventDefine.S_C_FrameData, OnReceiveFrameInput);

        LSUpdateStart();
    }

    public void LSUpdateStart()
    {
        m_timer = new LSTimer();
        m_timer.OnTick += OnLogicTick;
        m_timer.Start();
    }

    private void OnLogicTick()
    {
        if (!m_hasReceivedServerFrame)
            return;

        SendInputForFutureFrame();
        ExecuteReadyFrames();
        CleanupLocalInputHistory();
    }

    private void SendInputForFutureFrame()
    {
        int clientId = GameEntry.Instance.model.login.clientID;
        if (clientId < 0)
            return;

        Vector2 input = CollectInput();
        int targetFrame = m_latestServerFrame + InputDelayFrames;

        FrameSync.PlayerInput playerInput = new FrameSync.PlayerInput
        {
            PlayerId = clientId,
            TargetFrame = targetFrame,
            InputX = input.x,
            InputY = input.y,
            Jump = Input.GetKey(KeyCode.Space),
        };

        m_localInputHistory[targetFrame] = playerInput;
        FrameSyncProcessor.C_S_FrameData(targetFrame, playerInput.InputX, playerInput.InputY, playerInput.Jump);
    }

    private Vector2 CollectInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        return new Vector2(horizontal, vertical);
    }

    private void ExecuteReadyFrames()
    {
        int maxExecutableFrame = m_latestServerFrame - BufferFrames;

        while (m_localExecutedFrame < maxExecutableFrame)
        {
            int nextFrame = m_localExecutedFrame + 1;

            if (!m_serverInputFrames.TryGetValue(nextFrame, out FrameSync.FrameInput frameInput))
                break;

            SimulateFrame(frameInput);
            m_serverInputFrames.Remove(nextFrame);
            m_localExecutedFrame = nextFrame;
        }
    }

    private void SimulateFrame(FrameSync.FrameInput frameInput)
    {
        foreach (FrameSync.PlayerInput input in frameInput.Inputs)
            m_currentPlayerInputs[input.PlayerId] = input;

        Debug.Log($"[LSWorld] Execute frame={frameInput.FrameNumber}, inputCount={frameInput.Inputs.Count}");
    }

    private void OnReceiveFrameInput(FrameSync.FrameInput frameInput)
    {
        if (!m_hasReceivedServerFrame)
        {
            m_hasReceivedServerFrame = true;
            m_localExecutedFrame = frameInput.FrameNumber - 1;
        }

        m_latestServerFrame = Mathf.Max(m_latestServerFrame, frameInput.FrameNumber);

        if (!m_serverInputFrames.ContainsKey(frameInput.FrameNumber))
        {
            m_serverInputFrames[frameInput.FrameNumber] = frameInput;
            Debug.Log($"[LSWorld] Receive server frame={frameInput.FrameNumber}, inputCount={frameInput.Inputs.Count}");
        }
    }

    private void CleanupLocalInputHistory()
    {
        int minFrame = m_localExecutedFrame - 60;
        if (minFrame <= 0)
            return;

        List<int> expiredFrames = new List<int>();
        foreach (int frame in m_localInputHistory.Keys)
        {
            if (frame < minFrame)
                expiredFrames.Add(frame);
        }

        foreach (int frame in expiredFrames)
        {
            m_localInputHistory.Remove(frame);
        }
    }

    public void ShutDown()
    {
        GameEntry.Instance.eventPool.Remove<FrameSync.FrameInput>(EventDefine.S_C_FrameData, OnReceiveFrameInput);
        m_timer?.Dispose();
    }
}
