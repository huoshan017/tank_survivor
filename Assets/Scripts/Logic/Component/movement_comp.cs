using System;
using Common;
using Common.Geometry;
using Logic.Interface;
using Logic.Base;

namespace Logic.Component
{
    public enum MovementType
    {
        None = 0, // 不动
        Linear = 1, // 直线
        Circular = 2, // 圆周
        Elliptical = 3, // 椭圆
        Spiral = 4, // 螺线运动
    }

    // 运动组件定义
    public class MovementCompDef : CompDef
    {
        public MovementType MoveType; // 类型
        public bool MoveOnAwake; // 一出世就移动
        public int Speed; // 速度
        public int RotationSpeed; // 转向（自转速度），每秒度数

        public override IComponent Create(IComponentContainer container)
        {
            return new MovementComponent(container);
        }

        public override Type GetCompType()
        {
            return typeof(MovementComponent);
        }
    }

    // 圆周运动组件定义
    public class CircularMovementCompDef : MovementCompDef
    {
        public Position Center; // 圆心，本地坐标
        public int Radius; // 半径
        public int AngularVelocity; // 角速度

        public override IComponent Create(IComponentContainer container)
        {
            return new MovementComponent(container);
        }

        public override Type GetCompType()
        {
            return typeof(MovementComponent);
        }
    }

    // 椭圆运动组件定义
    public class EllipticalMovementCompDef : MovementCompDef
    {
        public override IComponent Create(IComponentContainer container)
        {
            return new MovementComponent(container);
        }

        public override Type GetCompType()
        {
            return typeof(MovementComponent);
        }
    }

    // 螺线运动组件定义
    public class SpiralMovementCompDef : MovementCompDef
    {
        public override IComponent Create(IComponentContainer container)
        {
            return new MovementComponent(container);
        }

        public override Type GetCompType()
        {
            return typeof(MovementComponent);
        }
    }

    internal enum MoveState
    {
        // 空闲，停止
        idle = 0,
        // 将要移动
        toMove = 1,
        // 移动中
        moving = 2,
        // 将要停止
        toStop = 3,
    }

    public class MovementComponent : BaseComponent
    {
        // 移动组件定义的引用
        MovementCompDef compDef_;
        // 移动方向
        Angle moveDir_;
        // 变换
        TransformComponent transform_;
        // 当前和先前状态
        MoveState currState_, prevState_;
        // 是否正在自转
        bool isRotating_;
        // 下个位置
        Position nextPos_;
        // 设置下个位置
        bool setNextPos_;
        // 移动事件
        protected event Action moveEvent_;
        // 停止移动事件
        protected event Action stopMoveEvent_;
        // 更新事件
        protected event Action updateEvent_;

        public MovementComponent(IComponentContainer container) : base(container)
        {
        }

        public override void Init(CompDef compDef)
        {
            compDef_ = (MovementCompDef)compDef;
            transform_ = container_.GetComponent<TransformComponent>();
        }

        public override void Uninit()
        {
        }

        public override void Update(uint frameMs)
        {
            // 每次Update都要重置上一次的位置
            transform_.LastPos = transform_.Pos;

            if (isRotating_)
            {
                if (!transform_.Orientation.Equal(moveDir_))
                {
                    var s = transform_.RotateProcess(compDef_.RotationSpeed, frameMs, moveDir_);
                    if (s >= Angle.HalfPi())
                    {
                        return;
                    }
                }
                else
                {
                    isRotating_ = false;
                }
            }

            if (currState_ == MoveState.idle)
            {
                if (prevState_ == MoveState.toStop)
                {
                    stopMoveEvent_?.Invoke();
                }
                return;
            }
            
            if (currState_ == MoveState.toMove)
            {
                SetState(MoveState.moving);
                moveEvent_?.Invoke();
            }

            if (setNextPos_) { transform_.Pos = nextPos_; setNextPos_ = false; }
            else { transform_.Pos = NextMovePos(frameMs); }

            updateEvent_?.Invoke();

            if (currState_ == MoveState.toStop)
            {
                SetState(MoveState.idle);
            }
        }
        
        public void Move(Angle moveDir)
        {
            // 先设置移动方向
            moveDir_ = moveDir;
            // 判断朝向和移动方向是否一致
            if (!transform_.Orientation.Equal(moveDir))
            {
                isRotating_ = true;
            }
            if (currState_ == MoveState.idle)
            {
                if (!CheckMove()) return;
                SetState(MoveState.toMove);
            }
        }

        public void Move()
        {
            SetState(MoveState.toMove);
        }

        // 停止
        public void Stop()
        {
            if (currState_ == MoveState.toMove)
            {
                SetState(MoveState.idle);
            }
            else if (currState_ == MoveState.moving)
            {
                SetState(MoveState.toStop);
            }
        }

        // 立即停止
        public void StopNow()
        {
            if (currState_ == MoveState.toMove || currState_ == MoveState.moving)
            {
                SetState(MoveState.idle);
                if (currState_ == MoveState.toMove)
                {
                }
                else
                {
                }
            }
        }

        // 是否在移动
        public bool IsMoving()
        {
            return currState_ == MoveState.toMove || currState_ == MoveState.moving || currState_ == MoveState.toStop;
        }

        // 如果是停止状态就开始移动
        public void IfStopToMove()
        {
            if (currState_ == MoveState.idle)
            {
                SetState(MoveState.toMove);
            }
        }

        public Position NextMovePos(uint frameMs)
        {
            Position pos = new();
            var moveType = compDef_.MoveType;
            if (moveType == MovementType.None || moveType == MovementType.Linear)
            {
                pos = Movement.LinearMove(transform_.Pos, compDef_.Speed, moveDir_, frameMs);
            }
            else if (moveType == MovementType.Circular)
            {
                var circularMoveCompDef = (CircularMovementCompDef)compDef_;
                pos = Movement.CircleMove(transform_.Pos, circularMoveCompDef.Center, circularMoveCompDef.Radius, circularMoveCompDef.AngularVelocity, frameMs);
            }
            else if (moveType == MovementType.Elliptical)
            {

            }
            else if (moveType == MovementType.Spiral)
            {

            }
            return pos;
        }

        public void UpdatePos(uint frameMs, Position pos, bool isStop = false)
        {
            nextPos_ = pos;
            setNextPos_ = true;
            if (isStop)
            {
                SetState(MoveState.toStop);
            }
            Update(frameMs);
        }

        // 检测是否可移动
        bool CheckMove()
        {
            return true;
        }

        void SetState(MoveState state)
        {
            prevState_ = currState_;
            currState_ = state;
        }

        // 注册移动事件
        public void RegisterMoveEvent(Action action)
        {
            moveEvent_ += action;
        }

        // 注销移动事件
        public void UnregisterMoveEvent(Action action)
        {
            moveEvent_ -= action;
        }

        // 注册停止移动事件
        public void RegisterStopMoveEvent(Action action)
        {
            stopMoveEvent_ += action;
        }

        // 注销停止移动事件
        public void UnregisterStopMoveEvent(Action action)
        {
            stopMoveEvent_ -= action;
        }

        // 注册更新事件
        public void RegisterUpdateEvent(Action action)
        {
            updateEvent_ += action;
        }

        // 注销更新事件
        public void UnregisterUpdateEvent(Action action)
        {
            updateEvent_ -= action;
        }

        public Angle MoveDir
        {
            get => moveDir_;
            set => moveDir_ = value;
        }

        public MovementCompDef CompDef
        {
            get => compDef_;
        }
    }
}