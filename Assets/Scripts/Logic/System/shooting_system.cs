using System;
using Common;
using Common.Geometry;
using Logic.Base;
using Logic.Interface;
using Logic.Component;
using Logic.Reader;

namespace Logic.System
{
  public class ShootingSystem : SystemBase
  {
    GridSystem gridSystem_;
    WeaponSystem weaponSystem_;
    MinBinaryHeap<HitEntityInfo> rayHitInfoCache_;

    public ShootingSystem(IContext context) : base(context)
    {
    }

    public override void Init(CompTypeConfig config)
    {
      base.Init(config);
      gridSystem_ = context_.SystemList().GetSystem<GridSystem>();
      weaponSystem_ = context_.SystemList().GetSystem<WeaponSystem>();
      rayHitInfoCache_ = new();
    }

    public override void Uninit()
    {
      rayHitInfoCache_.Clear();
      rayHitInfoCache_ = null;
      gridSystem_ = null;
      weaponSystem_ = null;
      base.Uninit();
    }

    public override void DoUpdate(uint frameMs)
    {
      ForeachEntity((uint entityInstId) =>
      {
        ProcessShooting(entityInstId);
      });
    }

    public void CheckAndStart(ShootingComponent shootingComp)
    {
      var projectileDef = ConfigManager.GetEntityDef(shootingComp.CompDef.Projectile);
      var projCompDef = projectileDef.GetCompDerivedFromT<ProjectileCompDef>() ?? throw new Exception("Get derived class from ProjectileCompDef failed");
      if (projCompDef.PType == ProjectileType.Beam || projCompDef.PType == ProjectileType.Shockwave)
      {
        for (int i = 0; i < shootingComp.PosList.Length; i++)
        {
          var projectile = shootingComp.GetAttachedProjectile(i);
          projectile ??= context_.CreateEntity(projectileDef, (IEntity e)=>
            {
              shootingComp.AttachProjectile(i, e);
            });
        }
      }
      shootingComp.CheckAndStart();
    }

    public void CheckAndStop(ShootingComponent shootingComp)
    {
      shootingComp.CheckAndStop();
    }

    void ProcessShooting(uint shooterInstId)
    {
      IEntity entity = context_.GetEntity(shooterInstId);
      if (entity == null) return;
      var behaviourComp = entity.GetComponent<BehaviourComponent>();
      if (behaviourComp == null)
      {
        var child = entity.GetChild(0);
        ChildWeaponShooting(entity, child);
      }
      else
      {
        int childCount = entity.ChildCount();
        for (int i=0; i<childCount; i++)
        {
          ChildWeaponShooting(entity, entity.GetChild(i));
        }
      }
    }

