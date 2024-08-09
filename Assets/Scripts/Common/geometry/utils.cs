using System;
using System.Collections.Generic;

namespace Common.Geometry
{
  public struct IntersectInfo
  {
    //public Position[] IntersectionList;
    public List<Position> IntersectionList;
  }

  public class Foundation
  {
    static readonly Position[] posListCache_ = new Position[4];

    // 线段的法向量（线段的向量方向逆时针旋转90度）
    public static Vec2 GetLineNormal(Position lineStart, Position lineEnd)
    {
      int dx = lineEnd.X() - lineStart.X();
      int dy = lineEnd.Y() - lineEnd.Y();

      int nx = dy;
      int ny = -dx;
      return new Vec2(nx, ny);
    }

    // 点到直线的距离
    public static int GetDistanceFromPointToLine(Position pos, Position linePos1, Position linePos2)
    {
      var (distance, _) = GetDistanceFromPointToLineOrSegment(pos, linePos1, linePos2, true);
      return distance;
    }

    // 点到线段的距离
    public static (int, bool) GetDistanceFromPointToSegment(Position pos, Position segmentStart, Position segmentEnd)
    {
      return GetDistanceFromPointToLineOrSegment(pos, segmentStart, segmentEnd, false);
    }

    // 点到直线或线段的距离
    public static (int, bool) GetDistanceFromPointToLineOrSegment(Position pos, Position lineStart, Position lineEnd, bool isLineNotSegment = true)
    {
      // AB
      var AB = lineEnd - lineStart;
      // AP
      var AP = pos - lineStart;
      // 如果是线段
      if (!isLineNotSegment)
      {
        // 点积用来判断夹角
        var dot = AB.Dot(AP);
        // 夹角大于90度
        if (dot < 0)
        {
          return (0, false);
        }
        if (dot == 0)
        {
          return ((int)Position.Distance(lineStart, pos), true);
        }

        // BA
        var BA = lineStart - lineEnd;
        // BP
        var BP = pos - lineEnd;
        // 点积用来判断夹角
        dot = BA.Dot(BP);
        // 夹角大于90度
        if (dot < 0)
        {
          return (0, false);
        }
        if (dot == 0)
        {
          return ((int)Position.Distance(lineEnd, pos), true);
        }
      }

      // 两倍的三角形ABP面积，平行四边形的面积
      var twoAreaABP = Math.Abs(AB.Cross(AP));
      // AB的长度
      var lengthAB = Position.Distance(lineStart, lineEnd);
      // 求高度即点P到AB的距离
      return ((int)(twoAreaABP / lengthAB), true);
    }

    // 点在直线上的投影
    public static Position GetPointProjectionOnLine(Position pos, Position linePos1, Position linePos2)
    {
      var (projection, _) = GetPointProjectionOnLineOrSegment(pos, linePos1, linePos2, true);
      return projection;
    }

    // 点在线段上的投影
    public static (Position, bool) GetPointProjectionOnSegment(Position pos, Position lineStart, Position lineEnd)
    {
      return GetPointProjectionOnLineOrSegment(pos, lineStart, lineEnd, false);
    }

    // 点在直线或线段上的投影
    public static (Position, bool) GetPointProjectionOnLineOrSegment(Position pos, Position lineStart, Position lineEnd, bool isLineNotSegment = true)
    {
      // AB
      var AB = lineEnd - lineStart;
      // AP
      var AP = pos - lineStart;

      // 如果是线段
      if (!isLineNotSegment)
      {
        // 角BAP大于90度，则投影在线段外
        if (AB.Dot(AP) < 0)
        {
          return (new(), false);
        }
        // BA
        var BA = lineStart - lineEnd;
        // BP
        var BP = pos - lineEnd;
        // 角ABP大于90度，则投影在线段外
        if (BA.Dot(BP) < 0)
        {
          return (new(), false);
        }
      }

      // 求AP在AB上的投影  Dot(AB,AP) = |AB||AP|*cos(BAP)
      //var lengthAB = AB.Length();
      // 投影的长度是有符号的
      //var projectionAP2AB = (int)(AB.Dot(AP)/lengthAB);
      // 投影坐标是线段开始点加上投影的向量(投影的长度*AB方向的单位向量) (AB/lengthAB)表示AB方向的单位向量
      //var projectionP = lineStart + projectionAP2AB*AB/(int)lengthAB;
      var projectionP = lineStart + AB * (int)(AB.Dot(AP) / (long)AB.LengthSquare());

      return (projectionP, true);
    }

