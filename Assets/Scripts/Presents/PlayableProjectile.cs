using Logic.Component;
using Logic.Entity;

public class PlayableProjectile : PlayableMoveEntity
{
    protected ProjectileComponent projectileComp_;

    public override void Attach(Entity entity, AssetConfig assetConfig)
    {
        base.Attach(entity, assetConfig);
        projectileComp_ = entity.GetComponent<ProjectileComponent>();
        projectileComp_.RegisterShootingHitEvent(OnShootingHitHandle);
        projectileComp_.RegisterEmitEvent(OnEmitHandle);
        projectileComp_.RegisterHitEvent(OnHitHandle);
        var shootingComp = projectileComp_.ShootingComp;
        if (shootingComp != null)
        {
            shootingComp.RegisterShootingStartEvent(OnShootingStartHandle);
            shootingComp.RegisterShootingStopEvent(OnShootingStopHandle);
        }
    }

    public override void Detach()
    {
        var shootingComp = projectileComp_.ShootingComp;
        if (shootingComp != null)
        {
            shootingComp.UnregisterShootingStopEvent(OnShootingStopHandle);
            shootingComp.UnregisterShootingStartEvent(OnShootingStartHandle);
        }
        projectileComp_.UnregisterHitEvent(OnHitHandle);
        projectileComp_.UnregisterEmitEvent(OnEmitHandle);
        projectileComp_.UnregisterShootingHitEvent(OnShootingHitHandle);
        projectileComp_ = null;
        base.Detach();
    }

    protected virtual void OnShootingStartHandle()
    {

    }

    protected virtual void OnShootingStopHandle()
    {

    }

    protected virtual void OnShootingHitHandle(ShootingHitInfo shootingInfo)
    {

    }

    protected virtual void OnEmitHandle(EmitInfo emitInfo)
    {

    }
}