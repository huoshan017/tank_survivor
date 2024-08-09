using Common;
using Common.Geometry;

namespace Logic
{
    public static class Movement
    {
        /// 直线距离
        // @param speed 速度 距离单位/秒
        // @param tickMs 毫秒数
        // @return 直线距离
        public static int GetLinearDistance(int speed, uint tickMs)
        {
            return speed * (int)tickMs / 1000;
        }

        /// 直线运动
        // @param pos 当前位置，相对于父坐标系
        // @param speed 速度 距离单位/秒
        // @param moveDir 移动方向
        // @param tickMs 毫秒数
        // @return 运动后的位置
        public static Position LinearMove(Position pos, int speed, Angle moveDir, uint tickMs)
        {
            var distance = GetLinearDistance(speed, tickMs);
            var s = MathUtil.Sine(moveDir);
            var c = MathUtil.Cosine(moveDir);
            var dx = distance * c / MathUtil.Denominator();
            var dy = distance * s / MathUtil.Denominator();
            return new Position(pos.X() + dx, pos.Y()+dy);
        }

        /// 圆周运动
        // @param pos 当前位置，相对于父坐标系
        // @param center 圆心，相对于父坐标系
        // @param radius 半径
        // @param angularVelocity  角度/秒，大于零表示逆时针，小于零表示顺时针
        // @param tickMs 毫秒数
        // @return 运动后的位置
        public static Position CircleMove(Position pos, Position center, int radius, int angularVelocity, uint tickMs)
        {
            var cx = center.X();
            var cy = center.Y();
            var distance = Position.Distance(pos, center);
            if (distance != radius)
            {
                // 找到pos和center连线上的点，使得距离center等于半径radius
                var x = (pos.X() - center.X()) * radius / distance + cx;
                var y = (pos.Y() - center.Y()) * radius / distance + cy;
                pos.Set((int)x, (int)y);
            }
            var a = angularVelocity * tickMs / 1000;
            var angle = new Angle((short)a, 0);
            pos.Rotate(cx, cy, angle);
            return pos; 
        }

        /// 圆周运动
        // @param relativeAngle 与圆心连线逆时针角度，相对于父坐标系
        // @param center 圆心，相对于父坐标系
        // @param radius 半径
        // @param angularVeloctiy 角速度 角度/秒
        // @param tickMs 毫秒数
        // @return 运动后的位置
        public static Position CircleMove(Angle relativeAngle, Position center, int radius, int angularVelocity, uint tickMs)
        {
            var cx = center.X();
            var cy = center.Y();
            int x = MathUtil.Cosine(relativeAngle) * radius / MathUtil.Denominator() + cx;
            int y = MathUtil.Sine(relativeAngle) * radius / MathUtil.Denominator() + cy;
            var pos = new Position(x, y);
            var a = angularVelocity * tickMs / 1000;
            var angle = new Angle((short)a, 0);
            pos.Rotate(cx, cy, angle);
            return pos;
        }
    }
}