    void ChildWeaponShooting(IEntity entity, IEntity child)
    {
      if (entity != null && child != null)
      {
        var (transformComp, shootingComp) = child.GetComponents<TransformComponent, ShootingComponent>();
        if (transformComp != null && shootingComp != null)
        {
          if (!shootingComp.IsShooting())
          {
            return;
          }

          Angle shootingDir;
          var worldOrientation = transformComp.WorldOrientation;
          var inputComp = entity.GetComponent<InputComponent>();
          if (inputComp != null)
          {
            // 如果这时世界朝向与射击方向不一致，则调整世界朝向与射击方向保持一致
            shootingDir = inputComp.WorldOrientation;

            if (shootingDir != worldOrientation)
            {
              transformComp.WorldOrientationTo(shootingDir);
            }
          }
          else
          {
            shootingDir = transformComp.WorldOrientation;
          }

          // 判断射击位置是否在地图范围内
          for (int i = 0; i < shootingComp.PosList.Length; i++)
          {
            var worldPos = transformComp.Local2World(shootingComp.PosList[i]);
            if (!gridSystem_.InBounds(worldPos))
            {
              DebugLog.Warning("Not allowed fire out of bounds");
              return;
            }
          }

          var projectileDef = ConfigManager.GetEntityDef(shootingComp.CompDef.Projectile);
          var projCompDef = projectileDef.GetCompDerivedFromT<ProjectileCompDef>() ?? throw new Exception("Get derived class from ProjectileCompDef failed");

          if (projCompDef.PType != ProjectileType.Beam)
          {
            if (!shootingComp.CheckCooldown(context_.FrameNum(), context_.FrameMs()))
            {
              return;
            }
          }

          IEntity projectile;
          for (int i = 0; i < shootingComp.PosList.Length; i++)
          {
            if (projCompDef.PType == ProjectileType.Beam || projCompDef.PType == ProjectileType.Shockwave)
            {
              projectile = shootingComp.GetAttachedProjectile(i);
            }
            else
            {
              projectile = context_.CreateEntity(projectileDef);
            }

            var (projectileMovementComp, projectileTransformComp, projectileCampComp, projectileProjComp)
                = projectile.GetComponents<MovementComponent, TransformComponent, CampComponent, ProjectileComponent>();
            projectileCampComp.CampType = entity.GetComponent<CampComponent>().CampType;
            var worldPos = transformComp.Local2World(shootingComp.PosList[i]);
            projectileTransformComp.Pos = worldPos;
            // 朝向与移动方向保持一致
            projectileTransformComp.WorldOrientationTo(shootingDir);
            projectileProjComp.ShooterInstId = entity.InstId();

            // 光束类投射物或冲击波
            if (projCompDef.PType == ProjectileType.Beam || projCompDef.PType == ProjectileType.Shockwave)
            {
              var ray = new Ray(worldPos, shootingDir, projCompDef.Range);
              ColliderComponent colliderComp = null; // 被碰撞实体碰撞组件
              Position pos = worldPos; //projectileTransformComp.Pos;
              bool isCollision = false;
              rayHitInfoCache_.Clear();

              // 射线检测过滤出相交的实体
              if (gridSystem_.Raycast(in ray, (IEntity entity) =>
              {
                return true;
              }, ref rayHitInfoCache_))
              {
                (colliderComp, pos, isCollision) = GetRayCollisionsResult(entity, rayHitInfoCache_);
                if (!isCollision) pos = worldPos; //projectileTransformComp.Pos;
              }
              if (!isCollision)
              {
                int dx = MathUtil.Cosine(shootingDir) * projCompDef.Range / MathUtil.Denominator();
                int dy = MathUtil.Sine(shootingDir) * projCompDef.Range / MathUtil.Denominator();
                pos.Translate(dx, dy);
              }
              else
              {
                var behitEntity = (IEntity)colliderComp.Container();
                var hitResult = weaponSystem_.ProjectileHit(projectileProjComp, behitEntity, pos);
                if (hitResult == HitResult.Dead)
                {
                  RecycleEntity(behitEntity);
                }
              }
              projectileProjComp.OnShootingHit(new ShootingHitInfo { IsHit=isCollision, Pos=pos, Dir=shootingDir });
            }
            else // 其他投射物
            {
              projectileMovementComp.Move(shootingDir);
              projectileProjComp.OnEmit(new EmitInfo{ ShootingComp=shootingComp, Dir=shootingDir, Pos=shootingComp.PosList[i] });
            }
            shootingComp.OnEmit(new EmitInfo{ ShootingComp=shootingComp, Dir=shootingDir });
          }
        }
      }
    }

    // 射线碰撞结果
    (ColliderComponent, Position, bool) GetRayCollisionsResult(IEntity rayEntity, MinBinaryHeap<HitEntityInfo> rayHitInfo)
    {
      ColliderComponent colliderComp = null;
      var pos = new Position();
      bool isCollision = false;
      var rayCamp = rayEntity.GetComponent<CampComponent>().CampType;
      while (rayHitInfo.Get(out var hitInfo))
      {
        var hitCamp = hitInfo.entity.GetComponent<CampComponent>().CampType;
        var relation = ConfigManager.GetRelation(rayCamp, hitCamp);
        if (relation == CampRelation.Friendly)
        {
          continue;
        }
        colliderComp = hitInfo.entity.GetComponent<ColliderComponent>();
        if (relation == CampRelation.Hostile || relation == CampRelation.Neutral)
        {
          if (colliderComp.IsRigidBody)
          {
            pos = hitInfo.HitPoint;
            isCollision = true;
            break;
          }
        }
      }
      return (colliderComp, pos, isCollision);
    }
  }
}