using System;
using System.Collections.Generic;
using Common;
using Logic.Interface;

namespace Logic.Base
{
    // 命令数据
    public struct CmdData
    {
        public int Cmd;
        public long[] Args;
    }

    // 组件定义基类
    public abstract class CompDef
    {
        public string Type; // 类型字符串
        public uint Uid; // 唯一id
        public abstract IComponent Create(IComponentContainer container);
        public abstract Type GetCompType();
    }

    // 实体定义
    public class EntityDef
    {
        public int Id; // 类型id
        public string Name; // 名称
        public List<CompDef> CompDefList; // 组件列表
        public List<EntityDef> SubEntityDefList; // 子实体列表

        public void CloneOrMerge(EntityDef to)
        {
            to.Name = Name;
            to.CompDefList ??= new();
            to.SubEntityDefList ??= new();
            foreach (var compDef in CompDefList)
            {
                to.CompDefList.Add(compDef);
            }
            foreach (var subEntity in SubEntityDefList)
            {
                to.SubEntityDefList.Add(subEntity);
            }
        }

        public T GetCompDef<T>() where T : CompDef
        {
            return Funcs.GetElement<CompDef, T>(CompDefList);
        }

        public (T, U) GetCompDef<T, U>()
            where T : CompDef
            where U : CompDef
        {
            return Funcs.GetElements<CompDef, T, U>(CompDefList);
        }

        public (T, U, V) GetCompDef<T, U, V>()
            where T : CompDef
            where U : CompDef
            where V : CompDef
        {
            return Funcs.GetElements<CompDef,T, U, V>(CompDefList);
        }

        public (T, U, V, W) GetCompDef<T, U, V, W>()
            where T : CompDef
            where U : CompDef
            where V : CompDef
            where W : CompDef
        {
            return Funcs.GetElements<CompDef, T, U, V, W>(CompDefList);
        }

        public (T, U, V, W, X) GetCompDef<T, U, V, W, X>()
            where T : CompDef
            where U : CompDef
            where V : CompDef
            where W : CompDef
            where X : CompDef
        {
            return Funcs.GetElements<CompDef, T, U, V, W, X>(CompDefList);
        }

        public (T, U, V, W, X, Y) GetCompDef<T, U, V, W, X, Y>()
            where T : CompDef
            where U : CompDef
            where V : CompDef
            where W : CompDef
            where X : CompDef
            where Y : CompDef
        {
            return Funcs.GetElements<CompDef, T, U, V, W, X, Y>(CompDefList);
        }

        public (T, U, V, W, X, Y, Z) GetCompDef<T, U, V, W, X, Y, Z>()
            where T : CompDef
            where U : CompDef
            where V : CompDef
            where W : CompDef
            where X : CompDef
            where Y : CompDef
            where Z : CompDef
        {
            return Funcs.GetElements<CompDef, T, U, V, W, X, Y, Z>(CompDefList);
        }

        public bool HasCompDefDerivedFromT<T>() where T : CompDef
        {
            return Funcs.HasElementDerivedFromT<CompDef, T>(CompDefList);
        }

        public T GetCompDerivedFromT<T>() where T : CompDef
        {
            return Funcs.GetElementDerivedFromT<CompDef, T>(CompDefList);
        }
    }

    public struct CompTypeConfig
    {
        public Type[] CompTypeList; // 必须的组件类型
        public Type[] NoCompTypeList; // 没有的组件类型
        public Type[] ChildCompTypeList; // 至少一个子实体必须有的组件类型
    }
}