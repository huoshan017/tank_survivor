using Common;
using Logic.Interface;
using Logic.Component;

namespace Logic.System
{
    public class LifeSystem : SystemBase
    {
        public LifeSystem(IContext context) : base(context)
        {
        }

        public override void DoUpdate(uint frameMs)
        {
            ForeachEntity((uint key)=>{
                var entity = context_.GetEntity(key);
                if (entity == null)
                {
                    return;
                }
                var comp = entity.GetComponent<LifeTimeComponent>();
                comp.Update(frameMs);
                if (comp.IsEnd)
                {
                    RecycleEntity(entity);
                }
            });
        }

        public override bool AddEntity(uint entityInstId)
        {
            if (!base.AddEntity(entityInstId)) return false;
            var entity = context_.GetEntity(entityInstId);
            if (entity == null) return false;
            var comp = entity.GetComponent<LifeTimeComponent>();
            if (comp != null)
            {
                //entityList_.Add(entity.InstId(), entity.InstId());
                comp.Begin();
                DebugLog.Info("entity " + entity.InstId() + " life time begin");
                return true;
            }
            return false;
        }
    }
}