using Logic.Base;
using Logic.Component;
using UnityEngine;

public class PlayableShell : PlayableProjectile
{
  protected override void OnEmitHandle(EmitInfo emitInfo)
  {
  }

  protected override void OnHitHandle(HitInfo hitInfo)
  {
    var logicPos = transformComp_.Pos;
    transform.position = new Vector2(logicPos.X(), logicPos.Y()) / GlobalConstant.LogicAndUnityRatio;
    var hitPos = hitInfo.Pos;
  }
}