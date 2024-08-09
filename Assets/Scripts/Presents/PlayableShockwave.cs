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
        var hitPos = shootingInfo.Pos;

        //var degree = shootingInfo.Dir.Degree() + (float)shootingInfo.Dir.Minute()/60;
        //var eulerAngles = new Vector3(0, 0, degree);
        //transform.eulerAngles = eulerAngles;
        //particleSystem_.transform.eulerAngles = eulerAngles;
        particleSystem_.transform.position = origin;
        particleSystem_.transform.LookAt(new Vector2(hitPos.X(), hitPos.Y())/GlobalConstant.LogicAndUnityRatio);
        var distance = Position.Distance(logicPos, hitPos);
        var lifeTime = laserParticleSystem_.main.startLifetime.constant;
        var speed = distance/GlobalConstant.LogicAndUnityRatio/lifeTime;
        var main = laserParticleSystem_.main;
        main.startSpeed = speed;
        particleSystem_.Play();
        DebugLog.Info("@@@ hitPos with shockwave " + hitPos.X() + ", " + hitPos.Y());
    }

    bool firstPlayed_;
    ParticleSystem particleSystem_;
    ParticleSystem laserParticleSystem_;
}