namespace Common.Geometry
{
    public struct Angle
    {
        short degree, minute;

        public Angle(short degree, short minute)
        {
            this.degree = degree;
            this.minute = minute;
        }

        public readonly short Degree()
        {
            return degree;
        }

        public readonly short Minute()
        {
            return minute;
        }

        public void Reset(short degree, short minute)
        {
            this.degree = degree;
            this.minute = minute;
            Normalize();
        }

        public void Set(int minutes)
        {
            Normalize(minutes);
        }

        public void Clear()
        {
            degree = 0;
            minute = 0;
        }

        // 把角度限制到[0, 360)
        public void Normalize()
        {
            if (degree >= 0 && degree < 360 && minute >= 0 && minute < 60) 
            {
                return;
            }
            if (degree == 360 && minute == 0)
            {
                degree = 0;
                return;
            }
            var minutes = degree * 60 + minute;
            Normalize(minutes);
        }

        public void Add(Angle angle)
        {
            var minutes = 60*(degree+angle.degree) + minute + angle.minute;
            Normalize(minutes);
        }

        public void AddDegree(short degree)
        {
            var minutes = 60*(this.degree + degree) + minute;
            Normalize(minutes);
        }

        public void AddMinutes(short minutes)
        {
            var newMinutes = 60*degree + minute + minutes;
            Normalize(newMinutes);
        }

        public void Sub(Angle angle)
        {
            var minutes = 60*(degree-angle.degree) + minute - angle.minute;
            Normalize(minutes);
        }

        public void SubDegree(short degree)
        {
            this.degree -= degree;
        }

        public readonly int ToMinutes()
        {
            return degree * 60 + minute;
        }

        public readonly Angle Negative() 
        {
            var d = (short)-degree;
            var m = (short)-minute;
            return new Angle(d, m);
        }

        public readonly bool IsNegative()
        {
            return degree < 0 && minute < 0;
        }

        public readonly bool Equal(Angle angle)
        {
            return degree == angle.degree && minute == angle.minute;
        }

        public static bool Greater(Angle angle1, Angle angle2)
        {
            angle1.Normalize();
            angle2.Normalize();
            var m1 = angle1.ToMinutes();
            var m2 = angle2.ToMinutes();
            return m1 > m2;
        }

        public static bool GreaterEqual(Angle angle1, Angle angle2)
        {
            angle1.Normalize();
            angle2.Normalize();
            var m1 = angle1.ToMinutes();
            var m2 = angle2.ToMinutes();
            return m1 >= m2;
        }

        public static bool Less(Angle angle1, Angle angle2)
        {
            return !GreaterEqual(angle1, angle2);
        }

        public static bool LessEqual(Angle angle1, Angle angle2)
        {
            return !Greater(angle1, angle2);
        }

        public static Angle Add(Angle angle1, Angle angle2)
        {
            angle1.Add(angle2);
            return angle1;
        }

        public static Angle Sub(Angle angle1, Angle angle2)
        {
            angle1.Sub(angle2);
            return angle1;
        }

        public static Angle Zero()
        {
            return new Angle();
        }

        public static Angle HalfPi()
        {
            return new Angle{degree = 90};
        }

        public static Angle Pi()
        {
            return new Angle{degree = 180};
        }

        public static Angle OneAndHalfPi()
        {
            return new Angle{degree = 270};
        }

        public static Angle TwoPi()
        {
            return new Angle{degree = 360};
        }

        public static bool operator == (Angle angle1, Angle angle2)
        {
            return angle1.Equal(angle2);
        }

        public static bool operator != (Angle angle1, Angle angle2)
        {
            return !angle1.Equal(angle2);
        }

        public static bool operator > (Angle angle1, Angle angle2)
        {
            return Greater(angle1, angle2);
        }

        public static bool operator < (Angle angle1, Angle angle2)
        {
            return Less(angle2, angle1);
        }

        public static bool operator >= (Angle angle1, Angle angle2)
        {
            return GreaterEqual(angle1, angle2);
        }

        public static bool operator <= (Angle angle1, Angle angle2)
        {
            return LessEqual(angle2, angle1);
        }

        void Normalize(int minutes)
        {
            if (minutes >= 360 * 60)
            {
                minutes %= (360 * 60);
            }
            else if (minutes < 0)
            {
                minutes = -minutes;
                minutes %= (360 * 60);
                minutes = (360 * 60) - minutes;
            }

            if (minutes == 0)
            {
                degree = 0;
                minute = 0;
            }
            else 
            {
                degree = (short)(minutes / 60);
                minute = (short)(minutes % 60);
            }
        }

        public readonly Vec2 ToVec2()
        {
            if (minute == 0)
            {
                if (degree == 0)
                {
                    return new Vec2(1, 0);
                }
                if (degree == 90)
                {
                    return new Vec2(0, 1);
                }
                if (degree == 180)
                {
                    return new Vec2(-1, 0);
                }
                if (degree == 270)
                {
                    return new Vec2(0, -1);
                }
            }
            var y = MathUtil.Tangent(this);
            if (y < 0)
            {
                if (degree >= 90 && degree < 180)
                {
                    return new Vec2(-MathUtil.Denominator(), -y);
                }
            }
            else if (y > 0)
            {
                if (degree >= 180 && degree < 270)
                {
                    return new Vec2(-MathUtil.Denominator(), -y);
                }
            }
            return new Vec2(MathUtil.Denominator(), y);
        }

        public readonly override bool Equals(object obj)
        {
            return Equal((Angle)obj);
        }

        public readonly override int GetHashCode()
        {
            return degree*60+minute;
        }
    }
}