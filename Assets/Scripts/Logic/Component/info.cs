using Common.Geometry;
using Logic.Base;

namespace Logic.Component
{
    public struct CollisionInfo
    {
        public bool IsPassive;
        public ColliderComponent HitEntityCollider;
        public ColliderComponent BehitEntityCollider;
        public Position Pos;
    }

    public struct HitInfo
    {
        public bool IsPassive;
        public ProjectileComponent HitEntityProjectile;
        public ColliderComponent BehitCollider;
        public Position Pos;
        public int Damage;
    }

    public struct HealInfo
    {
        public bool IsFullHp;
        public HealReason Reason;
        public int HealHp;
        public uint Healer;
    }

    /*public struct DamageInfo
    {
      public bool IsDead;
      public DamageReason Reason;
      public int DamageHp;
      public uint Attacker;
    }*/

    public struct DeadInfo
    {
        public DeadReason Reason;
        public int HpLoss;
        public uint Killer;
    }
}