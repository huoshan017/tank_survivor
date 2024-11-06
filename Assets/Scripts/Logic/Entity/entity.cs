using System;
using System.Collections.Generic;
using Common;
using Logic.Base;
using Logic.Interface;
using Logic.Component;
using Logic.Const;

namespace Logic.Entity
{
    public abstract class BaseEntity : IEntity
    {
        // 上下文
        protected IContext context_;
        // 实例id
        protected uint instId_;
        // 类型id
        protected int id_;
        // 名称
        protected string name_;
        // 父实体
        protected IEntity parent_;
        // 是否暂停
        protected bool paused_;
        // 暂停事件
        protected event Action PauseEvent_;
        // 恢复事件
        protected event Action ResumeEvent_;
        // 添加组件事件
        protected event Action<IComponent> AddCompEvent_;
        // 删除组件事件
        protected event Action<IComponent> RemoveCompEvent_;
        // 组件队列
        protected List<IComponent> compList_;
        // 子实体队列
        protected List<IEntity> children_;

        // 初始化
        public abstract void Init();

        // 反初始化
        public abstract void Uninit();

        // 反激活
        public abstract void Deactive();

        // 是否激活
        public abstract bool IsActive();

        // 名称
        public string Name
        {
            get => name_;
            set => name_ = value;
        }

        // 父实体
        public IEntity Parent
        {
            get => parent_;
            set => parent_ = value;
        }

        // 上下文
        public IContext Context
        {
            get => context_;
        }

        // 获取组件
        public T GetComponent<T>() where T : class, IComponent
        {
            IComponent comp = null;
            if (compList_ != null)
            {
                for (int i = 0; i < compList_.Count; i++)
                {
                    if (compList_[i].GetType() == typeof(T))
                    {
                        comp = compList_[i];
                        break;
                    }
                }
            }
            return (T)comp;
        }

        public IComponent GetComponent(Type type)
        {
            IComponent comp = null;
            if (compList_ != null)
            {
                for (int i = 0; i < compList_.Count; i++)
                {
                    if (compList_[i].GetType() == type)
                    {
                        comp = compList_[i];
                        break;
                    }
                }
            }
            return comp;
        }

        public IComponent[] GetComponents()
        {
            if (compList_ == null) return null;
            var compList = new IComponent[compList_.Count];
            for (int i = 0; i < compList_.Count; i++)
            {
                compList[i] = compList_[i];
            }
            return compList;
        }

