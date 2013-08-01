using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text;
using System.Configuration;
using System.IO;

namespace Clipoff
{
    public class Config
    {
        //static Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        //static string KERNEL_PATH = Application.StartupPath + "\\ClipoffKernel.exe";
        static Configuration config;
        public static bool InitializeConfig()
        {
            try
            {
                config = ConfigurationManager.OpenExeConfiguration(App.KERNEL_PATH);
            }
            catch { }
            return config != null;
        }

        public static bool UseHotkey
        {
            get
            {
                return (Get("UseHotkey") == "True");
            }
            //set
            //{
            //    Set("UseHotkey", value ? "True" : "False");
            //}
        }

        const char DEFAULT_HOTKEY = 'A';
        public static char HotKey
        {
            //always return a digit or uppercase letter
            get
            {
                string key = Get("Hotkey");
                char hotKey = (key.Length > 0 && char.IsLetterOrDigit(key[0])) ? key[0] : DEFAULT_HOTKEY;
                return char.IsLower(hotKey) ? (hotKey = char.ToUpper(hotKey)) : hotKey;
            }
        }

        public static bool AutoRun
        {
            get
            {
                return (Get("AutoRun") == "True");
            }
            //set
            //{
            //    Set("AutoRun", value ? "True" : "False");
            //}
        }

        //never return null; return empty string instead
        static string Get(string key)
        {
            if (config == null && !InitializeConfig())
            {
                return "";
            }
            return config.AppSettings.Settings[key] == null ? "" : config.AppSettings.Settings[key].Value;
        }

        //accept any type of value but save value.ToString(). If value is null, save empty string.
        //static void Set(string key, object value = null)
        //{
        //    if (config == null && !initializeConfig())
        //    {
        //        return;
        //    }
        //    value = (value == null) ? "" : value;
        //    if (config.AppSettings.Settings[key] == null)
        //    {
        //        config.AppSettings.Settings.Add(key, value.ToString());
        //    }
        //    else
        //    {
        //        config.AppSettings.Settings[key].Value = value.ToString();
        //    }
        //    try
        //    {
        //        config.Save(ConfigurationSaveMode.Modified);
        //    }
        //    catch { }
        //}

    }
}
