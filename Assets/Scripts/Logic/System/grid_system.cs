using System;
using System.Collections.Generic;
using Common;
using Common.Geometry;
using Logic.Base;
using Logic.Component;
using Logic.Interface;

namespace Logic.System
{
    public struct GridMapInfo
    {
        public int MapLeft, MapBottom; // 地图左下角
        public int SizeX, SizeY; // 地图大小
        public int GridWidth, GridHeight;
    }

    struct GridEntityInfo
    {
        public short line, column;
    }

    struct LineColumn
    {
        public short line, column;
    }

    public struct HitEntityInfo : IComparable<HitEntityInfo>
    {
        internal Position hitPoint;
        internal ulong hitDistanceSquare;
        internal IEntity entity;

        public readonly int CompareTo(HitEntityInfo other)
        {
            if (hitDistanceSquare < other.hitDistanceSquare) return -1;
            else if (hitDistanceSquare > other.hitDistanceSquare) return 1;
            else return 0;
        }

        public readonly Position HitPoint
        {
            get => hitPoint;
        }

        public readonly IEntity HitEntity
        {
            get => entity;
        }
    }

    public class GridSystem : SystemBase
    {
        int mapLeft_, mapBottom_;
        int mapSizeX_, mapSizeY_;
        int gridWidth_, gridHeight_; // 网格宽度高度
        short gridLineNum_, gridColNum_; // 网格的行数和列数
        MapListCombo<uint, GridEntityInfo> entityInstId2EntityList_; // 实体实例id到Rect映射集合     
        List<uint>[,] gridEntityInstIdListArray_; // 网格实体列表，用于提高查找性能
        readonly LineColumn[,] nineSquared_ = new LineColumn[3, 3]; // 九宫格
        List<Position> posListCache_;
        GridEntityInfo[] indexListCache_;

        public GridSystem(IContext context) : base(context)
        {
        }

        public override void Init(CompTypeConfig config)
        {
            base.Init(config);

            var mapInfo = (IMapInfo)context_;
            mapLeft_ = mapInfo.MapBounds.Left();
            mapBottom_ = mapInfo.MapBounds.Bottom();
            mapSizeX_ = mapInfo.MapBounds.Width();
            mapSizeY_ = mapInfo.MapBounds.Height();
            gridWidth_ = mapInfo.GridWidth;
            gridHeight_ = mapInfo.GridHeight;
            if (mapSizeY_ % gridHeight_ != 0 || mapSizeX_ % gridWidth_ != 0)
            {
                throw new Exception("map size or grid width or grid height invalid");
            }
            gridLineNum_ = (short)(mapSizeY_ / gridHeight_);
            gridColNum_ = (short)(mapSizeX_ / gridWidth_);
            DebugLog.Info("!!!!! Map gridLineNum " + gridLineNum_ + ", gridColNum " + gridColNum_);
            entityInstId2EntityList_ = new();
            gridEntityInstIdListArray_ = new List<uint>[gridLineNum_, gridColNum_];
            posListCache_ = new();
            indexListCache_ = new GridEntityInfo[3];
        }

        public override void Uninit()
        {
            indexListCache_ = null;
            posListCache_.Clear();
            posListCache_ = null;
            for (int i=0; i<gridLineNum_; i++)
            {
                for (int j=0; j<gridColNum_; j++)
                {
                    var l = gridEntityInstIdListArray_[i, j];
                    l?.Clear();
                    gridEntityInstIdListArray_[i, j] = null;
                }
            }
            gridEntityInstIdListArray_ = null;
            entityInstId2EntityList_.Clear();
            entityInstId2EntityList_ = null;

            base.Uninit();
            
        }

        public override void DoUpdate(uint frameMs)
        {
            entityInstId2EntityList_.Foreach((uint entityInstId, GridEntityInfo entityInfo)=>{
                var entity = context_.GetEntity(entityInstId);
                if (entity == null) return;
                var transformComp = entity.GetComponent<TransformComponent>();
                GetEntityLineCol(transformComp, out var line, out var col);
                // 更新实体的网格位置
                if (entityInfo.line != line || entityInfo.column != col)
                {
                    gridEntityInstIdListArray_[entityInfo.line, entityInfo.column].Remove(entityInstId);
                    gridEntityInstIdListArray_[line, col] ??= new();
                    gridEntityInstIdListArray_[line, col].Add(entityInstId);
                    //DebugLog.Info("Entity " + entityInstId + " removed from grid(" + entityInfo.line + ", " + entityInfo.column + ") add into grid(" + line + ", " + col + ")");
                    entityInfo.line = line;
                    entityInfo.column = col;
                    entityInstId2EntityList_.Set(entityInstId, entityInfo);
                }
            });
        }

