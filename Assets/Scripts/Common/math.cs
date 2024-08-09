using Common.Geometry;

namespace Common
{
    static class MathUtil
    {
        public static int Denominator()
        {
            return Const.denominator;
        }

        public static int Sine(Angle angle)
        {
            angle.Normalize();
            var negative = 1;
            var degree = angle.Degree();
            if (degree >= 90 && degree < 180)
            {
                var pi = Angle.Pi();
                angle = Angle.Sub(pi, angle);
            }
            else if (degree >= 180 && degree < 270)
            {
                angle.SubDegree(180);
                negative = -1;
            }
            else if (degree >= 270 && degree < 360)
            {
                var twoPi = Angle.TwoPi();
                angle = Angle.Sub(twoPi, angle);
                negative = -1;
            }
            return negative * Const.sinval[angle.Degree(), angle.Minute()/10];
        }

        public static int Cosine(Angle angle)
        {
            angle.Normalize();
            var negative = 1;
            var degree = angle.Degree();
            if (degree >= 90 && degree < 180)
            {
                var pi = Angle.Pi();
                angle = Angle.Sub(pi, angle);
                negative = -1;
            }
            else if (degree >= 180 && degree < 270)
            {
                angle.SubDegree(180);
                negative = -1;
            }
            else if (degree >= 270 && degree < 360)
            {
                var twoPi = Angle.TwoPi();
                angle = Angle.Sub(twoPi, angle);
            }
            var dm = 90 * 60 - angle.ToMinutes();
            return negative * Const.sinval[dm/60, dm%60/10];
        }

        public static int Tangent(Angle angle)
        {
            angle.Normalize();
            var degree = angle.Degree();
            
            if (degree == 90 || degree == 270)
            {
                if (angle.Minute() == 0)
                {
                    return int.MaxValue;
                }
                else if (angle.Minute() < 10) // TODO 用小于10的原因是除以10之后就等于0了
                {
                    angle.AddMinutes((short)(10-angle.Minute()));
                }
            }
            if (degree >= 90 && degree < 180)
            {
                angle.SubDegree(90);
                var n = Cotangent(angle);
                return -n;
            }
            else if (degree >= 180 && degree < 270)
            {
                angle.SubDegree(180);
            }
            else if (degree >= 270 && degree < 360)
            {
                var tp = Angle.TwoPi();
                tp.Sub(angle);
                var n = Tangent(tp);
                return -n;
            }
            return Const.tanval[angle.Degree(), angle.Minute()/10];
        }

        public static int Cotangent(Angle angle)
        {
            var a = Angle.HalfPi();
            a.Sub(angle);
            return Tangent(a);
        }

        // 反正弦 [-90, 90]
        public static Angle Arcsin(int numerator)
        {
            var column = Const.sinval.GetLength(1);
            var totalNum = Const.sinval.GetLength(0) * column;
            var left = 0;
            var right = totalNum-1;
            var mid = (left+right) >> 1;
            short degree, minute;
            if (numerator >= 0)
            {
                // TODO 用小于而不是小于等于是因为重新设置left和right时不会在mid上加1和减1了
                // 终止循环的条件是left >= mind
                while (left < mid)
                {
                    var v = Const.sinval[mid/column, mid%column];
                    if (numerator < v)
                    {
                        right = mid;
                        mid = (left + right) >> 1;
                    }
                    else if (numerator > v)
                    {
                        left = mid;
                        mid = (left + right) >> 1;
                    }
                    else
                    {
                        break;
                    }
                }
                degree = (short)(mid/column);
                minute = (short)(10*(mid%column));
            }
            else
            {
                while (left < mid)
                {
                    var v = Const.sinval[mid/column, mid%column];
                    if (numerator < -v)
                    {
                        left = mid;
                        mid = (left + right) >> 1;
                    }
                    else if (numerator > -v)
                    {
                        right = mid;
                        mid = (left + right) >> 1;
                    }
                    else
                    {
                        break;
                    }
                }
                degree = (short)-(mid/column);
                minute = (short)-(10*(mid%column));
            }
            return new Angle(degree, minute);
        }