    // 点到直线的投影和距离
    public static (Position, int) GetProjectionAndDistanceFromPointToLine(Position pos, Position linePos1, Position linePos2)
    {
      // AB
      var AB = linePos2 - linePos1;
      // AP
      var AP = pos - linePos1;
      // AB的长度
      var lengthAB = AB.Length();
      // 求AP在AB上的投影  Dot(AB,AP) = |AB||AP|*cos(BAP) 投影的长度是带有符号的
      var projectionAP2AB = (int)(AB.Dot(AP) / lengthAB);
      // 投影坐标是线段开始点加上投影的向量(投影的长度*AB方向的单位向量)
      var projectionP = linePos1 + AB * projectionAP2AB / (int)lengthAB;
      // 两倍的三角形ABP面积，平行四边形的面积
      var twoAreaABP = Math.Abs(AB.Cross(AP));
      // 求高度即点P到AB的距离
      return (projectionP, (int)(twoAreaABP / lengthAB));
    }

    // 点是否在矩形内
    public static bool IsPointInRect(Position point, Rect rect)
    {
      return point.X() > rect.Left() && point.X() < rect.Right() && point.Y() > rect.Bottom() && point.Y() < rect.Top();
    }

    // 点是否在圆内
    public static bool IsPointInCircle(Position point, Circle circle)
    {
      var center = circle.Center();
      var radius = circle.Radius();
      var ds = Position.DistanceSquare(center, point);
      return ds < (ulong)radius * (ulong)radius;
    }

    // 点是否在扇形内
    public static bool IsPointInSector(Position point, Sector sector)
    {
      var center = sector.Center();
      var radius = sector.Radius();
      if (Position.DistanceSquare(center, point) >= (ulong)radius * (ulong)radius)
      {
        return false;
      }
      var posA = center;
      posA.Translate(radius, 0);
      posA.Rotate(center.X(), center.Y(), sector.RotationBegin());
      var posB = center;
      posB.Translate(radius, 0);
      posB.Rotate(center.X(), center.Y(), sector.RotationEnd());
      var AC = center - posA;
      var BC = center - posB;
      return IsPointInSector(point, center, AC, BC);
    }

    static bool IsPointInSector(Position point, Position center, Vec2 AC, Vec2 BC)
    {
      var PC = center - point;
      var crossACPC = AC.Cross(PC);
      var crossPCBC = PC.Cross(BC);
      // 叉积同号
      return crossACPC * crossPCBC > 0;
    }

    // 直线与直线是否相交
    public static bool IsLineIntersectLine(Position line1PosA, Position line1PosB, Position line2PosC, Position line2PosD)
    {
      // AB
      var AB = line1PosB - line1PosA;
      // CD
      var CD = line2PosD - line2PosC;
      // 用叉积判断，四个点构成的平行四边形面积不为0则两条执行不平行且不重合
      return AB.Cross(CD) != 0;
    }

    // 直线和线段是否有交点
    public static bool IsLineIntersectSegment(Position linePos1, Position linePos2, Position segmentStart, Position segmentEnd)
    {
      // 直线的方向向量(pos1->pos2)
      var AB = linePos2 - linePos1;
      // 点linePos1到线段端点segmentStart的向量
      var AC = segmentStart - linePos1;
      // 点linePos1到线段端点segmentEnd的向量
      var AD = segmentEnd - linePos1;
      // 判断线段的两个端点是否在直线的一侧，就是判断AB与AC的叉积是否跟AB与AD的叉积符号相同
      var c1 = AB.Cross(AC);
      var c2 = AB.Cross(AD);
      return c1 * c2 > 0;
    }

    // 直线与圆是否有相交
    public static bool IsLineIntersectCircle(Position linePos1, Position linePos2, Position circleCenter, int circleRadius, bool includeTangent = true)
    {
      var d = GetDistanceFromPointToLine(circleCenter, linePos1, linePos2);
      if (d > circleRadius)
      {
        return false;
      }
      if (d == circleRadius && !includeTangent)
      {
        return false;
      }
      return true;
    }

