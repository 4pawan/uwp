using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unicorn.UWP;

namespace UnicornWidget.Business
{
    public class LogManager
    {
        public static void Log(string msg)
        {
            File.AppendAllText(AppVariables.LogFolder, string.Format("{0} {1}", msg, DateTime.Now) + Environment.NewLine);
        }
    }
}
