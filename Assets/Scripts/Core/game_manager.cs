using System;
using Logic;
using Logic.Base;

namespace Core
{
    enum state {
        Idle = 0,
        Prepare = 1,
        Running = 2,
    }

    public struct GameArgs
    {
        public int PlayerNum;
        public uint FrameMs;
    }

    public class GameManager
    {
        readonly Instance inst_;
        readonly RecordManager recordMgr_;
        state state_;
        long lastMs_;
        bool isRecord_;

        public GameManager(GameArgs args)
        {
            inst_ = new Instance(args.PlayerNum, args.FrameMs);
            recordMgr_ = new RecordManager(inst_);
        }

        public bool LoadMap(/*MapDef map*/string mapFilePath)
        {
            var loaded = inst_.LoadMap(mapFilePath);
            return loaded;
        }

        public void UnloadMap()
        {
            inst_.UnloadMap();
        }

        public Instance GetInst()
        {
            return inst_;
        }

        public bool LoadRecordByIndex(int index)
        {
            recordMgr_.Select(index);
            var record = recordMgr_.SelectedRecord();
            if (record == null)
            {
                return false;
            }
            isRecord_ = false;
            return inst_.LoadRecord(inst_.GetGame().GetWorld().MapName, record);
        }

        public bool Start(ulong[] playerIdList, bool record)
        {
            if (!inst_.CheckAndStart(playerIdList)) return false;
            state_ = state.Prepare;
            isRecord_ = record;
            lastMs_ = GTime.CurrMs();
            return true;
        }

        public bool LoadRecordStart(int index)
        {
            if (!LoadRecordByIndex(index)) return false;
            return Start(null, false);
        }

        public bool Restart()
        {
            CheckSaveRecord();
            if (!inst_.Restart()) return false;
            state_ = state.Prepare;
            lastMs_ = GTime.CurrMs();
            return true;
        }

        public bool PushPlayerCmd(uint frameNum, ulong playerId, CmdData cmd)
        {
            return inst_.PushPlayerCmd(frameNum, playerId, cmd.Cmd, cmd.Args);
        }

        public bool PushSyncPlayerCmd(ulong playerId, CmdData cmd)
        {
            return inst_.PushPlayerCmd(inst_.GetFrame(), playerId, cmd.Cmd, cmd.Args);
        }

        public void Update()
        {
            if (state_ == state.Prepare)
            {
                state_ = state.Running;
            }
            if (state_ != state.Running)
            {
                return;
            }
            var currMs = GTime.CurrMs();
            var usedMs = currMs - lastMs_;
            while (usedMs >= inst_.frameMs_)
            {
                inst_.UpdateFrame();
                usedMs -= inst_.frameMs_;
                lastMs_ += inst_.frameMs_;
            }
        }

        public void End()
        {
            CheckSaveRecord();
            inst_.UnloadMap();
            state_ = state.Idle;
        }

        public void Pause()
        {
            inst_.Pause();
        }

        public void Resume()
        {
            inst_.Resume();
            lastMs_ = GTime.CurrMs();
        }

        public uint GetFrame()
        {
            return inst_.GetFrame();
        }

        public void RegisterStartGameHandle(Action handle)
        {
            inst_.RegisterStartGameHandle(handle);
        }

        public void UnregisterStartGameHandle(Action handle)
        {
            inst_.UnregisterStartGameHandle(handle);
        }

        public void RegisterBeforeLoadMapHandle(Action handle)
        {
            inst_.RegisterBeforeLoadMapHandle(handle);
        }

        public void UnregisterBeforeLoadMapHandle(Action handle)
        {
            inst_.UnregisterBeforeLoadMapHandle(handle);
        }

        public void RegisterAfterLoadMapHandle(Action handle)
        {
            inst_.RegisterAfterLoadMapHandle(handle);
        }

        public void UnregisterAfterLoadMapHandle(Action handle)
        {
            inst_.UnregisterAfterLoadMapHandle(handle);
        }

        public void RegisterBeforeUnloadMapHandle(Action handle)
        {
            inst_.RegisterBeforeUnloadMapHandle(handle);
        }

        public void UnregisterBeforeUnloadMapHandle(Action handle)
        {
            inst_.UnregisterBeforeUnloadMapHandle(handle);
        }

        public void RegisterAfterUnloadMapHandle(Action handle)
        {
            inst_.RegisterAfterUnloadMapHandle(handle);
        }

        public void UnregisterAfterUnloadMapHandle(Action handle)
        {
            inst_.UnregisterAfterUnloadMapHandle(handle);
        }

        void CheckSaveRecord()
        {
            if (!isRecord_) return;
            var world = inst_.GetGame().GetWorld();
            recordMgr_.Save(world.MapName, world.MapId);
        }
    }
}