    // 线段与线段是否相交
    public static bool IsSegmentIntersectSegment(Position segment1Start, Position segment1End, Position segment2Start, Position segment2End)
    {
      // 1. 快速排斥(如果两根线段构成的两个矩形不相交，则两线段必定不相交)
      if (Math.Max(segment1Start.X(), segment1End.X()) < Math.Min(segment2Start.X(), segment2End.X()) ||
          Math.Max(segment2Start.X(), segment2End.X()) < Math.Min(segment1Start.X(), segment1End.X()) ||
          Math.Max(segment1Start.Y(), segment1End.Y()) < Math.Min(segment2Start.Y(), segment2End.Y()) ||
          Math.Max(segment2Start.Y(), segment2End.Y()) < Math.Min(segment1Start.Y(), segment1End.Y()))
      {
        return false;
      }

      // 2. 跨立实验
      var AB = segment1End - segment1Start;
      var AC = segment2Start - segment1Start;
      var AD = segment2End - segment1Start;
      var crossABAC = AB.Cross(AC);
      var crossABAD = AB.Cross(AD);
      if (crossABAC * crossABAD > 0)
      {
        return false;
      }

      var CD = segment2End - segment2Start;
      var CA = segment1Start - segment2Start;
      var CB = segment1End - segment2Start;
      var crossCDCA = CD.Cross(CA);
      var crossCDCB = CD.Cross(CB);
      if (crossCDCA * crossCDCB > 0)
      {
        return false;
      }

      return true;
    }

    // 线段与圆是否相交
    public static bool IsSegmentIntersectCircle(Position segmentStart, Position segmentEnd, Position circleCenter, int circleRadius, bool includeInsideCircle = true)
    {
      // 圆心与线段所在直线的距离
      var d = GetDistanceFromPointToLine(circleCenter, segmentStart, segmentEnd);
      if (d > circleRadius)
      {
        return false;
      }
      /// 考虑线段与圆的几种位置关系
      var distanceSquare1 = Position.DistanceSquare(circleCenter, segmentStart);
      var distanceSquare2 = Position.DistanceSquare(circleCenter, segmentEnd);
      ulong radiusSquare = (ulong)circleRadius * (ulong)circleRadius;

      bool intersect = false;
      // 1. 两个端点都在圆内或圆上，说明整条线段都在圆内，但是如果参数为不包括圆内的线段，则不相交
      if (distanceSquare1 <= radiusSquare && distanceSquare2 <= radiusSquare)
      {
        if (includeInsideCircle)
        {
          intersect = true;
        }
      }
      // 2. 两个端点都在圆外，要么整条线段在圆外，要么中间一部分线段在圆内，其余在圆外
      else if (distanceSquare1 > radiusSquare && distanceSquare2 > radiusSquare)
      {
        // 向量AB与AC
        var AB = segmentEnd - segmentStart;
        var AC = circleCenter - segmentStart;
        // 向量BA与BC
        var BA = segmentStart - segmentEnd;
        var BC = circleCenter - segmentEnd;
        // 两个夹角小于等于90度
        if (AB.Dot(AC) >= 0 && BA.Dot(BC) >= 0)
        {
          intersect = true;
        }
      }
      // 4. 一个端点(A)在圆上
      else if (distanceSquare1 == radiusSquare)
      {
        // 向量AB与AC
        var AB = segmentEnd - segmentStart;
        var AC = circleCenter - segmentStart;
        // 夹角小于等于90度
        if (AB.Dot(AC) >= 0)
        {
          intersect = true;
        }
      }
      // 5. 另一个端点(B)在圆上
      else if (distanceSquare2 == radiusSquare)
      {
        // 向量BA与BC
        var BA = segmentStart - segmentEnd;
        var BC = circleCenter - segmentEnd;
        // 夹角小于等于90度
        if (BA.Dot(BC) >= 0)
        {
          intersect = true;
        }
      }
      // 6. 剩下的情况
      return intersect;
    }

    // 线段是否在圆内
    public static bool IsSegmentInsideCircle(Position segmentStart, Position segmentEnd, Position circleCenter, int circleRadius)
    {
      var distanceSquareCA = Position.DistanceSquare(circleCenter, segmentStart);
      var distanceSquareCB = Position.DistanceSquare(circleCenter, segmentEnd);
      var radiusSquare = (ulong)circleRadius * (ulong)circleRadius;
      // 线段的两个端点都在圆内则线段就在圆内
      return distanceSquareCA <= radiusSquare && distanceSquareCB <= radiusSquare;
    }

