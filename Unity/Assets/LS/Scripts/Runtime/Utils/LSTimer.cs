using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using System;
using System.Threading;
using UnityEngine;

public class LSTimer : IDisposable
{
    [SerializeField] private int serverFPS = 30;        // 服务端逻辑帧率
    //[SerializeField] private int renderFPS = 60;        // 客户端渲染帧率
    //[SerializeField] private int bufferFrames = 3;      // 输入缓冲帧数

    private int currentLogicFrame = 0;
    private CancellationTokenSource logicCts;

    public event Action OnTick;     // 逻辑帧事件

    public void Start()
    {
        StartFrameSync();
    }

    void StartFrameSync()
    {
        // 启动逻辑帧协程
        logicCts = new CancellationTokenSource();
        _ = RunLogicFrames(logicCts.Token);
    }

    private async UniTask RunLogicFrames(CancellationToken token)
    {
        double frameIntervalMs = 1000.0 / serverFPS;

        // 使用 UniTask 定时器
        await foreach (var _ in UniTaskAsyncEnumerable.Timer(
            TimeSpan.Zero,
            TimeSpan.FromMilliseconds(frameIntervalMs))
            .WithCancellation(token))
        {
            if (token.IsCancellationRequested) break;

            // 记录开始时间
            var frameStartTime = Time.realtimeSinceStartup;

            try
            {
                OnTick?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"逻辑帧 {currentLogicFrame} 执行异常: {ex.Message}");
            }
        }
    }

    public void Dispose()
    {
        logicCts?.Cancel();
        logicCts?.Dispose();
    }
}
