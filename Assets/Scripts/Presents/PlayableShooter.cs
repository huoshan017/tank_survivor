using Common;
using Common.Geometry;
using Logic.Base;
using Logic.Component;
using Logic.Entity;
using Logic.Reader;
using UnityEngine;

public class PlayableShooter : PlayableEntity
{
  protected new void Update()
  {
    transform.eulerAngles = new Vector3(0, 0, WorldRotateDegree());
  }

  public override void Attach(Entity entity, AssetConfig assetConfig)
  {
    base.Attach(entity, assetConfig);
    shootingComp_ = entity.GetComponent<ShootingComponent>();
    shootingComp_.RegisterShootingStartEvent(OnShootingStartHandle);
    shootingComp_.RegisterShootingStopEvent(OnShootingStopHandle);
    shootingComp_.RegisterEmitEvent(OnEmitHandle);
  }

  public override void Detach()
  {
    shootingComp_.UnregisterEmitEvent(OnEmitHandle);
    shootingComp_.UnregisterShootingStartEvent(OnShootingStopHandle);
    shootingComp_.UnregisterShootingStartEvent(OnShootingStartHandle);
    base.Detach();
  }

  protected virtual void OnShootingStartHandle()
  {
    isShooting_ = true;
    DebugLog.Info("Shooting Start");
  }

  protected virtual void OnShootingStopHandle()
  {
    isShooting_ = false;
    DebugLog.Info("Shooting Stop");
  }

  protected virtual void OnEmitHandle(EmitInfo emitInfo)
  {
    if (isShooting_)
    {
      if (emitAudio_ == null)
      {
        emitAudio_ = gameObject.AddComponent<AudioSource>();
        // TODO 发射的音效，要根据投射物的不同来配置，这里暂时简单使用同一个音效
        emitAudio_.clip = Resources.Load<AudioClip>("Sounds/SFX/EnemyFire");
      }
      var projectileId = emitInfo.ShootingComp.CompDef.Projectile;
      var projectDef = ConfigManager.GetEntityDef(projectileId);
      var projCompDef = projectDef.GetCompDerivedFromT<ProjectileCompDef>();
      if (projCompDef.PType != ProjectileType.Beam && projCompDef.PType != ProjectileType.Shockwave)
      {
        emitAudio_.Play();
      }
    }
  }

  float WorldRotateDegree()
  {
    InputComponent inputComp = null;
    if (entity_.Parent != null)
    {
      inputComp = entity_.Parent.GetComponent<InputComponent>();
    }

    Angle rotateAngle;
    if (inputComp != null)
    {
      rotateAngle = transformComp_.GetWorldRotationFromWorldOrientation(inputComp.WorldOrientation);
    }
    else
    {
      rotateAngle = transformComp_.WorldRotation; // 朝向
    }
    return rotateAngle.Degree() + (float)rotateAngle.Minute() / 60;
  }

  protected ShootingComponent shootingComp_;

  protected bool isShooting_;

  protected AudioSource emitAudio_;
}