using System;
using System.Collections.Generic;
using Common;
using Logic;
using Logic.Base;

namespace Core
{
    enum instanceMode {
        Play = 0,
        Replay = 1,
    }

    class playerData
    {
        internal ulong playerId;
        internal List<CmdData> cmdList;

        internal bool IsEmpty()
        {
            return cmdList.Count == 0;
        }

        internal void Clear()
        {
            cmdList?.Clear();
            playerId = 0;
        }

        internal void Add(CmdData cmdData)
        {
            cmdList ??= new List<CmdData>();
            cmdList.Add(cmdData);;
        }
    }

    class frameData
    {
        internal uint frameNum;
        internal List<playerData> playerDataList;

        internal void clear()
        {
            playerDataList.Clear();
            frameNum = 0;
        }
    }

    public class Instance
    {
        internal int playerNum_; // 玩家人数
        internal uint frameMs_; // 帧毫秒数
        internal instanceMode mode_; // 实例播放模式
        internal Record record_; // 录像数据，只有在重播模式下才有用
        internal List<frameData> frameList_; // 游戏帧队列
        internal List<ulong> playerIdList_; // 玩家队列
        internal Game game_; // 游戏逻辑
        internal uint frameIndexInList_; // 帧队列frameList_或者record_.frameList_的当前索引
        internal LinkedList<frameData> frameDataFreeList_; // 帧数据freelist
        internal event Action startGameEvent_; // 开始游戏事件
        internal event Action exitGameEvent_; // 退出游戏事件
        internal event Action pauseGameEvent_; // 暂停游戏事件
        internal event Action resumeGameEvent_; // 继续游戏事件

        public Instance(int playerNum, uint frameMs)
        {
            playerNum_ = playerNum;
            frameMs_ = frameMs;
            frameList_ = new();
            game_ = new Game();
            frameDataFreeList_ = new();
        }

        public Game GetGame()
        {
            return game_;
        }

        public bool LoadMap(string mapFilePath)
        {
            game_.Init();
            if (!game_.LoadMap(mapFilePath)) return false;
            mode_ = instanceMode.Play;
            return true;
        }

        public void UnloadMap()
        {
            RecycleFrames();
            game_.ReloadMap();
            game_.Uninit();
            frameIndexInList_ = 0;
        }

        public bool LoadRecord(string mapFilePath, Record record)
        {
            if (!game_.LoadMap(mapFilePath)) return false;
            mode_ = instanceMode.Replay;
            record_ = record;
            return true;
        }

        public bool CheckAndStart(ulong[] playerList)
        {
            List<ulong> playerIdList = null;
            if (playerList != null)
            {
                playerIdList = new();
                foreach (var pid in playerList)
                {
                    playerIdList.Add(pid);
                }
            }
            return CheckAndStart(playerIdList);
        }

        public bool CheckAndStart(List<ulong> playerList)
        {
            if ((playerList == null || playerList.Count == 0) && mode_ == instanceMode.Replay)
            {
                playerList = record_.playerIdList_;
            }
            if (playerList == null || playerList.Count != playerNum_)
            {
                return false;
            }
            for (int i=0; i<playerList.Count; i++)
            {
                var pid = playerList[i];
                if (!game_.PlayerEnter(new EnterGameInfo{ PlayerId=pid, No=i}))
                {
                    DebugLog.Error("Player " + pid + " enter game failed");
                    return false;
                }
            }
            playerIdList_ = playerList;
            frameIndexInList_ = 0;
            game_.Start();
            startGameEvent_?.Invoke();
            return true;
        }

        public bool Restart()
        {
            if (playerIdList_.Count == 0) return false;
            RecycleFrames();
            game_.ReloadMap();
            return CheckAndStart(playerIdList_);
        }

        public void Exit()
        {
            exitGameEvent_?.Invoke();
        }

        public void Pause()
        {
            if (mode_ == instanceMode.Replay && game_.GetCurrFrame() >= record_.frameNum_)
            {
                return;
            }
            game_.Pause();
            pauseGameEvent_?.Invoke();
        }

        public void Resume()
        {
            if (mode_ == instanceMode.Replay && game_.GetCurrFrame() >= record_.frameNum_)
            {
                return;
            }
            game_.Resume();
            resumeGameEvent_?.Invoke();
        }

        public void RegisterStartGameHandle(Action handle)
        {
            startGameEvent_ += handle;
        }

        public void UnregisterStartGameHandle(Action handle)
        {
            startGameEvent_ -= handle;
        }

        public void RegisterExitGameHandle(Action handle)
        {
            exitGameEvent_ += handle;
        }

        public void UnregisterExitGameHandle(Action handle)
        {
            exitGameEvent_ -= handle;
        }

        public void RegisterPauseGameHandle(Action handle)
        {
            pauseGameEvent_ += handle;
        }

        public void UnregisterPauseGameHandle(Action handle)
        {
            pauseGameEvent_ -= handle;
        }

        public void RegisterResumeGameHandle(Action handle)
        {
            resumeGameEvent_ += handle;
        }

        public void UnregisterResumeGameHandle(Action handle)
        {
            resumeGameEvent_ -= handle;
        }

        public void RegisterBeforeLoadMapHandle(Action handle)
        {
            game_.RegisterBeforeLoadMapHandle(handle);
        }

        public void UnregisterBeforeLoadMapHandle(Action handle)
        {
            game_.UnregisterBeforeLoadMapHandle(handle);
        }

