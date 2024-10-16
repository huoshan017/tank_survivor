using System;
using System.Collections.Generic;
using Common;
using Common.Geometry;
using Logic.Base;
using Logic.Interface;
using Logic.Component;

namespace Logic.System
{
  public class CollisionSystem : SystemBase
  {
    struct AbsXY : IComparable<AbsXY>
    {
      public uint entityInstId;
      public int x, y;
      public Position contactPoint;

      public readonly int CompareTo(AbsXY other)
      {
        // 比较x或者y的绝对值
        var x1 = Math.Abs(x);
        var x2 = Math.Abs(other.x);
        if (x1 < x2) return -1;
        else if (x1 > x2) return 1;
        else return 0;
      }
    }

    MinBinaryHeap<AbsXY> entityCollisionDistanceHeap_;

    public CollisionSystem(IContext context) : base(context)
    {
      entityCollisionDistanceHeap_ = new();
    }

    public override void Init(CompTypeConfig config)
    {
      base.Init(config);
    }

    public override void Uninit()
    {
      entityCollisionDistanceHeap_.Clear();
      entityCollisionDistanceHeap_ = null;
      base.Uninit();
    }

    public override void DoUpdate(uint frameMs)
    {
    }

    // 一个实体与一堆实体的碰撞结果
    internal (Position, CollisionResult) GetEntityCollisionsResult(IEntity entity, Position nextPos, IEnumerable<IEntity> collisionEntityList)
    {
      var entityPos = entity.GetComponent<TransformComponent>().Pos;
      var deltaPos = nextPos - entityPos;
      var dx = deltaPos.X();
      var dy = deltaPos.Y();

      if (dx == 0 && dy == 0)
      {
        return (entityPos, CollisionResult.MoveNotAffected);
      }

      entityCollisionDistanceHeap_.Clear();

      // 遍历碰撞实体列表，并把实体插入到按照实际移动距离建立的小顶堆中，
      var colliderComp = entity.GetComponent<ColliderComponent>();
      foreach (var ce in collisionEntityList)
      {
        var colliderComp2 = ce.GetComponent<ColliderComponent>();
        var (x, y, isContact) = RealDistanceOfColliderMove2Collider(colliderComp, colliderComp2, dx, dy, out var contactPoint);
        //if (isContact)
        {
          entityCollisionDistanceHeap_.Set(new AbsXY() { entityInstId = ce.InstId(), x = x, y = y, contactPoint = contactPoint });
        }
      }

      // 处理小顶堆，从距离最近的开始
      CollisionResult result = CollisionResult.MoveNotAffected;
      while (entityCollisionDistanceHeap_.Get(out var kv))
      {
        var entity2 = context_.GetEntity(kv.entityInstId);
        if (entity2 == null) continue;
        result = TwoEntityContactResult(entity, entityPos, kv.x, kv.y, kv.contactPoint, entity2);
        if (result == CollisionResult.Blocked)
        {
          entityPos.Translate(kv.x, kv.y);
          nextPos = entityPos;
          break;
        }
        if (!entity.IsActive())
        {
          break;
        }
      }

      return (nextPos, result);
    }

    CollisionResult TwoEntityContactResult(IEntity entity, Position entityPos, int nextDx, int nextDy, Position contactPoint, IEntity entity2)
    {
      var colliderComp = entity.GetComponent<ColliderComponent>();
      var colliderComp2 = entity2.GetComponent<ColliderComponent>();

      CollisionResult result = CollisionResult.MoveNotAffected;

      // 都是刚体都有阻挡，移动kv.x kv.y距离，不再处理后面的实体
      // 互相阻挡的实体默认结果是停止移动，距离更远的实体就不用处理了
      if (colliderComp.IsRigidBody && colliderComp2.IsRigidBody)
      {
        //entityPos.Translate(nextDx, nextDy);
        result = CollisionResult.Blocked;
      }

      SystemPolicy.DoCollision(context_.SystemList(), entity, entity2, contactPoint);

      return result;
    }

