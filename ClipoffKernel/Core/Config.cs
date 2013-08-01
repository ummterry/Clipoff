using System;
using System.Text;
using System.Configuration;
using System.IO;
using System.Windows;
using System.Security.Cryptography;
using Microsoft.Win32;

namespace Clipoff
{
    public class Config
    {
        //static Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
        static Configuration config;
        static bool initializeConfig()
        {
            try
            {
                config = ConfigurationManager.OpenExeConfiguration(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
            }
            catch { }
            return config != null;
        }

        static string facebookAccessToken;
        public static string FacebookAccessToken
        {
            get
            {
                return String.IsNullOrEmpty(facebookAccessToken) ? (facebookAccessToken = Decrypt(Get("FacebookAccessToken"))) : facebookAccessToken;
            }
            set
            {
                Set("FacebookAccessToken", Encrypt((facebookAccessToken = value)));
            }
        }

        static string twitterAccessToken;
        public static string TwitterAccessToken
        {
            get
            {
                return String.IsNullOrEmpty(twitterAccessToken) ? (twitterAccessToken = Decrypt(Get("TwitterAccessToken"))) : twitterAccessToken;
            }
            set
            {
                Set("TwitterAccessToken", Encrypt((twitterAccessToken = value)));
            }
        }
        static string twitterTokenSecret;
        public static string TwitterTokenSecret
        {
            get
            {
                return String.IsNullOrEmpty(twitterTokenSecret) ? (twitterTokenSecret = Decrypt(Get("TwitterTokenSecret"))) : twitterTokenSecret;
            }
            set
            {
                Set("TwitterTokenSecret", Encrypt((twitterAccessToken = value)));
            }
        }

        static string renrenAccessToken;
        public static string RenrenAccessToken
        {
            get
            {
                return String.IsNullOrEmpty(renrenAccessToken) ? (renrenAccessToken = Decrypt(Get("RenrenAccessToken"))) : renrenAccessToken;
            }
            set
            {
                Set("RenrenAccessToken", Encrypt((renrenAccessToken = value)));
            }
        }
        static string renrenRefreshToken;
        public static string RenrenRefreshToken
        {
            get
            {
                return String.IsNullOrEmpty(renrenRefreshToken) ? (renrenRefreshToken = Decrypt(Get("RenrenRefreshToken"))) : renrenRefreshToken;
            }
            set
            {
                Set("RenrenRefreshToken", Encrypt((renrenRefreshToken = value)));
            }
        }

        static string weiboAccessToken;
        public static string WeiboAccessToken
        {
            get
            {
                return String.IsNullOrEmpty(weiboAccessToken) ? (weiboAccessToken = Decrypt(Get("WeiboAccessToken"))) : weiboAccessToken;
            }
            set
            {
                Set("WeiboAccessToken", Encrypt((weiboAccessToken = value)));
            }
        }

        static string picLibPath;
        public static string ImagePath
        {
            get
            {
                if (string.IsNullOrEmpty(picLibPath)
                    && (!Directory.Exists(picLibPath))
                    && string.IsNullOrEmpty(picLibPath = ConfigurationManager.AppSettings["PictureLibraryPath"])
                    && (!Directory.Exists(picLibPath)))
                {
                    picLibPath = String.Format("{0}\\{1}", Environment.GetFolderPath(Environment.SpecialFolder.MyPictures), "Clipoff Pictures");
                    try
                    {
                        Directory.CreateDirectory(picLibPath);
                        Set("PictureLibraryPath", picLibPath);
                    }
                    catch (Exception)
                    {
                        picLibPath = "Clipoff Pictures";
                        Directory.CreateDirectory(picLibPath);
                        Set("PictureLibraryPath", picLibPath);
                    }
                    finally
                    {
                        if (string.IsNullOrEmpty(picLibPath) || (!Directory.Exists(picLibPath)))
                        {
                            MessageBox.Show(
                                "Failed to initialize picture library, please run the program as administrator and try again.",
                                "Initialization Failed",
                                MessageBoxButton.OK,
                                MessageBoxImage.Error
                                );
                            App.Current.Shutdown();
                        }
                    }
                }
                return String.Format("{0}\\{1}.png", picLibPath, System.DateTime.Now.ToString().Replace('/', '-').Replace(':', '-'));
            }
        }

        public static bool UseHotkey
        {
            get
            {
                return (Get("UseHotkey") == "True");
            }
            set
            {
                Set("UseHotkey", value ? "True" : "False");
            }
        }

        const char DEFAULT_HOTKEY = 'A';
        static char hotKey;
        public static char HotKey
        {
            //always return a digit or uppercase letter
            get
            {
                if (hotKey == '\0')
                {
                    string key = Get("Hotkey");
                    hotKey = (key.Length > 0 && char.IsLetterOrDigit(key[0])) ? key[0] : DEFAULT_HOTKEY;
                }
                return char.IsLower(hotKey) ? (hotKey = char.ToUpper(hotKey)) : hotKey;
            }
            set
            {
                if (char.IsLetterOrDigit(value))
                {
                    hotKey = value;
                }
                Set("Hotkey", value);
            }
        }

        public static bool AutoRun
        {
            get
            {
                try
                {
                    using (RegistryKey rkApp = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                    {
                        return (rkApp.GetValue(KEY) != null);
                    }
                }
                catch
                {
                    return false;
                }
            }
            set
            {
                try
                {
                    using (RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
                    {
                        if (value)
                        {
                            rk.SetValue(KEY, App.Instance.ClipoffPath);
                        }
                        else
                        {
                            rk.DeleteValue(KEY, false);
                        }
                    }
                }
                catch { }
            }
        }

        public static bool UseFacebook
        {
            get
            {
                return (Get("UseFacebook") == "True");
            }
            set
            {
                Set("UseFacebook", value ? "True" : "False");
            }
        }
        public static bool UseTwitter
        {
            get
            {
                return (Get("UseTwitter") == "True");
            }
            set
            {
                Set("UseTwitter", value ? "True" : "False");
            }
        }
        public static bool UseWeibo
        {
            get
            {
                return (Get("UseWeibo") == "True");
            }
            set
            {
                Set("UseWeibo", value ? "True" : "False");
            }
        }
        public static bool UseRenren
        {
            get
            {
                return (Get("UseRenren") == "True");
            }
            set
            {
                Set("UseRenren", value ? "True" : "False");
            }
        }

        //never return null; return empty string instead
        static string Get(string key)
        {
            if (config == null && !initializeConfig())
            {
                return "";
            }
            return config.AppSettings.Settings[key] == null ? "" : config.AppSettings.Settings[key].Value;
        }

        //accept any type of value but save value.ToString(). If value is null, save empty string.
        static void Set(string key, object value = null)
        {
            if (config == null && !initializeConfig())
            {
                return;
            }
            value = (value == null) ? "" : value;
            if (config.AppSettings.Settings[key] == null)
            {
                config.AppSettings.Settings.Add(key, value.ToString());
            }
            else
            {
                config.AppSettings.Settings[key].Value = value.ToString();
            }
            try
            {
                config.Save(ConfigurationSaveMode.Modified);
            }
            catch { }
        }

        #region --- encryption ---
        static byte[] bytes = ASCIIEncoding.ASCII.GetBytes("798nhj#k");

        static string Encrypt(string originalString)
        {
            if (String.IsNullOrEmpty(originalString))
            {
                return "";
            }
            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            MemoryStream memoryStream = new MemoryStream();
            CryptoStream cryptoStream = new CryptoStream(memoryStream,
                cryptoProvider.CreateEncryptor(bytes, bytes), CryptoStreamMode.Write);
            StreamWriter writer = new StreamWriter(cryptoStream);
            writer.Write(originalString);
            writer.Flush();
            cryptoStream.FlushFinalBlock();
            writer.Flush();
            return Convert.ToBase64String(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
        }

        static string Decrypt(string cryptedString)
        {
            if (String.IsNullOrEmpty(cryptedString))
            {
                return "";
            }
            DESCryptoServiceProvider cryptoProvider = new DESCryptoServiceProvider();
            MemoryStream memoryStream = new MemoryStream
                    (Convert.FromBase64String(cryptedString));
            CryptoStream cryptoStream = new CryptoStream(memoryStream,
                cryptoProvider.CreateDecryptor(bytes, bytes), CryptoStreamMode.Read);
            StreamReader reader = new StreamReader(cryptoStream);
            return reader.ReadToEnd();
        }
        #endregion

        public const string FACEBOOK_SUCCEED = "Picture Uploaded to Facebook";
        public const string FACEBOOK_ERROR = "Failed to Upload to Facebook";
        public const string FACEBOOK_TITLE = "Message From Facebook: ";
        public const string TWITTER_SUCCEED = "Picture Uploaded to Twitter";
        public const string TWITTER_ERROR = "Failed to Upload to Twitter";
        public const string TWITTER_TITLE = "Message From Twitter: ";
        public const string WEIBO_SUCCEED = "成功上传至微博";
        public const string WEIBO_ERROR = "上传至微博失败";
        public const string WEIBO_TITLE = "来自微博的消息: ";
        public const string RENREN_SUCCEED = "成功上传至人人网";
        public const string RENREN_ERROR = "上传至人人网失败";
        public const string RENREN_TITLE = "来自人人网的消息: ";
        public const string RENREN_PHOTO_ERROR = "无效的照片格式,照片的宽和高不能小于50,照片的宽与高的比不能小于1:3";
        public const string AUTHORIZATION_ERROR_EN = "Authorization Failed";
        public const string AUTHORIZATION_ERROR_CN = "认证失败";
        public const string SAVE_ERROR = "Failed to Save Picture. Operation Canceled";
        public const string DEFAULT_MESSAGE_CN = "通过Clipoff上传了一张照片";
        public const string DEFAULT_MESSAGE_EN = "Posted from Clipoff";


        const string KEY = "Clipoff";
    }
}