    // 线段是否接触到了圆(线段在圆的外部，其中一个端点在圆上)
    public static bool IsSegmentContactCircle(Position segmentStart, Position segmentEnd, Position circleCenter, int circleRadius)
    {
      var (projectionC2AB, distanceC2AB) = GetProjectionAndDistanceFromPointToLine(circleCenter, segmentStart, segmentEnd);
      bool contact = false;
      // 相切
      if (distanceC2AB == circleRadius)
      {
        // 判断切点是否在线段上(投影点肯定在线段所在直线上，如果不在线段内则在其中一个端点的一侧)
        if ((projectionC2AB.X() - segmentStart.X()) * (projectionC2AB.X() - segmentEnd.X()) <= 0 && (projectionC2AB.Y() - segmentStart.Y()) * (projectionC2AB.Y() - segmentEnd.Y()) <= 0)
        {
          contact = true;
        }
      }
      else if (distanceC2AB < circleRadius)
      {
        var distanceSquare1 = Position.DistanceSquare(circleCenter, segmentStart);
        var distanceSquare2 = Position.DistanceSquare(circleCenter, segmentEnd);
        var radiusSquare = (ulong)circleRadius * (ulong)circleRadius;
        // segmentStart在圆上，segmentEnd在圆外
        if (distanceSquare1 == radiusSquare && distanceSquare2 > radiusSquare)
        {
          // AB
          var AB = segmentEnd - segmentStart;
          // AC
          var AC = circleCenter - segmentStart;
          // 夹角判断，点积小于0夹角为钝角
          // 说明线段在圆外，与圆没有相交
          // 如果与圆相交，这个角一定是锐角，点积是大于0的
          if (AB.Dot(AC) < 0)
          {
            contact = true;
          }
        }
        // segmentEnd在圆上，segmentStart在圆外
        else if (distanceSquare1 > radiusSquare && distanceSquare2 == radiusSquare)
        {
          // BA
          var BA = segmentStart - segmentEnd;
          // BC
          var BC = circleCenter - segmentEnd;
          // 夹角判断
          if (BA.Dot(BC) < 0)
          {
            contact = true;
          }
        }
      }
      return contact;
    }

    // 线段是否跟矩形相交
    public static bool IsSegmentInterectRect(Position segmentStart, Position segmentEnd, Rect rect)
    {
      var lb = rect.LeftBottom();
      var rb = rect.RightBottom();
      var rt = rect.RightTop();
      var lt = rect.LeftTop();
      return IsSegmentIntersectBox(segmentStart, segmentEnd, lb, rb, rt, lt);
    }

    // 线段是否跟盒子相交
    public static bool IsSegmentIntersectBox(Position segmentStart, Position segmentEnd, Box box)
    {
      box.GetFourVertices(out var lb, out var rb, out var rt, out var lt);
      return IsSegmentIntersectBox(segmentStart, segmentEnd, lb, rb, rt, lt);
    }

    // 线段是否跟盒子相交
    public static bool IsSegmentIntersectBox(Position segmentStart, Position segmentEnd, Position leftBottom, Position rightBottom, Position rightTop, Position leftTop)
    {
      if (IsSegmentIntersectSegment(segmentStart, segmentEnd, leftBottom, rightBottom))
      {
        return true;
      }
      if (IsSegmentIntersectSegment(segmentStart, segmentEnd, rightBottom, rightTop))
      {
        return true;
      }
      if (IsSegmentIntersectSegment(segmentStart, segmentEnd, rightTop, leftTop))
      {
        return true;
      }
      if (IsSegmentIntersectSegment(segmentStart, segmentEnd, leftTop, leftBottom))
      {
        return true;
      }
      return false;
    }

    // 直线是否与矩形相交
    public static bool IsLineIntersectRect(Position linePos1, Position linePos2, Rect rect)
    {
      var lb = rect.LeftBottom();
      var rb = rect.RightBottom();
      var rt = rect.RightTop();
      var lt = rect.LeftTop();
      return IsLineIntersectBox(linePos1, linePos2, lb, rb, rt, lt);
    }

    // 直线是否跟盒子相交
    public static bool IsLineIntersectBox(Position linePos1, Position linePos2, Box box)
    {
      box.GetFourVertices(out var lb, out var rb, out var rt, out var lt);
      return IsLineIntersectBox(linePos1, linePos2, lb, rb, rt, lt);
    }

    // 直线是否跟盒子相交
    public static bool IsLineIntersectBox(Position linePos1, Position linePos2, Position leftBottom, Position rightBottom, Position rightTop, Position leftTop)
    {
      if (IsLineIntersectSegment(linePos1, linePos2, leftBottom, rightBottom))
      {
        return true;
      }
      if (IsLineIntersectSegment(linePos1, linePos2, rightBottom, rightTop))
      {
        return true;
      }
      if (IsLineIntersectSegment(linePos1, linePos2, rightTop, leftTop))
      {
        return true;
      }
      if (IsLineIntersectSegment(linePos1, linePos2, leftTop, leftBottom))
      {
        return true;
      }
      return false;
    }

