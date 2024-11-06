using System;
using System.Linq;
using System.Collections.Generic;
using Common.Geometry;
using Logic.Base;
using Logic.Interface;
using Logic.Entity;
using Logic.System;
using YamlDotNet.RepresentationModel;

namespace Logic
{
    public struct MapData
    {
        public int MapLeft, MapBottom;
        public int GridWidth, GridHeight;
        public YamlMappingNode[] PlayersData;
    }

    public class World : IContext, IMapInfo
    {
        public World()
        {
            entityFactory_ = new();
            mapReader_ = new(this);
            entityList_ = new();
            entityMap_ = new();
            markedRecycleEntityList_ = new();
            systemManager_ = new();
        }

        public bool LoadMap(string mapFilePath, ref MapData mapData)
        {
            mapCache_ = new();
            if (!mapReader_.LoadFile(mapFilePath, ref mapCache_))
            {
                return false;
            }

            mapId_ = mapCache_.Id;
            mapName_ = mapCache_.Name;
            mapBounds_ = mapCache_.Bounds;

            mapData.MapLeft = mapCache_.Bounds.Left();
            mapData.MapBottom = mapCache_.Bounds.Bottom();
            mapData.GridWidth = mapCache_.GridWidth;
            mapData.GridHeight = mapCache_.GridHeight;
            mapData.PlayersData = mapCache_.PlayerDataNodes;
            frameNum_ = 0;

            return true;
        }

        public void UnloadMap()
        {
            foreach (var e in entityList_)
            {
                BeforeEntityRemoveEvent_?.Invoke(e.InstId(), e.Id());
                entityFactory_.Recycle(e);
            }
            entityList_.Clear();
            entityMap_.Clear();
        }

        public void ReloadMap()
        {
            frameNum_ = 0;
        }

        public void Init()
        {
            systemManager_.Init();

            systemDataList_ = SystemPolicy.Initialize(this);

            // 所有的System创建完，添加到System管理器后
            for (int i = 0; i < systemDataList_.Length; i++)
            {
                var sd = systemDataList_[i];
                AfterEntityAddEvent_ += sd.system.AddEntity;
                BeforeEntityRemoveEvent_ += sd.system.RemoveEntity;
                systemManager_.AddSystem(sd.system, sd.config);
            }
            // 再去按次序初始化System
            for (int i = 0; i < systemDataList_.Length; i++)
            {
                var sd = systemDataList_[i];
                sd.system.Init(sd.config);
            }

            // TODO 没有载入地图时创建实体的原因是System还未创建，System注册到创建实体的事件就不能被执行
            // 所以要延迟到载入地图后，等所有System创建完毕事件都注册完再来创建实体
            foreach (var e in mapCache_.EntityDataList)
            {
                var entity = mapReader_.GenerateEntity(e);
            }
            foreach (var e in mapCache_.FenceEntityList)
            {
                mapReader_.GenerateEntity(e);
            }
        }

        public void Uninit()
        {
            for (int i = 0; i < systemDataList_.Length; i++)
            {
                var sd = systemDataList_[i];
                systemManager_.RemoveSystem(sd.system);
                BeforeEntityRemoveEvent_ -= sd.system.RemoveEntity;
                AfterEntityAddEvent_ -= sd.system.AddEntity;
                sd.system.Uninit();
            }

            systemManager_.Uninit();
        }

        public void Pause()
        {
            if (paused_) return;
            foreach (var e in entityList_)
            {
                if (e is Entity.Entity entity)
                {
                    entity.Pause();
                }
            }
            paused_ = true;
        }

        public void Resume()
        {
            if (!paused_) return;
            foreach (var e in entityList_)
            {
                if (e is Entity.Entity entity)
                {
                    entity.Resume();
                }
            }
            paused_ = false;
        }

        public bool Update(uint frameMs)
        {
            if (paused_) return false;

            frameMs_ = frameMs;
            frameNum_ += 1;

            if (markedRecycleEntityList_.Count > 0)
            {
                foreach (var e in markedRecycleEntityList_)
                {
                    RemoveEntity(e.InstId());
                }
                markedRecycleEntityList_.Clear();
            }

            systemManager_.Update(frameMs);

            return true;
        }

        public uint FrameMs()
        {
            return frameMs_;
        }

        public uint FrameNum()
        {
            return frameNum_;
        }

        public IEntity GetEntity(uint instId)
        {
            if (!entityMap_.TryGetValue(instId, out var e))
            {
                return null;
            }
            var entity = e.Value;
            if (entity == null || !entity.IsActive())
            {
                return null;
            }
            return entity;
        }

        public IEntity CreateEntity(EntityDef def)
        {
            var e = (Entity.Entity)entityFactory_.Create(this, def);
            var node = entityList_.AddLast(e);
            entityMap_.Add(e.InstId(), node);
            AfterEntityAddEvent_?.Invoke(e.InstId());
            return e;
        }