        public override bool AddEntity(uint entityInstId)
        {
            if (!base.AddEntity(entityInstId))
            {
                return false;
            }

            var entity = context_.GetEntity(entityInstId);
            if (entity == null) return false;
            //var aabbComp = entity.GetComponent<AABBComponent>();
            //if (aabbComp == null) return false;
            var colliderComp = entity.GetComponent<ColliderComponent>();
            if (colliderComp == null) return false;

            if (!colliderComp.GetAABB(out var rect)) return false;

            // TODO 网格的宽和高必须是实体矩形的宽高的1.5倍以上，为了保证实体矩形在旋转一定角度后还能维持在宽松包围盒的范围内
            //if (gridWidth_ < aabbComp.CompDef.Width * 1.5)
            if (gridWidth_ < rect.Width()/* * 1.5*/)
            {
                //DebugLog.Warning("!!! Entity " + entity.InstId() + " width " + aabbComp.CompDef.Width + " too large for add into grid");
                DebugLog.Warning("!!! Entity " + entity.InstId() + " width " + rect.Width() + " too large for add into grid");
                return false;
            }
            //if (gridHeight_ < aabbComp.CompDef.Height * 1.5)
            if (gridHeight_ < rect.Height()/* * 1.5*/)
            {
                //DebugLog.Warning("!!! Entity " + entity.InstId() + " height " + aabbComp.CompDef.Height + " too large for add into grid");
                DebugLog.Warning("!!! Entity " + entity.InstId() + " height " + rect.Height() + " too large for add into grid");
                return false;
            }
            var transformComp = entity.GetComponent<TransformComponent>();
            GetEntityLineCol(transformComp, out var line, out var col);
            entityInstId2EntityList_.Add(entity.InstId(), new GridEntityInfo{line=line, column=col});
            var list = gridEntityInstIdListArray_[line, col];
            if (list == null)
            {
                list = new();
                gridEntityInstIdListArray_[line, col] = list;
            }
            list.Add(entityInstId);
            DebugLog.Info("Entity " + entity.InstId() + " added into grid(" + line + ", " + col + ")");

            return true;
        }

        public override bool RemoveEntity(uint entityInstId, int entityId)
        {
            var entity = context_.GetEntity(entityInstId);
            if (entity == null) return false;
            GridEntityInfo info = new();
            if (!entityInstId2EntityList_.Get(entityInstId, ref info)) return false;
            gridEntityInstIdListArray_[info.line, info.column].Remove(entityInstId);
            entityInstId2EntityList_.Remove(entityInstId);
            DebugLog.Info("Entity " + entityInstId + " removed from grid(" + info.line + ", " + info.column + ")");
            return base.RemoveEntity(entityInstId, entityId);
        }

        public void GetAroundEntityList(uint entityInstId, Func<IEntity, bool> filterHandle, List<IEntity> entityList)
        {
            ForeachFilteredEntityAround(entityInstId, filterHandle, (IEntity e)=>{
                entityList?.Add(e);
            });
        }

        public void ForeachFilteredEntityAround(uint entityInstId, Func<IEntity, bool> filterHandle, Action<IEntity> action)
        {
            // 九宫格
            GetNineSquaredGridLineColumnWithEntityInstId(entityInstId);
            // 从九宫格中过滤出实体
            for (int i=0; i<nineSquared_.GetLength(0); i++)
            {
                for (int j=0; j<nineSquared_.GetLength(1); j++)
                {
                    var line = nineSquared_[i, j].line;
                    if (line < 0)
                    {
                        continue;
                    }
                    
                    var column = nineSquared_[i, j].column;
                    var elist = gridEntityInstIdListArray_[line, column];
                    if (elist == null) continue;
                    foreach (var e in elist)
                    {
                        var entity = context_.GetEntity(e);
                        if (entity == null) continue;
                        if (e == entityInstId) continue;
                        if (filterHandle == null || (filterHandle != null && filterHandle(entity)))
                        {
                            action?.Invoke(entity);
                        }
                    }
                }
            }
        }

