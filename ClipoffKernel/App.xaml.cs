using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Windows;
using System.IO;
using System.Runtime.InteropServices;
using Clipoff.UI;
namespace Clipoff
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        void App_Startup(object sender, StartupEventArgs e)
        {
            if (e.Args == null || e.Args.Length < 1)
            {
                return;
            }
            Instance = this;
            if (e.Args[0].Equals("c"))//clip
            {
                this.ShutdownMode = System.Windows.ShutdownMode.OnExplicitShutdown;
                if (e.Args.Length > 1)
                {
                    this.infoPath = e.Args[1];
                }
                ClipWindow clipWindow = new ClipWindow();
                System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(clipWindow);
                clipWindow.Show();

                //clipWindow.Activate();
                SetForegroundWindow(System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle);
            }
            else if (e.Args[0].Equals("s"))//settings
            {
                if (e.Args.Length > 1)
                {
                    this.ClipoffPath = e.Args[1];
                }
                (new SettingWindow()).Show();
            }
            else if (e.Args[0].Equals("a"))//about
            {
            }
        }

        
        public static App Instance { get; private set; }
        public string ClipoffPath { get; private set; }
        int taskCount = 0;
        public int TaskCount
        {
            get
            {
                lock (this)
                {
                    return taskCount;
                }
            }
            set
            {
                lock (this)
                {
                    taskCount = value;
                }
            }
        }

        string infoPath;
        List<String[]> msgList = new List<string[]>();
        bool hasError = false;

        [DllImport("user32.dll")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        protected override void OnExit(ExitEventArgs e)
        {
            if (!String.IsNullOrEmpty(infoPath) && msgList != null && msgList.Count > 0)
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter(infoPath))
                    {
                        if (msgList.Count == 1 && msgList[0] != null && msgList[0].Length > 1)
                        {
                            if (!String.IsNullOrEmpty(msgList[0][0])) sw.WriteLine(msgList[0][0]);
                            if (!String.IsNullOrEmpty(msgList[0][1])) sw.WriteLine(msgList[0][1]);
                        }
                        else if(msgList.Count > 1)
                        {
                            sw.WriteLine(hasError ? "Error" : "Info");
                            foreach (string[] msg in msgList)
                            {
                                if (msg != null && msg.Length > 1)
                                {
                                    if (!String.IsNullOrEmpty(msg[0]))
                                    {
                                        sw.Write(msg[0]);
                                        sw.WriteLine(String.IsNullOrEmpty(msg[1]) ? "" : msg[1]);
                                    }
                                    else
                                    {
                                        if (!String.IsNullOrEmpty(msg[1])) sw.WriteLine(msg[1]);
                                    }
                                }
                            }
                        }
                    }
                }
                catch { }
            }
            base.OnExit(e);
        }

        public void AddMessage(string message, string title = "Info: ")
        {
            lock (this)
            {
                this.msgList.Add(new string[] { title, message });
                if (--taskCount <= 0)
                {
                    BeginInvoke(new Action(() =>
                        {
                            Shutdown();
                        }
                    ));
                }
            }
        }

        public void AddError(string message, string title = "Error: ")
        {
            lock (this)
            {
                hasError = true;
                this.msgList.Add(new string[] { title, message });
                if (--taskCount <= 0)
                {
                    BeginInvoke(new Action(() =>
                        {
                            Shutdown();
                        }
                    ));
                }
            }
        }

        public void TryExit()
        {
            if (this.TaskCount <= 0)
            {
                BeginInvoke(new Action(() =>
                    {
                        Shutdown();
                    }
                ));
            }
        }


        public void BeginInvoke(Action action)
        {
            Application.Current.Dispatcher.BeginInvoke(action);
        }

        public void BeginClip()
        {
            ClipWindow clipWindow = new ClipWindow();
            clipWindow.Topmost = true;
            System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(clipWindow);
            clipWindow.Show();
            clipWindow.Activate();
        }

    }

}


//namespace System.Runtime.CompilerServices
//{
//    public class ExtensionAttribute : Attribute { }
//}
