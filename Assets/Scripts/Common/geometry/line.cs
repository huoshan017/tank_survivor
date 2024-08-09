namespace Common.Geometry
{
    // 直线
    public struct Line
    {
        Position pos1_;
        Position pos2_;

        public Line(Position pos1, Position pos2)
        {
            pos1_ = pos1;
            pos2_ = pos2;
        }

        public readonly Position Pos1()
        {
            return pos1_;
        }

        public readonly Position Pos2()
        {
            return pos2_;
        }

        // 是否与直线相交
        public readonly bool IsIntersectWithLine(Line line)
        {
            return Foundation.IsLineIntersectLine(pos1_, pos2_, line.pos1_, line.pos2_);
        }

        // 是否与线段相交
        public readonly bool IsIntersectWithSegment(Segment segment)
        {
            return Foundation.IsLineIntersectSegment(pos1_, pos2_, segment.Start(), segment.End());
        }

        // 是否与圆相交
        public readonly bool IsIntersectWithCircle(Circle circle, bool includeTangent = false)
        {
            return Foundation.IsLineIntersectCircle(pos1_, pos2_, circle.Center(), circle.Radius(), includeTangent);
        }

        // 与另一条直线的交点
        public readonly Position GetIntersectionWithLine(Line line)
        {
            return Foundation.GetIntersectionOfTwoLine(pos1_, pos2_, line.pos1_, line.pos2_);
        }

        // 与线段的交点
        public readonly (Position, bool) GetIntersectionWithSegment(Segment segment)
        {
            return Foundation.GetIntersectionOfLineAndSegment(pos1_, pos2_, segment.Start(), segment.End());
        }

        // 与圆的交点
        public readonly IntersectInfo GetIntersectionWithCircle(Circle circle)
        {
            return Foundation.GetIntersectionOfLineWithCircle(pos1_, pos2_, circle.Center(), circle.Radius());
        }
    }
}