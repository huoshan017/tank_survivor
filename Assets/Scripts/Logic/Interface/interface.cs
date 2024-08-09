using System;
using Common.Geometry;
using Logic.Base;

namespace Logic.Interface
{
    public interface IComponent
    {
        public void Init(CompDef compDef);
        public void Uninit();
        public void Update(uint frameMs);
        public IComponentContainer Container();
    }

    public interface IComponentContainer
    {
        public T GetComponent<T>() where T : class, IComponent;
        public (T, U) GetComponents<T, U>()
            where T : class, IComponent
            where U : class, IComponent;
        public (T, U, V) GetComponents<T, U, V>()
            where T : class, IComponent
            where U : class, IComponent
            where V : class, IComponent;
        public (T, U, V, W) GetComponents<T, U, V, W>()
            where T : class, IComponent
            where U : class, IComponent
            where V : class, IComponent
            where W : class, IComponent;
        public (T, U, V, W, X) GetComponents<T, U, V, W, X>()
            where T : class, IComponent
            where U : class, IComponent
            where V : class, IComponent
            where W : class, IComponent
            where X : class, IComponent;
        public (T, U, V, W, X, Y) GetComponents<T, U, V, W, X, Y>()
            where T : class, IComponent
            where U : class, IComponent
            where V : class, IComponent
            where W : class, IComponent
            where X : class, IComponent
            where Y : class, IComponent;
        public (T, U, V, W, X, Y, Z) GetComponents<T, U, V, W, X, Y, Z>()
            where T : class, IComponent
            where U : class, IComponent
            where V : class, IComponent
            where W : class, IComponent
            where X : class, IComponent
            where Y : class, IComponent
            where Z : class, IComponent;
        public IComponent GetComponent(Type type);
        public IComponent[] GetComponents();
        public bool HasComponent<T>() where T : IComponent;
        public bool HasComponent(Type compType);
        public bool HasComponent(string compName);
        public void AddComponent<T>(T comp) where T : IComponent;
        public void AddComponent(Type type);
        public void AddComponent(string compName);
        public void RemoveComponent(string compName);
        public void RemoveComponent<T>() where T : IComponent;
        public void RemoveComponent(Type compType);
    }

    public interface IEntity : IComponentContainer
    {
        public int Id();
        public uint InstId();
        public void Init();   
        public void Uninit();
        public void Deactive();
        public bool IsActive();
        public IContext Context { get; }
        public string Name { get; set; }
        public IEntity Parent { get; set; }
        public int ChildCount();
        public T GetChild<T>() where T : IEntity;
        public IEntity GetChildWithComponent<T>(Func<T, bool> filter) where T : IComponent;
        public IEntity GetChild(string name);
        public IEntity GetChild(int index);
        public IEntity[] GetChildren();
        public void AddChild<T>(T child) where T : IEntity;
        public void ForeachChild(Action<IEntity> action);
    }

    public interface ISystem
    {
        void Init(CompTypeConfig config);
        void Uninit();
        bool AddEntity(uint entityInstId);
        bool RemoveEntity(uint entityInstId, int entityId);
        void RecycleEntity(IEntity entity);
        void Update(uint frameMs);
        Type[] GetCompTypeList();
    }

    public interface ISystemList
    {
        public T GetSystem<T>() where T : class, ISystem;
        public void ForeachSystem(Action<ISystem> action);
    }

    public interface IContext
    {
        public uint FrameMs();
        public uint FrameNum();
        public IEntity CreateEntity(EntityDef def);
        public IEntity CreateEntity(EntityDef def, Action<IEntity> followingCreateHandle);
        public IEntity GetEntity(uint entityInstId);
        public bool RemoveEntity(uint entityInstId);
        public void RecycleEntity(IEntity entity);
        public void MarkRemoveEntity(uint entityInstId);
        public IEntity FindFirstEntity(Func<IEntity, bool> filterHandle);
        public IEntity FindFirstEntity(Func<IEntity, IEntity, bool> compareFilterHandle, IEntity compareEntity);
        public IEntity[] FindEntities(Func<IEntity, bool> filterHandle);
        public IEntity[] FindEntities(Func<IEntity, IEntity, bool> compareFilterHandle, IEntity compareEntity);
        public ISystemList SystemList();
    }

    public interface IMapInfo
    {
        Rect MapBounds{ get; }
        int GridWidth{ get; }
        int GridHeight{ get; }
    }
}