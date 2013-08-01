using System;
using System.Text;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Reflection;

namespace Clipoff.UI
{
    public class TrayIcon
    {
        System.Windows.Forms.NotifyIcon notifyIcon = new NotifyIcon();
        System.Windows.Forms.Timer timer = new Timer();
        Stack<AppMessage> msgStack = new Stack<AppMessage>(), errorStack = new Stack<AppMessage>();
        List<AppMessage> msgList = new List<AppMessage>();

        public TrayIcon()
        {
            notifyIcon.Icon = new System.Drawing.Icon("icon.ico");
            notifyIcon.Text = "Double Click to Clip Screen";
            notifyIcon.Visible = true;
            notifyIcon.ContextMenuStrip = new TrayMenu();
            notifyIcon.MouseClick += notifyIcon_MouseClick;
            notifyIcon.MouseDoubleClick += notifyIcon_MouseDoubleClick;

            timer.Interval = 100;
            timer.Tick += timer_Tick;
            timer.Start();
        }

        void timer_Tick(object sender, EventArgs e)
        {
            //if (!App.Clipping && App.clipWindow != null)
            //{
            //    App.EndClip();
            //}
            if (msgList == null)
            {
                msgList = new List<AppMessage>();
            }
            else
            {
                msgList.Clear();
            }
            bool hasError = false;

            lock (msgStack)
            {
                if (msgStack.Count > 0)
                {
                    while (msgStack.Count > 0)
                    {
                        msgList.Add(msgStack.Pop());
                    }
                }
            }
            lock (errorStack)
            {
                if (errorStack.Count > 0)
                {
                    hasError = true;
                    while (errorStack.Count > 0)
                    {
                        msgList.Add(errorStack.Pop());
                    }
                }
            }

            if (msgList.Count == 1)
            {
                notifyIcon.ShowBalloonTip(1, msgList[0].Title, msgList[0].Message, hasError ? ToolTipIcon.Error : ToolTipIcon.Info);
            }
            else if (msgList.Count > 1)
            {
                StringBuilder sb = new StringBuilder();
                foreach (AppMessage am in msgList)
                {
                    sb.Append(am.Title);
                    sb.Append(am.Message);
                    sb.Append('\n');
                }
                notifyIcon.ShowBalloonTip(1, hasError ? "Error" : "Info", sb.ToString(), hasError ? ToolTipIcon.Error : ToolTipIcon.Info);
            }
            msgList.Clear();
        }

        public void ShowMessage(string msg, string title)
        {
            lock (msgStack)
            {
                msgStack.Push(new AppMessage(title, msg));
            }
        }

        public void ShowError(string msg, string title)
        {
            lock (errorStack)
            {
                errorStack.Push(new AppMessage(title, msg));
            }
        }

        void notifyIcon_MouseClick(object sender, MouseEventArgs e)
        {
            MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
            mi.Invoke(notifyIcon, null);
        }

        void notifyIcon_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                notifyIcon.ContextMenuStrip.Hide();
                App.BeginClip();
            }
        }


        //TimeSpan timeSpan;
        //DateTime lastClickTime;
        //void notifyIcon_MouseUp(object sender, MouseEventArgs e)
        //{
        //    timeSpan = DateTime.Now - lastClickTime;
        //    lastClickTime = DateTime.Now;
        //    if (timeSpan.TotalMilliseconds > Convert.ToDouble(SystemInformation.DoubleClickTime)
        //        && e.Button == MouseButtons.Left)
        //    {
        //        MethodInfo mi = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
        //        mi.Invoke(notifyIcon, null);
        //    }
        //}

        class AppMessage
        {
            public string Title, Message;
            public AppMessage(string title, string message)
            {
                this.Title = title == null ? String.Empty : title;
                this.Message = message == null ? String.Empty : message;
            }
        }

        class TrayMenu : ContextMenuStrip
        {
            public TrayMenu()
            {
                ToolStripMenuItem item;
                ToolStripSeparator sep;

                item = new ToolStripMenuItem();
                item.Text = "Clip";
                item.Click += clip_Click;
                //item.Image = Resources.Explorer;
                this.Items.Add(item);

                item = new ToolStripMenuItem();
                item.Text = "Settings";
                item.Click += setting_Click;
                //item.Image = Resources.Explorer;
                this.Items.Add(item);

                listenItem.Click += listen_Click;
                this.Items.Add(listenItem);

                sep = new ToolStripSeparator();
                this.Items.Add(sep);

                item = new ToolStripMenuItem();
                item.Text = "Exit";
                item.Click += exit_Click;
                //item.Image = Resources.Exit;
                this.Items.Add(item);

                this.changeListenState();
            }
            
            ToolStripMenuItem listenItem = new ToolStripMenuItem("Start Listening Hot Key");
            bool fListening = false;


            void listen_Click(object sender, EventArgs e)
            {
                this.changeListenState();
            }

            void changeListenState()
            {
                if (fListening)
                {
                    if (HotKeyManager.ReleaseHook())
                    {
                        fListening = false;
                        this.listenItem.Text = "Start Listening Hot Key";
                    }
                    else
                    {
                        MessageBox.Show("Failed to stop listening hot key");
                    }
                }
                else
                {
                    if (HotKeyManager.StartHook())
                    {
                        fListening = true;
                        this.listenItem.Text = "Stop Listening Hot Key";
                    }
                    else
                    {
                        MessageBox.Show("Failed to start listening hot key");
                    }
                }
            }

            void clip_Click(object sender, EventArgs e)
            {
                App.BeginClip();
            }

            void setting_Click(object sender, EventArgs e)
            {
                SettingWindow settingWindow = new SettingWindow();
                settingWindow.Show();
            }

            void exit_Click(object sender, EventArgs e)
            {
                App.Current.Shutdown();
            }
        }
    }
}
