using System;
using Common.Geometry;
using Logic.Base;
using Logic.Interface;

namespace Logic.Component
{
    public class InputCompDef : CompDef
    {
        public override IComponent Create(IComponentContainer container)
        {
            return new InputComponent(container);
        }

        public override Type GetCompType()
        {
            return typeof(InputComponent);
        }
    }
    public class InputComponent : BaseComponent
    {
        Angle worldOrientation_;

        public InputComponent(IComponentContainer container) : base(container)
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

        public Angle WorldOrientation
        {
            get => worldOrientation_;
            set => worldOrientation_ = value;
        }
    }
}