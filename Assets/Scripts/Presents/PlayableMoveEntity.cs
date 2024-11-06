using Common.Geometry;
using Logic.Component;
using Logic.Entity;
using UnityEngine;

public class PlayableMoveEntity : PlayableEntity
{
    // Update is called once per frame
    protected new void Update()
    {
        if (state_ == MoveState.Moving || state_ == MoveState.ToStop)
        {
            //transform.localPosition = new Vector3(currLogicPos_.X(), currLogicPos_.Y(), 0) / GlobalConstant.LogicAndUnityRatio;
            transform.localPosition = Vector3.Lerp(
              new Vector3(lastLogicPos_.X(), 0, lastLogicPos_.Y()) / GlobalConstant.LogicAndUnityRatio,
              new Vector3(currLogicPos_.X(), 0, currLogicPos_.Y()) / GlobalConstant.LogicAndUnityRatio,
              moveLerpTotalDeltaTime_ * 1000 / entity_.Context.FrameMs()
            );
            moveLerpTotalDeltaTime_ += Time.deltaTime;
            if (state_ == MoveState.ToStop)
            {
                state_ = MoveState.Stopped;
            }
        }
        base.Update();
        if (isRotating_)
        {
            rotateLerpTotalDeltaTime_ += Time.deltaTime;
            var a = transformComp_.RotateGetRotationResult(movementComp_.CompDef.RotationSpeed, (uint)(rotateLerpTotalDeltaTime_ * 1000), targetDir_, true);
            var degree = a.Degree() + a.Minute() / 60;
            if (entity_.Parent == null)
            {
                transform.eulerAngles = new Vector3(rotateX_, -degree, rotateZ_);
            }
            else
            {
                transform.eulerAngles = new Vector3(rotateX_, rotateZ_, degree);
            }
        }
    }

    public override void Attach(Entity entity, AssetConfig assetConfig)
    {
        base.Attach(entity, assetConfig);
        movementComp_ = entity.GetComponent<MovementComponent>();
        if (movementComp_ != null)
        {
            movementComp_.RegisterUpdateEvent(OnUpdateHandle);
            movementComp_.RegisterMoveEvent(OnMoveHandle);
            movementComp_.RegisterStopMoveEvent(OnStopMoveHandle);
            movementComp_.RegisterRotateStartEvent(OnRotateStartHandle);
            movementComp_.RegisterRotateEndEvent(OnRotateEndHandle);
        }
    }

    public override void Detach()
    {
        base.Detach();
        if (movementComp_ != null)
        {
            movementComp_.UnregsiterRotateEndEvent(OnRotateEndHandle);
            movementComp_.UnregisterRotateStartEvent(OnRotateStartHandle);
            movementComp_.UnregisterUpdateEvent(OnUpdateHandle);
            movementComp_.UnregisterMoveEvent(OnMoveHandle);
            movementComp_.UnregisterStopMoveEvent(OnStopMoveHandle);
            movementComp_ = null;
        }
    }

    // 逻辑更新事件处理，频率一般低于渲染帧率
    protected virtual void OnUpdateHandle()
    {
        // 更新当前显示位置
        UpdatePos(false);
        moveLerpTotalDeltaTime_ = 0;
        logicPosUpdated_ = true;
    }

    // 移动事件处理
    protected virtual void OnMoveHandle()
    {
        UpdatePos(false);
        state_ = MoveState.Moving;
        logicPosUpdated_ = true;
    }

    // 停止移动事件处理
    protected virtual void OnStopMoveHandle()
    {
        // 更新当前显示位置
        UpdatePos(false);
        moveLerpTotalDeltaTime_ = 0;
        state_ = MoveState.ToStop;
        logicPosUpdated_ = true;
    }

    // 旋转开始事件处理
    protected virtual void OnRotateStartHandle(Angle targetDir)
    {
        isRotating_ = true;
        lastRotation_ = transformComp_.Rotation;
        targetDir_ = targetDir;
        rotateLerpTotalDeltaTime_ = 0;
    }

    // 旋转结束事件处理
    protected virtual void OnRotateEndHandle()
    {
        isRotating_ = false;
    }

    protected float TargetDirDegree()
    {
        return targetDir_.Degree() + (float)targetDir_.Minute() / 60;
    }

    protected MovementComponent movementComp_;
    protected bool logicPosUpdated_;
    enum MoveState
    {
        Stopped = 0,
        Moving = 1,
        ToStop = 2,
    }
    MoveState state_;
    bool isRotating_;
    Angle targetDir_;
    float moveLerpTotalDeltaTime_;
    float rotateLerpTotalDeltaTime_;
}