        // 获取轴对称矩形中的实体列表
        public bool GetEntityListInRect(Position center, int width, int height, List<uint> entityList)
        {
            (short line, short col, short line2, short col2) t = new();
            if (!GetRectLeftBottomRightTopLineCol(center, width, height, ref t))
            {
                return false;
            }

            for (int i=t.line; i<=t.line2; i++)
            {
                for (int j=t.col; j<=t.col2; j++)
                {
                    var elist = gridEntityInstIdListArray_[i, j];
                    if (elist == null) continue;
                    foreach (var e in elist)
                    {
                        var entity = context_.GetEntity(e);
                        if (entity == null) continue;
                        // TODO 精确的做法是用矩形是否相交来判断是否在范围内
                        var transformComp = entity.GetComponent<TransformComponent>();
                        var pos = transformComp.Pos;
                        // 判断中心点是否在矩形范围内
                        if (pos.X() >= center.X()-width/2 && pos.X() <= center.X()+width/2 && pos.Y() >= center.Y()-height/2 && pos.Y() <= center.Y()+height/2)
                        {
                            entityList.Add(e);
                        }
                    }
                }
            }

            return true;
        }

        // 获取圆形内的实体列表
        public bool GetEntityListInCircle(Circle circle, List<IEntity> entityList)
        {
            return GetEntityListInCircle(circle, null, entityList);
        }

        // 获取扇形内的实体列表
        public bool GetEntityListInSector(Sector sector, List<IEntity> entityList)
        {
            return GetEntityListInSector(sector, null, entityList);
        }

        // 获取圆形内的实体列表
        public bool GetEntityListInCircle(Circle circle, Func<IEntity, bool> filter, List<IEntity> entityList)
        {
            var center = circle.Center();
            var radius = circle.Radius();
            
            (short line, short col, short line2, short col2) t = new();
            if (!GetRectLeftBottomRightTopLineCol(center, 2*radius, 2*radius, ref t)) return false;

            for (int i=t.line; i<=t.line2; i++)
            {
                for (int j=t.col; j<=t.col2; j++)
                {
                    var elist = gridEntityInstIdListArray_[i, j];
                    if (elist == null) continue;
                    foreach (var e in elist)
                    {
                        var entity = context_.GetEntity(e);
                        if (entity == null) continue;
                        // TODO 精确的做法是用矩形是否相交来判断是否在范围内
                        //var aabbComp = entity.GetComponent<AABBComponent>();
                        var colliderComp = entity.GetComponent<ColliderComponent>();
                        // 判断矩形与圆是否相交或在圆内
                        //if (Foundation.IsRectIntersectCircle(aabbComp.BodyRect(), circle))
                        if (!colliderComp.GetAABB(out var rect)) continue;
                        if (Foundation.IsRectIntersectCircle(rect, circle))
                        {
                            if (filter == null || filter(entity))
                            {
                                entityList.Add(entity);
                            }
                        }
                    }
                }
            }

            return true;
        }

        // 获取圆内最近的实体
        public IEntity GetNearestEntityInCircle(Circle circle, Func<IEntity, bool> condition)
        {
            var center = circle.Center();
            var radius = circle.Radius();
            
            (short line, short col, short line2, short col2) t = new();
            if (!GetRectLeftBottomRightTopLineCol(center, 2*radius, 2*radius, ref t)) return null;

            var distanceSquare = ulong.MaxValue;
            IEntity entity = null;
            for (int i=t.line; i<=t.line2; i++)
            {
                for (int j=t.col; j<=t.col2; j++)
                {
                    var elist = gridEntityInstIdListArray_[i, j];
                    if (elist == null) continue;
                    foreach (var eid in elist)
                    {
                        var tempEntity = context_.GetEntity(eid);
                        if (tempEntity == null) continue;
                        // TODO 精确的做法是用矩形是否相交来判断是否在范围内
                        //var (transformComp, aabbComp) = tempEntity.GetComponents<TransformComponent, AABBComponent>();
                        var (transformComp, colliderComp) = tempEntity.GetComponents<TransformComponent, ColliderComponent>();
                        if (!colliderComp.GetAABB(out var rect)) continue;
                        // 判断矩形与圆是否相交或在圆内
                        //if (!Foundation.IsRectIntersectCircle(aabbComp.BodyRect(), circle))
                        if (!Foundation.IsRectIntersectCircle(rect, circle))
                        {
                            continue;
                        }
                        if (condition != null && !condition(tempEntity))
                        {
                            continue;
                        }
                        var ds = Position.DistanceSquare(center, transformComp.Pos);
                        if (ds < distanceSquare)
                        {
                            distanceSquare = ds;
                            entity = tempEntity;
                        }
                    }
                }
            }
            return entity;
        }

