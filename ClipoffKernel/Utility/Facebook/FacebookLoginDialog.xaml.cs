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
using Facebook;
//using System.Dynamic;

namespace Clipoff.Utility
{
    /// <summary>
    /// Interaction logic for FacebookLoginDialog.xaml
    /// </summary>
    public partial class FacebookLoginDialog : Window
    {
        private readonly Uri _loginUrl;
        protected FacebookClient _fb;
        System.Windows.Forms.WebBrowser shadowBrowser;

        public FacebookOAuthResult FacebookOAuthResult { get; private set; }

        public FacebookLoginDialog(string appId, string extendedPermissions)
            : this(new FacebookClient(), appId, extendedPermissions)
        {
        }

        public FacebookLoginDialog(FacebookClient fb, string appId, string extendedPermissions)
        {
            if (fb == null)
                throw new ArgumentNullException("fb");
            if (string.IsNullOrEmpty(appId))
                throw new ArgumentNullException("appId");

            _fb = fb;
            _loginUrl = GenerateLoginUrl(appId, extendedPermissions);

            InitializeComponent();
        }

        private Uri GenerateLoginUrl(string appId, string extendedPermissions)
        {
            //the .net 3.5 way 
            var param = new Dictionary<String, Object>();
            param.Add("client_id", appId);
            param.Add("redirect_uri", "https://www.facebook.com/connect/login_success.html");
            param.Add("response_type", "token");
            param.Add("display", "popup");
            if (!string.IsNullOrEmpty(extendedPermissions))
            {
                param.Add("scope", extendedPermissions);
            }
            return _fb.GetLoginUrl(param);

            ////the .net 4.0 way
            //dynamic parameters = new ExpandoObject();
            //parameters.client_id = appId;
            //parameters.redirect_uri = "https://www.facebook.com/connect/login_success.html";

            //// The requested response: an access token (token), an authorization code (code), or both (code token).
            //parameters.response_type = "token";

            //// list of additional display modes can be found at http://developers.facebook.com/docs/reference/dialogs/#display
            //parameters.display = "popup";

            //// add the 'scope' parameter only if we have extendedPermissions.
            //if (!string.IsNullOrWhiteSpace(extendedPermissions))
            //    parameters.scope = extendedPermissions;

            //// when the Form is loaded navigate to the login url.
            //return _fb.GetLoginUrl(parameters);
        }


        private void webBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            FacebookOAuthResult oauthResult;
            if (_fb.TryParseOAuthCallbackUrl(e.Uri, out oauthResult))
            {
                FacebookOAuthResult = oauthResult;
                this.Close();
            }
            else if (e.Uri.AbsoluteUri.StartsWith("https://www.facebook.com/connect/login_success.html"))
            {
                // the last try on XP
                shadowBrowser = new System.Windows.Forms.WebBrowser();
                shadowBrowser.Navigated += shadowBrowser_Navigated;
                shadowBrowser.Navigate(_loginUrl);
            }
            else
            {
                FacebookOAuthResult = null;
            }
            
        }

        void shadowBrowser_Navigated(object sender, System.Windows.Forms.WebBrowserNavigatedEventArgs e)
        {
            FacebookOAuthResult oauthResult;
            if (_fb.TryParseOAuthCallbackUrl(shadowBrowser.Url, out oauthResult))
            {
                FacebookOAuthResult = oauthResult;
                this.Close();
            }
            else
            {
                FacebookOAuthResult = null;
            }
        }

        private void FacebookLoginDialog_Loaded(object sender, RoutedEventArgs e)
        {
            //.net 3.5
            webBrowser.Navigate(_loginUrl);

            ////.net 4.0
            //webBrowser.Navigate(_loginUrl.AbsoluteUri);
        }
    }
}