    // 直线是否与扇形相交
    public static bool IsLineIntersectSector(Position linePos1, Position linePos2, Sector sector)
    {
      var center = sector.Center();
      var radius = sector.Radius();
      var distance = GetDistanceFromPointToLine(center, linePos1, linePos2);
      if (distance >= radius)
      {
        return false;
      }
      var posA = center;
      posA.Translate(radius, 0);
      posA.Rotate(center.X(), center.Y(), sector.RotationBegin());
      if (IsLineIntersectSegment(linePos1, linePos2, center, posA))
      {
        return true;
      }
      var posB = center;
      posB.Translate(radius, 0);
      posB.Rotate(center.X(), center.Y(), sector.RotationEnd());
      if (IsLineIntersectSegment(linePos1, linePos2, center, posB))
      {
        return true;
      }

      var projection = GetPointProjectionOnLine(center, linePos1, linePos2);
      var AC = center - posA;
      var BC = center - posB;
      return IsPointInSector(projection, center, AC, BC);
    }

    // 线段是否与扇形相交
    public static bool IsSegmentIntersectSector(Position segmentBegin, Position segmentEnd, Sector sector)
    {
      var center = sector.Center();
      var radius = sector.Radius();
      // 先判断与线段所在直线的距离，大于等于半径则肯定不相交
      var distance = GetDistanceFromPointToLine(center, segmentBegin, segmentEnd);
      if (distance >= radius)
      {
        return false;
      }

      // 分别判断两个端点是否在扇形内，只要有一个在扇形内就相交
      var posA = center;
      posA.Translate(radius, 0);
      posA.Rotate(center.X(), center.Y(), sector.RotationBegin());
      var posB = center;
      posB.Translate(radius, 0);
      posB.Rotate(center.X(), center.Y(), sector.RotationEnd());
      var AC = center - posA;
      var BC = center - posB;
      if (IsPointInSector(segmentBegin, center, AC, BC)) return true;
      if (IsPointInSector(segmentEnd, center, AC, BC)) return true;

      // 如果两端点都不在扇形内，且圆心到线段的投影在扇形内，则线段与扇形也相交
      var (projection, has) = GetPointProjectionOnSegment(center, segmentBegin, segmentEnd);
      if (!has) return false;

      return IsPointInSector(projection, center, AC, BC);
    }

    // 圆是否与圆相交
    public static bool IsCircleIntersectCircle(Circle circle, Circle circle2, bool includeTangent = false)
    {
      var center = circle.Center();
      var radius = circle.Radius();
      var center2 = circle2.Center();
      var radius2 = circle2.Radius();
      var distanceSquare = Position.DistanceSquare(center, center2);
      var twoRadiusSquare = (ulong)(radius + radius2) * (ulong)(radius + radius2);
      if (twoRadiusSquare < distanceSquare)
      {
        return false;
      }
      if (!includeTangent && twoRadiusSquare == distanceSquare)
      {
        return false;
      }
      return true;
    }

    // 两条直线的交点
    public static Position GetIntersectionOfTwoLine(Position line1PosA, Position line1PosB, Position line2PosC, Position line2PosD)
    {
      // AB
      var AB = line1PosB - line1PosA;
      // CD
      var CD = line2PosD - line2PosC;

      return line1PosA + AB * (int)(line2PosC - line1PosA).Cross(CD) / (int)AB.Cross(CD);
      // 点A到直线CD的距离
      //var da = GetDistanceFromPointToLine(line1PosA, line2PosC, line2PosD);
      // 点B到直线CD的距离
      //var db = GetDistanceFromPointToLine(line1PosB, line2PosC, line2PosD);
      // AB的长度
      //var lengthAB = AB.Length();
      // 通过相似三角形得到点A到交点P的向量
      //var AP = AB * ((int)lengthAB * da / (da + db));
      // 交点P就是点A加上向量AP
      //var pos = line1PosA + AP;
      //return pos;
    }

    // 直线与线段的交点
    public static (Position, bool) GetIntersectionOfLineAndSegment(Position linePos1, Position linePos2, Position segmentStart, Position segmentEnd)
    {
      var intersection = GetIntersectionOfTwoLine(linePos1, linePos2, segmentStart, segmentEnd);
      // 判断交点是否在线段上
      if ((intersection.X() - segmentStart.X()) * (intersection.X() - segmentEnd.X()) > 0 &&
          (intersection.Y() - segmentStart.Y()) * (intersection.Y() - segmentEnd.Y()) > 0)
      {
        return (new(), false);
      }
      return (intersection, true);
    }

