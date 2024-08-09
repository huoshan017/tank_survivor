using System;
using Logic.Interface;
using Logic.Base;

namespace Logic.Component
{
    public class LifeTimeCompDef : CompDef
    {
        public int Seconds;

        public override IComponent Create(IComponentContainer container)
        {
            return new LifeTimeComponent(container);
        }

        public override Type GetCompType()
        {
            return typeof(LifeTimeComponent);
        }
    }

    public class LifeTimeComponent : BaseComponent
    {
        LifeTimeCompDef compDef_;
        uint usedMilliSeconds_;
        bool isBegin_;

        public LifeTimeComponent(IComponentContainer container) : base(container)
        {
        }

        public override void Init(CompDef compDef)
        {
            compDef_ = (LifeTimeCompDef)compDef;
        }

        public override void Uninit()
        {

        }

        public override void Update(uint frameMs)
        {
            if (!isBegin_) return;
            if (IsEnd) return;
            usedMilliSeconds_ += frameMs;
        }

        public void Begin()
        {
            isBegin_ = true;
        }

        public bool IsBegin
        {
            get => isBegin_;
        }

        public bool IsEnd
        {
            get
            {
                return usedMilliSeconds_ >= compDef_.Seconds*1000;
            }
        }
    }
}