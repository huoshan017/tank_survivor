namespace Common.Geometry
{
    public struct Position
    {
        int x, y;

        public Position(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public readonly int X()
        {
            return x;
        }

        public readonly int Y()
        {
            return y;
        }

        public void Set(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public readonly long Dot(Position pos)
        {
            return (long)x * pos.x + (long)y * pos.y;
        }

        public readonly long Cross(Position pos)
        {
            return (long)x * pos.y - (long)y * pos.x;
        }

        public void Translate(int dx, int dy)
        {
            x += dx;
            y += dy;
        }

        // 绕点(ax, ay)旋转angle角度
        public void Rotate(int ax, int ay, Angle angle)
        {
            var s = MathUtil.Sine(angle);
            var c = MathUtil.Cosine(angle);
            int nx = (x-ax)*c/Const.denominator - (y-ay)*s/Const.denominator + ax;
            int ny = (x-ax)*s/Const.denominator + (y-ay)*c/Const.denominator + ay;
            x = nx; y = ny;
        }

        // 绕点(ax, ay)旋转到angle角度
        public void RotateTo(int ax, int ay, Angle angle)
        {
            int r = (int)MathUtil.Sqrt64((ulong)((x-ax)*(x-ax)) + (ulong)((y-ay)*(y-ay)));
            // x - ax = cos(angle) * r
            // y - ay = sin(angle) * r
            x = r * MathUtil.Cosine(angle)/Const.denominator + ax;
            y = r * MathUtil.Sine(angle)/Const.denominator + ay;
        }

        // 是否在矩形内
        public readonly bool IsInsideRect(Rect rect)
        {
            if (x <= rect.Left() || x >= rect.Right() || y <= rect.Bottom() || y >= rect.Top()) return false;
            return true;
        }

        // 是否在矩形的边上
        public readonly bool IsOnRectEdge(Rect rect)
        {
            return (x == rect.Left() || x == rect.Right()) && y >= rect.Bottom() && y <= rect.Top();
        }

        // 是否在盒内
        public readonly bool IsInsideBox(Box box, bool includeOnBox = true)
        {
            var center = box.Center();
            box.GetFourVertices(out var pos0, out var pos1, out var pos2, out var pos3);
            // AB pos1-pos0
            var ab = pos1 - pos0;
            // AP center-pos0
            var ap = center - pos0;
            // AB X AP
            var c1 = ab.Cross(ap);

            // CD pos3-pos2
            var cd = pos3 - pos2;
            // CP center-pos2
            var cp = center - pos2;
            // CD X CP
            var c2 = cd.Cross(cp);

            if (includeOnBox)
                if (c1*c2 < 0) return false;
            else
                if (c1*c2 <= 0) return false;

            // BC pos2-pos1
            var bc = pos2 - pos1;
            // BP center-pos1
            var bp = center - pos1;
            // BC X BP
            var c3 = bc.Cross(bp);

            // DA pos0-pos3
            var da = pos0 - pos3;
            // DP center-pos3
            var dp = center - pos3;
            // DA X DP
            var c4 = da.Cross(dp);

            if (includeOnBox)
                return c3*c4 >= 0;
            else
                return c3*c4 > 0;
        }

        public readonly bool IsInsideCircle(Circle circle, bool includeOnCircle)
        {
            var center = circle.Center();
            var radius = circle.Radius();
            var square = (center.X() - x)*(center.X() - x) + (center.Y() - y) * (center.Y() - y);
            if (includeOnCircle)
            {
                return radius * radius >= square;
            }
            else
            {
                return radius * radius > square;
            }
        }

        public readonly Vec2 ToVec2()
        {
            return new Vec2(x, y);
        }

        public override readonly bool Equals(object obj)
        {
            Position pos = (Position)obj;
            return (x == pos.x) && (y == pos.y);
        }
        
        public override readonly int GetHashCode()
        {
            return x * 10 + y;
        }

        public static bool operator == (Position pos1, Position pos2)
        {
            return (pos1.x == pos2.x) || (pos1.y == pos2.y);
        }

        public static bool operator != (Position pos1, Position pos2)
        {
            return !(pos1 == pos2);
        }

        public static Vec2 operator - (Position pos1, Position pos2)
        {
            return new Vec2(pos1.x-pos2.x, pos1.y-pos2.y);
        }

        public static Position operator - (Position pos, Vec2 vec)
        {
            pos.Translate(-vec.X(), -vec.Y());
            return pos;
        }

        public static Position operator + (Position pos, Vec2 vec)
        {
            pos.Translate(vec.X(), vec.Y());
            return pos;
        }

        public static Position operator + (Vec2 vec, Position pos)
        {
            pos.Translate(vec.X(), vec.Y());
            return pos;
        }

        public static uint Distance(Position pos1, Position pos2)
        {
            return MathUtil.Sqrt64(DistanceSquare(pos1, pos2));
        }

        public static ulong DistanceSquare(Position pos1, Position pos2)
        {
            return (ulong)(pos1.x - pos2.x)*(ulong)(pos1.x - pos2.x) + (ulong)(pos1.y - pos2.y)*(ulong)(pos1.y - pos2.y);
        }
    }
}