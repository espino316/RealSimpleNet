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

        static void InitializeManifest(ref lib.ArgumentsParser parser)
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
        } // end function InitializeManifest

        static void Main(string[] args)
        {
            
            IntPtr hwnd;
            hwnd = GetConsoleWindow();
            //ShowWindow(hwnd, SW_HIDE);

            lib.ArgumentsParser parser = new lib.ArgumentsParser();

            if (args.Length >= 1)
            {
                if (args[0] == "init")
                {
                    InitializeManifest(ref parser);
                    return;
                }

                if (args[0] == "publish")
                {
                    string version = null;
                    if (args.Length == 2)
                    {
                        version = args[1];
                    }

                    parser.PublishRelease(version);
                    return;
                }
            } // end if args.len = 1

            parser.Upgrade();

            try
            {
                
            } catch(Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        } // end function main

        static void Tests()
        {
            lib.models.Credentials creds = new lib.models.Credentials();
            creds.Pwd = "some";
            creds.Url = "url";
            creds.User = "res";
            creds.Encrypt();

            if (creds.Pwd == "some" || creds.Url == "url" || creds.User == "res")
            {
                throw new Exception("Credentias.Encrypt fail");
            }

            creds.Decrypt();

            if (creds.Pwd != "some" || creds.Url != "url" || creds.User != "res")
            {
                throw new Exception("Credentias.Decrypt fail");
            }
        } // end function Test
    }
}
