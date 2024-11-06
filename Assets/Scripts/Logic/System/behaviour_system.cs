using Common.Geometry;
using Logic.Base;
using Logic.Interface;
using Logic.Component;
using Logic.Reader;

namespace Logic.System
{
    public class BehaviourSystem : SystemBase
    {
        GridSystem gridSystem_;
        ShootingSystem shootingSystem_;

        public BehaviourSystem(IContext context) : base(context)
        {
        }

        public override void Init(CompTypeConfig config)
        {
            base.Init(config);
            gridSystem_ = context_.SystemList().GetSystem<GridSystem>();
            shootingSystem_ = context_.SystemList().GetSystem<ShootingSystem>();
        }

        public override void Uninit()
        {
            shootingSystem_ = null;
            gridSystem_ = null;
            base.Uninit();
        }

        public override void DoUpdate(uint frameMs)
        {
            ForeachEntity((uint entityInstId) =>
            {
                var entity = context_.GetEntity(entityInstId);
                if (entity == null) return;
                var (transformComp, movementComp, campComp, searchComp, projectileComp, shootingComp, behaviourComp)
              = entity.GetComponents<TransformComponent, MovementComponent, CampComponent, SearchComponent, ProjectileComponent, ShootingComponent, BehaviourComponent>();
                // 待机或巡逻状态时，搜索周围的目标
                if (behaviourComp.IsStateStandby() || behaviourComp.IsStatePatrol())
                {
                    var targetEntity = DoSearch(entity, out var relation);
                    if (targetEntity != null)
                    {
                        // todo 暂时只处理敌对的情况
                        if (relation == CampRelation.Hostile)
                        {
                            // 发现有敌人时进入警戒状态
                            behaviourComp.EnterStateAlert();
                        }
                    }
                }
                // 警戒状态时检测目标是否进入攻击范围
                else if (behaviourComp.IsStateAlert() || behaviourComp.IsStateAttack())
                {
                    if (behaviourComp.Attacker > 0 && behaviourComp.Attacker != searchComp.TargetEntityInstId)
                    {
                        searchComp.TargetEntityInstId = behaviourComp.Attacker;
                    }

                    bool aimDone;
                    Position childPosition;
                    Angle targetDirAngle;
                    var targetEntity = context_.GetEntity(searchComp.TargetEntityInstId);
                    //var currTag = behaviourComp.CurrentChildTag;
                    //behaviourComp.Move2NextChildTag();
                    var childCount = entity.ChildCount();
                    for (int i = 0; i < childCount; i++)
                    {
                        (shootingComp, childPosition, targetDirAngle, aimDone) = DoAim(entity, i, targetEntity);
                        if (!aimDone || shootingComp == null) return;

                        var targetTransformComp = targetEntity.GetComponent<TransformComponent>();
                        var range = shootingComp.CompDef.Range;
                        // 不在射程之内就接近
                        if (Position.DistanceSquare(childPosition, targetTransformComp.Pos) > (ulong)range * (ulong)range)
                        {
                            if (behaviourComp.IsStateAttack())
                            {
                                behaviourComp.EnterStateAlert();
                            }
                            shootingSystem_.CheckAndStop(shootingComp);
                            movementComp?.Move(targetDirAngle);
                        }
                        else
                        {
                            if (behaviourComp.IsStateAlert())
                            {
                                behaviourComp.EnterStateAttack();
                            }
                            if (aimDone)
                            {
                                shootingSystem_.CheckAndStart(shootingComp);
                            }
                            movementComp?.Stop();
                        }
                    }
                }
                else if (behaviourComp.IsStateReturn())
                {

                }
            });
        }

        (ShootingComponent, Position, Angle, bool) DoAim(IEntity entity, int childIndex, IEntity targetEntity)
        {
            // 子实体
            var child = entity.GetChild(childIndex); //GetChildWithTag(entity, childTag);
            if (child == null)
            {
                return (null, new Position(), new Angle(), false);
            }
            // 必须有射击组件
            var shootingComp = child.GetComponent<ShootingComponent>();
            if (shootingComp == null)
            {
                return (null, new Position(), new Angle(), false);
            }
            // 找不到目标
            if (targetEntity == null)
            {
                // 射击结束
                shootingComp.CheckAndStop();
                return (null, new Position(), new Angle(), false);
            }

            // 旋转炮塔到目标角度
            var (childTransformComp, childMovementComp) = child.GetComponents<TransformComponent, MovementComponent>();
            var targetTransformComp = targetEntity.GetComponent<TransformComponent>();
            //var transformComp = entity.GetComponent<TransformComponent>();
            var targetDir = targetTransformComp.Pos - childTransformComp.WorldPos;
            var targetDirAngle = targetDir.ToAngle();
            childTransformComp.RotateProcess(childMovementComp.CompDef.RotationSpeed, context_.FrameMs(), targetDirAngle, true);
            bool aimDone = false;
            if (childTransformComp.WorldOrientation == targetDirAngle)
            {
                aimDone = true;
            }

            return (shootingComp, childTransformComp.WorldPos, targetDirAngle, aimDone);
        }

        IEntity DoSearch(IEntity entity, out CampRelation relation)
        {
            var (transformComp, moveComp, campComp, searchComp) = entity.GetComponents<TransformComponent, MovementComponent, CampComponent, SearchComponent>();
            IEntity targetEntity = null;
            if (searchComp.TargetEntityInstId != 0)
            {
                targetEntity = context_.GetEntity(searchComp.TargetEntityInstId);
            }
            relation = CampRelation.Neutral;
            if (targetEntity == null)
            {
                uint currMs = context_.FrameMs() * context_.FrameNum();
                if (!searchComp.CanSearch(currMs))
                {
                    relation = CampRelation.Neutral;
                    return null;
                }
                searchComp.LastSearchMs = currMs;
                var circle = new Circle(transformComp.Pos, searchComp.CompDef.Radius);
                CampRelation campRelation = CampRelation.Neutral;
                targetEntity = gridSystem_.GetNearestEntityInCircle(circle, (IEntity entity2) =>
                {
                    // 默认不能是投射物
                    if (entity2.HasComponent<ProjectileComponent>())
                    {
                        return false;
                    }
                    var campComp2 = entity2.GetComponent<CampComponent>();
                    campRelation = ConfigManager.GetRelation(campComp.CampType, campComp2.CampType);
                    return campRelation == searchComp.CompDef.TargetRelation;
                });
            }
            if (targetEntity != null)
            {
                searchComp.TargetEntityInstId = targetEntity.InstId();
                relation = ConfigManager.GetRelation(campComp.CampType, targetEntity.GetComponent<CampComponent>().CampType);
                searchComp.TargetRelation = relation;
            }
            return targetEntity;
        }
    }
}