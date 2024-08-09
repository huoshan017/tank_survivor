using System;
using System.Collections.Generic;

namespace ECS
{
    public class Entity
    {
        ulong instId_;
        List<IComponent> compList_;

        public void AddComponent(IComponent comp)
        {
            compList_ ??= new();
            compList_.Add(comp);
        }

        public T AddComponent<T>() where T : IComponent, new()
        {
            compList_ ??= new();
            var comp = new T();
            compList_.Add(comp);
            return comp;
        }

        public void AddComponent<T>(T comp) where T : IComponent
        {
            compList_ ??= new();
            compList_.Add(comp);
        }

        public T GetComponent<T>() where T : IComponent
        {
            T comp = default(T);
            if (compList_ != null)
            {
                foreach (var c in compList_)
                {
                    if (c.GetType() == typeof(T))
                    {
                        comp = (T)c;
                        break;
                    }
                }
            }
            return comp;
        }

        public IComponent GetComponent(Type type)
        {
            IComponent comp = null;
            if (compList_ != null)
            {
                foreach (var c in compList_)
                {
                    if (c.GetType() == type)
                    {
                        comp = c;
                        break;
                    }
                }
            }
            return comp;
        }

        public IComponent GetComponentAt(int index)
        {
            IComponent comp = null;
            if (compList_ != null)
            {
                comp = compList_[index];
            }
            return comp;
        }

        public T[] GetComponents<T>() where T : IComponent
        {
            T[] comps = null;
            if (compList_ != null)
            {
                int count = 0;
                for (int i=0; i<compList_.Count; i++)
                {
                    if (compList_[i].GetType() == typeof(T))
                    {
                        count += 1;
                    }
                }
                if (count > 0)
                {
                    comps = new T[count];
                    for (int i=0; i<compList_.Count; i++)
                    {
                        int n = 0;
                        if (compList_[i].GetType() == typeof(T))
                        {
                            comps[n] = (T)compList_[i];
                            n += 1;
                        }
                        if (n >= count) break;
                    }
                }
            }
            return comps;
        }

        public IComponent[] GetComponents(Type type) 
        {
            IComponent[] comps = null;
            if (compList_ != null)
            {
                int count = 0;
                for (int i=0; i<compList_.Count; i++)
                {
                    if (compList_[i].GetType() == type)
                    {
                        count += 1;
                    }
                }
                if (count > 0)
                {
                    comps = new IComponent[count];
                    for (int i=0; i<compList_.Count; i++)
                    {
                        int n = 0;
                        if (compList_[i].GetType() == type)
                        {
                            comps[n] = compList_[i];
                            n += 1;
                        }
                        if (n >= count) break;
                    }
                }
            }
            return comps;
        }

        public bool HasComponent<T>() where T : IComponent
        {
            bool has = false;
            if (compList_ != null)
            {
                foreach (var c in compList_)
                {
                    if (c.GetType() == typeof(T))
                    {
                        has = true;
                        break;
                    }
                }
            }
            return has;
        }

        public bool HasComponent(IComponent comp)
        {
            bool has = false;
            if (compList_ != null)
            {
                foreach (var c in compList_)
                {
                    if (c == comp)
                    {
                        has = true;
                        break;
                    }
                }
            }
            return has;
        }

        public bool RemoveComponent<T>() where T : IComponent
        {
            bool removed = false;
            if (compList_ != null)
            {
                for (int i=0; i<compList_.Count; i++)
                {
                    var c = compList_[i];
                    if (c.GetType() == typeof(T))
                    {
                        compList_.RemoveAt(i);
                        removed = true;
                        break;
                    }
                }
            }
            return removed;
        }

        public bool RemoveComponent(IComponent comp)
        {
            bool removed = false;
            if (compList_ != null)
            {
                for (int i=0; i<compList_.Count; i++)
                {
                    var c = compList_[i];
                    if (c == comp)
                    {
                        compList_.RemoveAt(i);
                        removed = true;
                        break;
                    }
                }
            }
            return removed;
        }

        public void RemoveComponentAt(int index)
        {
            if (compList_ != null)
            {
                compList_.RemoveAt(index);
            }
        }

        public bool RemoveComponents<T>()
        {
            if (compList_ == null) return false;
            bool removed = false;
            for (int i=0; i<compList_.Count; i++)
            {
                if (compList_[i].GetType() == typeof(T))
                {
                    compList_.RemoveAt(i);
                    removed = true;
                }
            }
            return removed;
        }

        public bool RemoveComponents(Type type)
        {
            if (compList_ == null) return false;
            bool removed = false;
            for (int i=0; i<compList_.Count; i++)
            {
                if (compList_[i].GetType() == type)
                {
                    compList_.RemoveAt(i);
                    removed = true;
                }
            }
            return removed;
        }
    }
}