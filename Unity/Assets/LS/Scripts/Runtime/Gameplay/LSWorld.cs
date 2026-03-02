using LS;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LSWorld
{
    private LSTimer m_timer;

    private int m_frameCount;
    public int FrameCount { get { return m_frameCount; } }

    public void Init()
    {
        m_frameCount = 0;

        LSUpdateStart();
    }

    public void LSUpdateStart()
    {
        m_timer = new LSTimer();
        m_timer.OnTick += () =>
        {
            m_frameCount++;
            FrameSyncProcessor.C_S_FrameData(m_frameCount, 0, 0);
        };
        m_timer.Start();
    }

    public void ShutDown()
    {
        m_timer.Dispose();
    }

}
