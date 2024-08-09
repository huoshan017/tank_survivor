using Logic.Base;
using Logic.Interface;

namespace Logic.Component
{
    public abstract class BaseComponent : IComponent
    {
        protected IComponentContainer container_;

        public BaseComponent(IComponentContainer container)
        {
            container_ = container;
        }

        public abstract void Init(CompDef compDef);

        public abstract void Uninit();

        public abstract void Update(uint frameMs);

        public IComponentContainer Container()
        {
            return container_;
        }
    }
}