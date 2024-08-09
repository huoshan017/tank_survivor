namespace Common
{
    public interface IDebugger
    {
        public void Info(object message);
        public void Warning(object message);
        public void Error(object message);
    }

    public class DebugLog
    {
        public static void SetDebug(IDebugger debugger)
        {
            debugger_ = debugger;
        }

        public static void Info(object message)
        {
            debugger_.Info(message);
        }

        public static void Warning(object message)
        {
            debugger_.Warning(message);
        }

        public static void Error(object message)
        {
            debugger_.Error(message);
        }

        static IDebugger debugger_;
    }
}