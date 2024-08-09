using System;
using Logic.Interface;
using Logic.Base;

namespace Logic.Component
{
    public class TagCompDef : CompDef
    {
        public string Name;

        public override IComponent Create(IComponentContainer container)
        {
            return new TagComponent(container);
        }

        public override Type GetCompType()
        {
            return typeof(TagComponent);
        }
    }

    public class TagComponent : BaseComponent
    {
        TagCompDef compDef_;

        string name_;

        public TagComponent(IComponentContainer container) : base(container)
        {
        }

        public override void Init(CompDef compDef)
        {
            compDef_ = (TagCompDef)compDef;
            name_ = compDef_.Name;
        }

        public override void Uninit()
        {

        }

        public override void Update(uint frameMs)
        {

        }

        public string Name
        {
            get => name_;
            set => name_ = value;
        }
    }
}