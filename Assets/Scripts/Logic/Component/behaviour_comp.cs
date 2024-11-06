using System;
using System.Collections.Generic;
using Logic.Base;
using Logic.Interface;

namespace Logic.Component
{
    public class BehaviourCompDef : CompDef
    {
        public override IComponent Create(IComponentContainer container)
        {
            return new BehaviourComponent(container);
        }

        public override Type GetCompType()
        {
            return typeof(BehaviourComponent);
        }
    }

    public class BehaviourComponent : BaseComponent
    {
        BehaviourCompDef compDef_;
        BehaviourState state_ = BehaviourState.Standby;
        CharacterComponent charComp_;
        uint attacker_;
        List<string> childTagList_;
        int childTagIndex_;

        public BehaviourComponent(IComponentContainer container) : base(container)
        {

        }

        public override void Init(CompDef compDef)
        {
            compDef_ = (BehaviourCompDef)compDef;
            charComp_ = container_.GetComponent<CharacterComponent>();
            charComp_.RegisterHitHandle(OnHitHandle);
        }

        public override void Uninit()
        {
            if (childTagList_ != null)
            {
                childTagList_.Clear();
                childTagList_ = null;
            }
            charComp_.UnregisterHitHandle(OnHitHandle);
            charComp_ = null;
            compDef_ = null;
        }

        public override void Update(uint frameMs)
        {
        }

        public void EnterStateStandby()
        {
            state_ = BehaviourState.Standby;
        }

        public void EnterStatePatrol()
        {
            state_ = BehaviourState.Patrol;
        }

        public void EnterStateAlert()
        {
            state_ = BehaviourState.Alert;
        }

        public void EnterStateChase()
        {
            state_ = BehaviourState.Chase;
        }

        public void EnterStateAttack()
        {
            state_ = BehaviourState.Attack;
        }

        public void EnterStateReturn()
        {
            state_ = BehaviourState.Return;
        }

        public bool IsStateStandby()
        {
            return state_ == BehaviourState.Standby;
        }

        public bool IsStatePatrol()
        {
            return state_ == BehaviourState.Patrol;
        }

        public bool IsStateAlert()
        {
            return state_ == BehaviourState.Alert;
        }

        public bool IsStateChase()
        {
            return state_ == BehaviourState.Chase;
        }

        public bool IsStateAttack()
        {
            return state_ == BehaviourState.Attack;
        }

        public bool IsStateReturn()
        {
            return state_ == BehaviourState.Return;
        }

        public BehaviourCompDef CompDef
        {
            get => compDef_;
        }

        public uint Attacker
        {
            get => attacker_;
        }

        public int ChildTagListLength
        {
            get
            {
                CheckAndBuildChildTagList();
                if (childTagList_ == null) return 0;
                return childTagList_.Count;
            }
        }

        public string CurrentChildTag
        {
            get
            {
                CheckAndBuildChildTagList();
                if (childTagList_ == null) return "";
                return childTagList_[childTagIndex_];
            }
        }

        public string FirstChildTag
        {
            get
            {
                CheckAndBuildChildTagList();
                if (childTagList_ == null) return "";
                return childTagList_[0];
            }
        }

        public void Move2NextChildTag()
        {
            if (childTagList_ == null) return;
            childTagIndex_ = (childTagIndex_ + 1) % childTagList_.Count;
        }

        void OnHitHandle(HitInfo hitInfo)
        {
            if (hitInfo.Damage > 0)
            {
                if (state_ == BehaviourState.Standby || state_ == BehaviourState.Patrol)
                {
                    state_ = BehaviourState.Alert;
                    attacker_ = hitInfo.HitEntityProjectile.ShooterInstId;
                }
            }
        }

        void CheckAndBuildChildTagList()
        {
            var entity = (IEntity)container_;
            if (entity == null) return;
            entity.ForeachChild((IEntity child) =>
            {
                var tagComp = child.GetComponent<TagComponent>();
                if (tagComp != null)
                {
                    childTagList_ ??= new();
                    childTagList_.Add(tagComp.Name);
                }
            });
        }
    }
}