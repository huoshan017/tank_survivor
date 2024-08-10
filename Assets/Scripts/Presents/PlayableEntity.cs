using Common.Geometry;
using Logic.Base;
using Logic.Component;
using Logic.Entity;
using UnityEngine;

public class PlayableEntity : MonoBehaviour
{
  protected IPlayableContext playableContext_;
  protected GameObject hpBarObj_;
  protected AssetConfig assetConfig_;

  // Start is called before the first frame update
  protected void Start()
  {
    if (transformComp_ != null)
    {
      transform.localEulerAngles = new Vector3(0, 0, RotateDegree());
      lastLogicPos_ = transformComp_.Pos;
      transform.localPosition = new Vector3(lastLogicPos_.X(), lastLogicPos_.Y(), 0) / GlobalConstant.LogicAndUnityRatio;
    }
  }

  protected void Update()
  {
    UpdateHpBar(false);
  }

  public virtual void Attach(Entity entity, AssetConfig assetConfig)
  {
    entity_ = entity;
    assetConfig_ = assetConfig;
    entity_.RegisterPauseEvent(OnPauseHandle);
    entity_.RegisterResumeEvent(OnResumeHandle);
    (transformComp_, colliderComp_, charComp_) = entity.GetComponents<TransformComponent, ColliderComponent, CharacterComponent>();
    colliderComp_?.RegisterCollisionHandle(OnCollisionHandle);
    if (charComp_ != null)
    {
      charComp_?.RegisterHitHandle(OnHitHandle);
      charComp_.RegisterDeadHandle(OnDeadHandle);
      charComp_.RegisterHealHandle(OnHealHandle);
      charComp_.RegisterExpAddHandle(OnExpAddHandle);
      charComp_.RegisterLevelupHandle(OnLevelupHandle);
    }
  }

  public virtual void Detach()
  {
    entity_.UnregisterPauseEvent(OnPauseHandle);
    entity_.UnregisterResumeEvent(OnResumeHandle);
    entity_ = null;
    transformComp_ = null;
    if (colliderComp_ != null)
    {
      colliderComp_.UnregisterCollisionHandle(OnCollisionHandle);
      colliderComp_ = null;
    }
    if (charComp_ != null)
    {
      charComp_.UnregisterLevelupHandle(OnLevelupHandle);
      charComp_.UnregisterExpAddHandle(OnExpAddHandle);
      charComp_.UnregisterHealHandle(OnHealHandle);
      charComp_.UnregisterDeadHandle(OnDeadHandle);
      charComp_.UnregisterHitHandle(OnHitHandle);
    }
  }

  public IPlayableContext PlayableContext
  {
    set => playableContext_ = value;
  }

  // 暂停事件处理
  protected virtual void OnPauseHandle()
  {
    paused_ = true;
    //DebugLog.Info("entity " + entity_.InstId() + " paused");
  }

  // 恢复事件处理
  protected virtual void OnResumeHandle()
  {
    paused_ = false;
    //DebugLog.Info("entity " + entity_.InstId() + " resumed");
  }

  // 碰撞事件处理
  protected virtual void OnCollisionHandle(CollisionInfo hitInfo)
  {

  }

  // 击中事件处理
  protected virtual void OnHitHandle(HitInfo hitInfo)
  {
    string hitPfxName = "";
    var ptype = hitInfo.HitEntityProjectile.CompDef.PType;
    if (ptype != ProjectileType.Beam && ptype != ProjectileType.Shockwave)
    {
      hitPfxName = "bullet_hit";
    }
    else if (ptype == ProjectileType.Shockwave)
    {
      hitPfxName = "laser_impact";
    }
    if (hitPfxName != "")
    {
      var hitPfx = playableContext_.InstantiatePfxGameObject(hitPfxName, "Pfx");
      hitPfx.transform.position = new Vector2(hitInfo.Pos.X(), hitInfo.Pos.Y()) / GlobalConstant.LogicAndUnityRatio;
      var ps = hitPfx.GetComponent<ParticleSystem>();
      ps.Play();
      playableContext_.RecyclePfxGameObject(hitPfx, (uint)(ps.main.duration * 1000));
    }
    if (hitInfo.Damage > 0)
    {
      UpdateHpBar(true);
    }
  }

  // 死亡事件处理
  protected virtual void OnDeadHandle(DeadInfo dmgInfo)
  {
    var deadPfx = playableContext_.InstantiatePfxGameObject("big_death_explosion", "Pfx");
    if (deadPfx != null)
    {
      deadPfx.transform.position = new Vector2(lastLogicPos_.X(), lastLogicPos_.Y())/GlobalConstant.LogicAndUnityRatio;
      //deadPfx.transform.Rotate(new Vector3(-90, 0, 0));
      var ps = deadPfx.GetComponent<ParticleSystem>();
      ps.Play();
      playableContext_.RecyclePfxGameObject(deadPfx, (uint)(ps.main.duration * 1000));
    }
  }

  // 治疗事件处理
  protected virtual void OnHealHandle(HealInfo healInfo)
  {

  }

  // 经验增加事件处理
  protected virtual void OnExpAddHandle(int exp)
  {

  }

  // 升级事件处理
  protected virtual void OnLevelupHandle(int level, int exp)
  {

  }

  protected float RotateDegree()
  {
    var rotateAngle = transformComp_.Rotation; // 朝向
    return rotateAngle.Degree() + (float)rotateAngle.Minute() / 60;
  }

  void UpdateHpBar(bool hasHp)
  {
    if (hpBarObj_ == null && hasHp)
    {
      hpBarObj_ = playableContext_.InstantiateUiGameObject("hp_bar", "ui");
    }
    if (hpBarObj_ != null)
    {
      var hpbarOffset = assetConfig_.HpBarOffset;
      hpBarObj_.transform.position = new Vector2(transformComp_.Pos.X()+hpbarOffset.X(), transformComp_.Pos.Y()+hpbarOffset.Y())/GlobalConstant.LogicAndUnityRatio;
      if (hpBarObj_.TryGetComponent<HPBar>(out var hpBar))
      {
        hpBar.UpdateHealth((float)charComp_.Hp/charComp_.MaxHp);
      }
    }
  }

  protected Entity entity_;
  protected TransformComponent transformComp_;
  protected ColliderComponent colliderComp_;
  protected CharacterComponent charComp_;
  protected Position lastLogicPos_;
  protected Angle lastRotation_;
  protected bool paused_;
}