using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Clipoff.Utility
{
    class RenrenUtility : PostBase
    {
        public RenrenUtility(string filePath, string message)
            : base(filePath, String.IsNullOrEmpty(message) ? Config.DEFAULT_MESSAGE_CN : message) { }

        RenrenClient rrClient;
        const string ClientID = "c3feafc7baca43faa802146c2dd4035a";
        const string ClientSecret = "190493ba61e943cf8784f4c3d6d59ad1";

        public override void Post()
        {
            rrClient = new RenrenClient(ClientID, ClientSecret, Config.RenrenAccessToken, Config.RenrenRefreshToken);
            if (rrClient.PostPhoto(filePath, message))
            {
                App.Instance.AddMessage(Config.RENREN_SUCCEED, Config.RENREN_TITLE);
            }
            else if (rrClient.LastError == RenrenClient.RenrenException.InvalidToken)
            {
                AuthAndUpload();
            }
            else if (rrClient.LastError == RenrenClient.RenrenException.ExpiredToken)
            {
                if (rrClient.RefreshAccessToken())
                {
                    SaveAndUpload();
                }
                else
                {
                    AuthAndUpload();
                }
            }
            else
            {
                this.AddError();
            }
        }

        void AuthAndUpload()
        {
            RenrenLoginDialog loginDialog = new RenrenLoginDialog(rrClient);
            System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(loginDialog);
            if (loginDialog.ShowDialog() == true && rrClient.GetAccessToken())
            {
                SaveAndUpload();
            }
            else
            {
                App.Instance.AddError(Config.AUTHORIZATION_ERROR_CN, Config.RENREN_TITLE);
            }
        }

        void SaveAndUpload()
        {
            Config.RenrenAccessToken = rrClient.AccessToken;
            Config.RenrenRefreshToken = rrClient.RefreshToken;
            if (rrClient.PostPhoto(filePath, message))
            {
                App.Instance.AddMessage(Config.RENREN_SUCCEED, Config.RENREN_TITLE);
            }
            else
            {
                this.AddError();
            }
        }

        void AddError()
        {
            if (rrClient.LastError == RenrenClient.RenrenException.InvalidPhoto)
            {
                App.Instance.AddError(Config.RENREN_PHOTO_ERROR, Config.RENREN_TITLE);
            }
            else if (rrClient.LastError == RenrenClient.RenrenException.InvalidOperation)
            {
                App.Instance.AddError(rrClient.LastErrorMessage, Config.RENREN_TITLE);
            }
            else
            {
                App.Instance.AddError(Config.RENREN_ERROR, Config.RENREN_TITLE);
            }
        }

    }
}
