using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NetDimension.Weibo;
using System.Windows;
using System.Threading;

namespace Clipoff.Utility
{
    class WeiboUtility : PostBase
    {
        const string AppKey = "624241061";
        const string AppSecret = "e2062d12819679c8d790bf9ca6fad3fd";
        NetDimension.Weibo.Client sinaClient;
        NetDimension.Weibo.OAuth oauth;

        bool fRefreshed = false;
        string accessToken;

        public WeiboUtility(string filePath, string message)
            : base(filePath, String.IsNullOrEmpty(message) ? Config.DEFAULT_MESSAGE_CN : message) { }

        public override void Post()
        {
            if (!String.IsNullOrEmpty(this.accessToken = Config.WeiboAccessToken))
            {
                oauth = new OAuth(AppKey, AppSecret, accessToken, null);
                sinaClient = new Client(oauth);
                PostWeibo();
            }
            else
            {
                AuthAndPost();
            }
        }


        void AuthAndPost()
        {
            fRefreshed = true;
            oauth = new OAuth(AppKey, AppSecret, "https://api.weibo.com/oauth2/default.html");
            WeiboLoginDialog loginDialog = new WeiboLoginDialog(oauth);
            System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(loginDialog);
            if (loginDialog.ShowDialog() == true)
            {
                SaveAndPost();
            }
            else
            {
                App.Instance.AddError(Config.AUTHORIZATION_ERROR_CN, Config.WEIBO_TITLE);
            }

        }

        void SaveAndPost()
        {
            Config.WeiboAccessToken = this.accessToken = oauth.AccessToken;
            sinaClient = new Client(oauth);
            PostWeibo();
        }

        //void RefreshAndPost()
        //{
        //    fRefreshed = true;
        //    try
        //    {
        //        NetDimension.Weibo.AccessToken at = oauth.GetAccessTokenByRefreshToken(this.refreshToken);
        //        if (at != null && !String.IsNullOrEmpty(at.Token))
        //        {
        //            oauth = new OAuth(AppKey, AppSecret, at.Token);
        //            if (oauth.VerifierAccessToken() == TokenResult.Success)
        //            {
        //                Config.WeiboAccessToken = this.accessToken = oauth.AccessToken;
        //                sinaClient = new Client(oauth);
        //                PostWeibo();
        //                return;
        //            }
        //        }
        //    }
        //    catch { }
        //    App.Instance.AddError(Config.AUTHORIZATION_ERROR_EN);
        //}

        void PostWeibo()
        {
            byte[] buf = File.ReadAllBytes(filePath);
            try
            {
                sinaClient.API.Statuses.Upload(message, buf, 0, 0, null);
                App.Instance.AddMessage(Config.WEIBO_SUCCEED, Config.WEIBO_TITLE);
            }
            catch (Exception exp)
            {
                //NetDimension.Weibo.TokenResult tokenResult = oauth.VerifierAccessToken();
                //if (tokenResult == TokenResult.Success)
                //{
                //    if (!fRetried)
                //    {
                //        fRetried = true;
                //        PostWeibo();
                //        return;
                //    }
                //}
                //else if (!fRefreshed)
                if (!fRefreshed)
                {
                    //if (tokenResult == TokenResult.TokenExpired)
                    //{
                    //    RefreshAndPost();
                    //}
                    AuthAndPost();
                }
                else
                {
                    App.Instance.AddError(exp.Message, Config.WEIBO_TITLE);
                }
            }
        }
    }
}
