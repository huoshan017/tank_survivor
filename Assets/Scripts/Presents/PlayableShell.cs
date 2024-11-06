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
        transform.position = new Vector3(logicPos.X(), 0, logicPos.Y()) / GlobalConstant.LogicAndUnityRatio;
    }
}