        // 获取扇形内的实体列表
        public bool GetEntityListInSector(Sector sector, Func<IEntity, bool> filter, List<IEntity> entityList)
        {
            var center = sector.Center();
            var radius = sector.Radius();
            var rotationBegin = sector.RotationBegin();
            var rotationEnd = sector.RotationEnd();
            var posA = center; posA.Translate(radius, 0); posA.Rotate(center.X(), center.Y(), rotationBegin);
            var posB = center; posB.Translate(radius, 0); posB.Rotate(center.X(), center.Y(), rotationEnd);
            var zero = Angle.Zero(); var halfPi = Angle.HalfPi();
            var pi = Angle.Pi(); var oneHalfPi = Angle.OneAndHalfPi();
            var twoPi = Angle.TwoPi();

            // 确定扇形占据的矩形范围
            int left, bottom, right, top;
            // 第一象限
            if (rotationBegin >= zero && rotationBegin < halfPi)
            {
                // 第一象限
                if (rotationEnd >= zero && rotationEnd < halfPi)
                {
                    // 两条边在同一象限要考虑谁大谁小的情况
                    if (rotationBegin < rotationEnd)
                    {
                        left = center.X();
                        bottom = center.Y();
                        right = posA.X();
                        top = posB.Y();
                    }
                    else
                    {
                        left = center.X() - radius;
                        bottom = center.Y() - radius;
                        right = center.X() + radius;
                        top = center.Y() + radius;
                    }
                }
                // 第二象限
                else if (rotationEnd >= halfPi && rotationEnd < pi)
                {
                    left = posB.X();
                    bottom = center.Y();
                    right = posA.X();
                    top = center.Y() + radius;
                }
                // 第三象限
                else if (rotationEnd >= pi && rotationEnd < oneHalfPi)
                {
                    left = center.X() - radius;
                    bottom = posB.Y();
                    right = posA.X();
                    top = center.Y() + radius;
                }
                // 第四象限
                else if (rotationEnd >= oneHalfPi && rotationEnd < twoPi)
                {
                    left = center.X() - radius;
                    bottom = center.Y() - radius;
                    right = Math.Max(posA.X(), posB.X());
                    top = center.Y() + radius;
                }
                else
                {
                    throw new Exception("rotationEnd cant greater 2PI");
                }
            }
            // 第二象限
            else if (rotationBegin >= halfPi && rotationBegin < pi)
            {
                // 第一象限
                if (rotationEnd >= zero && rotationEnd < halfPi)
                {
                    left = center.X() - radius;
                    bottom = center.Y() - radius;
                    right = center.X() + radius;
                    top = Math.Max(posA.Y(), posB.Y());
                }
                // 第二象限
                else if (rotationEnd >= halfPi && rotationEnd < pi)
                {
                    // 同一象限考虑大小
                    if (rotationBegin < rotationEnd)
                    {
                        left = posB.X();
                        bottom = center.Y();
                        right = center.X();
                        top = posA.Y();
                    }
                    else
                    {
                        left = center.X() - radius;
                        bottom = center.Y() - radius;
                        right = center.X() + radius;
                        top = center.Y() + radius;
                    }
                }
                // 第三象限
                else if (rotationEnd >= pi && rotationEnd < oneHalfPi)
                {
                    left = center.X() - radius;
                    bottom = posB.Y();
                    right = center.X();
                    top = posA.Y();
                }
                // 第四象限
                else if (rotationEnd >= oneHalfPi && rotationEnd < twoPi)
                {
                    left = center.X() - radius;
                    bottom = center.Y() - radius;
                    right = posB.X();
                    top = posA.Y();
                }
                else
                {
                    throw new Exception("rotationEnd cant greater 2PI");
                }
            }
            // 第三象限
            else if (rotationBegin >= pi && rotationBegin < oneHalfPi)
            {
                // 第一象限
                if (rotationEnd >= zero && rotationEnd < halfPi)
                {
                    left = posA.X();
                    bottom = center.Y() - radius;
                    right = center.X() + radius;
                    top = posB.Y();
                }
                // 第二象限
                else if (rotationEnd >= halfPi && rotationEnd < pi)
                {
                    left = Math.Min(posA.X(), posB.X());
                    bottom = center.Y() - radius;
                    right = center.X() + radius;
                    top = center.Y() + radius;
                }
                // 第三象限
                else if (rotationEnd >= pi && rotationEnd < oneHalfPi)
                {
                    // 考虑两个旋转角的大小
                    if (rotationBegin < rotationEnd)
                    {
                        left = posA.X();
                        bottom = posB.Y();
                        right = center.X();
                        top = center.Y();
                    }
                    else
                    {
                        left = center.X() - radius;
                        bottom = center.Y() - radius;
                        right = center.X() + radius;
                        top = center.Y() + radius;
                    }
                }
                // 第四象限
                else if (rotationEnd >= oneHalfPi && rotationEnd < twoPi)
                {
                    left = posA.X();
                    bottom = center.Y() - radius;
                    right = posB.X();
                    top = center.Y();
                }
                else
                {
                    throw new Exception("rotationEnd cant greater 2Pi");
                }
            }
            // 第四象限
            else if (rotationBegin >= oneHalfPi && rotationBegin < twoPi)
            {
                // 第一象限
                if (rotationEnd >= zero && rotationEnd < halfPi)
                {
                    left = center.X();
                    bottom = posA.Y();
                    right = center.X() + radius;
                    top = posB.Y();
                }
                // 第二象限
                else if (rotationEnd >= halfPi && rotationEnd < pi)
                {
                    left = posB.X();
                    bottom = posA.Y();
                    right = center.X() + radius;
                    top = center.Y() + radius;
                }
                // 第三象限
                else if (rotationEnd >= pi && rotationEnd < oneHalfPi)
                {
                    left = posB.X();
                    bottom = Math.Min(posA.Y(), posB.Y());
                    right = center.X() + radius;
                    top = center.Y() + radius;
                }
                // 第四象限
                else if (rotationEnd >= oneHalfPi && rotationEnd < twoPi)
                {
                    // 考虑角度大小
                    if (rotationBegin < rotationEnd)
                    {
                        left = center.X();
                        bottom = posA.Y();
                        right = posB.X();
                        top = center.Y();
                    }
                    else
                    {
                        left = center.X() - radius;
                        bottom = center.Y() - radius;
                        right = center.X() + radius;
                        top = center.Y() + radius;
                    }
                }
                else
                {
                    throw new Exception("rotationEnd cant greater 2Pi");
                }
            }
            else
            {
                throw new Exception("rotationBegin cant greater 2Pi");
            }
            
            (short line, short col, short line2, short col2) t = new();
            if (!GetRectLeftBottomRightTopLineCol(left, bottom, right, top, ref t)) return false;

            for (int i=t.line; i<=t.line2; i++)
            {
                for (int j=t.col; j<=t.col2; j++)
                {
                    var elist = gridEntityInstIdListArray_[i, j];
                    if (elist == null) continue;
                    foreach (var e in elist)
                    {
                        var entity = context_.GetEntity(e);
                        if (entity == null) continue;
                        // TODO 精确的做法是用矩形是否相交来判断是否在范围内
                        //var aabbComp = entity.GetComponent<AABBComponent>();
                        var colliderComp = entity.GetComponent<ColliderComponent>();
                        // 判断矩形与扇形是否相交或在扇形内
                        //if (Foundation.IsRectIntersectSector(aabbComp.BodyRect(), sector))
                        if (!colliderComp.GetAABB(out var rect)) continue;
                        if (Foundation.IsRectIntersectSector(rect, sector))
                        {
                            if (filter == null || filter(entity))
                            {
                                entityList.Add(entity);
                            }
                        }
                    }
                }
            }

            return true;
        }

