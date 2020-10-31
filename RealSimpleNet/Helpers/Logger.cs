using System;
using System.IO;

namespace RealSimpleNet.Helpers
{
    public class Logger
    {
        public static string AppName { get; set; }

        private static string LogFileName;

        private static void SetLogFileName()
        {
            if (String.IsNullOrEmpty(AppName))
                throw new Exception("No app name");

            LogFileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), AppName.Replace(" ","").Replace("$","").Replace("!",""));
            LogFileName = LogFileName + string.Format("_{0:yyyyMMdd}.log", DateTime.Now);
        } // end function SetLogFileName

        public static void WriteLine(string line)
        {
            SetLogFileName();
            using (StreamWriter file = new StreamWriter(LogFileName))
            {
                file.WriteLine(line);
            } // end using StreamWriter
        } // end function WriteLine
    } // end class
} // end namespace
