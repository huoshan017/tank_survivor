using System;
using Common;
using Common.Geometry;
using Logic.Interface;
using Logic.Base;
using System.Runtime.InteropServices;

namespace Logic.Component
{
  public class ColliderCompDef : CompDef
  {
    public Shape Shape;
    public bool IsRigidBody;

    public override IComponent Create(IComponentContainer container)
    {
      return new ColliderComponent(container);
    }

    public override Type GetCompType()
    {
      return typeof(ColliderComponent);
    }
  }

  public class ColliderComponent : BaseComponent
  {
    ColliderCompDef compDef_;
    Rect aabb_;
    TransformComponent transformComp_;
    Shape shape_;
    bool isRigidBody_;

    event Action<CollisionInfo> CollisionEvent_;
    event Action<CollisionInfo> TriggerEvent_;

    public ColliderComponent(IComponentContainer container) : base(container)
    {
    }

    public override void Init(CompDef compDef)
    {
      compDef_ = (ColliderCompDef)compDef;
      aabb_ = new Rect();
      transformComp_ = container_.GetComponent<TransformComponent>();
      shape_ = compDef_.Shape;
      isRigidBody_ = compDef_.IsRigidBody;
    }

    public override void Uninit()
    {
      compDef_ = null;
      transformComp_ = null;
    }

    public override void Update(uint frameMs)
    {
    }

    public void RegisterCollisionHandle(Action<CollisionInfo> handle)
    {
      CollisionEvent_ += handle;
    }

    public void UnregisterCollisionHandle(Action<CollisionInfo> handle)
    {
      CollisionEvent_ -= handle;
    }

    public void RegisterTriggerHandle(Action<CollisionInfo> handle)
    {
      TriggerEvent_ += handle;
    }

    public void UnregisterTriggerHandle(Action<CollisionInfo> handle)
    {
      TriggerEvent_ -= handle;
    }

    public void OnCollision(CollisionInfo collisionInfo)
    {
      CollisionEvent_?.Invoke(collisionInfo);
    }

    public void OnTrigger(CollisionInfo triggerInfo)
    {
      TriggerEvent_?.Invoke(triggerInfo);
    }

    public bool GetRect(out Rect rect)
    {
      var shapeType = compDef_.Shape.GetShapeType();
      if (shapeType == ShapeType.Rect)
      {
        if (!compDef_.Shape.GetRect(out aabb_))
        {
          rect = aabb_;
          return false;
        }
        aabb_.MoveTo(transformComp_.Pos);
        rect = aabb_;
        return true;
      }
      rect = new();
      return false;
    }

    public bool GetCircle(out Circle circle)
    {
      var shapeType = compDef_.Shape.GetShapeType();
      if (shapeType == ShapeType.Circle)
      {
        if (!compDef_.Shape.GetCircle(out circle))
        {
          return false;
        }
        return true;
      }
      circle = new();
      return false;
    }

    public bool GetSegment(out Segment segment)
    {
      var shapeType = compDef_.Shape.GetShapeType();
      if (shapeType == ShapeType.Segment)
      {
        if (!compDef_.Shape.GetSegment(out segment))
        {
          return false;
        }
        return true;
      }
      segment = new();
      return false;
    }

    public bool GetAABB(out Rect rect)
    {
      if (GetRect(out rect))
      {
        return true;
      }
      if (GetCircle(out var circle))
      {
        rect = circle.BoundingRect();
        return true;
      }
      return false;
    }

    public bool IsInsideRect(Rect rect)
    {
      return TestIsInsideRectAtPosition(rect, transformComp_.Pos);
    }

    internal bool TestIsInsideRectAtPosition(Rect rect, Position pos)
    {
      var inside = false;
      var shape = compDef_.Shape;
      var shapeType = shape.GetShapeType();
      if (shapeType == ShapeType.Rect)
      {
        shape.GetRect(out var r);
        r.SetCenter(pos);
        if (r.Left()>=rect.Left() && r.Right()<=rect.Right() && r.Bottom()>=rect.Bottom() && r.Top()<=rect.Top())
        {
          inside = true;
        }
      }
      else if (shapeType == ShapeType.Circle)
      {
        shape.GetCircle(out var c);
        c.MoveTo(pos);
        var x = pos.X();
        var y = pos.Y();
        var radius = c.Radius();
        if (x-radius>=rect.Left() && x+radius<=rect.Right() && y-radius>=rect.Bottom() && y+radius<=rect.Top())
        {
          inside = true;
        }
      }
      return inside;
    }

    public bool IsIntersect(ColliderComponent colliderComp, bool includeContact = false)
    {
      return TestIsIntersectAtPosition(transformComp_.Pos, colliderComp, includeContact);
    }

    internal bool TestIsIntersectAtPosition(Position pos, ColliderComponent colliderComp, bool includeContact = false)
    {
      bool isIntersect = false;
      var shape = compDef_.Shape;
      var shapeType = shape.GetShapeType();
      var shape2 = colliderComp.compDef_.Shape;
      var shapeType2 = shape2.GetShapeType();
      if (shapeType == ShapeType.Rect)
      {
        shape.GetRect(out var rect);
        rect.SetCenter(pos);
        if (shapeType2 == ShapeType.Rect)
        {
          shape2.GetRect(out var rect2);
          rect2.SetCenter(colliderComp.transformComp_.Pos);
          if (rect.Intersect(rect2, includeContact))
          {
            isIntersect = true;
          }
        }
        else if (shapeType2 == ShapeType.Circle)
        {
          shape2.GetCircle(out var circle2);
          circle2.MoveTo(colliderComp.transformComp_.Pos);
          if (Foundation.IsRectIntersectCircle(rect, circle2))
          {
            isIntersect = true;
          }
        }
      }
      else if (shapeType == ShapeType.Circle)
      {
        shape.GetCircle(out var circle);
        circle.MoveTo(pos);
        if (shapeType2 == ShapeType.Circle)
        {
          shape2.GetCircle(out var circle2);
          circle2.MoveTo(colliderComp.transformComp_.Pos);
          if (Foundation.IsCircleIntersectCircle(circle, circle2, includeContact))
          {
            isIntersect = true;
          }
        }
        else if (shapeType2 == ShapeType.Rect)
        {
          shape2.GetRect(out var rect2);
          rect2.SetCenter(colliderComp.transformComp_.Pos);
          if (Foundation.IsRectIntersectCircle(rect2, circle))
          {
            isIntersect = true;
          }
        }
      }
      return isIntersect;
    }

    public ColliderCompDef CompDef
    {
      get => compDef_;
    }

    public Shape Shape
    {
      get => shape_;
      set => shape_ = value;
    }

    public bool IsRigidBody
    {
      get => isRigidBody_;
      set => isRigidBody_ = value;
    }
  }
}