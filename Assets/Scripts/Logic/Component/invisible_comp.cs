using System;
using Logic.Base;
using Logic.Interface;

namespace Logic.Component
{
    public class InvisibleCompDef : CompDef
    {
        public bool Value;
        public override IComponent Create(IComponentContainer container)
        {
            return new InvisibleComponent(container);
        }

        public override Type GetCompType()
        {
            return typeof(InvisibleComponent);
        }
    }

    public class InvisibleComponent : BaseComponent
    {
        InvisibleCompDef compDef_;

        public InvisibleComponent(IComponentContainer container) : base(container)
        {
        }

        public override void Init(CompDef compDef)
        {
            compDef_ = (InvisibleCompDef)compDef;
        }

        public override void Uninit()
        {
        }

        public override void Update(uint frameMs)
        {
        }

        public InvisibleCompDef CompDef
        {
            get => compDef_;
            set => compDef_ = value;
        }
    }
}