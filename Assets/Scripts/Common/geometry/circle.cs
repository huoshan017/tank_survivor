namespace Common.Geometry
{
  public struct Circle
  {
    Position center_;
    int radius_;

    public Circle(int radius)
    {
      center_ = new();
      radius_ = radius;
    }

    public Circle(Position center, int radius)
    {
      center_ = center;
      radius_ = radius;
    }

    public readonly Position Center()
    {
      return center_;
    }

    public readonly int Radius()
    {
      return radius_;
    }

    public readonly Rect BoundingRect()
    {
      return new Rect(center_.X()-radius_, center_.Y()-radius_, 2*radius_, 2*radius_);
    }

    public void Move(int x0, int y0)
    {
      center_.Translate(x0, y0);
    }

    public void MoveTo(int x, int y)
    {
      center_.Set(x, y);
    }

    public void MoveTo(Position pos)
    {
      center_.Set(pos.X(), pos.Y());
    }

    public void ChangeRadius(int radius)
    {
      radius_ = radius;
    }
  }
}