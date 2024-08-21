using System;
using Logic.Base;
using Logic.Interface;

namespace Logic.Component
{

    public class SearchCompDef : CompDef
    {
        public int Radius; // 半径
        public CampRelation TargetRelation; // 目标关系
        public int Intervals; // 间隔毫秒数

        public override IComponent Create(IComponentContainer container)
        {
            return new SearchComponent(container);
        }

        public override Type GetCompType()
        {
            return typeof(SearchComponent);
        }
    }

    public class SearchComponent : BaseComponent
    {
        SearchCompDef compDef_;

        uint targetEntityInstId_;
        uint lastSearchMs_;

        public SearchComponent(IComponentContainer container) : base(container)
        {

        }

        public SearchCompDef CompDef
        {
            get => compDef_;
        }

        public override void Init(CompDef compDef)
        {
            compDef_ = (SearchCompDef)compDef;
        }

        public override void Uninit()
        {
            targetEntityInstId_ = 0;
        }

        public override void Update(uint frameMs)
        {
        }

        public bool CanSearch(uint currMs)
        {
            return lastSearchMs_ == 0 || currMs - lastSearchMs_ >= compDef_.Intervals;
        }

        public bool CheckAndSetSearched(uint currMs)
        {
            bool canSearch = lastSearchMs_ == 0 || currMs - lastSearchMs_ >= compDef_.Intervals;
            if (canSearch)
            {
                lastSearchMs_ = currMs;
            }
            return canSearch;
        }

        public uint TargetEntityInstId
        {
            get => targetEntityInstId_;
            set => targetEntityInstId_ = value;
        }

        public uint LastSearchMs
        {
            get => lastSearchMs_;
            set => lastSearchMs_ = value;
        }
    }
}