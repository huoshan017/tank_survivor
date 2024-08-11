using Common;
using Common.Geometry;
using Logic;
using Logic.Component;
using Logic.Entity;
using UnityEngine;

public class PlayableMoveEntity : PlayableEntity
{
  // Update is called once per frame
  protected new void Update()
  {
    base.Update();
    transform.localEulerAngles = new Vector3(0, 0, RotateDegree());
    //if (posUpdated_)
    if (state_ == MoveState.Moving)
    {
      //var nextPos = movementComp_.NextMovePos(entity_.Context.FrameMs());
      //transform.position = new Vector3(lastLogicPos_.X(), lastLogicPos_.Y(), 0) / GlobalConstant.LogicAndUnityRatio;
      lerpTotalDeltaTime_ += Time.deltaTime;
      transform.position = Vector3.Lerp(
        new Vector3(lastLogicPos_.X(), lastLogicPos_.Y(), 0)/GlobalConstant.LogicAndUnityRatio,
        new Vector3(nextPos_.X(), nextPos_.Y(), 0)/GlobalConstant.LogicAndUnityRatio,
        lerpTotalDeltaTime_*1000/entity_.Context.FrameMs());
      //if (movementComp_.IsMoving())
      {
        DebugLog.Info("entity " + entity_.InstId() + " transform position (" + transform.position.x + ", " + transform.position.y + ")");
      }
    }

    // TODO 画出包围盒，用于调试
    var colliderComp = entity_.GetComponent<ColliderComponent>();
    if (colliderComp != null && colliderComp.GetAABB(out var r))
    {
      var lb = r.LeftBottom();
      var rb = r.RightBottom();
      var rt = r.RightTop();
      var lt = r.LeftTop();
      var lbv2 = new Vector2(lb.X(), lb.Y()) / GlobalConstant.LogicAndUnityRatio;
      var rbv2 = new Vector2(rb.X(), rb.Y()) / GlobalConstant.LogicAndUnityRatio;
      var rtv2 = new Vector2(rt.X(), rt.Y()) / GlobalConstant.LogicAndUnityRatio;
      var ltv2 = new Vector2(lt.X(), lt.Y()) / GlobalConstant.LogicAndUnityRatio;
      Debug.DrawLine(lbv2, rbv2, Color.red);
      Debug.DrawLine(rbv2, rtv2, Color.red);
      Debug.DrawLine(rtv2, ltv2, Color.red);
      Debug.DrawLine(ltv2, lbv2, Color.red);
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
    }
  }

  public override void Detach()
  {
    base.Detach();
    if (movementComp_ != null)
    {
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
    lastLogicPos_ = transformComp_.Pos;
    nextPos_ = movementComp_.NextMovePos(entity_.Context.FrameMs());
    lerpTotalDeltaTime_ = 0;
    DebugLog.Info("entity " + entity_.InstId() + " real position (" + (float)lastLogicPos_.X()/GlobalConstant.LogicAndUnityRatio + ", " + (float)lastLogicPos_.Y()/GlobalConstant.LogicAndUnityRatio);
    DebugLog.Info("entity " + entity_.InstId() + " next position (" + (float)nextPos_.X()/GlobalConstant.LogicAndUnityRatio + ", " + (float)nextPos_.Y()/GlobalConstant.LogicAndUnityRatio);
  }

  // 移动事件处理
  protected virtual void OnMoveHandle()
  {
    state_ = MoveState.Moving;
  }

  // 停止移动事件处理
  protected virtual void OnStopMoveHandle()
  {
    // 更新当前显示位置
    lastLogicPos_ = transformComp_.Pos;
    nextPos_ = movementComp_.NextMovePos(entity_.Context.FrameMs());
    lerpTotalDeltaTime_ = 0;
    state_ = MoveState.Stopped;
  }

  protected MovementComponent movementComp_;
  enum MoveState
  {
    Stopped = 0,
    Moving = 1,
  }
  MoveState state_;
  Position nextPos_;
  float lerpTotalDeltaTime_;
}