    // 求两条线段的交点
    public static (Position, bool) GetIntersectionOfTwoSegment(Position start1, Position end1, Position start2, Position end2)
    {
      var intersection = new Position();
      // 判断两条线段有没有交点，这里用的是面积法
      /*long area_abc = (long)(start1.X()-start2.X())*(end1.Y()-start2.Y()) - (long)(start1.Y()-start2.Y())*(end1.X()-start2.X());
      long area_abd = (long)(start1.X()-end2.X())*(end1.Y()-end2.Y()) - (long)(start1.Y()-end2.Y())*(end1.X()-end2.X());
      if (area_abc * area_abd > 0)
      {
          return (intersection, false);
      }
      long area_cda = (long)(start2.X()-start1.X())*(end2.Y()-start1.Y()) - (long)(start2.Y()-start1.Y())*(end2.X()-start1.X());
      long area_cdb = area_cda + area_abc - area_abd;
      if (area_cda * area_cdb > 0)
      {
          return (intersection, false);
      }*/
      if (!IsSegmentIntersectSegment(start1, end1, start2, end2))
      {
        return (intersection, false);
      }

      long A1 = end1.Y() - start1.Y();
      long B1 = start1.X() - end1.X();
      long C1 = A1 * start1.X() + B1 * start1.Y();

      long A2 = end2.Y() - start2.Y();
      long B2 = start2.X() - end2.X();
      long C2 = A2 * start2.X() + B2 * start2.Y();

      long denominator = A1 * B2 - A2 * B1;
      if (denominator == 0)
      {
        return (intersection, false);
      }

      int x = (int)((B2 * C1 - B1 * C2) / denominator);
      int y = (int)((A1 * C2 - A2 * C1) / denominator);
      intersection.Set(x, y);

      return (intersection, true);
    }

    // 直线与圆的交点
    public static IntersectInfo GetIntersectionOfLineWithCircle(Position linePos1, Position linePos2, Position circleCenter, int circleRadius)
    {
      var (intersectInfo, _) = GetIntersectionOfLineOrSegmentWithCircle(linePos1, linePos2, circleCenter, circleRadius, true);
      return intersectInfo;
    }

    // 线段与圆的交点
    public static (IntersectInfo, bool) GetIntersectionOfSegmentWithCircle(Position lineStart, Position lineEnd, Position circleCenter, int circleRadius)
    {
      return GetIntersectionOfLineOrSegmentWithCircle(lineStart, lineEnd, circleCenter, circleRadius, false);
    }

    // 直线或者线段与圆的交点
    public static (IntersectInfo, bool) GetIntersectionOfLineOrSegmentWithCircle(Position lineStart, Position lineEnd, Position circleCenter, int circleRadius, bool isLineNotSegment = true)
    {
      // 先求圆心到线段所在直线的距离
      var distance = GetDistanceFromPointToLine(circleCenter, lineStart, lineEnd);
      if (distance > circleRadius)
      {
        return (new(), false);
      }

      var info = new IntersectInfo();
      var (projection, _) = GetPointProjectionOnLineOrSegment(circleCenter, lineStart, lineEnd);
      // 直线是圆的切线
      if (distance == circleRadius)
      {
        // 如果是线段要判断投影是否在线段上
        if (!isLineNotSegment && (projection.X() - lineStart.X()) * (projection.X() - lineEnd.X()) > 0 && ((projection.Y() - lineStart.Y()) * (projection.Y() - lineEnd.Y())) > 0)
        {
          return (new(), false);
        }

        info.IntersectionList ??= new();
        info.IntersectionList.Add(projection);
        return (info, true);
      }

      // 求圆心在线段上的投影点到直线与圆交点的距离
      var pc = MathUtil.Sqrt64((ulong)(circleRadius * circleRadius) - (ulong)(distance * distance));
      var vecAB = lineEnd - lineStart;
      var lengthAB = vecAB.Length();
      var intersectionC = projection + ((int)-pc) * vecAB / (int)lengthAB;
      bool c = true, d = true;
      if (!isLineNotSegment && (intersectionC.X() - lineStart.X()) * (intersectionC.X() - lineEnd.X()) > 0 && (intersectionC.Y() - lineStart.Y()) * (intersectionC.Y() - lineEnd.Y()) > 0)
      {
        // 不在线段内
        c = false;
      }
      var intersectionD = projection + (int)pc * vecAB / (int)lengthAB;
      if (!isLineNotSegment && (intersectionD.X() - lineStart.X()) * (intersectionD.X() - lineEnd.X()) > 0 && (intersectionD.Y() - lineStart.Y()) * (intersectionD.Y() - lineEnd.Y()) > 0)
      {
        // 不在线段内
        d = false;
      }
      if (c && d)
      {
        info.IntersectionList ??= new();
        info.IntersectionList.Add(intersectionC);
        info.IntersectionList.Add(intersectionD);
      }
      else if (c)
      {
        info.IntersectionList ??= new();
        info.IntersectionList.Add(intersectionC);
      }
      else if (d)
      {
        info.IntersectionList ??= new();
        info.IntersectionList.Add(intersectionD);
      }
      return (info, true);
    }