        // 射线检测
        public bool Raycast(in Ray ray, Func<IEntity, bool> filterFunc, ref MinBinaryHeap<HitEntityInfo> rayHitInfoCache)
        {
            var start = ray.Origin;
            if (!InBounds(start))
            {
                return false;
            }

            var maxDistance = ray.MaxDistance;
            if (maxDistance <= 0)
            {
                return false;
            }

            var dirAngle = ray.Direction;
            var dx = (long)MathUtil.Cosine(dirAngle) * maxDistance / MathUtil.Denominator();
            var dy = (long)MathUtil.Sine(dirAngle) * maxDistance / MathUtil.Denominator();
            var ex = start.X() + (int)dx; var ey = start.Y() + (int)dy;
            //LimitPositionInBounds(start.X(), start.Y(), ref ex, ref ey);

            var end = new Position(ex, ey);
            return GetSegmentEntitiesIntersection(start, end, filterFunc, ref rayHitInfoCache);
        }

        // 在界限以内(包含边界)
        public bool InBounds(Position pos)
        {
            return pos.X()>=mapLeft_ && pos.X()<=mapLeft_+mapSizeX_ && pos.Y()>=mapBottom_ && pos.Y()<=mapBottom_+mapSizeY_;
        }
        
        public int MapLeft
        {
            get => mapLeft_;
        }

