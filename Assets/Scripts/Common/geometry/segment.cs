using System.Collections.Generic;

namespace Common.Geometry
{
  public enum TwoSegmentRelation
  {
    // 没有交点
    NoInterSection = 0,
    // 有接触点
    HasContactPoint = 1,
    // 有交点
    HasIntersection = 2,
    // 部分重合
    Overlap = 3,
    // 部分(A是B的一部分)
    Part = 4,
    // 完全重合(A与B完全重合)
    Coinside = 5,
    // 包含(A包含B)
    Contain
  }

  /// 线段
  public struct Segment
  {
    Position start_;
    Position end_;

    public Segment(Position s, Position e)
    {
      start_ = s;
      end_ = e;
    }

    public readonly Position Start()
    {
      return start_;
    }

    public readonly Position End()
    {
      return end_;
    }

    public void Reset(Position s, Position e)
    {
      start_ = s;
      end_ = e;
    }

    // 法线
    public readonly Vec2 GetNormal()
    {
      return new Vec2(end_.Y() - start_.Y(), start_.X() - end_.X());
    }

    // 是否跟线段相交
    public readonly bool IsIntersectWithSegment(Segment segment, bool includeOnSegment = true)
    {
      return IsIntersectWithSegment(segment.start_, segment.end_, includeOnSegment);
    }

    // 是否跟线段相交
    public readonly bool IsIntersectWithSegment(Position start, Position end, bool includeOnSegment)
    {
      // 线段(start_, end_)
      var se = new Vec2(end_.X() - start_.X(), end_.Y() - start_.Y());
      // 线段(start_, start)
      var sa = new Vec2(start.X() - start_.X(), end.Y() - start_.Y());
      // 线段(start_, end)
      var sb = new Vec2(end.X() - start_.X(), end.Y() - start_.Y());

      var c1 = se.Cross(sa);
      var c2 = se.Cross(sb);

      // 线段(start, end)
      var ab = new Vec2(end.X() - start.X(), end.Y() - start.Y());
      // 线段(start, start_)
      var ass = new Vec2(start_.X() - start.X(), start_.Y() - start.Y());
      // 线段(start, end_)
      var bs = new Vec2(end_.X() - start.X(), end_.Y() - start.Y());

      var c3 = ab.Cross(ass);
      var c4 = ab.Cross(bs);

      // c1*c2 和 c3*c4 都小於0才能判斷相交
      // TODO c1*c2=0 或者 c3*c4=0 屬於相切，不是嚴格意義上的相交
      if (includeOnSegment)
      {
        return c1 * c2 <= 0 && c3 * c4 <= 0;
      }
      return c1 * c2 < 0 && c3 * c4 < 0;
    }

    public readonly bool IsIntersectWithRect(Rect rect, bool includeOnRect = true)
    {
      var lb = rect.LeftBottom();
      var rb = rect.RightBottom();
      Segment segment = new(lb, rb);
      if (IsIntersectWithSegment(segment, includeOnRect))
      {
        return true;
      }
      var rt = rect.RightTop();
      segment.Reset(rb, rt);
      if (IsIntersectWithSegment(segment, includeOnRect))
      {
        return true;
      }
      var lt = rect.LeftTop();
      segment.Reset(rt, lt);
      if (IsIntersectWithSegment(segment, includeOnRect))
      {
        return true;
      }
      segment.Reset(lt, lb);
      if (IsIntersectWithSegment(segment, includeOnRect))
      {
        return true;
      }
      return false;
    }

    public readonly bool IsIntersectWithBox(Box box, bool includeOnBox = true)
    {
      box.GetFourVertices(out var lb, out var rb, out var rt, out var lt);
      Segment segment = new(lb, rb);
      if (IsIntersectWithSegment(segment, includeOnBox))
      {
        return true;
      }
      segment.Reset(rb, rt);
      if (IsIntersectWithSegment(segment, includeOnBox))
      {
        return true;
      }
      segment.Reset(rt, lt);
      if (IsIntersectWithSegment(segment, includeOnBox))
      {
        return true;
      }
      segment.Reset(lt, lb);
      if (IsIntersectWithSegment(segment, includeOnBox))
      {
        return true;
      }
      return false;
    }

    public readonly bool IsIntersectWithCircle(Circle circle, bool includeInsideCircle = true)
    {
      return Foundation.IsSegmentIntersectCircle(start_, end_, circle.Center(), circle.Radius(), includeInsideCircle);
    }

    public readonly (Position, bool) GetIntersectionWithSegment(Segment segment)
    {
      return GetIntersectionWithSegment(segment.start_, segment.end_);
    }