    // 直线与矩形的交点(与矩形四个边共线不算做交点，经过四个顶点中的一个也不算有交点)
    public static bool GetIntersectionOfLineAndRect(Position linePos1, Position linePos2, Rect rect, List<Position> intersectionList)
    {
      var lb = rect.LeftBottom();
      var rb = rect.RightBottom();
      var rt = rect.RightTop();
      var lt = rect.LeftTop();
      return GetIntersectionOfLineAndBox(linePos1, linePos2, lb, rb, rt, lt, intersectionList);
    }

    // 直线与盒子的交点(与盒子四个边共线不算做交点，经过四个顶点中的一个也不算有交点)
    public static bool GetIntersectionOfLineAndBox(Position linePos1, Position linePos2, Box box, List<Position> intersectionList)
    {
      box.GetFourVertices(out var lb, out var rb, out var rt, out var lt);
      return GetIntersectionOfLineAndBox(linePos1, linePos2, lb, rb, rt, lt, intersectionList);
    }

    // 直线与盒子的交点(与盒子四个边共线不算做交点，经过四个顶点中的一个也不算有交点)
    public static bool GetIntersectionOfLineAndBox(Position linePos1, Position linePos2, Position leftBottom, Position rightBottom, Position rightTop, Position leftTop, List<Position> intersectionList)
    {
      intersectionList.Clear();
      var (pos, has) = GetIntersectionOfLineAndSegment(linePos1, linePos2, leftBottom, rightBottom);
      if (has)
      {
        intersectionList.Add(pos);
      }
      (pos, has) = GetIntersectionOfLineAndSegment(linePos1, linePos2, rightBottom, rightTop);
      if (has)
      {
        intersectionList.Add(pos);
      }
      (pos, has) = GetIntersectionOfLineAndSegment(linePos1, linePos2, rightTop, leftTop);
      if (has)
      {
        intersectionList.Add(pos);
      }
      (pos, has) = GetIntersectionOfLineAndSegment(linePos1, linePos2, leftTop, leftBottom);
      if (has)
      {
        intersectionList.Add(pos);
      }
      return intersectionList.Count > 0;
    }

    // 线段与矩形的交点(不包括共线和线段端点在矩形四条边上的情况)
    public static bool GetIntersectionOfSegmentAndRect(Position segmentStart, Position segmentEnd, Rect rect, List<Position> intersectionList)
    {
      var lb = rect.LeftBottom();
      var rb = rect.RightBottom();
      var rt = rect.RightTop();
      var lt = rect.LeftTop();
      return GetIntersectionOfSegmentAndBox(segmentStart, segmentEnd, lb, rb, rt, lt, intersectionList);
    }

    // 线段与矩形的交点(不包括共线和线段端点在矩形四条边上的情况)
    public static bool GetIntersectionOfSegmentAndBox(Position segmentStart, Position segmentEnd, Box box, List<Position> intersectionList)
    {
      box.GetFourVertices(out var lb, out var rb, out var rt, out var lt);
      return GetIntersectionOfSegmentAndBox(segmentStart, segmentEnd, lb, rb, rt, lt, intersectionList);
    }

