using System.Collections.Generic;
using Common.Geometry;
using Logic.Interface;
using Logic.Component;
using Logic.Base;

namespace Logic.System
{
  public class MoveSystem : SystemBase
  {
    GridSystem gridSystem_;
    CollisionSystem collisionSystem_;
    readonly LinkedList<IEntity> listCache_;
    Rect mapBounds_;

    public MoveSystem(IContext context) : base(context)
    {
      listCache_ = new();
    }

    public override void Init(CompTypeConfig config)
    {
      base.Init(config);
      var systemList = context_.SystemList();
      gridSystem_ = systemList.GetSystem<GridSystem>();
      collisionSystem_ = systemList.GetSystem<CollisionSystem>();
      mapBounds_ = ((IMapInfo)context_).MapBounds;
    }

    public override void Uninit()
    {
      gridSystem_ = null;
      collisionSystem_ = null;
      listCache_.Clear();
      base.Uninit();
    }

    public override void DoUpdate(uint frameMs)
    {
      entityList_.Foreach(EachEntityAction);
    }

    public override bool AddEntity(uint entityInstId)
    {
      if (!base.AddEntity(entityInstId)) return false;
      var entity = context_.GetEntity(entityInstId);
      var movementComp = entity.GetComponent<MovementComponent>();
      if (movementComp.CompDef.MoveOnAwake)
      {
        movementComp.Move();
      }
      return true;
    }

    void EachEntityAction(uint entityInstId, uint _)
    {
      var entity = GetEntity(entityInstId);
      if (entity == null) return;

      var (transformComp, movementComp, colliderComp)
        = entity.GetComponents<TransformComponent, MovementComponent, ColliderComponent>();
      var frameMs = context_.FrameMs();
      transformComp.Update(frameMs);

      // 移动且有碰撞盒
      if (movementComp.IsMoving() && colliderComp != null)
      {
        // 无障碍物时下一帧位置
        var nextPos = movementComp.NextMovePos(frameMs);

        // 测试处于下一个位置时是否超出了地图边界
        if (!colliderComp.TestIsInsideRectAtPosition(mapBounds_, nextPos))
        {
          // 如果是投射物，直接销毁
          if (entity.HasComponent<ProjectileComponent>())
          {
            RecycleEntity(entity);
            return;
          }
          movementComp.Stop();
          return;
        }

        // 本地过滤处理函数
        bool filterFunc(IEntity entity2)
        {
          var colliderComp2 = entity2.GetComponent<ColliderComponent>();
          // 没有碰撞体
          if (colliderComp2 == null)
          {
            return false;
          }
          // 检测处于下一位置时是否与另一个碰撞体相交
          if (!colliderComp.TestIsIntersectAtPosition(nextPos, colliderComp2))
          {
            return false;
          }
          //DebugLog.Info("entity " + entity.InstId() + " and entity " + entity2.InstId() + " will intersect");
          listCache_.AddLast(entity2);
          return true;
        }

        // 先把围栏检测一下，有碰撞则加入处理列表
        collisionSystem_.ForeachFence((IEntity entity2) =>
        {
          filterFunc(entity2);
        });

        // 找到地图中下一帧会碰撞的实体，并加入列表
        gridSystem_.ForeachFilteredEntityAround(entityInstId, filterFunc, null);

        // 碰撞系统处理，得到一个位置或者实体消失
        var (pos, res) = collisionSystem_.GetEntityCollisionsResult(entity, nextPos, listCache_);
        listCache_.Clear();

        if (res == CollisionResult.Disappear)
        {
          if (!entity.IsActive()) return;
        }
        else if (res == CollisionResult.Blocked)
        {
          movementComp.UpdatePos(frameMs, pos, true);
        }
        else
        {
          movementComp.UpdatePos(frameMs, pos, false);
        }
      }
      else
      {
        movementComp.Update(frameMs);
      }
      entity.ForeachChild(EachSubEntityAction);
    }

    void EachSubEntityAction(IEntity subEntity)
    {
      var (transformComp, movementComp) = subEntity.GetComponents<TransformComponent, MovementComponent>();
      var frameMs = context_.FrameMs();
      transformComp.Update(frameMs);
      movementComp.Update(frameMs);
    }
  }
}