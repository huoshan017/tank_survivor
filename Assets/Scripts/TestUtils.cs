using System;

public class MathUtils
{
    public static void Rotate(ref int x, ref int y, int ax, int ay, float angle)
    {
        var radian = angle * Math.PI/180;
        var s = Math.Sin(radian);
        var c = Math.Cos(radian);
        int nx = (int)((x - ax) * c  - (y - ay) * s) + ax;
        int ny = (int)((x - ax) * s  + (y - ay) * c) + ay;
        x = nx; y = ny;
    }
}