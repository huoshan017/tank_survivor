using System;
using Common.Geometry;
using Logic.Base;
using Logic.Interface;
using Logic.Component;

namespace Logic.System
{
    public struct SystemData
    {
        public SystemBase system;
        public CompTypeConfig config;
    }

    public class SystemPolicy
    {
        public static SystemData[] Initialize(IContext context)
        {
            return new SystemData[] {
                new() {
                    // 网格地图系统
                    system = new GridSystem(context),
                    config = new CompTypeConfig{
                        CompTypeList = new Type[] {
                            typeof(ColliderComponent),
                            // typeof(TransformComponent)
                        },
                        //NoCompTypeList = new Type[] {
                        //  typeof(FenceComponent),
                        //}
                    }
                },
                new() {
                    // 行为系统
                    system = new BehaviourSystem(context),
                    config = new CompTypeConfig {
                        CompTypeList = new Type[] {
                            typeof(SearchComponent),
                        }
                    }
                },
                new() {
                    // 碰撞系统
                    system = new CollisionSystem(context),
                    config = new CompTypeConfig {
                        CompTypeList = new Type[] {
                            //typeof(FenceComponent),
                            //typeof(TransformComponent),
                            typeof(ColliderComponent)
                        }
                    }
                },
                new() {
                    // 移动系统
                    system = new MoveSystem(context),
                    config = new CompTypeConfig {
                        CompTypeList = new Type[] {
                            //typeof(TransformComponent),
                            typeof(MovementComponent),
                        }
                    }
                },
                new() {
                    // 射击系统
                    system = new ShootingSystem(context),
                    config = new CompTypeConfig {
                        ChildCompTypeList = new Type[] {
                            typeof(ShootingComponent),
                        }
                    }
                },
                new() {
                    // 武器系统
                    system = new WeaponSystem(context),
                    config = new CompTypeConfig {
                        CompTypeList = new Type[] {
                            typeof(CharacterComponent),
                        }
                    }
                },
                new() {
                    // 生命期系统
                    system = new LifeSystem(context),
                    config = new CompTypeConfig {
                        CompTypeList = new Type[] {
                            typeof(LifeTimeComponent)
                        }
                    }
                },
                new() {
                    // 输入系统
                    system = new InputSystem(context),
                    config = new CompTypeConfig {
                        CompTypeList = new Type[] {
                            typeof(InputComponent),
                            typeof(MovementComponent),
                        }
                    }
                },
            };
        }

        public static void DoCollision(ISystemList systemList, IEntity entity, IEntity entity2, Position contactPoint)
        {
            var (colliderComp, projComp, campComp) = entity.GetComponents<ColliderComponent, ProjectileComponent, CampComponent>();
            var (colliderComp2, projComp2, campComp2) = entity2.GetComponents<ColliderComponent, ProjectileComponent, CampComponent>();

            // 检测阵营
            if (campComp.CampType != campComp2.CampType)
            {
                var collisionSystem = systemList.GetSystem<CollisionSystem>();

                // 主动碰撞的投射物
                if (projComp != null)
                {
                    if (projComp2 == null)
                    {
                        var weaponSystem = systemList.GetSystem<WeaponSystem>();
                        var hitResult = weaponSystem.ProjectileHit(projComp, entity2, contactPoint);
                        if (hitResult == HitResult.Dead)
                        {
                            collisionSystem.RecycleEntity(entity2);
                        }
                        collisionSystem.RecycleEntity(entity);
                    }
                }
                else
                {
                    // 被碰撞的投射物碰撞完就销毁
                    if (projComp2 != null)
                    {
                        var weaponSystem = systemList.GetSystem<WeaponSystem>();
                        var hitResult = weaponSystem.ProjectileHit(projComp2, entity, contactPoint);
                        if (hitResult == HitResult.Dead)
                        {
                            collisionSystem.RecycleEntity(entity);
                        }
                        collisionSystem.RecycleEntity(entity2);
                    }
                    else
                    {
                        colliderComp.OnCollision(new CollisionInfo { IsPassive = true, HitEntityCollider = colliderComp, BehitEntityCollider = colliderComp2, Pos = contactPoint });
                        colliderComp2.OnCollision(new CollisionInfo { HitEntityCollider = colliderComp, BehitEntityCollider = colliderComp2, Pos = contactPoint });
                    }
                }
            }
        }
    }
}