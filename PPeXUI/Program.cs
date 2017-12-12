using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PPeXUI
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

#if !DEBUG
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
#endif

            Application.Run(new formMain());
        }

        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            HandleCrash(e.Exception);
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            HandleCrash(e.ExceptionObject as Exception);
        }

        static void HandleCrash(Exception e)
        {
            string[] files;

            if (saveLog(e, out string filename))
            {
                files = new string[] { filename };
            }
            else
            {
                files = new string[0];
            }

            var crash = new formError(e.Message, files);
            crash.ShowDialog();

            Application.Exit();
        }

        static Version GetVersion()
        {
            return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
        }

        static bool saveLog(Exception ex, out string filename)
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

            string log = "";

            try
            {
                log += $"PPeXUI {GetVersion().ToString()}\r\n";
            }
            catch { }
            try
            {
                log += $"PPeX Base {PPeX.Core.GetVersion().ToString()}\r\n";
            }
            catch { }
            try
            {
                string platform = Enum.GetName(typeof(PlatformID), Environment.OSVersion.Platform);
                string process = Environment.Is64BitProcess ? "x64" : "x86";
                string system = Environment.Is64BitOperatingSystem ? "x64" : "x86";
                log += $"Platform {platform}/{process}/{system}\r\n";
            }
            catch { }
            
            try
            {
                log += "\r\n";
                log += "User data:\r\n";
                foreach (KeyValuePair<object, object> item in ex.Data)
                {
                    log += $"{item.Key.ToString()}={item.Value.ToString()}";
                }
            }
            catch { }

            try
            {
                log += "\r\n";
                log += "Exception message:\r\n";
                log += ex.Message;
            }
            catch { }

            try
            {
                log += "\r\n";
                log += "Exception stacktrace:\r\n";
                log += ex.StackTrace;
            }
            catch { }

            try
            {
                filename = $"PPeXUI crash {DateTime.Now.ToString("d-M-yyyy hh-mm-ss")}.log";
                File.WriteAllText(filename, log);
                return true;
            }
            catch
            {
                filename = "";
                return false;
            }
        }
    }
}