    static (int, int, bool) RealDistanceOfColliderMove2Collider(in ColliderComponent collider1, in ColliderComponent collider2, int dx, int dy, out Position contactPoint)
    {
      bool isContact = false;
      contactPoint = new();
      var shapeType = collider1.CompDef.Shape.GetShapeType();
      var shapeType2 = collider2.CompDef.Shape.GetShapeType();
      if (shapeType == ShapeType.Rect)
      {
        collider1.GetRect(out var rect);
        if (shapeType2 == ShapeType.Rect)
        {
          collider2.GetRect(out var rect2);
          (dx, dy, isContact) = RealDistanceOfRectMoveToRect(rect, rect2, dx, dy, out contactPoint);
        }
        else if (shapeType2 == ShapeType.Circle)
        {
          collider2.GetCircle(out var circle2);
          (dx, dy, isContact) = RealDistanceOfRectMoveToCircle(rect, circle2, dx, dy, out contactPoint);
        }
      }
      else if (shapeType == ShapeType.Circle)
      {
        collider1.GetCircle(out var circle);
        if (shapeType2 == ShapeType.Rect)
        {
          collider2.GetRect(out var rect2);
          (dx, dy, isContact) = RealDistanceOfCircleMoveToRect(circle, rect2, dx, dy, out contactPoint);
        }
      }
      return (dx, dy, isContact);
    }

    // 矩形朝另一个矩形移动时遇到阻挡实际移动的距离(x, y方向)
    static (int, int, bool) RealDistanceOfRectMoveToRect(in Rect rect, in Rect rect2, int dx, int dy, out Position contactPoint)
    {
      // 阻挡时x和y方向上的最大移动距离
      int ddx = 0, ddy = 0;

      /// 两个矩形碰撞，只可能是左碰右，右碰左，上碰下，下碰上这四种情况一种或者两种(一个左右和一个上下)   
      // 右碰左             
      if (dx > 0)
      {
        ddx = rect2.Left() - rect.Right();
      }
      // 左碰右
      else if (dx < 0)
      {
        ddx = rect2.Right() - rect.Left();
      }

      // 下碰上
      if (dy > 0)
      {
        ddy = rect2.Bottom() - rect.Top();
      }
      // 上碰下
      else if (dy < 0)
      {
        ddy = rect2.Top() - rect.Bottom();
      }

      /// 讨论三种移动情况
      // 1. 竖直移动时, Math.Abs(ddy)不能大于最大距离Math.Abs(dy)
      if (dx == 0)
      {
        if (Math.Abs(ddy) > Math.Abs(dy))
        {
          ddy = dy;
        }
      }
      // 2. 水平移动时，Math.Abs(ddx)不能大于最大距离Math.Abs(dx);
      else if (dy == 0)
      {
        if (Math.Abs(ddx) > Math.Abs(dx))
        {
          ddx = dx;
        }
      }
      // 3. 斜线移动时，要按Math.Abs(dx)/Math.Abs(dy)的比例算出最小的Math.Abs(ddx)或Math.Abs(ddy)
      else
      {
        // 新的位置与没有阻挡时的位置坐标是成比例关系的，且绝对值不大于有阻挡的坐标
        // Math.Abs(dx) / Math.Abs(dy) == Math.Abs(ddx) / Math.Abs(ddy);
        if (Math.Abs(dx) * Math.Abs(ddy) > Math.Abs(dy) * Math.Abs(ddx))
        {
          ddy = dy * ddx / dx;
        }
        else
        {
          ddx = dx * ddy / dy;
        }
      }

      bool isContact = false;
      contactPoint = new();
      {
        // 判断是否接触和计算接触点
        if (ddy > 0)
        {
          if (rect.Top() + ddy >= rect2.Bottom()
              && rect.Right() + ddx > rect2.Left()
              && rect.Left() + ddx < rect2.Right())
          {
            isContact = true;
            // 接触线的中点作为接触点
            contactPoint.Set((Math.Max(rect.Left()+ddx, rect2.Left()) + Math.Min(rect.Right()+ddx, rect2.Right())) / 2, rect.Top()+ddy);
          }
        }
        else if (ddy < 0)
        {
          if (rect.Bottom() + ddy <= rect2.Top()
              && rect.Right() + ddx > rect2.Left()
              && rect.Left() + ddx < rect2.Right())
          {
            isContact = true;
            contactPoint.Set((Math.Max(rect.Left()+ddx, rect2.Left()) + Math.Min(rect.Right()+ddx, rect2.Right())) / 2, rect.Bottom()+ddy);
          }
        }
        if (ddx > 0)
        {
          if (rect.Right() + ddx >= rect2.Left()
              && rect.Top() + ddy > rect2.Bottom()
              && rect.Bottom() + ddy < rect2.Top())
          {
            isContact = true;
            contactPoint.Set(rect.Right()+ddx, (Math.Min(rect.Top()+ddy, rect2.Top()) + Math.Max(rect.Bottom()+ddy, rect2.Bottom())) / 2);
          }
        }
        else if (ddx < 0)
        {
          if (rect.Left() + ddx <= rect2.Right()
              && rect.Top() + ddy > rect2.Bottom()
              && rect.Bottom() + ddy < rect2.Top())
          {
            isContact = true;
            contactPoint.Set(rect.Left()+ddx, (Math.Min(rect.Top()+ddy, rect2.Top()) + Math.Max(rect.Bottom()+ddy, rect2.Bottom())) / 2);
          }
        }
      }

      return (ddx, ddy, isContact);
    }

