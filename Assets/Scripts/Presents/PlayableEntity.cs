using Common.Geometry;
using Logic.Base;
using Logic.Component;
using Logic.Entity;
using UnityEngine;

public class PlayableEntity : MonoBehaviour
{
    // Start is called before the first frame update
    protected void Start()
    {
        if (transformComp_ != null)
        {
            rotateX_ = transform.localEulerAngles.x;
            rotateZ_ = transform.localEulerAngles.z;
            transform.localEulerAngles = new Vector3(rotateX_, RotateDegree(), rotateZ_);
            UpdatePos(true);
            transform.localPosition = new Vector3(lastLogicPos_.X(), 0, lastLogicPos_.Y()) / GlobalConstant.LogicAndUnityRatio;
        }
    }

    protected void Update()
    {
        transform.localEulerAngles = new Vector3(rotateX_, RotateDegree(), rotateZ_);
        UpdateHpBar(false);
        // TODO 画出包围盒，用于调试
        var colliderComp = entity_.GetComponent<ColliderComponent>();
        if (colliderComp != null && colliderComp.GetAABB(out var r))
        {
            var lb = r.LeftBottom();
            var rb = r.RightBottom();
            var rt = r.RightTop();
            var lt = r.LeftTop();
            var lbv2 = new Vector3(lb.X(), 0, lb.Y()) / GlobalConstant.LogicAndUnityRatio;
            var rbv2 = new Vector3(rb.X(), 0, rb.Y()) / GlobalConstant.LogicAndUnityRatio;
            var rtv2 = new Vector3(rt.X(), 0, rt.Y()) / GlobalConstant.LogicAndUnityRatio;
            var ltv2 = new Vector3(lt.X(), 0, lt.Y()) / GlobalConstant.LogicAndUnityRatio;
            Debug.DrawLine(lbv2, rbv2, Color.red);
            Debug.DrawLine(rbv2, rtv2, Color.red);
            Debug.DrawLine(rtv2, ltv2, Color.red);
            Debug.DrawLine(ltv2, lbv2, Color.red);
        }
    }

    protected void OnDestroy()
    {
        if (hpBarObj_ != null)
        {
            playableContext_.RecycleUiGameObject(hpBarObj_);
        }
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
            hitPfx.transform.position = new Vector3(hitInfo.Pos.X(), 0, hitInfo.Pos.Y()) / GlobalConstant.LogicAndUnityRatio;
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
            deadPfx.transform.position = new Vector3(lastLogicPos_.X(), 0, lastLogicPos_.Y()) / GlobalConstant.LogicAndUnityRatio;
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

    // 更新位置
    protected void UpdatePos(bool first)
    {
        if (first)
        {
            currLogicPos_ = transformComp_.Pos;
            lastLogicPos_ = currLogicPos_;
        }
        else
        {
            lastLogicPos_ = currLogicPos_;
            currLogicPos_ = transformComp_.Pos;
        }
    }

    // 旋转角度
    protected float RotateDegree()
    {
        var rotateAngle = transformComp_.Rotation; // 朝向
        return -(rotateAngle.Degree() + (float)rotateAngle.Minute() / 60);
    }

    void UpdateHpBar(bool hasHp)
    {
        if (hpBarObj_ == null && hasHp)
        {
            hpBarObj_ = playableContext_.InstantiateUiGameObject("hp_bar", "Ui");
        }
        if (hpBarObj_ != null)
        {
            var hpbarOffset = assetConfig_.HpBarOffset;
            hpBarObj_.transform.localPosition = new Vector3(
              transform.localPosition.x + hpbarOffset.X() / GlobalConstant.LogicAndUnityRatio,
              HPBar.YHeight / GlobalConstant.LogicAndUnityRatio,
              transform.localPosition.z + hpbarOffset.Y() / GlobalConstant.LogicAndUnityRatio);
            if (hpBarObj_.TryGetComponent<HPBar>(out var hpBar))
            {
                hpBar.UpdateHealth((float)charComp_.Hp / charComp_.MaxHp);
            }
        }
    }

    protected IPlayableContext playableContext_;
    protected GameObject hpBarObj_;
    protected AssetConfig assetConfig_;

    protected Entity entity_;
    protected TransformComponent transformComp_;
    protected ColliderComponent colliderComp_;
    protected CharacterComponent charComp_;
    protected Position lastLogicPos_, currLogicPos_;
    protected Angle lastRotation_;
    protected float rotateX_, rotateZ_;
    protected bool paused_;
}