using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.IO;
using NetDimension.Weibo;

namespace Clipoff.Utility
{
    public partial class WeiboLoginDialog : Window
    {
        public WeiboLoginDialog(NetDimension.Weibo.OAuth oauth)
        {
            this.oauth = oauth;
            InitializeComponent();
        }

        NetDimension.Weibo.OAuth oauth;


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.oauth != null)
            {
                webBrowser.Navigate(new Uri(oauth.GetAuthorizeURL()));
            }
        }

        private void webBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.Uri.AbsoluteUri.StartsWith(oauth.CallbackUrl))
            {
                string code = "";
                if (this.parseCode(e.Uri.AbsoluteUri, out code)
                    && oauth.GetAccessTokenByAuthorizationCode(code) != null)
                {
                    this.DialogResult = true;
                    this.Close();
                }
            }
        }

        private bool parseCode(string url, out string code)
        {
            string key = "code=";
            int index = -1;
            if ((index = url.IndexOf(key)) > 0
                && (index = index + key.Length) < url.Length)
            {
                code = url.Substring(index);
                return true;
            }
            code = "";
            return false;
        }
    }
}
