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
        // TODO 暂时不用调整了
        // 把光束的方向调整到XY坐标系
        // laserParticleSystem_.transform.Rotate(new Vector3(-90, 90, 0)); 
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
        var origin = new Vector3(logicPos.X(), 0, logicPos.Y())/GlobalConstant.LogicAndUnityRatio;
        particleSystem_.transform.position = origin;
        particleSystem_.Play();
    }

    bool firstPlayed_;
    ParticleSystem particleSystem_;
    ParticleSystem laserParticleSystem_;
}