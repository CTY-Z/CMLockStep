using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using System;
using System.Threading;
using UnityEngine;

public class LSTimer : IDisposable
{
    [SerializeField] private int serverFPS = 30;        // �������߼�֡��
    [SerializeField] private int renderFPS = 60;        // �ͻ�����Ⱦ֡��
    [SerializeField] private int bufferFrames = 3;      // ���뻺��֡��

    private int currentLogicFrame = 0;
    private CancellationTokenSource logicCts;

    public event Action OnTick;     // ������֡�¼�

    public void Start()
    {
        StartFrameSync();
    }

    async void StartFrameSync()
    {
        // �����߼�֡Э��
        logicCts = new CancellationTokenSource();
        _ = RunLogicFrames(logicCts.Token);

    }

    private async UniTask RunLogicFrames(CancellationToken token)
    {
        double frameIntervalMs = 1000.0 / serverFPS;

        // ʹ��UniTask��ʱ��
        await foreach (var _ in UniTaskAsyncEnumerable.Timer(
            TimeSpan.Zero,
            TimeSpan.FromMilliseconds(frameIntervalMs))
            .WithCancellation(token))
        {
            if (token.IsCancellationRequested) break;

            // ��¼��ʼʱ��
            var frameStartTime = Time.realtimeSinceStartup;

            try
            {
                OnTick?.Invoke();
            }
            catch (Exception ex)
            {
                Debug.LogError($"�߼�֡{currentLogicFrame}����: {ex.Message}");
            }
        }
    }

    public void Dispose()
    {
        logicCts?.Cancel();
        logicCts?.Dispose();
    }
}
