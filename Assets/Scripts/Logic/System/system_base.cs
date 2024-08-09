using System;
using System.Collections.Generic;
using Common;
using Logic.Base;
using Logic.Interface;
using Logic.Component;

namespace Logic.System
{
  public abstract class SystemBase : ISystem
  {
    public SystemBase(IContext context)
    {
      context_ = context;
      entityList_ = new();
      inactiveList_ = new();
      dummyIdList_ = new();
    }

    public virtual void Init(CompTypeConfig config)
    {
      config_ = config;
    }

    public virtual void Uninit()
    {
      entityList_.Clear();
    }

    public virtual bool AddEntity(uint entityInstId)
    {
      var entity = context_.GetEntity(entityInstId);
      if (entity == null)
      {
        return false;
      }

      if (entityList_.Exists(entity.InstId()))
      {
        return true;
      }

      // 必无的组件类型
      if (config_.NoCompTypeList != null)
      {
        foreach (var nct in config_.NoCompTypeList)
        {
          // 出现一个不感兴趣的组件就添加失败
          if (entity.HasComponent(nct))
          {
            return false;
          }
        }
      }

      bool added = true;

      // 必有的组件类型
      if (config_.CompTypeList != null)
      {
        foreach (var ct in config_.CompTypeList)
        {
          // 只要有一个组件没有，就不能添加
          if (!entity.HasComponent(ct))
          {
            added = false;
            break;
          }
        }
      }

      // 子实体中必有的组件类型
      if (config_.ChildCompTypeList != null)
      {
        added = false;
        foreach (var ct in config_.ChildCompTypeList)
        {
          entity.ForeachChild((IEntity child) =>
          {
            if (child.HasComponent(ct))
            {
              added = true;
            }
          });
        }
      }

      if (added)
      {
        entityList_.Add(entity.InstId(), entity.InstId());
      }

      return added;
    }

    public virtual bool RemoveEntity(uint entityInstId, int entityId)
    {
      return entityList_.Remove(entityInstId);
    }

    public virtual void RecycleEntity(IEntity entity)
    {
      if (entity == null || !entity.IsActive())
      {
        return;
      }
      DeactiveAttachedEntity(entity);
      entity.Deactive();
      inactiveList_.Add(entity.InstId());
      context_.MarkRemoveEntity(entity.InstId());
      DebugLog.Info("!!! Deactive entity " + entity.InstId());
    }

    public Type[] GetCompTypeList()
    {
      return config_.CompTypeList;
    }

    public Type[] GetNoCompTypeList()
    {
      return config_.NoCompTypeList;
    }

    public Type[] GetChildCompTypeList()
    {
      return config_.ChildCompTypeList;
    }

    protected void ForeachOtherSystems(Action<ISystem> action)
    {
      context_.SystemList().ForeachSystem((ISystem system) =>
      {
        if (system != this)
        {
          action(system);
        }
      });
    }

    public virtual void Update(uint frameMs)
    {
      DoUpdate(frameMs);
      if (inactiveList_.Count > 0)
      {
        foreach (var eid in inactiveList_)
        {
          entityList_.Remove(eid);
        }
        inactiveList_.Clear();
      }
      if (dummyIdList_.Count > 0)
      {
        foreach (var did in dummyIdList_)
        {
          entityList_.Remove(did);
        }
        dummyIdList_.Clear();
      }
    }

    public IEntity GetEntity(uint entityInstId)
    {
      var entity = context_.GetEntity(entityInstId);
      // 不存在或者已经是非活动的
      if (entity == null)
      {
        CleanDummyId(entityInstId);
      }
      return entity;
    }

    protected void CleanDummyId(uint entityInstId)
    {
      dummyIdList_.Add(entityInstId);
    }

    protected IEntity GetChildWithTag(IEntity entity, string tag)
    {
      if (entity == null || !entity.IsActive()) return null;
      var child = entity.GetChildWithComponent((TagComponent comp) =>
      {
        return comp.Name == tag;
      });
      return child;
    }
    
    protected virtual void DeactiveAttachedEntity(IEntity entity)
    {
      var shootingComp = entity.GetComponent<ShootingComponent>();
      if (shootingComp == null)
      {
        var tower = GetChildWithTag(entity, "tower");
        if (tower != null)
        {
          shootingComp = tower.GetComponent<ShootingComponent>();
        }
      }
      if (shootingComp != null && shootingComp.AttachedProjectileList != null)
      {
        var attachedProjectileList = shootingComp.AttachedProjectileList;
        for (int i = 0; i < attachedProjectileList.Length; i++)
        {
          if (attachedProjectileList[i] != null)
          {
            RecycleEntity(attachedProjectileList[i]);
          }
        }
      }
    }

    public abstract void DoUpdate(uint frameMs);

    protected IContext context_;
    protected CompTypeConfig config_;
    // 实体列表
    protected MapListCombo<uint, uint> entityList_;
    readonly List<uint> inactiveList_;
    readonly List<uint> dummyIdList_;
  }
}