        public int MapBottom
        {
            get => mapBottom_;
        }

        public int MapWidth
        {
            get => mapSizeX_;
        }

        public int MapHeight
        {
            get => mapSizeY_;
        }

        public int MapGridWidth
        {
            get => gridWidth_;
        }

        public int MapGridHeight
        {
            get => gridHeight_;
        }

        bool GetRectLeftBottomRightTopLineCol(Position center, int width, int height, ref (short line, short col, short line2, short col2) t)
        {
            int left = center.X() - width/2;
            int bottom = center.Y() - height/2;
            return GetRectLeftBottomRightTopLineCol(left, bottom, width, height, ref t);
        }

        bool GetRectLeftBottomRightTopLineCol(int left, int bottom, int right, int top, ref (short line, short col, short line2, short col2) t)
        {
            if (left >= mapLeft_ + mapSizeX_)
            {
                return false;
            }
            if (right <= mapLeft_)
            {
                return false;
            }
            if (bottom >= mapBottom_ + mapSizeY_)
            {
                return false;
            }
            if (top <= mapBottom_)
            {
                return false;
            }
            if (left < mapLeft_)
            {
                left = mapLeft_;
            }
            if (right > mapLeft_ + mapSizeX_)
            {
                right = mapLeft_ + mapSizeX_;
            }
            if (bottom < mapBottom_)
            {
                bottom = mapBottom_;
            }
            if (top > mapBottom_ + mapSizeY_)
            {
                top = mapBottom_ + mapSizeY_;
            }
            var leftBottom = new Position(left, bottom);
            GetPositionLineCol(leftBottom, out t.line, out t.col);
            var rightTop = new Position(right, top);
            GetPositionLineCol(rightTop, out t.line2, out t.col2);
            return true;
        }

        bool GetEntityLineCol(TransformComponent transformComp, out short line, out short col)
        {
            var entityPos = transformComp.Pos;
            return GetPositionLineCol(entityPos, out line, out col);
        }

        bool GetPositionLineCol(Position pos, out short line, out short col)
        {
            return GetPositionLineCol(pos.X(), pos.Y(), out line, out col);
        }

        bool GetPositionLineCol(int x, int y, out short line, out short col, bool checkValid = true)
        {
            line = (short)((y - mapBottom_) / gridHeight_);
            col = (short)((x - mapLeft_) / gridWidth_);
            if (checkValid && (line < 0 || line >= gridLineNum_))
            {
                DebugLog.Error("invalid line " + line + " with position Y: " + y);
                return false;
            }
            if (checkValid && (col < 0 || col >= gridColNum_))
            {
                DebugLog.Error("invalid column " + col + " with position X: " + x);
                return false;
            }
            return true;
        }