    public readonly (Position, bool) GetIntersectionWithSegment(Position start, Position end)
    {
      var intersection = new Position();
      // 平行没有交点
      // (end.X-start.X)/(end.Y-start.Y) == (line.end.X-line.start.X)/(line.end.Y-line.start.Y)
      if ((end_.X() - start_.X()) * (end.Y() - start.Y()) == (end_.Y() - start_.Y()) * (end.X() - start.X()))
      {
        return (intersection, false);
      }

      int x, y;
      if (end_.X() == start_.X()) /// 自身垂直
      {
        if (end.X() == start.X())
        {
          return (intersection, false);
        }
        // (end.X - start.X) / (end.Y - start.Y) == (x - start.X) / (y - start.Y)
        x = end_.X();
        y = (end.Y() - start.Y()) * (x - start.X()) / (end.X() - start.X()) + start.Y();
        if ((start_.Y() - y) * (end_.Y() - y) > 0)
        {
          return (intersection, false);
        }
      }
      else if (end_.Y() == start_.Y()) /// 自身水平
      {
        if (end.X() == start.X())
        {
          return (intersection, false);
        }
        // (end.X() - start.X()) / (end.Y() - start.Y()) == (x - start.X()) / (y - start.Y())
        y = end_.Y();
        x = (end.X() - start.X()) * (y - start.Y()) / (end.Y() - start.Y()) + start.X();
        if ((start.X() - x) * (end.X() - x) > 0)
        {
          return (intersection, false);
        }
      }
      else if (end.X() == start.X()) // line平行于y轴
      {
        if (end.X() == start.X())
        {
          return (intersection, false);
        }
        // (end_.X() - start_.X()) / (end_.Y() - start_.Y()) == (x - start_.X()) / (y - start_.Y())
        x = end.X();
        y = (x - start_.X()) * (end_.Y() - start_.Y()) / (end_.X() - start_.X()) + start_.Y();
        if ((start.Y() - y) * (end.Y() - y) > 0)
        {
          return (intersection, false);
        }
      }
      else if (end.Y() == start.Y()) // line平行于x轴
      {
        if (end_.Y() == start_.Y())
        {
          return (intersection, false);
        }
        // (end_.X() - start_.X()) / (end_.Y() - start_.Y()) == (x - start_.X()) / (y - start_.Y())
        y = end.Y();
        x = (end_.X() - start_.X()) * (end.Y() - start_.Y()) / (end_.Y() - start_.Y()) + start_.X();
        if ((start.X() - x) * (end.X() - x) > 0)
        {
          return (intersection, false);
        }
      }
      else
      {
        /// 利用斜率求出交点坐标
        /// (end_.X() - start_.X()) / (end_.Y() - start_.Y()) = (x - start_.X()) / (y - start_.Y())
        /// (end.X() - start.X()) / (end.Y() - start.Y()) = (x - start.X()) / (y - start.Y())
        long fm = (long)start_.X() * (end_.Y() - start_.Y()) * (end.X() - start.X())
                    - (long)start.X() * (end.Y() - start.Y()) * (end_.X() - start_.X())
                    + (long)(start.Y() - start_.Y()) * (end_.X() - start_.X()) * (end.X() - start.X());
        long fz = (long)(end_.Y() - start_.Y()) * (end.X() - start.X()) - (long)(end_.X() - start_.X()) * (end.Y() - start.Y());
        x = (int)(fm / fz);
        y = start_.Y() + (end_.Y() - start_.Y()) * (x - start_.X()) / (end_.X() - start_.X());
      }

      // 再判斷交點是否在兩條綫段上（因爲上面求出的交點有可能只是在綫段延長綫上）
      // 交點與綫段兩端的差值符號相反
      long a, b;
      if (start_.X() != end_.X())
      {
        a = (long)(start_.X() - x) * (end_.X() - x);
      }
      else
      {
        a = (long)(start_.Y() - y) * (end_.Y() - y);
      }
      if (start.X() != end.X())
      {
        b = (long)(start.X() - x) * (end.X() - x);
      }
      else
      {
        b = (long)(start.Y() - y) * (end.Y() - y);
      }

      if (a > 0 || b > 0)
      {
        return (intersection, false);
      }

      intersection.Set(x, y);

      return (intersection, true);
    }

    public readonly bool GetIntersectionWithRect(Rect rect, out List<Position> intersectionList)
    {
      var lb = rect.LeftBottom();
      var rb = rect.RightBottom();
      var rt = rect.RightTop();
      var lt = rect.LeftTop();

      intersectionList = null;
      var (intersection1, has1) = Foundation.GetIntersectionOfTwoSegment(start_, end_, lb, rb);
      if (has1)
      {
        intersectionList = new()
                {
                    intersection1
                };
      }
      var (intersection2, has2) = Foundation.GetIntersectionOfTwoSegment(start_, end_, rb, rt);
      if (has2)
      {
        intersectionList ??= new();
        intersectionList.Add(intersection2);
      }
      var (intersection3, has3) = Foundation.GetIntersectionOfTwoSegment(start_, end_, rt, lt);
      if (has3)
      {
        intersectionList ??= new();
        intersectionList.Add(intersection3);
      }
      var (intersection4, has4) = Foundation.GetIntersectionOfTwoSegment(start_, end_, lt, lb);
      if (has4)
      {
        intersectionList ??= new();
        intersectionList.Add(intersection4);
      }
      if (intersectionList == null) return false;
      return true;
    }

    public readonly bool GetIntersectionWithBox(Box box, out List<Position> intersectionList)
    {
      intersectionList = null;
      box.GetFourVertices(out var lb, out var rb, out var rt, out var lt);
      var (intersection1, has1) = Foundation.GetIntersectionOfTwoSegment(start_, end_, lb, rb);
      if (has1)
      {
        intersectionList = new()
        {
          intersection1,
        };
      }
      var (intersection2, has2) = Foundation.GetIntersectionOfTwoSegment(start_, end_, rb, rt);
      if (has2)
      {
        intersectionList ??= new();
        intersectionList.Add(intersection2);
      }
      var (intersection3, has3) = Foundation.GetIntersectionOfTwoSegment(start_, end_, rt, lt);
      if (has3)
      {
        intersectionList ??= new();
        intersectionList.Add(intersection3);
      }
      var (intersection4, has4) = Foundation.GetIntersectionOfTwoSegment(start_, end_, lt, lb);
      if (has4)
      {
        intersectionList ??= new();
        intersectionList.Add(intersection4);
      }
      if (intersectionList == null) return false;
      return true;
    }

    public readonly (IntersectInfo, bool) GetIntersectionWithCircle(Circle circle)
    {
      return Foundation.GetIntersectionOfSegmentWithCircle(start_, end_, circle.Center(), circle.Radius());
    }
  }
}