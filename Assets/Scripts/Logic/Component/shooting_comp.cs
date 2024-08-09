using System;
using Common;
using Logic.Interface;
using Logic.Base;
using Logic.Const;
using Common.Geometry;

namespace Logic.Component
{
    public class ShootingCompDef : CompDef
    {
        public Position Pos;
        public Position[] PosList;

        public int Projectile;
        public int Range;
        public int Interval;

        public override IComponent Create(IComponentContainer container)
        {
            return new ShootingComponent(container);
        }

        public override Type GetCompType()
        {
            return typeof(ShootingComponent);
        }
    }

    public struct EmitInfo
    {
      public ShootingComponent ShootingComp;
      public Position Pos;
      public Angle Dir;
    }

    public struct ShootingHitInfo
    {
        public bool IsHit;
        public Position Pos;
        public Angle Dir;
    }

    public class ShootingComponent : BaseComponent
    {
        ShootingCompDef compDef_;
        bool isMultiPos_;

        Position[] posList_;
        uint lastShootingFrame_;
        bool isShooting_;
        IEntity[] attachedProjectileList_;

        event Action EventShootingStart_;
        event Action EventShootingStop_;
        event Action<EmitInfo> EventEmit_;

        public ShootingComponent(IComponentContainer container) : base(container)
        {
        }

        public override void Init(CompDef compDef)
        {
            compDef_ = (ShootingCompDef)compDef;
            if (!IsMultiPos)
            {
                posList_ ??= new Position[1];
                posList_[0] = compDef_.Pos;
            }
            else
            {
                posList_ = compDef_.PosList;
            }
        }

        public override void Uninit()
        {
        }

        public override void Update(uint frameMs)
        {
        }

        bool Check(uint currFrameNum, uint frameMs)
        {
            return !(lastShootingFrame_ > 0 &&
                ((compDef_.Interval <= 0 && (currFrameNum - lastShootingFrame_)*frameMs < Static.DefaultShootingInterval)
                || (compDef_.Interval > 0 && (currFrameNum - lastShootingFrame_)*frameMs < compDef_.Interval)));
        }

        public bool CheckCooldown(uint currFrameNum, uint frameMs)
        {
            if (!Check(currFrameNum, frameMs)) return false;
            lastShootingFrame_ = currFrameNum;
            return true;
        }

        public void CheckAndStart()
        {
            if (!isShooting_)
            {
                isShooting_ = true;
                EventShootingStart_?.Invoke();
            }
        }

        public void CheckAndStop()
        {
            if (isShooting_)
            {
                isShooting_ = false;
                EventShootingStop_?.Invoke();
            }
        }

        public void OnEmit(EmitInfo emitInfo)
        {
            if (isShooting_)
            {
                EventEmit_?.Invoke(emitInfo);
            }
        }

        public bool IsShooting()
        {
            return isShooting_;
        }

        public void AttachProjectile(int index, IEntity projectile)
        {
            AttachedProjectileList[index] = projectile;
            projectile.GetComponent<ProjectileComponent>().ShootingComp = this;
        }

        public IEntity GetAttachedProjectile(int index = 0)
        {
            return AttachedProjectileList[index];
        }

        public ShootingCompDef CompDef
        {
            get => compDef_;
        }

        public bool IsMultiPos
        {
            get
            {
                if (!isMultiPos_ && compDef_ != null && compDef_.PosList != null)
                {
                    isMultiPos_ = true;
                }
                return isMultiPos_;
            }
        }

        public Position[] PosList
        {
            get => posList_;
        }

        internal IEntity[] AttachedProjectileList
        {
            get
            {
                if (attachedProjectileList_ == null)
                {
                    if (compDef_.PosList != null)
                    {
                        attachedProjectileList_ = new IEntity[compDef_.PosList.Length];
                    }
                    else
                    {
                        attachedProjectileList_ = new IEntity[1];
                    }
                }
                return attachedProjectileList_;
            }
        }

        public void RegisterShootingStartEvent(Action action)
        {
            EventShootingStart_ += action;
        }

        public void UnregisterShootingStartEvent(Action action)
        {
            EventShootingStart_ -= action;
        }

        public void RegisterShootingStopEvent(Action action)
        {
            EventShootingStop_ += action;
        }

        public void UnregisterShootingStopEvent(Action action)
        {
            EventShootingStop_ -= action;
        }

        public void RegisterEmitEvent(Action<EmitInfo> action)
        {
            EventEmit_ += action;
        }

        public void UnregisterEmitEvent(Action<EmitInfo> action)
        {
            EventEmit_ -= action;
        }
    }
}