        public IEntity CreateEntity(EntityDef def, Action<IEntity> followingCreateHandle)
        {
            var e = (Entity.Entity)entityFactory_.Create(this, def);
            var node = entityList_.AddLast(e);
            entityMap_.Add(e.InstId(), node);
            followingCreateHandle(e);
            AfterEntityAddEvent_?.Invoke(e.InstId());
            return e;
        }

        public bool RemoveEntity(uint instId)
        {
            if (!entityMap_.TryGetValue(instId, out var node))
            {
                return false;
            }
            BeforeEntityRemoveEvent_?.Invoke(instId, node.Value.Id());
            entityFactory_.Recycle(node.Value);
            entityList_.Remove(node);
            entityMap_.Remove(instId);
            return true;
        }

        public void RecycleEntity(IEntity entity)
        {
            entityFactory_.Recycle(entity);
        }

        public void MarkRemoveEntity(uint entityInstId)
        {
            if (!entityMap_.TryGetValue(entityInstId, out var entityNode)) return;
            var entity = entityNode.Value;
            markedRecycleEntityList_.AddLast(entity);
        }

        public IEntity FindFirstEntity(Func<IEntity, bool> filterHandle)
        {
            IEntity entity = null;
            foreach (var e in entityList_)
            {
                if (filterHandle(e))
                {
                    entity = e;
                    break;
                }
            }
            return entity;
        }

        public IEntity FindFirstEntity(Func<IEntity, IEntity, bool> compareFilterHandle, IEntity compareEntity)
        {
            IEntity entity = null;
            foreach (var e in entityList_)
            {
                if (compareFilterHandle(compareEntity, e))
                {
                    entity = e;
                    break;
                }
            }
            return entity;
        }

        public IEntity[] FindEntities(Func<IEntity, bool> filterHandle)
        {
            IEntity[] entityArray = null;
            foreach (var e in entityList_)
            {
                if (filterHandle(e))
                {
                    entityArray.Append(e);
                }
            }
            return entityArray;
        }

        public IEntity[] FindEntities(Func<IEntity, IEntity, bool> compareFilterHandle, IEntity compareEntity)
        {
            IEntity[] entityArray = null;
            foreach (var e in entityList_)
            {
                if (compareFilterHandle(compareEntity, e))
                {
                    entityArray.Append(e);
                }
            }
            return entityArray;
        }

        public bool AddEntityToSystemsWithComponentAdded(IEntity entity, Type compType)
        {
            return systemManager_.AddEntityWithCompAdded(entity, compType);
        }

        public bool RemoveEntityFromSystemsWithComponentRemoved(IEntity entity, Type compType)
        {
            return systemManager_.RemoveEntityWithCompRemoved(entity, compType);
        }

        public ISystemList SystemList()
        {
            return systemManager_;
        }

        public void PushCmd(uint entityInstId, CmdData cmdData)
        {
            systemManager_.GetInputSystem().ExecuteCmd(entityInstId, cmdData);
        }

        public void RegisterAfterEntityAddHandle(Func<uint, bool> handle)
        {
            AfterEntityAddEvent_ += handle;
        }

        public void UnregisterAfterEntityAddHandle(Func<uint, bool> handle)
        {
            AfterEntityAddEvent_ -= handle;
        }

        public void RegisterBeforeEntityRemoveHandle(Func<uint, int, bool> handle)
        {
            BeforeEntityRemoveEvent_ += handle;
        }

        public void UnregisterBeforeEntityRemoveHandle(Func<uint, int, bool> handle)
        {
            BeforeEntityRemoveEvent_ -= handle;
        }

        public int MapId { get => mapId_; }
        public string MapName { get => mapName_; }
        public Rect MapBounds { get => mapBounds_; }
        public int MapLeft { get => mapCache_.Bounds.Left(); }
        public int MapBottom { get => mapCache_.Bounds.Bottom(); }
        public int MapWidth { get => mapCache_.Bounds.Width(); }
        public int MapHeight { get => mapCache_.Bounds.Height(); }
        public int GridWidth { get => mapCache_.GridWidth; }
        public int GridHeight { get => mapCache_.GridHeight; }

        readonly EntityFactory entityFactory_;
        readonly MapReader mapReader_;

        bool paused_;
        uint frameMs_;
        uint frameNum_;
        readonly LinkedList<IEntity> entityList_;
        readonly Dictionary<uint, LinkedListNode<IEntity>> entityMap_;
        readonly LinkedList<IEntity> markedRecycleEntityList_;
        MapLoadCache mapCache_;
        SystemData[] systemDataList_;
        readonly SystemManager systemManager_;

        event Func<uint, bool> AfterEntityAddEvent_;
        event Func<uint, int, bool> BeforeEntityRemoveEvent_;

        int mapId_;
        string mapName_;
        Rect mapBounds_;
    }
}