        public void RegisterAfterLoadMapHandle(Action handle)
        {
            game_.RegisterAfterLoadMapHandle(handle);
        }

        public void UnregisterAfterLoadMapHandle(Action handle)
        {
            game_.UnregisterAfterLoadMapHandle(handle);
        }

        public void RegisterBeforeUnloadMapHandle(Action handle)
        {
            game_.RegisterBeforeUnloadMapHandle(handle);
        }

        public void UnregisterBeforeUnloadMapHandle(Action handle)
        {
            game_.UnregisterBeforeUnloadMapHandle(handle);
        }

        public void RegisterAfterUnloadMapHandle(Action handle)
        {
            game_.RegisterAfterUnloadMapHandle(handle);
        }

        public void UnregisterAfterUnloadMapHandle(Action handle)
        {
            game_.UnregisterAfterUnloadMapHandle(handle);
        }

        public bool PushPlayerCmd(uint frameNum, ulong playerId, int cmd, long[] args)
        {
            if (frameNum == 0)
            {
                frameNum = game_.GetCurrFrame();
            }
            // 压入的帧号不能超过当前游戏逻辑的帧序号
            if (frameNum > game_.GetCurrFrame())
            {
                DebugLog.Error("Instance.PushFrame: push frame " + frameNum + " can't greater to current frame " + game_.GetCurrFrame());
                return false;
            }

            frameData fd = null;
            var l = frameList_.Count;
            int index = 0;
            if (l > 0)
            {
                // 从后往前遍历是为了最大程度的减少次数，提高性能
                for (index=l-1; index>=0; index--)
                {
                    // 正好是该帧序号，则更新该帧
                    if (frameList_[index].frameNum == frameNum)
                    {
                        fd = frameList_[index];
                        break;
                    }
                    // 比当前帧序号大，说明比之前所有帧序号都大，直接插入到该帧后面
                    if (frameList_[index].frameNum < frameNum)
                    {
                        break;
                    }
                }
            }
            // 需要新分配一个帧数据
            if (fd == null)
            {
                fd = GetAvailableFrameData();
                fd.frameNum = frameNum;
                if (index >= l-1)
                {
                    frameList_.Add(fd);
                }
                else
                {
                    // 插入到index帧后面一个
                    frameList_.Insert(index+1, fd);
                }
            }
            // 
            for (int i=0; i<fd.playerDataList.Count; i++)
            {
                var pd = fd.playerDataList[i];
                if (pd.playerId == 0)
                {
                    pd.playerId = playerIdList_[i];
                }
                if (playerId == pd.playerId)
                {
                    pd.Add(new CmdData{Cmd=cmd, Args=args});
                }
            }
            return true;
        }

        // 更新帧
        public void UpdateFrame()
        {
            if (!ProcessFrameCmdList()) return;
            if (!game_.Update(frameMs_)) return;
            if (mode_ == instanceMode.Replay)
            {
                if (game_.GetCurrFrame() == record_.frameNum_)
                {
                    game_.Pause();
                }
            }
        }

        // 获得帧
        public uint GetFrame()
        {
            return game_.GetCurrFrame();
        }

        // 得到可用帧数据
        frameData GetAvailableFrameData()
        {
            frameData fd;
            if (frameDataFreeList_ == null || frameDataFreeList_.Count == 0)
            {
                fd = new()
                {
                    playerDataList = new List<playerData>()
                };
                for (int i=0; i<playerIdList_.Count; i++)
                {
                    fd.playerDataList.Add(new playerData{playerId=playerIdList_[i]});
                }
            }
            else
            {
                fd = frameDataFreeList_.First.Value;
                frameDataFreeList_.RemoveFirst();
            }
            return fd;
        }

        // 执行每帧命令队列
        bool ProcessFrameCmdList()
        {
            if (mode_ == instanceMode.Replay)
            {
                if (record_.frameNum_ < game_.GetCurrFrame())
                {
                    return false;
                }
            }

            List<frameData> frameList;
            if (mode_ == instanceMode.Play)
            {
                frameList = frameList_;
            }
            else
            {
                frameList = record_.frameList_;
            }

            if (frameIndexInList_ + 1 > frameList.Count)
            {
                return true;
            }

            var fd = frameList[(int)frameIndexInList_];
            if (fd.frameNum != game_.GetCurrFrame())
            {
                return true;
            }

            for (int i=0; i<fd.playerDataList.Count; i++)
            {
                var playerData = fd.playerDataList[i];
                for (int j=0; j<playerData.cmdList.Count; j++)
                {
                    var cmd = playerData.cmdList[j];
                    ExecCmd(cmd.Cmd, cmd.Args, playerData.playerId);
                }
            }
            frameIndexInList_ += 1;
            return true;
        }

        // 执行命令
        void ExecCmd(int cmd, long[] args, ulong playerId)
        {
            game_.PushCmd(playerId, new CmdData{Cmd=cmd, Args=args});
        }

        // 回收帧
        void RecycleFrames()
        {
            if (mode_ == instanceMode.Play)
            {
                for (int i=0; i<frameList_.Count; i++)
                {
                    frameList_[i].clear();
                    frameDataFreeList_.AddLast(frameList_[i]);
                }
                frameList_.Clear();
            }
            else if (mode_ == instanceMode.Replay)
            {
                for (int i=0; i<record_.frameList_.Count; i++)
                {
                    record_.frameList_[i].clear();
                    frameDataFreeList_.AddLast(record_.frameList_[i]);
                }
                record_.frameList_.Clear();
            }
        }
    }
}