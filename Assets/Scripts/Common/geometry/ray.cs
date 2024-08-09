namespace Common.Geometry
{
    public struct RaycastHit
    {
        public Position FirstIntersection;
    }

    public struct Ray
    {
        Position startPoint_;
        Angle direction_;
        readonly int maxDistance_; // 0表示不限距离
        int length_;

        public Ray(Position startPoint, Angle direction, int maxDistance = 0)
        {
            startPoint_ = startPoint;
            direction_ = direction;
            maxDistance_ = maxDistance;
            length_ = 0;
        }

        public Position Origin
        {
            readonly get => startPoint_;
            set => startPoint_ = value;
        }

        public Angle Direction
        {
            readonly get => direction_;
            set => direction_ = value;
        }

        public readonly int MaxDistance
        {
            get => maxDistance_;
        }

        public int Length
        {
            readonly get => length_;
            set => length_ = value;
        }

        public readonly bool IsIntersectWithRay(Ray ray)
        {
            return true;
        }

        public readonly bool IsIntersectWithSegment(Segment segment)
        {
            return true;
        }

        public readonly bool IsIntersectWithLine(Line line)
        {
            return true;
        }

        public readonly bool IsIntersectWithRect(Rect rect)
        {
            return true;
        }

        public readonly bool IsIntersectWithBox(Box box)
        {
            return true;
        }

        public readonly bool IsIntersectWithCircle(Circle circle)
        {
            return true;
        }
    }
}