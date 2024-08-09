using System;
using System.Collections.Generic;
using Common;
using Logic.Base;
using Logic.Interface;

namespace Logic.System
{
    public class SystemManager : ISystemList
    {
        public SystemManager()
        {
            
        }

        public void Init()
        {
            systemList_ = new();
            compType2SystemList_ = new();
            noCompType2SystemList_ = new();
            childCompType2SystemList_ = new();
        }

        public void Uninit()
        {
            childCompType2SystemList_.Foreach((Type _, List<SystemBase> list)=>{
                list.Clear();
            });
            childCompType2SystemList_.Clear();
            childCompType2SystemList_ = null;
            noCompType2SystemList_.Foreach((Type _, List<SystemBase> list)=>{
                list.Clear();
            });
            noCompType2SystemList_.Clear();
            noCompType2SystemList_ = null;
            compType2SystemList_.Foreach((Type _, List<SystemBase> list)=>{
                list.Clear();
            });
            compType2SystemList_.Clear();
            compType2SystemList_ = null;
            foreach (var s in systemList_)
            {
                s.Uninit();
            }
            systemList_.Clear();
            systemList_ = null;
        }

        public void AddSystem(SystemBase system, CompTypeConfig config)
        {
            foreach (var s in systemList_)
            {
                if (system == s)
                {
                    return;
                }
            }
            systemList_.Add(system);
            if (config.CompTypeList != null)
            {
                foreach (var ct in config.CompTypeList)
                {
                    List<SystemBase> systemList = null;
                    if (!compType2SystemList_.Get(ct, ref systemList))
                    {
                        systemList = new();
                        compType2SystemList_.Add(ct, systemList);
                    }
                    systemList.Add(system);
                }
            }
            if (config.NoCompTypeList != null)
            {
                foreach (var nct in config.NoCompTypeList)
                {
                    List<SystemBase> systemList = null;
                    if (!noCompType2SystemList_.Get(nct, ref systemList))
                    {
                        systemList = new();
                        noCompType2SystemList_.Add(nct, systemList);
                    }
                    systemList.Add(system);
                }
            }
            if (config.ChildCompTypeList != null)
            {
                foreach (var cct in config.ChildCompTypeList)
                {
                    List<SystemBase> systemList = null;
                    if (!childCompType2SystemList_.Get(cct, ref systemList))
                    {
                        systemList = new();
                        childCompType2SystemList_.Add(cct, systemList);
                    }
                    systemList.Add(system);
                }
            }
            if (system.GetType() == typeof(InputSystem))
            {
                inputSystem_ = (InputSystem)system;
            }
        }

        public bool RemoveSystem(SystemBase system)
        {
            if (system.GetType() == typeof(InputSystem))
            {
                inputSystem_ = null;
            }
            var compTypeList = system.GetCompTypeList();
            if (compTypeList != null)
            {
                foreach (var ct in compTypeList)
                {
                    List<SystemBase> systemList = null;
                    if (compType2SystemList_.Get(ct, ref systemList))
                    {
                        systemList.Remove(system);
                    }
                }
            }
            var noCompTypeList = system.GetNoCompTypeList();
            if (noCompTypeList != null)
            {
                foreach (var nct in noCompTypeList)
                {
                    List<SystemBase> systemList = null;
                    if (noCompType2SystemList_.Get(nct, ref systemList))
                    {
                        systemList.Remove(system);
                    }
                }
            }
            var childCompTypeList = system.GetChildCompTypeList();
            if (childCompTypeList != null)
            {
                foreach (var cct in childCompTypeList)
                {
                    List<SystemBase> systemList = null;
                    if (childCompType2SystemList_.Get(cct, ref systemList))
                    {
                        systemList_.Remove(system);
                    }
                }
            }
            return systemList_.Remove(system);
        }

        // 添加组件后同时从系统中添加或删除实体
        public bool AddEntityWithCompAdded(IEntity entity, Type addedCompType)
        {
            List<SystemBase> systemList = null;
            if (!compType2SystemList_.Get(addedCompType, ref systemList))
            {
                return false;
            }
            foreach (var system in systemList)
            {
                if (!system.AddEntity(entity.InstId()))
                {
                    DebugLog.Info("add entity " + entity.InstId() + " to system: " + system.GetType().Name + " failed with component: " + addedCompType.Name + " added");
                }
            }
            // 需要删除对添加组件不感兴趣的系统中的实体
            if (noCompType2SystemList_.Get(addedCompType, ref systemList))
            {
                foreach (var system in systemList)
                {
                    system.RemoveEntity(entity.InstId(), entity.Id());
                }
            }
            return true;
        }

        // 删除组件后同时从系统中删除或添加实体
        public bool RemoveEntityWithCompRemoved(IEntity entity, Type removedCompType)
        {
            List<SystemBase> systemList = null;
            if (!compType2SystemList_.Get(removedCompType, ref systemList))
            {
                return false;
            }
            foreach (var system in systemList)
            {
                if (!system.RemoveEntity(entity.InstId(), entity.Id()))
                {
                    DebugLog.Warning("remove entity " + entity.InstId() + " from system: " + system.GetType().Name + " failed with component: " + removedCompType.Name + " removed");
                }
            }
            // 需要添加实体到对删掉组件不感兴趣的系统中
            if (noCompType2SystemList_.Get(removedCompType, ref systemList))
            {
                foreach (var system in systemList)
                {
                    system.AddEntity(entity.InstId());
                }
            }
            return true;
        }

        public void Update(uint frameMs)
        {
            foreach (var s in systemList_)
            {
                s.Update(frameMs);
            }
        }

        public InputSystem GetInputSystem()
        {
            return inputSystem_;
        }

        public T GetSystem<T>() where T : class, ISystem
        {
            return Funcs.GetElement<ISystem, T>(systemList_);
        }

        public void ForeachSystem(Action<ISystem> action)
        {
            foreach (var s in systemList_)
            {
                action(s);
            }
        }

        // 系统队列
        List<SystemBase> systemList_;
        // 必须的组件类型对应的系统列表
        MapListCombo<Type, List<SystemBase>> compType2SystemList_;
        // 必无的组件类型对应的系统列表
        MapListCombo<Type, List<SystemBase>> noCompType2SystemList_;
        // 至少一个子实体必有的组件类型对应的系统列表
        MapListCombo<Type, List<SystemBase>> childCompType2SystemList_;
        InputSystem inputSystem_;
    }
}