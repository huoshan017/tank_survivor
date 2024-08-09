namespace Common.Geometry
{
    public struct Sector
    {
        Position center_;
        int radius_;
        Angle begin_;
        Angle end_;

        public Sector(Position center, int radius, Angle begin, Angle end)
        {
            center_ = center;
            radius_ = radius;
            begin_ = begin;
            end_ = end;
        }

        readonly public Position Center()
        {
            return center_;
        }

        readonly public int Radius()
        {
            return radius_;
        }

        readonly public Angle RotationBegin()
        {
            return begin_;
        }

        readonly public Angle RotationEnd()
        {
            return end_;
        }

        public void Move(int dx, int dy)
        {
            center_.Translate(dx, dy);
        }

        public void MoveTo(int x, int y)
        {
            center_.Set(x, y);
        }

        public void Rotate(Angle da)
        {
            begin_.Add(da);
            end_.Add(da);
        }
    }
}