        void GetNineSquaredGridLineColumnWithEntityInstId(uint entityInstId)
        {
            GridEntityInfo info = new();
            if (!entityInstId2EntityList_.Get(entityInstId, ref info)) return;
            for (short l=-1; l<=1; l++)
            {
                if (info.line+l<0 || info.line+l>=gridLineNum_)
                {
                    for (short c=-1; c<=1; c++)
                    {
                        nineSquared_[l-(-1), c-(-1)].line = -1;
                    }
                    continue;
                }
                for (short c=-1; c<=1; c++)
                {
                    if (info.column+c<0 || info.column+c>=gridColNum_)
                    {
                        nineSquared_[l-(-1), c-(-1)].line = -1;
                        continue;
                    }
                    nineSquared_[l-(-1), c-(-1)] = new LineColumn{line=(short)(info.line+l), column=(short)(info.column+c)};
                }
            }
        }

        void LimitPositionInBounds(int sx, int sy, ref int ex, ref int ey)
        {
            // 已经在范围内
            if (ex >= mapLeft_ && ex < mapLeft_ + mapSizeX_ && ey >= mapBottom_ && ey < mapBottom_ + mapSizeY_)
            {
                return;
            }

            // 平行于y轴
            if (sx == ex)
            {
                if (ey < mapBottom_)
                {
                    ey = mapBottom_;
                }
                if (ey >= mapBottom_ + mapSizeY_)
                {
                    ey = mapBottom_ + mapSizeY_ - 1;
                }
            }
            // 平行于x轴
            else if (sy == ey)
            {
                if (ex < mapLeft_)
                {
                    ex = mapLeft_;
                }
                if (ex >= mapLeft_ + mapSizeX_)
                {
                    ex = mapLeft_ + mapSizeX_ - 1;
                }
            }
            else
            {
                int x, y;
                // 左下方
                if (ex < sx && ey < sy)
                {
                    // 与左边相交
                    x = mapLeft_;
                    y = (ey - sy) * (x - sx) / (ex - sx) + sy;
                    // y超出范围，则与底边相交，重新计算x y
                    if (y < mapBottom_)
                    {
                        y = mapBottom_;
                        x = (ex - sx) * (y - sy) / (ey - sy) + sx;
                    }
                }
                // 右下方
                else if (ex > sx && ey < sy)
                {
                    // 与右边相交
                    x = mapLeft_ + mapSizeX_ - 1;
                    y = (ey - sy) * (x - sx) / (ex - sx) + sy;
                    if (y < mapBottom_)
                    {
                        y = mapBottom_;
                        x = (ex - sx) * (y - sy) / (ey - sy) + sx;
                    }
                }
                // 右上方
                else if (ex > sy && ey > sy)
                {
                    // 与右边相交
                    x = mapLeft_ + mapSizeX_ - 1;
                    y = (ey - sy) * (x - sx) / (ex - sx) + sy;
                    if (y >= mapBottom_ + mapSizeY_)
                    {
                        y = mapBottom_ + mapSizeY_ - 1;
                        x = (ex - sx) * (y - sy) / (ey - sy) + sx;
                    }
                }
                // 左上方
                else
                {
                    // 与左边相交
                    x = mapLeft_;
                    y = (ey - sy) * (x - sx) / (ex - sx) + sy;
                    if (y >= mapBottom_ + mapSizeY_)
                    {
                        y = mapBottom_ + mapSizeY_ - 1;
                        x = (ex - sx) * (y - sy) / (ey - sy) + sx;
                    }
                }
                ex = x; ey = y;
            }
        }

