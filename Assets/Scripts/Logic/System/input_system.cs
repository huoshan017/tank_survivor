using Common;
using Common.Geometry;
using Logic.Base;
using Logic.Interface;
using Logic.Component;

namespace Logic.System
{
    public class InputSystem : SystemBase
    {
        ShootingSystem shootingSystem_;

        public InputSystem(IContext context) : base(context)
        {
        }

        public override void Init(CompTypeConfig config)
        {
            base.Init(config);
            shootingSystem_ = context_.SystemList().GetSystem<ShootingSystem>();
        }

        public override void Uninit()
        {
            shootingSystem_ = null;
            base.Uninit();
        }

        public override void DoUpdate(uint frameMs)
        {
        }

        public void ExecuteCmd(uint entityInstId, CmdData cmdData)
        {
            if (cmdData.Cmd == CommandDefine.CmdMove)
            {
                var entity = GetEntity(entityInstId);
                if (entity == null) return;
                if (entity.Parent == null)
                {
                    var movementComp = entity.GetComponent<MovementComponent>();
                    if (movementComp != null)
                    {
                        var dir = new Angle((short)cmdData.Args[0], 0);
                        movementComp.Move(dir);
                    }
                }
            }
            else if (cmdData.Cmd == CommandDefine.CmdStopMove)
            {
                var entity = GetEntity(entityInstId);
                if (entity == null) return;
                if (entity.Parent == null)
                {
                    var movementComp = entity.GetComponent<MovementComponent>();
                    movementComp?.Stop();
                }
            }
            else if (cmdData.Cmd == CommandDefine.CmdHeadForward)
            {
                var (entity, towerChild) = GetEntityAndTowerChild(entityInstId);
                if (entity != null && towerChild != null)
                {
                    var transformComp = towerChild.GetComponent<TransformComponent>();
                    if (transformComp != null)
                    {
                        var p = new Position((int)cmdData.Args[0], (int)cmdData.Args[1]);
                        var worldPos = transformComp.WorldPos;
                        var v = p - worldPos;
                        Angle orientation = v.ToAngle();
                        // 这里是唯一改变朝向的地方
                        transformComp.WorldOrientationTo(orientation);
                        var inputComp = entity.GetComponent<InputComponent>();
                        if (inputComp != null)
                        {
                            inputComp.WorldOrientation = orientation;
                        }
                    }
                }
            }
            else if (cmdData.Cmd == CommandDefine.CmdFire)
            {
                var towerChild = GetTowerChild(entityInstId);
                if (towerChild != null)
                {
                    var shootingComp = towerChild.GetComponent<ShootingComponent>();
                    shootingSystem_?.CheckAndStart(shootingComp);
                }
            }
            else if (cmdData.Cmd == CommandDefine.CmdStopFire)
            {
                var towerChild = GetTowerChild(entityInstId);
                if (towerChild != null)
                {
                    var shootingComp = towerChild.GetComponent<ShootingComponent>();
                    shootingSystem_?.CheckAndStop(shootingComp);
                }
            }
        }

        public IEntity GetTowerChild(uint entityInstId)
        {
            var (_, child) = GetEntityAndTowerChild(entityInstId);
            return child;
        }

        public (IEntity, IEntity) GetEntityAndTowerChild(uint entityInstId)
        {
            IEntity entity = null;
            entity = GetEntity(entityInstId);
            if (entity == null) return (null, null);
            var child = entity.GetChildWithComponent((TagComponent comp) =>
            {
                return comp.Name == "tower";
            });
            return (entity, child);
        }
    }
}