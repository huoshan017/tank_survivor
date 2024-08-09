using System;
using System.Collections.Generic;
using Common;
using Logic.Interface;
using Logic.Base;

namespace Logic.Entity
{
    public class EntityFactory
    {
        public IEntity Create(IContext context, EntityDef def)
        {
            return Create(context, null, def);
        }

        IEntity Create(IContext context, IEntity parent, EntityDef def)
        {
            IEntity entity;
            var instId = ++instIdCounter_;
            DebugLog.Info("create new entity instId " + instId);
            if (freeEntityList_.Count > 0)
            {
                entity = freeEntityList_.First.Value;
                freeEntityList_.RemoveFirst();
                if (parent != null)
                {
                    entity.Parent = parent;
                }
                Entity.Reinit((Entity)entity, context, def, instId);
            }
            else
            {
                entity = new Entity(context, def, instId);
                if (parent != null)
                {
                    entity.Parent = parent;
                }
                entity.Init();
            }
            EventEntityAfterCreate_?.Invoke(entity);
            
            // 子实体
            if (def.SubEntityDefList != null)
            {
                foreach (var subDef in def.SubEntityDefList)
                {
                    var child = Create(context, entity, subDef);
                    entity.AddChild(child);
                }
            }
            hasEntitySet_.Add(entity);
            return entity;
        }

        public void Recycle(IEntity entity)
        {
            if (!hasEntitySet_.Contains(entity)) return;
            EventEntityBeforeRecycle_?.Invoke(entity.InstId());
            // 反初始化中会回收子实体
            entity.Uninit();
            freeEntityList_.AddFirst(entity);
            hasEntitySet_.Remove(entity);
        }

        public void SetEntityCreateEventHandle(Action<IEntity> handle)
        {
            EventEntityAfterCreate_ += handle;
        }

        public void UnsetEntityCreateEventHandle(Action<IEntity> handle)
        {
            EventEntityAfterCreate_ -= handle;
        }

        public void SetEntityRecycleEventHandle(Action<uint> handle)
        {
            EventEntityBeforeRecycle_ += handle;
        }

        public void UnsetEntityRecycleEventHandle(Action<uint> handle)
        {
            EventEntityBeforeRecycle_ -= handle;
        }

        uint instIdCounter_ = 0;
        internal LinkedList<IEntity> freeEntityList_ = new();
        internal HashSet<IEntity> hasEntitySet_ = new();
        event Action<IEntity> EventEntityAfterCreate_;
        event Action<uint> EventEntityBeforeRecycle_;
    }
}