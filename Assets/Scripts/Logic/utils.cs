using Logic.Base;

namespace Logic
{
    public static class Utils
    {
        public static int Dir2Degree(DirType dir)
        {
            int degree = 0;
            switch (dir)
            {
                case DirType.Right:
                degree = 0;
                    break;
                case DirType.RightUp:
                    degree = 45;
                    break;
                case DirType.Up:
                    degree = 90;
                    break;
                case DirType.LeftUp:
                    degree = 135;
                    break;
                case DirType.Left:
                    degree = 180;
                    break;
                case DirType.LeftDown:
                    degree = 225;
                    break;
                case DirType.Down:
                    degree = 270;
                    break;
                case DirType.RightDown:
                    degree = 315;
                    break;
            }
            return degree;
        }
    }
}