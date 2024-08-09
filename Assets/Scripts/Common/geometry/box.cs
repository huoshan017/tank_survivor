namespace Common.Geometry
{
    public struct Box
    {
        Position center_;
        Angle rotation_;
        readonly int width_;
        readonly int height_;

        public readonly Position Center()
        {
            return center_;
        }

        public readonly int Width()
        {
            return width_;
        }

        public readonly int Height()
        {
            return height_;
        }

        public readonly Angle Rotation()
        {
            return rotation_;
        }

        public readonly bool IsIntersectWithBox(Box box)
        {
            return true;
        }

        public readonly void GetFourVertices(out Position pos0, out Position pos1, out Position pos2, out Position pos3)
        {
            pos0 = new Position(center_.X()-width_/2, center_.Y()-height_/2);
            pos1 = new Position(center_.X()+width_/2, center_.Y()-height_/2);
            pos2 = new Position(center_.X()+width_/2, center_.Y()+height_/2);
            pos3 = new Position(center_.X()-width_/2, center_.Y()+height_/2);
            pos0.Rotate(center_.X(), center_.Y(), rotation_);   
            pos1.Rotate(center_.X(), center_.Y(), rotation_);
            pos2.Rotate(center_.X(), center_.Y(), rotation_);
            pos3.Rotate(center_.X(), center_.Y(), rotation_);
        }

        public void Translate(int dx, int dy)
        {
            center_.Translate(dx, dy);
        }

        public void TranslateTo(int x, int y)
        {
            center_.Set(x, y);
        }

        public void Rotate(Angle dAngle)
        {
            rotation_.Add(dAngle);
        }

        public void RotateTo(Angle angle)
        {
            rotation_ = angle;
        }
    }
}