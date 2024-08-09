using System;
using Common;
using Logic.Base;
using Logic.Reader;

namespace Logic
{
    // 游戏状态
    enum GameState {
        NotStart = 0,
        Running = 1,
        Pause = 2,
        Finished = 3,
    }

    public struct EnterGameInfo
    {
        public ulong PlayerId; // 玩家id
        public int No; // 编号
    }

    public class Game
    {
        GameState state_; // 游戏状态
        uint maxFrame_; // 最大帧序号
        readonly World world_; // 游戏世界
        MapData mapData_; // 游戏数据
        readonly MapArrayCombo<ulong, uint> playerId2EntityId_; // 玩家id到实体id的映射
        readonly MapArrayCombo<uint, ulong> entityId2PlayerId_; // 实体id到玩家id的映射
        event Action beforeLoadMapEvent_; // 加载地图前事件
        event Action afterLoadMapEvent_; // 加载地图后事件
        event Action beforeUnloadMapEvent_; // 卸载地图前事件
        event Action afterUnloadMapEvent_; // 卸载地图后事件
        event Action<ulong, uint> playerEnterEvent_; // 玩家进入事件
        event Action<ulong, uint> playerLeaveEvent_; // 玩家离开事件
        // 逻辑帧毫秒
        const uint defaultMaxFrame = 10000000;

        public Game()
        {
            world_ = new World();
            playerId2EntityId_ = new MapArrayCombo<ulong, uint>();
            entityId2PlayerId_ = new MapArrayCombo<uint, ulong>();
        }

        public World GetWorld()
        {
            return world_;
        }

        public void Init()
        {
            afterLoadMapEvent_ += world_.Init;
            beforeUnloadMapEvent_ += world_.Uninit;
        }

        public void Uninit()
        {
            beforeUnloadMapEvent_ -= world_.Uninit;
            afterLoadMapEvent_ -= world_.Init;
        }

        // 加载地图
        public bool LoadMap(string mapFilePath)
        {
            beforeLoadMapEvent_?.Invoke();
            if (!world_.LoadMap(mapFilePath, ref mapData_)) return false;
            afterLoadMapEvent_?.Invoke();
            return true;
        }

        // 卸载地图
        public void UnloadMap()
        {
            beforeUnloadMapEvent_?.Invoke();
            world_.UnloadMap();
            afterUnloadMapEvent_?.Invoke();
        }

        // 重新加载地图
        public void ReloadMap()
        {
            world_.ReloadMap();
            state_ = GameState.NotStart;
        }

        public void Start()
        {
            if (state_ != GameState.NotStart && state_ != GameState.Finished)
            {
                return;
            }
            state_ = GameState.Running;
        }

        // 更新
        public bool Update(uint frameMs)
        {
            if (state_ != GameState.Running)
            {
                return false;
            }

            if (!world_.Update(frameMs))
            {
                return false;
            }

            
            if (maxFrame_ == 0)
            {
                maxFrame_ = defaultMaxFrame;
            }
            if (state_ != GameState.Finished && world_.FrameNum() >= maxFrame_)
            {
                state_ = GameState.Finished;
            }

            return true;
        }

        public void End()
        {
            state_ = GameState.Finished;
        }

        // 暂停
        public void Pause()
        {
            world_.Pause();
            state_ = GameState.Pause;
        }

        // 继续
        public void Resume()
        {
            world_.Resume();
            state_ = GameState.Running;
        }
        
        // 设置最大帧序号
        public void SetMaxFrame(uint maxFrame)
        {
            maxFrame_ = maxFrame;
        }

        // 当前帧
        public uint GetCurrFrame()
        {
            return world_.FrameNum();
        }

        // 注册加载地图前事件
        public void RegisterBeforeLoadMapHandle(Action handle)
        {
            beforeLoadMapEvent_ += handle;
        }

        // 注销加载地图前事件
        public void UnregisterBeforeLoadMapHandle(Action handle)
        {
            beforeLoadMapEvent_ -= handle;
        }

