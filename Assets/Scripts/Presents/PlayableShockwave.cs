using Common;
using Common.Geometry;
using Logic.Component;
using Logic.Entity;
using UnityEngine;

public class PlayableShockwave : PlayableProjectile
{
    protected new void Start()
    {
        base.Start();
    }

    public override void Attach(Entity entity, AssetConfig assetConfig)
    {
        base.Attach(entity, assetConfig);
        particleSystem_ = gameObject.GetComponent<ParticleSystem>();
        laserParticleSystem_ = transform.GetChild(0).transform.GetChild(1).GetComponent<ParticleSystem>();
        // 把光束的方向调整到XY坐标系
        laserParticleSystem_.transform.Rotate(new Vector3(-90, 90, 0));
    }

    public override void Detach()
    {
      laserParticleSystem_ = null;
      particleSystem_ = null;
      base.Detach();
    }

    protected override void OnShootingStartHandle()
    {
        //gameObject.SetActive(true);
        if (!firstPlayed_)
        {
            particleSystem_.Play();
            firstPlayed_ = true;
        }
    }

    protected override void OnShootingStopHandle()
    {
        //gameObject.SetActive(false);
    }

    protected override void OnShootingHitHandle(ShootingHitInfo shootingInfo)
    {
        var logicPos = transformComp_.Pos;
        var origin = new Vector2(logicPos.X(), logicPos.Y())/GlobalConstant.LogicAndUnityRatio;
        particleSystem_.transform.position = origin;
        particleSystem_.Play();
    }

    bool firstPlayed_;
    ParticleSystem particleSystem_;
    ParticleSystem laserParticleSystem_;
}