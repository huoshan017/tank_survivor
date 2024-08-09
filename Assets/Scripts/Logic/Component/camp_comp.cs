using System;
using Logic.Base;
using Logic.Interface;

namespace Logic.Component
{
    public class CampCompDef : CompDef
    {
        public CampType CampType;

        public override IComponent Create(IComponentContainer container)
        {
            return new CampComponent(container);
        }

        public override Type GetCompType()
        {
            return typeof(CampComponent);
        }
    }

    public class CampComponent : BaseComponent
    {
        CampCompDef compDef_;
        CampType campType_;

        public CampComponent(IComponentContainer container) : base(container)
        {
        }

        public override void Init(CompDef compDef)
        {
            compDef_ = (CampCompDef)compDef;
            campType_ = compDef_.CampType;
        }

        public override void Uninit()
        {

        }

        public override void Update(uint frameMs)
        {

        }

        public CampType CampType
        {
            get => campType_;
            set => campType_ = value;
        }
    }
}