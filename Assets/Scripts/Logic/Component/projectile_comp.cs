using System;
using Logic.Base;
using Logic.Interface;

namespace Logic.Component
{
    public class ProjectileCompDef : CompDef
    {
        public ProjectileType PType;
        public int Range;
        public int Damage;
        public override IComponent Create(IComponentContainer container)
        {
            return new ProjectileComponent(container);
        }

        public override Type GetCompType()
        {
            return typeof(ProjectileComponent);
        }
    }

    public class BulletCompDef : ProjectileCompDef
    {
        public override IComponent Create(IComponentContainer container)
        {
            return new ProjectileComponent(container);
        }

        public override Type GetCompType()
        {
            return typeof(ProjectileComponent);
        }
    }

    public class MissileCompDef : ProjectileCompDef
    {
        public override IComponent Create(IComponentContainer container)
        {
            return new ProjectileComponent(container);
        }

        public override Type GetCompType()
        {
            return typeof(ProjectileComponent);
        }
    }

    public class BombCompDef : ProjectileCompDef
    {
        public override IComponent Create(IComponentContainer container)
        {
            return new ProjectileComponent(container);
        }

        public override Type GetCompType()
        {
            return typeof(ProjectileComponent);
        }
    }

    public class BeamCompDef : ProjectileCompDef
    {
        public int Duration;
        public int DPS; // 每秒伤害

        public override IComponent Create(IComponentContainer container)
        {
            return new ProjectileComponent(container);
        }

        public override Type GetCompType()
        {
            return typeof(ProjectileComponent);
        }
    }

    public class PlasmaCompDef : ProjectileCompDef
    {
        public override IComponent Create(IComponentContainer container)
        {
            return new ProjectileComponent(container);
        }

        public override Type GetCompType()
        {
            return typeof(ProjectileComponent);
        }
    }

    public class ShockwaveCompDef : ProjectileCompDef
    {
        public override IComponent Create(IComponentContainer container)
        {
            return new ProjectileComponent(container);
        }

        public override Type GetCompType()
        {
            return typeof(ProjectileComponent);
        }
    }

    public class ProjectileComponent : BaseComponent
    {
        ProjectileCompDef compDef_;

        ShootingComponent shootingComp_;

        uint shooterEntityInstId_;

        event Action<ShootingHitInfo> EventShootingHit_; // 射击击中事件

        event Action<EmitInfo> EventEmit_; // 被发射事件

        event Action<HitInfo> EventHit_; // 击中事件

        public ProjectileComponent(IComponentContainer container) : base(container)
        {

        }

        public override void Init(CompDef compDef)
        {
            compDef_ = (ProjectileCompDef)compDef;
        }

        public override void Uninit()
        {
        }

        public override void Update(uint frameMs)
        {
        }

        public void OnShootingHit(ShootingHitInfo hitInfo)
        {
            EventShootingHit_?.Invoke(hitInfo);
        }

        public void OnEmit(EmitInfo emitInfo)
        {
          EventEmit_?.Invoke(emitInfo);
        }
        
        public void OnHit(HitInfo hitInfo)
        {
          EventHit_?.Invoke(hitInfo);
        }

        public void RegisterShootingHitEvent(Action<ShootingHitInfo> handle)
        {
            EventShootingHit_ += handle;
        }

        public void UnregisterShootingHitEvent(Action<ShootingHitInfo> handle)
        {
            EventShootingHit_ -= handle;
        }

        public void RegisterEmitEvent(Action<EmitInfo> handle)
        {
          EventEmit_ += handle;
        }

        public void UnregisterEmitEvent(Action<EmitInfo> handle)
        {
          EventEmit_ -= handle;
        }

        public void RegisterHitEvent(Action<HitInfo> handle)
        {
          EventHit_ += handle;
        }

        public void UnregisterHitEvent(Action<HitInfo> handle)
        {
          EventHit_ -= handle;
        }

        public ProjectileCompDef CompDef
        {
            get => compDef_;
        }

        public ShootingComponent ShootingComp
        {
          get => shootingComp_;
          set => shootingComp_ = value;
        }

        public uint ShooterInstId
        {
          get => shooterEntityInstId_;
          set => shooterEntityInstId_ = value;
        }
    }
}