    // 矩形朝圆形移动碰到阻挡时实际移动距离
    static (int, int, bool) RealDistanceOfRectMoveToCircle(Rect rect, Circle circle, int dx, int dy, out Position contactPoint)
    {
      // 矩形与圆碰撞有四种相切情况，分别是：
      // 1.矩形底部与圆顶部相切
      // 2.矩形顶部与圆底部相切
      // 3.矩形左边与圆右边相切
      // 4.矩形右边与圆左边相切

      // 还有四种矩形顶点与圆接触的情况
      // 1.矩形左下顶点与圆右上部分(0到90度的范围)
      // 2.矩形右下顶点与圆左上部分(90到180度的范围)
      // 3.矩形右上顶点与圆左下部分(180到270度的范围)
      // 4.矩形左上顶点与圆右下部分(270到360度的范围)

      // 所以先处理矩形与圆包围盒的碰撞
      var (ddx, ddy, isContact) = RealDistanceOfRectMoveToRect(rect, circle.BoundingRect(), dx, dy, out contactPoint);

      // 所以继续处理
      var rectLeft = rect.Left();
      var rectRight = rect.Right();
      var rectTop = rect.Top();
      var rectBottom = rect.Bottom();
      var circleCenter = circle.Center();
      var circleRadius = circle.Radius();
      var circleLeft = circleCenter.X() - circleRadius;
      var circleRight = circleCenter.X() + circleRadius;
      var circleBottom = circleCenter.Y() - circleRadius;
      var circleTop = circleCenter.Y() + circleRadius;

      (Position, bool) getContactPoint(Position pos, Position movedPos, Circle circle)
      {
        Position contactPos = new();
        var distanceSquare = Position.DistanceSquare(movedPos, circleCenter);
        var circleRadiusSquare = (ulong)circleRadius * (ulong)circleRadius;
        // 在圆内，则算出连线的交点，即为接触点
        if (distanceSquare < circleRadiusSquare)
        {
          if (!Foundation.GetFirstIntersectionOfDirectionalSegmentAndCircle(pos, movedPos, circle, ref contactPos))
          {
            throw new Exception("get first intersection of circle with segment failed!");
          }
          isContact = true;
        }
        // 正好在圆上
        else if (distanceSquare == circleRadiusSquare)
        {
          contactPos = pos;
          isContact = true;
        }
        return (contactPos, isContact);
      }

      // 是否相切
      var isTangent = false;
      // 将接触的点
      var toContactPoint = new Position();

      // 情况1 矩形左边与包围盒右边碰撞
      if (rectLeft + ddx == circleRight)
      {
        // 矩形左边与圆右侧相切
        if (rectTop + ddy >= circleCenter.Y() && rectBottom + ddy <= circleCenter.Y())
        {
          isTangent = true;
          contactPoint.Set(circleRight, circleCenter.Y());
          isContact = true;
        }
        // 矩形顶部Y坐标小于圆心Y坐标
        else if (rectTop + ddy < circleCenter.Y())
        {
          // 比较矩形左上顶点移动(dx,dy)与圆心的距离和圆半径的大小
          toContactPoint = rect.LeftTop();
          var toContactPoint2 = new Position(toContactPoint.X() + dx, toContactPoint.Y() + dy);
          (contactPoint, isContact) = getContactPoint(toContactPoint, toContactPoint2, circle);
        }
        // 矩形底部Y坐标大于圆心Y坐标
        else // rectBottom+ddy > circleCenter.Y()
        {
          // 比较矩形左下顶点移动(dx,dy)后与圆心的距离和圆半径的大小
          toContactPoint = rect.LeftBottom();
          var toContactPoint2 = new Position(toContactPoint.X() + dx, toContactPoint.Y() + dy);
          (contactPoint, isContact) = getContactPoint(toContactPoint, toContactPoint2, circle);
        }
      }
      // 情况2 矩形右边与包围盒左边碰撞
      else if (rectRight + ddx == circleLeft)
      {
        // 矩形右边与圆左侧相切
        if (rectTop + ddy >= circleCenter.Y() && rectBottom + ddy <= circleCenter.Y())
        {
          isTangent = true;
          contactPoint.Set(circleLeft, circleCenter.Y());
          isContact = true;
        }
        // 矩形顶部Y坐标小于圆心Y坐标
        else if (rectTop + ddy < circleCenter.Y())
        {
          // 比较矩形右上顶点移动(dx,dy)与圆心的距离和圆半径的大小
          toContactPoint = rect.RightTop();
          var toContactPoint2 = new Position(toContactPoint.X() + dx, toContactPoint.Y() + dy);
          (contactPoint, isContact) = getContactPoint(toContactPoint, toContactPoint2, circle);
        }
        else // 矩形底边Y坐标大于圆心Y坐标
        {
          // 比较矩形右下顶点移动(dx,dy)后与圆心的距离和圆半径的大小
          toContactPoint = rect.RightBottom();
          var toContactPoint2 = new Position(toContactPoint.X() + dx, toContactPoint.Y() + dy);
          (contactPoint, isContact) = getContactPoint(toContactPoint, toContactPoint2, circle);
        }
      }
      // 情况3 矩形底边和包围盒顶边碰撞
      else if (rectBottom + ddy == circleTop)
      {
        // 矩形底边与圆顶部相切(圆心X坐标在矩形左右X坐标之间)
        if (rectLeft + ddx <= circleCenter.X() && rectRight + ddx >= circleCenter.X())
        {
          contactPoint.Set(circleCenter.X(), circleTop);
          isContact = true;
        }
        // 矩形左侧X坐标大于圆心X坐标
        else if (rectLeft + ddx > circleCenter.X())
        {
          // 矩形左下顶点
          var leftBottom = rect.LeftBottom();
          var movedLeftBottom = new Position(leftBottom.X() + dx, leftBottom.Y() + dy);
          (contactPoint, isContact) = getContactPoint(leftBottom, movedLeftBottom, circle);
        }
        // 矩形右侧X坐标小于圆心坐标
        else
        {
          // 矩形右下顶点
          var rightBottom = rect.RightBottom();
          var movedRightBottom = new Position(rightBottom.X() + dx, rightBottom.Y() + dy);
          (contactPoint, isContact) = getContactPoint(rightBottom, movedRightBottom, circle);
        }
      }
      // 情况4 矩形顶边和包围盒底边碰撞
      else if (rectTop + ddy == circleBottom)
      {
        // 矩形顶边与圆底部相切(圆心X坐标在矩形左右两条边的X坐标之间)
        if (circleCenter.X() >= rectLeft + ddx && circleCenter.X() <= rectRight + ddx)
        {
          contactPoint.Set(circleCenter.X(), circleBottom);
          isContact = true;
        }
        // 矩形右侧X坐标小于圆心X坐标
        else if (circleCenter.X() > rectRight + ddx)
        {
          // 矩形右上顶点
          var rightTop = rect.RightTop();
          var rightTop2 = new Position(rightTop.X() + dx, rightTop.Y() + dy);
          (contactPoint, isContact) = getContactPoint(rightTop, rightTop2, circle);
        }
        // 矩形左侧X坐标大于圆心坐标
        else // circleCenter.X() < rectLeft+ddx
        {
          // 矩形左上顶点
          var leftTop = rect.LeftTop();
          var leftTop2 = new Position(leftTop.X() + dx, leftTop.Y() + dy);
          (contactPoint, isContact) = getContactPoint(leftTop, leftTop2, circle);
        }
      }
      else
      {
        throw new Exception("Invalid rect(" + rectLeft + ", " + rectBottom + ", " + rect.Width() + ", " + rect.Height() + ") move to circle with dx(" + dx + ") dy(" + dy + ")");
      }

      // 相切的情况不考虑，只有不相切时才计算最终移动的距离
      if (!isTangent && isContact)
      {
        ddx = contactPoint.X() - toContactPoint.X();
        ddy = contactPoint.Y() - toContactPoint.Y();
      }

      return (ddx, ddy, isContact);
    }

