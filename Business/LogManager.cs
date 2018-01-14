using System;
using System.IO;

namespace Unicorn.UWP.Business
{
    public class LogManager
    {
        public static void Log(string msg)
        {
            File.AppendAllText(Constant.Constant.LogFolder, string.Format("{0} {1}", msg, DateTime.Now) + Environment.NewLine);
        }
    }
}
