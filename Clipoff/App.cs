using System;
using System.Text;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Drawing;

namespace Clipoff
{
    public class App : ApplicationContext
    {
        System.Windows.Forms.NotifyIcon notifyIcon = new NotifyIcon();
        public static string KERNEL_PATH = Application.StartupPath + "\\ClipoffKernel.exe";
        public static App Instance { get; private set; }

        public App()
        {
            //AppDomain.CurrentDomain.SetData("APP_CONFIG_FILE", "App.config");

            notifyIcon.Icon = new System.Drawing.Icon(GetType(), "icon.ico");
            //notifyIcon.Icon = TaskTrayApplication.Properties.Resources.AppIcon;
            notifyIcon.Text = "Double Click to Clip Screen";
            notifyIcon.Visible = true;
            notifyIcon.ContextMenuStrip = new TrayMenu();
            notifyIcon.MouseClick += notifyIcon_MouseClick;
            notifyIcon.MouseDoubleClick += notifyIcon_MouseDoubleClick;

            Instance = this;
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
                Clip();
            }
        }


        public void Clip()
        {
            (new Thread(() =>
            {
                String fileName = DateTime.Now.ToFileTimeUtc().ToString();
                Process p = Process.Start(KERNEL_PATH, String.Format("c {0}", fileName));
                p.WaitForExit();
                if (File.Exists(fileName))
                {
                    string title = "", message = "";
                    try
                    {
                        using (StreamReader sr = new StreamReader(fileName))
                        {
                            title = sr.ReadLine();
                            if (!sr.EndOfStream)
                            {
                                message = sr.ReadToEnd();
                            }
                        }
                    }
                    catch { }
                    if (!String.IsNullOrEmpty(title) || !String.IsNullOrEmpty(message))
                    {
                        notifyIcon.ShowBalloonTip(1, title, message, ToolTipIcon.Info);
                    }
                    try
                    {
                        File.Delete(fileName);
                    }
                    catch { }
                }
            })).Start();
        }

        public void Setting()
        {
            (new Thread(() =>
            {
                Process p = Process.Start(KERNEL_PATH, String.Format("s {0}", Application.ExecutablePath));
                p.WaitForExit();
                Config.InitializeConfig();
            })).Start();
        }




        class TrayMenu : ContextMenuStrip
        {
            public TrayMenu()
            {
                //this.BackColor = Color.LightCyan;
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

                if (Config.UseHotkey)
                {
                    this.changeListenState();
                }
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
                App.Instance.Clip();
            }

            void setting_Click(object sender, EventArgs e)
            {
                App.Instance.Setting();
            }

            void exit_Click(object sender, EventArgs e)
            {
                Application.Exit();
            }

        }
    }
}