        // 注册加载地图后事件
        public void RegisterAfterLoadMapHandle(Action handle)
        {
            afterLoadMapEvent_ += handle;
        }

        // 注销加载地图后事件
        public void UnregisterAfterLoadMapHandle(Action handle)
        {
            afterLoadMapEvent_ -= handle;
        }

        // 注册卸载地图前事件
        public void RegisterBeforeUnloadMapHandle(Action handle)
        {
            beforeUnloadMapEvent_ += handle;
        }

        // 注销卸载地图前事件
        public void UnregisterBeforeUnloadMapHandle(Action handle)
        {
            beforeUnloadMapEvent_ -= handle;
        }

        // 注册卸载地图后事件
        public void RegisterAfterUnloadMapHandle(Action handle)
        {
            afterUnloadMapEvent_ += handle;
        }

        // 注销卸载地图后事件
        public void UnregisterAfterUnloadMapHandle(Action handle)
        {
            afterUnloadMapEvent_ -= handle;
        }

        // 注册玩家进入事件
        public void RegisterPlayerEnterHandle(Action<ulong, uint> handle)
        {
            playerEnterEvent_ += handle;
        }

        // 注销玩家进入事件
        public void UnregisterPlayerEnterHandle(Action<ulong, uint> handle)
        {
            playerEnterEvent_ -= handle;
        }

        // 注册玩家离开事件
        public void RegisterPlayerLeaveHandle(Action<ulong, uint> handle)
        {
            playerLeaveEvent_ += handle;
        }

        // 注销玩家离开事件
        public void UnregisterPlayerLeaveHandle(Action<ulong, uint> handle)
        {
            playerLeaveEvent_ -= handle;
        }

        public bool PlayerEnter(EnterGameInfo enterInfo)
        {
            // TODO 先用缺省的角色
            var defaultRoleIndex = ConfigManager.GetPlayerConfig().DefaultRoleIndex;
            var roleInfo = ConfigManager.GetRoleList().GetAt(defaultRoleIndex);
            var entityDef = ConfigManager.GetEntityDef(roleInfo.EntityId);
            if (entityDef == null)
            {
                DebugLog.Error("Cant get entity def with id " + roleInfo.EntityId);
                return false;
            }
            var entity = world_.CreateEntity(entityDef);
            Type compType = Type.GetType(ConfigManager.GetPlayerConfig().AdditionalComponent);
            entity.AddComponent(compType);
            world_.AddEntityToSystemsWithComponentAdded(entity, compType);
            MapReader.EntityInitData(entity, mapData_.PlayersData[enterInfo.No]);
            playerId2EntityId_.Add(enterInfo.PlayerId, entity.InstId());
            entityId2PlayerId_.Add(entity.InstId(), enterInfo.PlayerId);
            // 执行进入事件
            playerEnterEvent_?.Invoke(enterInfo.PlayerId, entity.InstId());
            return true;
        }

        public void PlayerLeave(ulong playerId)
        {
            uint entityInstId = 0;
            if (!playerId2EntityId_.Remove(playerId, ref entityInstId)) return;
            var entity = world_.GetEntity(entityInstId);
            Type compType = Type.GetType(ConfigManager.GetPlayerConfig().AdditionalComponent);
            world_.RemoveEntityFromSystemsWithComponentRemoved(entity, compType);
            entity?.RemoveComponent(compType);
            world_.RemoveEntity(entityInstId);
            entityId2PlayerId_.Remove(entityInstId, ref playerId);
            // 执行离开事件
            playerLeaveEvent_?.Invoke(playerId, entityInstId);
        }

        public void PushCmd(ulong playerId, CmdData cmdData)
        {
            uint entityInstId = 0;
            if (!playerId2EntityId_.Get(playerId, ref entityInstId)) return;
            world_.PushCmd(entityInstId, cmdData);
        }
    }
}