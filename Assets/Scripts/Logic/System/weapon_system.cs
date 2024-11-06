using Common.Geometry;
using Logic.Base;
using Logic.Interface;
using Logic.Component;
using Logic.System;

public class WeaponSystem : SystemBase
{
    public WeaponSystem(IContext context) : base(context)
    {

    }

    public override void DoUpdate(uint frameMs)
    {

    }

    public HitResult ProjectileHit(ProjectileComponent projectileComp, IEntity behitEntity, Position hitPoint)
    {
        int dmg = 0;
        HitResult result = HitResult.NoEffect;

        var ptype = projectileComp.CompDef.PType;
        if (ptype == ProjectileType.Beam)
        {
            var compDef = (BeamCompDef)projectileComp.CompDef;
            if (compDef != null)
            {
                dmg = (int)(compDef.DPS * context_.FrameMs() / 1000);
            }
        }
        else if (ptype == ProjectileType.Shockwave)
        {
            dmg = projectileComp.CompDef.Damage;
        }
        else if (ptype == ProjectileType.Bullet)
        {
            dmg = projectileComp.CompDef.Damage;
        }
        else if (ptype == ProjectileType.Missile)
        {

        }

        if (dmg > 0)
        {
            var charComp = behitEntity.GetComponent<CharacterComponent>();
            if (charComp != null)
            {
                charComp.ReduceHp(dmg);
                if (charComp.Dead)
                {
                    result = HitResult.Dead;
                    DeadReason reason = DeadReason.BulletHit;
                    if (ptype == ProjectileType.Beam)
                    {
                        reason = DeadReason.BeamHit;
                    }
                    charComp.OnDead(reason, dmg, projectileComp.ShooterInstId);
                }
                else
                {
                    result = HitResult.Damage;
                    charComp.OnHit(new HitInfo { IsPassive = true, HitEntityProjectile = projectileComp, BehitCollider = behitEntity.GetComponent<ColliderComponent>(), Damage = dmg, Pos = hitPoint });
                }
            }
        }

        return result;
    }
}