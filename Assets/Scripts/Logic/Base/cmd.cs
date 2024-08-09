namespace Logic.Base 
{
   public static class CommandDefine
    {
        // 移动命令，参数是方向角angle
        public static int CmdMove = 0;
        // 停止移动命令，无参数
        public static int CmdStopMove = 1;
        // 头部朝向命令，参数是指向的一个坐标(x,y)
        public static int CmdHeadForward = 2;
        // 开火命令，无参数
        public static int CmdFire = 3;
        // 停止开火，无参数
        public static int CmdStopFire = 4;
    } 
}