        // 反余弦 [0, 180]
        public static Angle Arccos(int numerator)
        {
            var angle = Arcsin(numerator);
            var minutes = 90 * 60 - angle.ToMinutes();
            angle.Set(minutes);
            return angle;
        }

        // 反正切 (-90, 90)
        public static Angle Arctan(int numerator)
        {
            var column = Const.tanval.GetLength(1);
            var totalNum = Const.tanval.GetLength(0) * column;
            var left = 0;
            var right = totalNum-1;
            var mid = (left+right) >> 1;
            short degree, minute;
            if (numerator >= 0)
            {
                // TODO 用小于而不是小于等于是因为重新设置left和right时不会在mid上加1和减1了
                // 终止循环的条件是left >= mid
                while (left < mid)
                {
                    var v = Const.tanval[mid/column, mid%column];
                    if (numerator < v)
                    {
                        right = mid;
                        mid = (left + right) >> 1;
                    }
                    else if (numerator > v)
                    {
                        left = mid;
                        mid = (left + right) >> 1;
                    }
                    else
                    {
                        break;
                    }
                }
                degree = (short)(mid/column);
                minute = (short)(10*(mid%column));
            }
            else
            {
                while (left < mid)
                {
                    var v = Const.tanval[mid/column, mid%column];
                    if (numerator < -v)
                    {
                        left = mid;
                        mid = (left + right) >> 1;
                    }
                    else if (numerator > -v)
                    {
                        right = mid;
                        mid = (left + right) >> 1;
                    }
                    else
                    {
                        break;
                    }
                }
                degree = (short)-(mid/column);
                minute = (short)-(10*(mid%column));
            }
            return new Angle(degree, minute);
        }

        public static Angle Arccot(int numerator)
        {
            var angle = Arctan(numerator);
            var minutes = 90 * 60 - angle.ToMinutes();
            angle.Set(minutes);
            return angle;
        }

        public static uint Sqrt(uint num)
        {
            if (num <= 1)
            {
                return num;
            }

            var s = 1;
            var num1 = num - 1;
            if (num1 > 65535)
            {
                s += 8;
                num1 >>= 16;
            }
            if (num1 > 255)
            {
                s += 4;
                num1 >>= 8;
            }
            if (num1 > 15)
            {
                s += 2;
                num1 >>= 4;
            }
            if (num1 > 3)
            {
                s += 1;
            }
            var x0 = (uint)(1 << s);
            var x1 = (x0 + (num >> s)) >> 1;
            while (x1 < x0) {
                x0 = x1;
                x1 = (x0 + (num / x0)) >> 1;
            }
            return x0;
        }

        public static uint Sqrt64(ulong num)
	    {
		    if (0 == num)
			    return 0;
            
            if (num <= uint.MaxValue) {
                return Sqrt((uint)num);
            }

		    var n = num / 2 + 1;
		    var n1 = (n + num / n) / 2;

		    while (n1 < n)
		    {
			    n = n1;
			    n1 = (n + num / n) / 2;
		    }

		    return (uint)n;
	    }

        public static short Abs(short num)
        {
            return num < 0 ? (short)-num : num;
        }

        public static int Abs(int num)
        {
            return num < 0 ? -num : num;
        }

        public static long Abs(long num)
        {
            return num < 0 ? -num : num;
        }

        public static short Max(short a, short b)
        {
            return a >= b ? a : b;
        }

        public static ushort Max(ushort a, ushort b)
        {
            return a >= b ? a : b;
        }

        public static int Max(int a, int b)
        {
            return a >= b ? a : b;
        }

        public static Vec2 Angle2Vec2(Angle angle)
        {
            angle.Normalize();
            var degree = angle.Degree();
            if (degree == 90)
            {
                return new Vec2(0, Const.denominator);
            }
            else if (degree == 270)
            {
                return new Vec2(0, -Const.denominator);
            }
            else if (degree < 90)
            {
                return new Vec2(Tangent(angle), Const.denominator);
            }
            else if (degree < 180)
            {
                return new Vec2(-Tangent(angle), Const.denominator);
            }
            else if (degree < 270)
            {
                return new Vec2(-Tangent(angle), -Const.denominator);
            }
            return new Vec2(Tangent(angle), -Const.denominator);
        }
    }
}