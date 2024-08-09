using Common.Geometry;

namespace Common
{
    public struct Vec2
    {
        int x, y;

        public Vec2(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        readonly public int X()
        {
            return x;
        }

        readonly public int Y()
        {
            return y;
        }

        public void Add(Vec2 vec)
        {
            x += vec.x;
            y += vec.y;
        }

        public void Sub(Vec2 vec)
        {
            x -= vec.x;
            y -= vec.y;
        }

        public void Mul(int a)
        {
            x *= a;
            y *= a;
        }

        public void Div(int a)
        {
            x /= a;
            y /= a;
        }

        public void Translate(int x0, int y0)
        {
            x += x0;
            y += y0;
        }

        public void Scale(int sx, int sy)
        {
            x *= sx;
            y *= sy;
        }

        public void Rotate(int ax, int ay, Angle angle)
        {
            var s = MathUtil.Sine(angle);
            var c = MathUtil.Cosine(angle);
            int nx = (x-ax)*c/Const.denominator - (y-ay)*s/Const.denominator + ax;
            int ny = (x-ax)*s/Const.denominator + (y-ay)*c/Const.denominator + ay;
            x = nx; y = ny;
        }

        public readonly uint Length()
        {
            return MathUtil.Sqrt64((ulong)x*(ulong)x + (ulong)y*(ulong)y);
        }

        public readonly ulong LengthSquare()
        {
            return (ulong)x*(ulong)x + (ulong)y*(ulong)y;
        }

        public readonly long Dot(Vec2 v)
        {
            return (long)x*v.x + (long)y*v.y;
        }

        public readonly long Cross(Vec2 v)
        {
            return (long)x*v.y - (long)y*v.x;
        }

        // 变成角度 [0, 360)
        public readonly Angle ToAngle()
        {
            if (x == 0)
            {
                if (y < 0)
                {
                    return new Angle(-90+360, 0);
                }
                if (y > 0)
                {
                    return new Angle(90, 0);
                }
                return new Angle(0, 0);
            }

            var numerator = Const.denominator * y / x;
            // 返回的角度范围是(-90, 90)
            var angle = MathUtil.Arctan(numerator);
            /// TODO 把角度限制到[0, 360)
            // 第四象限，把角度值从(-90, 0)变换到(270, 360)，所以加上360
            if (x > 0 && y < 0)
            {
                angle.Add(Angle.TwoPi());
            }
            // 第二象限，把角度值从(-90, 0)等价换成(90, 180)，所以加上180
            else if (x < 0 && y > 0)
            {
                angle.Add(Angle.Pi());
            }
            // 第三象限，把角度值从(0, 90)变换到(180, 270)，所以加上180
            else if (x < 0 && y < 0)
            {
                angle.Add(Angle.Pi());
            }
            return angle;
        }

        public static Vec2 Sub(Vec2 vec1, Vec2 vec2)
        {
            vec1.Sub(vec2);
            return vec1;
        }

        public static Vec2 Add(Vec2 vec1, Vec2 vec2)
        {
            vec1.Add(vec2);
            return vec1;
        }

        public static Vec2 Mul(Vec2 vec, int a)
        {
            vec.Mul(a);
            return vec;
        }

        public static Vec2 Div(Vec2 vec, int a)
        {
            vec.Div(a);
            return vec;
        }

        public static long Dot(Vec2 vec1, Vec2 vec2)
        {
            return vec1.Dot(vec2);
        }

        public static long Cross(Vec2 vec1, Vec2 vec2)
        {
            return vec1.Cross(vec2);
        }

        public static Vec2 operator * (Vec2 vec, int a)
        {
            vec.x *= a;
            vec.y *= a;
            return vec;
        }

        public static Vec2 operator * (int a, Vec2 vec)
        {
            vec.x *= a;
            vec.y *= a;
            return vec;
        }

        public static Vec2 operator / (Vec2 vec, int a)
        {
            vec.x /= a;
            vec.y /= a;
            return vec;
        }
    }
}