using Common.Geometry;

namespace Common
{
  public enum ShapeType
  {
    Rect = 0, // 矩形
    Circle = 1, // 圆
    Segment = 2, // 线段
  }

  public struct Shape
  {
    readonly ShapeType shapeType_;
    Position center_;
    readonly int param1_, param2_;

    public Shape(ShapeType type, int param1)
    {
      shapeType_ = type;
      param1_ = param1;
      param2_ = 0;
      center_ = new();
    }

    public Shape(ShapeType type, int param1, int param2)
    {
      shapeType_ = type;
      param1_ = param1;
      param2_ = param2;
      center_ = new();
    }

    public Shape(ShapeType type, Position center, int param1)
    {
      shapeType_ = type;
      center_ = center;
      param1_ = param1;
      param2_ = 0;
    }

    public Shape(ShapeType type, Position center, int param1, int param2)
    {
      shapeType_ = type;
      center_ = center;
      param1_ = param1;
      param2_ = param2;
    }

    public readonly ShapeType GetShapeType()
    {
      return shapeType_;
    }

    public readonly bool GetRect(out Rect rect)
    {
      if (shapeType_ != ShapeType.Rect)
      {
        rect = new Rect();
        return false;
      }
      rect = new Rect(center_.X()-param1_/2, center_.Y()-param2_/2, param1_, param2_);
      return true;
    }

    public readonly bool GetCircle(out Circle circle)
    {
      if (shapeType_ != ShapeType.Circle)
      {
        circle = new Circle();
        return false;
      }
      circle = new Circle(center_, param1_);
      return true;
    }

    public readonly bool GetSegment(out Segment segment)
    {
      if (shapeType_ != ShapeType.Segment)
      {
        segment = new Segment();
        return false;
      }
      int x0 = center_.X();
      int y0 = center_.Y();
      Angle rotation = new((short)param2_, 0);
      int dx = param1_ * MathUtil.Cosine(rotation) / MathUtil.Denominator();
      int dy = param1_ * MathUtil.Sine(rotation) / MathUtil.Denominator();
      int x1 = x0 - dx;
      int y1 = y0 - dy;
      int x2 = x0 + dx;
      int y2 = y0 + dy;
      segment = new Segment(new Position(x1, y1), new Position(x2, y2));
      return true;
    }
  }
}