using System;
using Common;
using Logic.Interface;
using Logic.Base;

namespace Logic.Component
{
    public class TrackingCompDef : CompDef
    {
        public int Speed; // 速度
        public int SteeringAngularVelocity; // 转向角速度
        public override IComponent Create(IComponentContainer container)
        {
            return new TrackingComponent(container);
        }

        public override Type GetCompType()
        {
            return typeof(TrackingComponent);
        }
    }

    // 匀速跟踪组件
    public class TrackingComponent : BaseComponent
    {
        // 组件定义
        TrackingCompDef compDef_;
        // 变换组件
        TransformComponent transformComp_;
        // 移动组件
        MovementComponent movementComp_;
        // 搜索目标函数
        Func<IEntity> searchTargetHandle_;
        // 跟踪目标
        uint trackingTargetInstId_;
        // 过滤函数
        Func<IEntity, bool> filterHandle_;

        public TrackingComponent(IComponentContainer container) : base(container)
        {
        }

        public override void Init(CompDef compDef)
        {
            compDef_ = (TrackingCompDef)compDef;
            transformComp_ = container_.GetComponent<TransformComponent>();
            movementComp_ = container_.GetComponent<MovementComponent>();
        }

        public override void Uninit()
        {
        }

        public override void Update(uint frameMs)
        {
            IEntity target = null;
            if (trackingTargetInstId_ == 0)
            {
                target = searchTargetHandle_();
                if (target == null)
                {
                    movementComp_.Update(frameMs);
                    return;
                }
                trackingTargetInstId_ = target.InstId();
            }
            UpdateSteering(target, frameMs);
            movementComp_.Update(frameMs);
        }

        // 更新转向
        protected void UpdateSteering(IEntity target, uint frameMs)
        {
            var targetTransformComp = target.GetComponent<TransformComponent>();
            var vecTarget = targetTransformComp.Pos.ToVec2();
            var vecSelf = transformComp_.Pos.ToVec2();
            vecTarget.Sub(vecSelf);
            var vecMoveDir = movementComp_.MoveDir.ToVec2();
            var dot = vecTarget.Dot(vecMoveDir);
            var cosseta = MathUtil.Denominator()*dot/(vecTarget.Length()*vecMoveDir.Length());
            var seta = MathUtil.Arccos((int)cosseta);
            
            // 一帧转向的分数（角度*60）
            short zj = (short)(60 * compDef_.SteeringAngularVelocity * frameMs / 1000);
            if (seta.ToMinutes() <= zj)
            {
                movementComp_.MoveDir = vecTarget.ToAngle();
            }
            else
            {
                var cross = vecTarget.Cross(vecMoveDir);
                // 目标在顺时针方向
                if (cross > 0)
                {
                    movementComp_.MoveDir.AddMinutes((short)-zj);
                }
                else
                {
                    movementComp_.MoveDir.AddMinutes(zj);
                }
            }
        }

        public TrackingCompDef CompDef
        {
            get => compDef_;
        }
    }
}