    // 圆形朝矩形移动碰到阻挡时实际移动距离
    static (int, int, bool) RealDistanceOfCircleMoveToRect(Circle circle, Rect rect, int dx, int dy, out Position contactPoint)
    {
      // 所以先处理圆包围盒与矩形的碰撞
      var (ddx, ddy, isContact) = RealDistanceOfRectMoveToRect(circle.BoundingRect(), rect, dx, dy, out contactPoint);

      var rectLeft = rect.Left();
      var rectRight = rect.Right();
      var rectTop = rect.Top();
      var rectBottom = rect.Bottom();
      var circleCenter = circle.Center();
      var circleRadius = circle.Radius();
      var circleLeft = circleCenter.X() - circleRadius;
      var circleRight = circleCenter.X() + circleRadius;
      var circleBottom = circleCenter.Y() - circleRadius;
      var circleTop = circleCenter.Y() + circleRadius;

      (int, int, bool) getContactDistance(Circle circle, Position movedCircleCenter, Position toContactPoint)
      {
        int mx = 0, my = 0;
        var distanceSquare = Position.DistanceSquare(movedCircleCenter, toContactPoint);
        var circleRadiusSquare = (ulong)circleRadius * (ulong)circleRadius;
        // 在圆内，则算出连线的交点，即为接触点
        if (distanceSquare < circleRadiusSquare)
        {
          var newPos = new Position(toContactPoint.X() - dx, toContactPoint.Y() - dy);
          var intersection = new Position();
          if (!Foundation.GetFirstIntersectionOfDirectionalSegmentAndCircle(toContactPoint, newPos, circle, ref intersection))
          {
            DebugLog.Error("get first intersection of circle with segment failed!");
            return (mx, my, false);
          }
          mx = toContactPoint.X() - intersection.X();
          my = toContactPoint.Y() - intersection.Y();
          isContact = true;
        }
        // 正好在圆上
        else if (distanceSquare == circleRadiusSquare)
        {
          mx = movedCircleCenter.X() - circle.Center().X();
          my = movedCircleCenter.Y() - circle.Center().Y();
          isContact = true;
        }
        return (mx, my, isContact);
      }

      bool isTangent = false;

      // 情况1 包围盒右边与矩形左边碰撞
      if (circleRight + ddx == rectLeft)
      {
        // 圆右侧与矩形左边相切(圆心Y坐标在矩形上下两条边的Y坐标之间)
        if (rectTop >= circleCenter.Y() + ddy && rectBottom <= circleCenter.Y() + ddy)
        {
          isTangent = true;
          contactPoint.Set(circleRight + ddx, circleCenter.Y() + ddy);
          isContact = true;
        }
        // 圆心Y坐标大于矩形顶部Y坐标
        else if (rectTop < circleCenter.Y() + ddy)
        {
          // 比较圆形移动(dx,dy)与圆心的距离和圆半径的大小
          contactPoint = rect.LeftTop();
        }
        // 矩形底部Y坐标大于圆心Y坐标
        else // rectBottom+ddy > circleCenter.Y()
        {
          // 比较矩形左下顶点移动(dx,dy)后与圆心的距离和圆半径的大小
          contactPoint = rect.LeftBottom();
        }
      }
      // 情况2 包围盒左边与矩形右边碰撞
      else if (circleLeft + ddx == rectRight)
      {
        // 圆左侧与矩形右边相切(圆心y坐标在矩形上下两条边的Y坐标之间)
        if (rectTop >= circleCenter.Y() + ddy && rectBottom <= circleCenter.Y() + ddy)
        {
          isTangent = true;
          contactPoint.Set(circleLeft + ddx, circleCenter.Y() + ddy);
          isContact = true;
        }
        // 圆心Y坐标大于矩形顶部Y坐标
        else if (rectTop < circleCenter.Y() + ddy)
        {
          // 比较矩形右上顶点移动(dx,dy)与圆心的距离和圆半径的大小
          contactPoint = rect.RightTop();
        }
        else // 圆心Y坐标小于矩形底边Y坐标
        {
          // 比较矩形右下顶点移动(dx,dy)后与圆心的距离和圆半径的大小
          contactPoint = rect.RightBottom();
        }
      }
      // 情况3 包围盒顶边和矩形底边碰撞
      else if (circleTop + ddy == rectBottom)
      {
        // 圆顶部与矩形底边相切(圆心X坐标在矩形左右X坐标之间)
        if (rectLeft <= circleCenter.X() + ddx && rectRight >= circleCenter.X() + ddx)
        {
          isTangent = true;
          contactPoint.Set(circleCenter.X() + ddx, circleTop + ddy);
          isContact = true;
        }
        // 圆心X坐标小于矩形左边X坐标
        else if (rectLeft > circleCenter.X() + ddx)
        {
          // 矩形左下顶点
          contactPoint = rect.LeftBottom();
        }
        // 圆心坐标大于矩形右边X坐标
        else
        {
          // 矩形右下顶点
          contactPoint = rect.RightBottom();
        }
      }
      // 情况4 包围盒底边和矩形顶边碰撞
      else if (circleBottom + ddy == rectTop)
      {
        // 圆底部与矩形顶边相切(圆心X坐标在矩形左右两条边的X坐标之间)
        if (circleCenter.X() + ddx >= rectLeft && circleCenter.X() + ddx <= rectRight)
        {
          isTangent = true;
          contactPoint.Set(circleCenter.X() + ddx, circleBottom + ddy);
          isContact = true;
        }
        // 矩形右侧X坐标小于圆心X坐标
        else if (circleCenter.X() + ddx > rectRight)
        {
          // 矩形右上顶点
          contactPoint = rect.RightTop();
        }
        // 矩形左侧X坐标大于圆心坐标
        else // circleCenter.X() < rectLeft+ddx
        {
          // 矩形左上顶点
          contactPoint = rect.LeftTop();
        }
      }
      else
      {
        throw new Exception("Invalid rect(" + rectLeft + ", " + rectBottom + ", " + rect.Width() + ", " + rect.Height() + ") move to circle with dx(" + dx + ") dy(" + dy + ")");
      }

      if (!isTangent)
      {
        var movedCircleCenter = new Position(circleCenter.X() + dx, circleCenter.Y() + dy);
        (ddx, ddy, isContact) = getContactDistance(circle, movedCircleCenter, contactPoint);
      }

      return (ddx, ddy, isContact);
    }
  }
}