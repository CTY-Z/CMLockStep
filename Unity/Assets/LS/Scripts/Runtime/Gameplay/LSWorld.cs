using FrameSync;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace LS
{
    public class LSWorld
    {
        private const int inputDelayFrames = 3;
        private const int bufferFrames = 3;
        /// <summary>
        /// 预测帧最多多跑多少帧
        /// </summary>
        private const int maxPredictAheadFrames = 6;

        private LSTimer m_timer;

        private int m_latestServerFrame;
        /// <summary>
        /// 本地执行到哪(可能包含预测帧)
        /// </summary>
        private int m_localExecutedFrame;
        /// <summary>
        /// 已经按权威确认/播放到哪
        /// </summary>
        private int m_authoritativeExecutedFrame;
        /// <summary>
        /// 用于推进本地输入的帧号，保证每一帧的输入不会被覆盖
        /// </summary>
        private int m_inputTargetFrame;
        private bool m_hasReceivedServerFrame;

        /// <summary>
        /// 已执行的权威帧
        /// </summary>
        private readonly Dictionary<int, FrameSync.FrameInput> dic_frame_executedAuthoritativeHistory = new();
        /// <summary>
        /// 收到过的权威帧
        /// </summary>
        private readonly Dictionary<int, FrameSync.FrameInput> dic_frame_receivedAuthoritativeHistory = new();
        /// <summary>
        /// 本地玩家某一帧的预测输入历史
        /// </summary>
        private readonly Dictionary<int, FrameSync.PlayerInput> dic_frame_localPredictedInputHistory = new();
        /// <summary>
        /// 执行过的预测帧历史
        /// </summary>
        public readonly Dictionary<int, FrameSync.FrameInput> dic_frame_executedPredictedHistory = new();
        /// <summary>
        /// 执行完该帧后的世界快照
        /// </summary>
        private readonly Dictionary<int, WorldSnapshot> dic_frame_worldSnapshot = new();
        /// <summary>
        /// 所有玩家最近已知的一次输入
        /// </summary>
        private readonly Dictionary<int, PlayerInput> dic_player_lastKnownInput = new();

        LSLogicBase m_lsLogic;
        LSViewBase m_lsView;

        public int LatestServerFrame => m_latestServerFrame;
        public int LocalExecutedFrame => m_localExecutedFrame;

        private int localPlayerID { get { return GameEntry.Instance.model.login.clientID; } }

        private IInputComparer m_inputComparer;

        #region LifeTime
        public void Init(LSLogicBase lsLogic, LSViewBase lsView, IInputComparer inputComparer)
        {
            m_latestServerFrame = 0;
            m_localExecutedFrame = 0;
            m_authoritativeExecutedFrame = 0;
            m_inputTargetFrame = 0;
            m_hasReceivedServerFrame = false;

            m_lsLogic = lsLogic;
            m_lsView = lsView;
            m_inputComparer = inputComparer;

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

            ExecuteReadyPredictedFrames();
            ExecuteReadyAuthoritativeFrames();

            m_lsView.Sync(m_lsLogic);
            CleanupDataHistory();
            CleanupLocalInputHistory();
        }

        public void ShutDown()
        {
            GameEntry.Instance.eventPool.Remove<FrameSync.FrameInput>(EventDefine.S_C_FrameData, OnReceiveFrameInput);
            m_timer?.Dispose();
        }
        #endregion

        #region Frame
        private Vector2Int CollectInput()
        {
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");
            return new Vector2Int((int)horizontal, (int)vertical);
        }

        private void ExecuteReadyPredictedFrames()
        {
            int nextFrame = m_localExecutedFrame + 1;
            if (nextFrame >= m_inputTargetFrame) return;

            var predictedFrameInput = BuildPredictedFrame(nextFrame);
            SimulatePredictedFrame(predictedFrameInput);
            m_localExecutedFrame = nextFrame;
        }

        private void ExecuteReadyAuthoritativeFrames()
        {
            while (dic_frame_receivedAuthoritativeHistory.TryGetValue(m_authoritativeExecutedFrame + 1, out var authoritativeFrameInput))
            {
                int nextFrame = m_authoritativeExecutedFrame + 1;

                if (dic_frame_executedPredictedHistory.TryGetValue(nextFrame, out var predictedFrameInput))
                {
                    bool isPredictedRight = m_inputComparer.IsFrameInputMatch(predictedFrameInput, authoritativeFrameInput);
                    if (!isPredictedRight)
                    {
                        RollbackAndReplay(nextFrame);
                        return;
                    }

                    dic_frame_executedPredictedHistory.Remove(nextFrame);
                    dic_frame_executedAuthoritativeHistory[nextFrame] = authoritativeFrameInput;
                    m_authoritativeExecutedFrame = nextFrame;
                    foreach (var item in authoritativeFrameInput.Inputs)
                        dic_player_lastKnownInput[item.PlayerId] = item;

                    continue;
                }

                //如果下一帧跟本地执行的下一帧相同, 说明权威帧跟本地帧是对齐的, 直接采用权威帧的数据
                if (nextFrame == m_localExecutedFrame + 1)
                {
                    SimulateAuthoritativeFrame(authoritativeFrameInput);
                    m_localExecutedFrame = nextFrame;
                    m_authoritativeExecutedFrame = nextFrame;
                    foreach (var item in authoritativeFrameInput.Inputs)
                        dic_player_lastKnownInput[item.PlayerId] = item;
                    continue;
                }

                break;
            }
        }

        private void CleanupLocalInputHistory()
        {
            int minFrame = m_localExecutedFrame - 60;
            if (minFrame <= 0)
                return;

            List<int> expiredFrames = new List<int>();
            foreach (int frame in dic_frame_localPredictedInputHistory.Keys)
            {
                if (frame < minFrame)
                    expiredFrames.Add(frame);
            }

            foreach (int frame in expiredFrames)
            {
                dic_frame_localPredictedInputHistory.Remove(frame);
            }
        }

        private void SimulateAuthoritativeFrame(FrameSync.FrameInput frameInput)
        {
            dic_frame_executedAuthoritativeHistory[frameInput.FrameNumber] = frameInput;

            m_lsLogic.Step(frameInput);

            dic_frame_worldSnapshot[frameInput.FrameNumber] = m_lsLogic.CreateSnapshot();

            //Debug.Log($"frame={frameInput.FrameNumber}, hash={m_lsLogic.GetHash()}");
        }

        private void SimulatePredictedFrame(FrameSync.FrameInput predictedFrameInput)
        {
            //dic_frame_authoritativeFrameInputHistory[frameInput.FrameNumber] = frameInput;
            dic_frame_executedPredictedHistory[predictedFrameInput.FrameNumber] = predictedFrameInput;

            m_lsLogic.Step(predictedFrameInput);
            dic_frame_worldSnapshot[predictedFrameInput.FrameNumber] = m_lsLogic.CreateSnapshot();

            //Debug.Log($"frame={predictedFrameInput.FrameNumber}, hash={m_lsLogic.GetHash()}");
        }
        #endregion

        #region Net
        private void SendInputForFutureFrame()
        {
            if (localPlayerID < 0)
                return;

            Vector2Int input = CollectInput();

            //minTargetFrame是当前应该在的帧, 如果m_inputTargetFrame已经落后了, 就应该重新对齐再计算输入
            int minTargetFrame = m_latestServerFrame + inputDelayFrames;
            //如果服务器停摆, 这个字段可以阻止本地一直往预测数据里塞输入
            int maxTargetFrame = m_latestServerFrame + maxPredictAheadFrames;
            if (m_inputTargetFrame < minTargetFrame)
                m_inputTargetFrame = minTargetFrame;

            if (m_inputTargetFrame > maxTargetFrame)
                return;

            int targetFrame = m_inputTargetFrame;
            //本地的输入帧号每帧推进, 保证每帧都会产出一个输入的数据
            m_inputTargetFrame++;

            FrameSync.PlayerInput playerInput = new FrameSync.PlayerInput
            {
                PlayerId = localPlayerID,
                TargetFrame = targetFrame,
                InputX = input.x,
                InputY = input.y,
                Jump = Input.GetKey(KeyCode.Space),
            };

            dic_frame_localPredictedInputHistory[targetFrame] = playerInput;
            FrameSyncProcessor.C_S_FrameData(targetFrame, playerInput.InputX, playerInput.InputY, playerInput.Jump);
        }

        private void OnReceiveFrameInput(FrameSync.FrameInput frameInput)
        {
            if (!m_hasReceivedServerFrame)
            {
                m_hasReceivedServerFrame = true;
                m_localExecutedFrame = frameInput.FrameNumber - 1;
                m_authoritativeExecutedFrame = frameInput.FrameNumber - 1;
                m_inputTargetFrame = frameInput.FrameNumber + inputDelayFrames;
                dic_frame_worldSnapshot[m_localExecutedFrame] = m_lsLogic.CreateSnapshot();
            }

            m_latestServerFrame = Mathf.Max(m_latestServerFrame, frameInput.FrameNumber);

            if (!dic_frame_receivedAuthoritativeHistory.ContainsKey(frameInput.FrameNumber))
                dic_frame_receivedAuthoritativeHistory[frameInput.FrameNumber] = frameInput;
        }
        #endregion

        #region Snapshot
        private void CleanupDataHistory()
        {
            int minFrame = m_localExecutedFrame - 120;
            if (minFrame <= 0)
                return;

            List<int> list_deleteFrame = new();

            foreach (int frame in dic_frame_worldSnapshot.Keys)
            {
                if (frame < minFrame)
                    list_deleteFrame.Add(frame);
            }

            for (int i = 0; i < list_deleteFrame.Count; i++)
            {
                int frame = list_deleteFrame[i];
                dic_frame_receivedAuthoritativeHistory.Remove(frame);
                dic_frame_executedAuthoritativeHistory.Remove(frame);
                dic_frame_worldSnapshot.Remove(frame);
                dic_frame_executedPredictedHistory.Remove(frame);
                dic_frame_localPredictedInputHistory.Remove(frame);
            }
        }
        #endregion

        #region Predicted

        private FrameInput BuildPredictedFrame(int frameNumber)
        {
            FrameInput pFrameInput = new FrameInput { FrameNumber = frameNumber };

            int localPlayerId = localPlayerID;
            var playerIds = GameEntry.Instance.model.game.GetAllPlayer().ToList();
            if (localPlayerID >= 0 && !playerIds.Contains(localPlayerID))
                playerIds.Add(localPlayerID);
            playerIds.Sort();

            foreach (var playerID in playerIds)
            {
                PlayerInput playerInputTemp = new PlayerInput
                {
                    PlayerId = playerID,
                    TargetFrame = frameNumber,
                    InputX = 0,
                    InputY = 0,
                    Jump = false
                };

                if (playerID == localPlayerId)
                {
                    if (dic_frame_localPredictedInputHistory.TryGetValue(frameNumber, out var pInput))
                    {
                        playerInputTemp.PlayerId = playerID;
                        playerInputTemp.TargetFrame = frameNumber;
                        playerInputTemp.InputX = pInput.InputX;
                        playerInputTemp.InputY = pInput.InputY;
                        playerInputTemp.Jump = pInput.Jump;
                    }
                }
                else
                {
                    if (dic_player_lastKnownInput.TryGetValue(playerID, out var lastKnownInput))
                    {
                        playerInputTemp.PlayerId = playerID;
                        playerInputTemp.TargetFrame = frameNumber;
                        playerInputTemp.InputX = lastKnownInput.InputX;
                        playerInputTemp.InputY = lastKnownInput.InputY;
                        playerInputTemp.Jump = lastKnownInput.Jump;
                    }
                }

                pFrameInput.Inputs.Add(playerInputTemp);
            }

            return pFrameInput;
        }

        private bool TryGetReplayFrameInput(int frameNumber, out FrameInput replayFrameInput)
        {
            if (dic_frame_receivedAuthoritativeHistory.TryGetValue(frameNumber, out replayFrameInput))
                return true;

            if (dic_frame_executedPredictedHistory.TryGetValue(frameNumber, out replayFrameInput))
                return true;

            replayFrameInput = null;
            return false;
        }

        private void RollbackAndReplay(int startFrameNumber)
        {
            //需要回滚到错帧前一帧再开始
            int restoreFrame = startFrameNumber - 1;
            if (restoreFrame < 0) return;

            if (!dic_frame_worldSnapshot.TryGetValue(restoreFrame, out var worldSnapshot))
                return;

            int curExecutedFrame = m_localExecutedFrame;

            m_lsLogic.RestoreSnapshot(worldSnapshot);
            m_localExecutedFrame = restoreFrame;
            m_authoritativeExecutedFrame = restoreFrame;

            var futureSnapshots = dic_frame_worldSnapshot.Keys.Where(f => f > restoreFrame).ToList();
            foreach (var frame in futureSnapshots)
                dic_frame_worldSnapshot.Remove(frame);

            var futureExecuted = dic_frame_executedAuthoritativeHistory.Keys.Where(f => f > restoreFrame).ToList();
            foreach (var frame in futureExecuted)
                dic_frame_executedAuthoritativeHistory.Remove(frame);

            dic_player_lastKnownInput.Clear();

            for (int i = startFrameNumber; i <= curExecutedFrame; i++)
            {
                if (!TryGetReplayFrameInput(i, out var replayFrameInput))
                    break;

                //如果权威帧历史已经包含了该帧，那么预测帧历史中的该帧就不应该存在了
                if (dic_frame_receivedAuthoritativeHistory.ContainsKey(i))
                {
                    dic_frame_executedPredictedHistory.Remove(i);
                    dic_frame_executedAuthoritativeHistory[i] = replayFrameInput;
                }
                else
                    dic_frame_executedPredictedHistory[i] = replayFrameInput;

                m_lsLogic.Step(replayFrameInput);

                foreach (var item in replayFrameInput.Inputs)
                    dic_player_lastKnownInput[item.PlayerId] = item;

                dic_frame_worldSnapshot[i] = m_lsLogic.CreateSnapshot();
                m_localExecutedFrame = i;
            }

            //权威帧要回到回滚前的一帧，然后根据已执行的权威数据递增计算
            while (dic_frame_executedAuthoritativeHistory.ContainsKey(m_authoritativeExecutedFrame + 1))
                m_authoritativeExecutedFrame++;

            m_lsView.Sync(m_lsLogic);
        }

        #endregion
    }
}
