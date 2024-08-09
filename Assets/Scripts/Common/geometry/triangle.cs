namespace Common.Geometry
{
    // 三角形
    public struct Triangle
    {
        Position pos1_;
        Position pos2_;
        Position pos3_;

        public Triangle(Position pos1, Position pos2, Position pos3)
        {
            pos1_ = pos1;
            pos2_ = pos2;
            pos3_ = pos3;
        }

        public readonly Position Pos1()
        {
            return pos1_;
        }

        public readonly Position Pos2()
        {
            return pos2_;
        }

        public readonly Position Pos3()
        {
            return pos3_;
        }

        // 计算面积
        public readonly int Area()
        {
            return (pos1_.X()*pos2_.Y() + pos2_.X()*pos3_.Y() + pos3_.X()*pos1_.Y() - pos1_.X()*pos3_.Y() - pos2_.X()*pos1_.Y() - pos3_.X()*pos2_.Y()) / 2;
        }

        // 是否与线段相交
        public readonly bool IsIntersectWithLine(Line line)
        {
            if (Foundation.IsLineIntersectSegment(line.Pos1(), line.Pos2(), pos1_, pos2_))
            {
                return true;
            }
            if (Foundation.IsLineIntersectSegment(line.Pos1(), line.Pos2(), pos2_, pos3_))
            {
                return true;
            }
            if (Foundation.IsLineIntersectSegment(line.Pos1(), line.Pos2(), pos3_, pos1_))
            {
                return true;
            }
            return false;
        }

        // 是否与线段相交
        public readonly bool IsIntersectWithSegment(Segment segment)
        {
            if (Foundation.IsSegmentIntersectSegment(segment.Start(), segment.End(), pos1_, pos2_))
            {
                return true;
            }
            if (Foundation.IsSegmentIntersectSegment(segment.Start(), segment.End(), pos2_, pos3_))
            {
                return true;
            }
            if (Foundation.IsSegmentIntersectSegment(segment.Start(), segment.End(), pos3_, pos1_))
            {
                return true;
            }
            return false;
        }

        // 是否包含点
        public readonly bool IsContainsPoint(Position point, bool includeOnEdge = false)
        {
            int relation = GetPointRelation(point);
            return relation > 0 || (includeOnEdge && relation == 0);
        }

        // 是否包含线段(不包含线段与三条边共线的情况)
        public readonly bool IsContainsSegment(Segment segment)
        {
            int relation1 = GetPointRelation(segment.Start());
            int relation2 = GetPointRelation(segment.End());
            return (relation1 >= 0 && relation2 > 0) || (relation1 > 0 && relation2 >= 0);
        }

        // 是否包含另一个三角形
        public readonly bool IsContainsTriangle(Triangle triangle)
        {
            int pointRelation1 = GetPointRelation(triangle.pos1_);
            if (pointRelation1 < 0)
            {
                return false;
            }
            int pointRelation2 = GetPointRelation(triangle.pos2_);
            if (pointRelation2 < 0)
            {
                return false;
            }
            int pointRelation3 = GetPointRelation(triangle.pos3_);
            return pointRelation3 >= 0;
        }

        // 是否与另一个三角形相交
        public readonly bool IsIntersectWithTriangle(Triangle triangle)
        {
            // TODO 如何实现 用SAT分离轴算法
            return false;
        }

        // 是否与另一个三角形有接触点(点接触和边接触)
        public readonly bool IsContactWithTriangle(Triangle triangle)
        {
            // TODO 如何实现
            return false;
        }

        // 私有函数，点跟三角形的位置关系，0在三角形边上，1在内部，-1在外部
        readonly int GetPointRelation(Position point)
        {
            // AB
            var AB = pos2_ - pos1_;
            // AO
            var AO = point - pos1_;
            // cross(AB, AO)
            var crossABAO = AB.Cross(AO);
            if (crossABAO == 0)
            {
                return 0;
            }
            // BC
            var BC = pos3_ - pos2_;
            // BO
            var BO = point - pos2_;
            // cross(BC, BO)
            var crossBCBO = BC.Cross(BO);
            if (crossBCBO == 0)
            {
                return 0;
            }
            // CA
            var CA = pos1_ - pos3_;
            // CO
            var CO = point - pos3_;
            // cross(CA, CO)
            var crossCACO = CA.Cross(CO);
            if (crossCACO == 0)
            {
                return 0;
            }

            // 三个内积都大于零，则AO,BO,CO为端点的三条线段都分别在三条边AB,BC,CA的逆时针方向
            if (crossABAO > 0 && crossBCBO > 0 && crossCACO > 0)
            {
                return 1;
            }

            return -1;
        }
    }
}