    // 线段与盒子的交点(不包括共线和线段端点在盒子四条边上的情况)
    static bool GetIntersectionOfSegmentAndBox(Position segmentStart, Position segmentEnd, Position leftBottom, Position rightBottom, Position rightTop, Position leftTop, List<Position> intersectionList)
    {
      intersectionList.Clear();
      var (pos, has) = GetIntersectionOfTwoSegment(segmentStart, segmentEnd, leftBottom, rightBottom);
      if (has)
      {
        intersectionList.Add(pos);
      }
      (pos, has) = GetIntersectionOfTwoSegment(segmentStart, segmentEnd, rightBottom, rightTop);
      if (has)
      {
        intersectionList.Add(pos);
      }
      (pos, has) = GetIntersectionOfTwoSegment(segmentStart, segmentEnd, rightTop, leftTop);
      if (has)
      {
        intersectionList.Add(pos);
      }
      (pos, has) = GetIntersectionOfTwoSegment(segmentStart, segmentEnd, leftTop, leftBottom);
      if (has)
      {
        intersectionList.Add(pos);
      }
      return intersectionList.Count > 0;
    }

    // 矩形是否与圆相交
    public static bool IsRectIntersectCircle(Rect rect, Circle circle)
    {
      var center = circle.Center();
      var radius = circle.Radius();
      var leftBottom = rect.LeftBottom();
      var rightBottom = rect.RightBottom();
      if (IsSegmentIntersectCircle(leftBottom, rightBottom, center, radius, true))
      {
        return true;
      }
      var rightTop = rect.RightTop();
      if (IsSegmentIntersectCircle(rightBottom, rightTop, center, radius, true))
      {
        return true;
      }
      var leftTop = rect.LeftTop();
      if (IsSegmentIntersectCircle(rightTop, leftTop, center, radius, true))
      {
        return true;
      }
      return IsSegmentIntersectCircle(leftTop, leftBottom, center, radius, true);
    }

    // 矩形是否与扇形相交
    public static bool IsRectIntersectSector(Rect rect, Sector sector)
    {
      var leftBottom = rect.LeftBottom();
      var rightBottom = rect.RightBottom();
      if (IsSegmentIntersectSector(leftBottom, rightBottom, sector))
      {
        return true;
      }
      var rightTop = rect.RightTop();
      if (IsSegmentIntersectSector(rightBottom, rightTop, sector))
      {
        return true;
      }
      var leftTop = rect.LeftTop();
      if (IsSegmentIntersectSector(rightTop, leftTop, sector))
      {
        return true;
      }
      return IsSegmentIntersectSector(leftTop, leftBottom, sector);
    }

    // 获得方向线段和矩形的第一个交点
    public static bool GetFirstIntersectionOfDirectionalSegmentAndRect(Position start, Position end, Rect rect, ref Position intersection)
    {
      var sx = start.X();
      var sy = start.Y();
      // 线段的起点在矩形内，则交点就是起点
      if (sx > rect.Left() && sx < rect.Right() && sy > rect.Bottom() && sy < rect.Top())
      {
        intersection.Set(sx, sy);
        return true;
      }
      posListCache_[0] = rect.LeftBottom();
      posListCache_[1] = rect.RightBottom();
      posListCache_[2] = rect.RightTop();
      posListCache_[3] = rect.LeftTop();
      int intersectionCount = 0;
      for (int i = 0; i < posListCache_.Length; i++)
      {
        var (pos, has) = GetIntersectionOfTwoSegment(start, end, posListCache_[i], posListCache_[(i + 1) % posListCache_.Length]);
        if (has)
        {
          if (intersectionCount == 0)
          {
            intersection = pos;
          }
          else
          {
            if (Position.DistanceSquare(start, intersection) > Position.DistanceSquare(start, pos))
            {
              intersection = pos;
              break;
            }
          }
          intersectionCount += 1;
        }
      }
      return intersectionCount > 0;
    }

    // 获得方向线段和圆的第一个交点
    public static bool GetFirstIntersectionOfDirectionalSegmentAndCircle(Position start, Position end, Circle circle, ref Position intersection)
    {
      // 起点在圆内，则交点就是起点
      var radius = circle.Radius();
      var center = circle.Center();
      if (Position.DistanceSquare(start, center) < (ulong)radius*(ulong)radius)
      {
        intersection = start;
        return true;
      }

      var (intersectionInfo, isIntersect) = GetIntersectionOfSegmentWithCircle(start, end, center, radius);
      if (isIntersect)
      {
        ulong ds = 0;
        for (int i=0; i<intersectionInfo.IntersectionList.Count; i++)
        {
          var intersectionPos = intersectionInfo.IntersectionList[i];
          var distanceSquare = Position.DistanceSquare(start, intersectionPos);
          if (ds == 0 || ds > distanceSquare)
          {
            intersection = intersectionPos;
            ds = distanceSquare;
          }
        }
      }
      return isIntersect;
    }
  }
}