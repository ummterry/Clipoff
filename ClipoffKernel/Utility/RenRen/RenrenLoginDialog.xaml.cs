using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Clipoff.Utility
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class RenrenLoginDialog : Window
    {
        public RenrenLoginDialog(RenrenClient rrClient)
        {
            this.rrClient = rrClient;
            InitializeComponent();
        }
        RenrenClient rrClient;
        bool fAuthorized = false;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //this.webBrowser.Navigate(new Uri(rrClient.LoginUri));
            this.webBrowser.Navigate(new Uri(rrClient.AuthorizationUri));
        }

        private void webBrowser_Navigated(object sender, NavigationEventArgs e)
        {
            //if (rrClient.TryParseCallbackUri(e.Uri.AbsoluteUri))
            //{
            //    this.DialogResult = true;
            //    this.Close();
            //}
            if (!this.fAuthorized && rrClient.TryParseAuthorizationCode(e.Uri.AbsoluteUri))
            {
                //this.fAuthorized = true;
                //this.webBrowser.Navigate(new Uri(rrClient.AccessTokenUri));
                this.DialogResult = true;
                this.Close();
            }
            //if (this.fAuthorized && rrClient.TryParseCallbackUri(e.Uri.AbsoluteUri))
            //{
            //    this.DialogResult = true;
            //    this.Close();
            //}
        }


    }
}
