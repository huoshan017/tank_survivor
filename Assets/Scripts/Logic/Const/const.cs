using Logic.Base;
using Logic.Component;

namespace Logic.Const
{
    public class Static
    {
        // 必须的组件列表
        public static CompDef[] NecessaryCompDefList = new CompDef[]{
            new TransformCompDef(), new MovementCompDef(), new CampCompDef(),
        };

        // 缺省搜索间隔时间(毫秒)
        public static int DefaultSearchInterval = 1000;

        // 缺省射击间隔时间(毫秒)
        public static int DefaultShootingInterval = 2000;

        // 缺省光束发射持续时间(毫秒)
        public static int DefaultBeamShootingDuration = 2000;
    }
}