using System;
using System.Runtime.InteropServices;

namespace RealSimpleReleases
{
    class Program
    {
        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("Kernel32")]
        private static extern IntPtr GetConsoleWindow();

        const int SW_HIDE = 0;
        const int SW_SHOW = 5;

        static void Main(string[] args)
        {
            IntPtr hwnd;
            hwnd = GetConsoleWindow();
            //ShowWindow(hwnd, SW_HIDE);

            lib.ArgumentsParser parser = new lib.ArgumentsParser();
            
            if (args.Length == 1 && args[0] == "init" )            
            {
                string appname, main, url, user, pwd;
                Console.WriteLine("Welcome to Real Simple Upgrades.");
                Console.WriteLine("Please type the following information:");
                Console.WriteLine("App name:");
                appname = Console.ReadLine();
                Console.WriteLine("Main executable file:");
                main = Console.ReadLine();
                Console.WriteLine("FTP Server:");
                url = Console.ReadLine();
                Console.WriteLine("FTP User name:");
                user = Console.ReadLine();
                Console.WriteLine("FTP Password:");
                pwd = Console.ReadLine();

                parser.Init(appname, main, url, user, pwd);

                Console.WriteLine("Manifest created!");
                return;
            }

            if (args.Length == 2 && args[0] == "publish")
            {
                parser.PublishRelease(args[1]);
                return;
            }

            parser.Update();
            //parser.Init("myapp", "RealSimpleReleases.exe");
            //parser.Parse(new string[]{"add-file", "some"});

            //Console.Read();
        }
    }
}
