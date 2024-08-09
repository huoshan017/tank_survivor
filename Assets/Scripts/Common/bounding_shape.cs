using Common.Geometry;

namespace Common
{
  public enum BoundingShapeType
  {
    Rect = 0,
    Circle = 1,
  }

  public struct BoundingShape
  {
    readonly BoundingShapeType shapeType_;
    Position center_;
    readonly int param1_, param2_;

    public BoundingShape(BoundingShapeType type, int param1)
    {
      shapeType_ = type;
      param1_ = param1;
      param2_ = 0;
      center_ = new();
    }

    public BoundingShape(BoundingShapeType type, int param1, int param2)
    {
      shapeType_ = type;
      param1_ = param1;
      param2_ = param2;
      center_ = new();
    }

    public BoundingShape(BoundingShapeType type, Position center, int param1)
    {
      shapeType_ = type;
      center_ = center;
      param1_ = param1;
      param2_ = 0;
    }

    public BoundingShape(BoundingShapeType type, Position center, int param1, int param2)
    {
      shapeType_ = type;
      center_ = center;
      param1_ = param1;
      param2_ = param2;
    }

    public readonly BoundingShapeType GetShapeType()
    {
      return shapeType_;
    }

    public readonly bool GetRect(out Rect rect)
    {
      if (shapeType_ != BoundingShapeType.Rect)
      {
        rect = new Rect();
        return false;
      }
      rect = new Rect(center_.X()-param1_/2, center_.Y()-param2_/2, param1_, param2_);
      return true;
    }

    public readonly bool GetCircle(out Circle circle)
    {
      if (shapeType_ != BoundingShapeType.Circle)
      {
        circle = new Circle();
        return false;
      }
      circle = new Circle(center_, param1_);
      return true;
    }
  }
}