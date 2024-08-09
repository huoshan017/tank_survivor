namespace Logic.Base
{
    // 阵营枚举
    public enum CampType
    {
        None = 0,
        One = 1,
        Two,
        Three,
        Four,
        Five,
        Six,
        Seven,
        Eight,
        Nine,
        Ten,
        Max,
    }

    // 阵营关系
    public enum CampRelation
    {
        // 中立
        Neutral = 0,
        // 友好
        Friendly = 1,
        // 敌对
        Hostile = 2,
    }

    // 所有者枚举
    public enum ObjOwnerType
    {
        None = 0, // 无所有者
        Player = 1, // 玩家所有
        Bot = 2, // 机器人，AI控制
        Bot4Player = 3, // 玩家拥有的机器人，AI控制，玩家部分控制
    }

    // 方向枚举
    public enum DirType
    {
        None = 0,
        Right = 1, // 右
        RightUp = 2, // 右上
        Up = 3, // 上
        LeftUp = 4, // 左上
        Left = 5, // 左
        LeftDown = 6, // 左下
        Down = 7, // 下
        RightDown = 8, // 右下
    }

    // 碰撞结果
    public enum CollisionResult
    {
        // 移动不受影响
        MoveNotAffected = 0,
        // 被阻挡，完全不能移动或只能移动一点
        Blocked = 1,
        // 消失，可能是因为销毁或爆炸或其他原因
        Disappear = 2,
    }

    // 投射物类型
    public enum ProjectileType
    {
        None = 0,
        Bullet = 1, // 子弹
        Missile = 2, // 导弹
        Bomb = 3, // 炸弹
        Beam = 4, // 光束
        Plasma = 5, // 等离子体
        Shockwave = 6, // 冲击波
    }

    // 伤害类型
    public enum DamageType
    {
        None = 0,
        Strike = 0x01, // 撞击伤害
        Explode = 0x02, // 爆炸伤害
        Penetrate = 0x04, // 穿透伤害
        Lighting = 0x08, // 闪电伤害
        Burns = 0x10, // 灼烧伤害
        Corrosion = 0x20, // 腐蚀伤害
    }

    // 行为状态
    public enum BehaviourState
    {
        Standby = 0, // 待机
        Patrol = 1, // 巡逻
        Alert = 2, // 警戒
        Chase = 3, // 追击
        Attack = 4, // 攻击
        Return = 5, // 返回中
    }

    // 受伤原因
    public enum DamageReason
    {
      BulletHit = 0, // 子弹攻击
      BeamHit = 1, // 光束攻击
      PulseHit = 2, // 脉冲攻击
      Explode = 3, // 爆炸
    }

    // 治疗原因
    public enum HealReason
    {
      BloodBottle = 0, // 血瓶
      LevelUp = 1, // 升级
    }

    // 死亡原因
    public enum DeadReason
    {
      BulletHit = 0,
      BeamHit = 1,
      Explode = 2,
    }

    // 被击结果
    public enum HitResult
    {
      NoEffect = 0, // 无影响
      Damage = 1, // 受伤
      Dead = 2, // 死亡
    }
}