        bool GetSegmentEntitiesIntersection(Position start, Position end, Func<IEntity, bool> filterFunc, ref MinBinaryHeap<HitEntityInfo> rayHitInfoCache)
        {
            var sx = start.X(); var sy = start.Y();
            var ex = end.X(); var ey = end.Y();
            var dx = ex - sx; var dy = ey - sy;
            int si, ei, sj, ej, di, dj;
            bool flag = false;
            
            // 计算从start到end的线段在水平和垂直两个方向上的步进增量
            // 以一个方向的网格宽度和高度为增量遍历经过的网格，而另一个
            // 方向遍历时的增量按网格宽高比计算，不超过网格的高度和宽度，
            // 这样才能保证遍历所有经过的网格，而不遗漏掉任何网格。
            var absDx = Math.Abs(dx); var absDy = Math.Abs(dy);
            if (absDx * gridHeight_ >= absDy * gridWidth_) // x轴变化量大于y轴变化量
            {
                (si, ei, sj, ej) = (sx, ex, sy, ey);
                if (dy >= 0)
                {
                    dj = absDy * gridWidth_ / absDx;
                }
                else
                {
                    dj = -(absDy * gridWidth_ / absDx);
                }
                if (dx >= 0)
                {
                    di = gridWidth_;
                }
                else
                {
                    di = -gridWidth_;
                }
            }
            else // y轴变化量大于x轴
            {
                (si, ei, sj, ej) = (sy, ey, sx, ex);
                if (dx >= 0)
                {
                    dj = absDx * gridHeight_ / absDy;
                }
                else
                {
                    dj = -(absDx * gridHeight_ / absDy);
                }
                if (dy >= 0)
                {
                    di = gridHeight_;
                }
                else
                {
                    di = -gridHeight_;
                }
                flag = true;
            }

            bool isIntersect = false;
            var (i, j) = (si, sj);
            while ((di > 0 && i <= ei) || (di < 0 && i >= ei) || (dj > 0 && j <= ej) || (dj < 0 && j >= ej))
            {
                if (!flag) // x轴方向
                {
                    if (!GetPositionLineCol(i, j, out var line, out var col, false))
                    {
                        throw new Exception("Invalid position: " + i + ", " + j);
                    }
                    indexListCache_[1].line = line;
                    indexListCache_[1].column = col;
                    indexListCache_[0].line = (short)(line - 1);
                    indexListCache_[0].column = col;
                    indexListCache_[2].line = (short)(line + 1);
                    indexListCache_[2].column = col;
                }
                else // y轴方向 
                {
                    if (!GetPositionLineCol(j, i, out var line, out var col, false))
                    {
                        throw new Exception("Invalid position: " + i + ", " + j);
                    }

                    indexListCache_[1].line = line;
                    indexListCache_[1].column = col;

                    indexListCache_[0].line = line;
                    if (indexListCache_[1].column == 0)
                    {
                        indexListCache_[0].column = -1;
                    }
                    else
                    {
                        indexListCache_[0].column = (short)(col - 1);
                    }

                    indexListCache_[2].line = line;
                    if (indexListCache_[1].column == gridColNum_-1)
                    {
                        indexListCache_[2].column = -1;
                    }
                    else
                    {
                        indexListCache_[2].column = (short)(col + 1);
                    }
                }

                for (int k=0; k<indexListCache_.Length; k++)
                {
                    var idx = indexListCache_[k];
                    if (idx.line < 0 || idx.line >= gridLineNum_ || idx.column < 0 || idx.column >= gridColNum_)
                    {
                        continue;
                    }
                    var gridEntityList = gridEntityInstIdListArray_[idx.line, idx.column];
                    if (gridEntityList == null) continue;
                    foreach (var eid in gridEntityList)
                    {
                        var entity = context_.GetEntity(eid);
                        if (entity == null) continue;
                        if (!filterFunc(entity)) continue;
                        var colliderComp = entity.GetComponent<ColliderComponent>();
                        if (!colliderComp.GetAABB(out var rect)) continue;
                        var intersection = new Position();
                        if (Foundation.GetFirstIntersectionOfDirectionalSegmentAndRect(start, end, rect, ref intersection))
                        {
                            rayHitInfoCache.Set(new HitEntityInfo{
                                hitPoint = intersection,
                                hitDistanceSquare = Position.DistanceSquare(start, intersection),
                                entity = entity,
                            });
                            isIntersect = true;
                        }
                    }
                }

                i += di;
                if ((di > 0 && i > ei && i-di < ei) || (di < 0 && i < ei && i-di > ei))
                {
                    i = ei;
                }
                j += dj;
                if ((dj > 0 && j > ej && j-dj < ej) || (dj < 0 && j < ej && j-dj > ej))
                {
                    j = ej;
                }
            }
            return isIntersect;
        }
    }
}