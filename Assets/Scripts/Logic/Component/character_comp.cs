using System;
using Logic.Base;
using Logic.Interface;

namespace Logic.Component
{
    public class CharacterCompDef : CompDef
    {
        public int MaxHp;

        public int MaxLevel;

        public override IComponent Create(IComponentContainer container)
        {
            return new CharacterComponent(container);
        }

        public override Type GetCompType()
        {
            return typeof(CharacterComponent);
        }
    }

    public class CharacterComponent : BaseComponent
    {
        CharacterCompDef compDef_;
        int hp_;
        int level_;
        int exp_;
        bool dead_;
        int maxHp_;
        int maxLevel_;

        event Action<HitInfo> HitEvent_;
        event Action<HealInfo> HealEvent_;
        event Action<DeadInfo> DeadEvent_;
        event Action<int> ExpAddEvent_;
        event Action<int, int> LevelupEvent_;

        public CharacterComponent(IComponentContainer container) : base(container)
        {

        }

        public override void Init(CompDef compDef)
        {
            compDef_ = (CharacterCompDef)compDef;
            hp_ = compDef_.MaxHp;
            level_ = 1;
        }

        public override void Uninit()
        {
        }

        public override void Update(uint frameMs)
        {
        }

        public int AddHp(int hp, HealReason reason, uint fromEntity)
        {
            if (hp < 0)
            {
                throw new Exception("Add hp must positive value");
            }
            bool isFull = false;
            if (hp_ + hp > compDef_.MaxHp)
            {
                hp_ = compDef_.MaxHp;
                isFull = true;
            }
            else
            {
                hp_ += hp;
            }
            HealEvent_?.Invoke(new HealInfo { IsFullHp = isFull, HealHp = hp, Reason = reason, Healer = fromEntity });
            return hp_;
        }

        public int ReduceHp(int hp)
        {
            if (hp < 0)
            {
                throw new Exception("Reduce hp must positive value");
            }
            hp_ -= hp;
            if (hp_ <= 0)
            {
                hp_ = 0;
                dead_ = true;
            }
            return hp_;
        }

        public int AddExp(int exp, Func<int, (int, int, bool)> levelupFunc)
        {
            exp_ += exp;
            var (newLevel, newExp, levelUp) = levelupFunc(exp_);
            if (levelUp)
            {
                level_ = newLevel;
                exp_ = newExp;
            }
            ExpAddEvent_?.Invoke(exp);
            if (levelUp)
            {
                LevelupEvent_?.Invoke(level_, exp_);
            }
            return exp_;
        }

        public void RegisterHitHandle(Action<HitInfo> handle)
        {
            HitEvent_ += handle;
        }

        public void UnregisterHitHandle(Action<HitInfo> handle)
        {
            HitEvent_ -= handle;
        }

        public void RegisterHealHandle(Action<HealInfo> handle)
        {
            HealEvent_ += handle;
        }

        public void UnregisterHealHandle(Action<HealInfo> handle)
        {
            HealEvent_ -= handle;
        }

        public void RegisterDeadHandle(Action<DeadInfo> handle)
        {
            DeadEvent_ += handle;
        }

        public void UnregisterDeadHandle(Action<DeadInfo> handle)
        {
            DeadEvent_ -= handle;
        }

        public void RegisterExpAddHandle(Action<int> handle)
        {
            ExpAddEvent_ += handle;
        }

        public void UnregisterExpAddHandle(Action<int> handle)
        {
            ExpAddEvent_ -= handle;
        }

        public void RegisterLevelupHandle(Action<int, int> handle)
        {
            LevelupEvent_ += handle;
        }

        public void UnregisterLevelupHandle(Action<int, int> handle)
        {
            LevelupEvent_ -= handle;
        }

        public void OnHit(HitInfo hitInfo)
        {
            HitEvent_?.Invoke(hitInfo);
        }

        public void OnDead(DeadReason reason, int hpLoss, uint killer)
        {
            DeadEvent_?.Invoke(new DeadInfo { Reason = reason, HpLoss = hpLoss, Killer = killer });
        }

        public int MaxHp
        {
            get
            {
                if (maxHp_ > 0) return maxHp_;
                else return compDef_.MaxHp;
            }
            set => maxHp_ = value;
        }

        public int MaxLevel
        {
            get
            {
                if (maxLevel_ > 0) return maxLevel_;
                else return compDef_.MaxLevel;
            }
            set => maxLevel_ = value;
        }

        public int Hp
        {
            get => hp_;
        }

        public bool Dead
        {
            get => dead_;
        }
    }
}