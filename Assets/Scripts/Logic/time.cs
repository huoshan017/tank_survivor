using System;

namespace Logic
{
    public static class GTime
    {
        static readonly long startMs_ = DateTime.Now.Ticks/10000;

        public static long CurrMs()
        {
            return DateTime.Now.ToUniversalTime().Ticks/10000 - startMs_;
        }

        public static string CurrTimeString()
        {
            return DateTime.Now.ToString();
        }
    }
}