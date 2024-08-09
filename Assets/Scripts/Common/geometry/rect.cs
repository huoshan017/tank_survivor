namespace Common.Geometry
{
    public struct Rect
    {
        int left_, bottom_;
        int width_, height_;

        public Rect(int left, int bottom, int width, int height)
        {
            left_ = left;
            bottom_ = bottom;
            width_ = width;
            height_ = height;
        }

        public void Reset(Position center, int width, int height)
        {
            left_ = center.X() - width/2;
            bottom_ = center.Y() - height/2;
            width_ = width;
            height_ = height;
        }

        public readonly int Left() { return left_; }
        public readonly int Bottom() { return bottom_; }
        public readonly int Right() { return left_ + width_; }
        public readonly int Top() { return bottom_ + height_; }
        public readonly int Width() { return width_; }
        public readonly int Height() { return height_; }
        public readonly Position LeftBottom() { return new Position(left_, bottom_); }
        public readonly Position LeftTop() { return new Position(left_, bottom_+height_); }
        public readonly Position RightTop() { return new Position(left_+width_, bottom_+height_); }
        public readonly Position RightBottom() { return new Position(left_+width_, bottom_); }
        public readonly Position Center() { return new Position(left_+width_/2, bottom_+height_/2); }

        public void SetCenter(int cx, int cy)
        {
            left_ = cx - width_/2;
            bottom_ = cy - height_/2;
        }

        public void SetCenter(Position center)
        {
            left_ = center.X() - width_/2;
            bottom_ = center.Y() - height_/2;
        }

        public void Move(int dx, int dy)
        {
            left_ += dx;
            bottom_ += dy;
        }

        public void MoveTo(int x, int y)
        {
            SetCenter(x, y);
        }

        public void MoveTo(Position pos)
        {
            SetCenter(pos.X(), pos.Y());
        }

        public readonly bool Intersect(Rect rect, bool includeTangent = false)
        {
            if (includeTangent)
            {
                return !(left_>rect.left_+rect.width_ || left_+width_<rect.left_ || bottom_>rect.bottom_+rect.height_ || bottom_+height_<rect.bottom_);
            }
            return !(left_>=rect.left_+rect.width_ || left_+width_<=rect.left_ || bottom_>=rect.bottom_+rect.height_ || bottom_+height_<=rect.bottom_);
        }
    }
}