        public bool HasComponent<T>() where T : IComponent
        {
            foreach (var c in compList_)
            {
                if (c.GetType() == typeof(T))
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasComponent(Type compType)
        {
            foreach (var c in compList_)
            {
                if (c.GetType() == compType)
                {
                    return true;
                }
            }
            return false;
        }

        public bool HasComponent(string compName)
        {
            foreach (var c in compList_)
            {
                if (c.GetType().Name == compName)
                {
                    return true;
                }
            }
            return false;
        }

        public void AddComponent(IComponent comp)
        {
            compList_ ??= new List<IComponent>();
            compList_.Add(comp);
        }

        public void AddComponent<T>(T comp) where T : IComponent
        {
            compList_ ??= new List<IComponent>();
            compList_.Add(comp);
            AddCompEvent_?.Invoke(comp);
        }

        public void AddComponent(Type compType)
        {
            var comp = Activator.CreateInstance(compType, this);
            compList_ ??= new List<IComponent>();
            compList_.Add((IComponent)comp);
            AddCompEvent_?.Invoke((IComponent)comp);
        }

        public void AddComponent(string compName)
        {
            var type = Type.GetType(compName);
            AddComponent(type);
        }

        public T AddComponent<T>() where T : BaseComponent
        {
            compList_ ??= new List<IComponent>();
            IComponent comp = (IComponent)Activator.CreateInstance(typeof(T), this); //new T(owner);
            compList_.Add(comp);
            AddCompEvent_?.Invoke(comp);
            return (T)comp;
        }

        public void RemoveComponent<T>() where T : IComponent
        {
            foreach (var c in compList_)
            {
                if (c.GetType() == typeof(T))
                {
                    RemoveCompEvent_?.Invoke(c);
                    compList_.Remove(c);
                    break;
                }
            }
        }

        public void RemoveComponent(Type type)
        {
            if (type is not IComponent)
            {
                return;
            }

            foreach (var c in compList_)
            {
                if (c.GetType() == type)
                {
                    RemoveCompEvent_?.Invoke(c);
                    compList_.Remove(c);
                    break;
                }
            }
        }

        public void RemoveComponent(string compName)
        {
            foreach (var c in compList_)
            {
                if (c.GetType().Name == compName)
                {
                    RemoveCompEvent_?.Invoke(c);
                    compList_.Remove(c);
                    break;
                }
            }
        }

        public T GetChild<T>() where T : IEntity
        {
            IEntity entity = null;
            if (children_ != null)
            {
                for (int i = 0; i < children_.Count; i++)
                {
                    if (children_[i].GetType() == typeof(T))
                    {
                        entity = children_[i];
                        break;
                    }
                }
            }
            return (T)entity;
        }

        public IEntity GetChild(string name)
        {
            IEntity entity = null;
            if (name != "" && children_ != null)
            {
                for (int i = 0; i < children_.Count; i++)
                {
                    if (children_[i].Name == name)
                    {
                        entity = children_[i];
                        break;
                    }
                }
            }
            return entity;
        }

        public int ChildCount()
        {
            if (children_ == null) return 0;
            return children_.Count;
        }

        public IEntity GetChild(int index)
        {
            if (children_ == null || index > children_.Count - 1)
            {
                return null;
            }
            return children_[index];
        }

        public IEntity GetChildWithComponent<T>(Func<T, bool> filter) where T : IComponent
        {
            if (children_ == null) return null;
            foreach (var subEntity in children_)
            {
                foreach (var c in ((Entity)subEntity).compList_)
                {
                    if (c.GetType() == typeof(T) && filter((T)c))
                    {
                        return subEntity;
                    }
                }
            }
            return null;
        }

        public IEntity[] GetChildren()
        {
            if (children_ == null) return null;
            var entityList = new IEntity[children_.Count];
            for (int i = 0; i < children_.Count; i++)
            {
                entityList[i] = children_[i];
            }
            return entityList;
        }

        public void AddChild<T>(T child) where T : IEntity
        {
            children_ ??= new List<IEntity>();
            children_.Add(child);
        }

        public void ForeachChild(Action<IEntity> action)
        {
            if (children_ != null)
            {
                foreach (var c in children_)
                {
                    action(c);
                }
            }
        }

        public uint InstId()
        {
            return instId_;
        }

        public int Id()
        {
            return id_;
        }

        public void Pause()
        {
            if (!paused_)
            {
                paused_ = true;
                PauseEvent_?.Invoke();
            }
        }

        public void Resume()
        {
            if (paused_)
            {
                paused_ = false;
                ResumeEvent_?.Invoke();
            }
        }

        public void RegisterAddComponentEvent<T>(Action<IComponent> action) where T : IComponent
        {
            AddCompEvent_ += action;
        }

        public void UnregisterAddComponentEvent<T>(Action<IComponent> action) where T : IComponent
        {
            AddCompEvent_ -= action;
        }

        public void RegisterRemoveComponentEvent<T>(Action<IComponent> action) where T : IComponent
        {
            RemoveCompEvent_ += action;
        }
        public void UnregisterRemoveComponentEvent<T>(Action<IComponent> action) where T : IComponent
        {
            RemoveCompEvent_ -= action;
        }

        public void RegisterPauseEvent(Action action)
        {
            PauseEvent_ += action;
        }

        public void UnregisterPauseEvent(Action action)
        {
            PauseEvent_ -= action;
        }

        public void RegisterResumeEvent(Action action)
        {
            ResumeEvent_ += action;
        }

        public void UnregisterResumeEvent(Action action)
        {
            ResumeEvent_ -= action;
        }

        public (T, U) GetComponents<T, U>()
            where T : class, IComponent
            where U : class, IComponent
        {
            return Funcs.GetElements<IComponent, T, U>(compList_);
        }

        public (T, U, V) GetComponents<T, U, V>()
            where T : class, IComponent
            where U : class, IComponent
            where V : class, IComponent
        {
            return Funcs.GetElements<IComponent, T, U, V>(compList_);
        }

        public (T, U, V, W) GetComponents<T, U, V, W>()
            where T : class, IComponent
            where U : class, IComponent
            where V : class, IComponent
            where W : class, IComponent
        {
            return Funcs.GetElements<IComponent, T, U, V, W>(compList_);
        }

        public (T, U, V, W, X) GetComponents<T, U, V, W, X>()
            where T : class, IComponent
            where U : class, IComponent
            where V : class, IComponent
            where W : class, IComponent
            where X : class, IComponent
        {
            return Funcs.GetElements<IComponent, T, U, V, W, X>(compList_);
        }

        public (T, U, V, W, X, Y) GetComponents<T, U, V, W, X, Y>()
            where T : class, IComponent
            where U : class, IComponent
            where V : class, IComponent
            where W : class, IComponent
            where X : class, IComponent
            where Y : class, IComponent
        {
            return Funcs.GetElements<IComponent, T, U, V, W, X, Y>(compList_);
        }

        public (T, U, V, W, X, Y, Z) GetComponents<T, U, V, W, X, Y, Z>()
            where T : class, IComponent
            where U : class, IComponent
            where V : class, IComponent
            where W : class, IComponent
            where X : class, IComponent
            where Y : class, IComponent
            where Z : class, IComponent
        {
            return Funcs.GetElements<IComponent, T, U, V, W, X, Y, Z>(compList_);
        }
    }

    public class Entity : BaseEntity
    {
        EntityDef def_;
        bool isActivate_;

        public static void Reinit(Entity entity, IContext context, EntityDef def, uint instId)
        {
            entity.context_ = context;
            entity.instId_ = instId;
            entity.id_ = def.Id;
            entity.def_ = def;
            entity.Init();
        }

        // 构造函数
        public Entity(IContext context, EntityDef def, uint instId)
        {
            context_ = context;
            instId_ = instId;
            id_ = def.Id;
            def_ = def;
        }

        public override void Init()
        {
            isActivate_ = true;
            for (int i = 0; i < Static.NecessaryCompDefList.Length; i++)
            {
                var comp = Static.NecessaryCompDefList[i].Create(this);
                comp.Init(Static.NecessaryCompDefList[i]);
                AddComponent(comp);
            }
            for (int i = 0; i < def_.CompDefList.Count; i++)
            {
                var compDef = def_.CompDefList[i];
                IComponent comp = GetComponent(compDef.GetCompType());
                if (comp == null)
                {
                    comp = compDef.Create(this);
                    AddComponent(comp);
                }
                comp.Init(compDef);
            }
        }

        public override void Uninit()
        {
            if (compList_ != null)
            {
                for (int i = 0; i < compList_.Count; i++)
                {
                    compList_[i].Uninit();
                }
                compList_.Clear();
            }
            // 回收子实体
            if (children_ != null)
            {
                foreach (var child in children_)
                {
                    context_.RecycleEntity(child);
                }
                children_.Clear();
            }
            parent_ = null;
            isActivate_ = false;
        }

        // 反激活
        public override void Deactive()
        {
            isActivate_ = false;
        }

        // 是否激活的
        public override bool IsActive()
        {
            return isActivate_;
        }
    }
}