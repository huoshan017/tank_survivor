using System;
using Logic.Base;
using Logic.Interface;

namespace Logic.Component
{
    public class FenceCompDef : CompDef
    {
        public override IComponent Create(IComponentContainer container)
        {
            return new FenceComponent(container);
        }

        public override Type GetCompType()
        {
            return typeof(FenceComponent);
        }
    }

    public class FenceComponent : BaseComponent
    {
        public FenceComponent(IComponentContainer container) : base(container)
        {

        }

        public override void Init(CompDef compDef)
        {
        }

        public override void Uninit()
        {
        }

        public override void Update(uint frameMs)
        {
        }
    }
}