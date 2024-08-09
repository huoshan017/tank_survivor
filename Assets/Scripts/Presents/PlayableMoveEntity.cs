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
    if (posUpdated_)
    {
      //lastLogicPos_ = Movement.LinearMove(lastLogicPos_, movementComp_.Speed, movementComp_.MoveDir, (uint)(Time.deltaTime*1000));
      transform.localPosition = new Vector3(lastLogicPos_.X(), lastLogicPos_.Y(), 0) / GlobalConstant.LogicAndUnityRatio;
      posUpdated_ = false;
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
    posUpdated_ = true;
  }

  // 移动事件处理
  protected virtual void OnMoveHandle()
  {
  }

  // 停止移动事件处理
  protected virtual void OnStopMoveHandle()
  {
    // 更新当前显示位置
    lastLogicPos_ = transformComp_.Pos;
    posUpdated_ = true;
  }

  protected MovementComponent movementComp_;
  bool posUpdated_;
}
