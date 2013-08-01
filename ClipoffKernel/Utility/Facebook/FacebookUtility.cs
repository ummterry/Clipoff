using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.IO;
//using System.Dynamic;
using Facebook;
using System.Threading;

namespace Clipoff.Utility
{
    class FacebookUtility : PostBase
    {
        private const string AppId = "306484852796084";
        private const string ExtendedPermissions = "publish_actions";
        private string accessToken;

        public FacebookUtility(string filePath, string message)
            : base(filePath, String.IsNullOrEmpty(message) ? Config.DEFAULT_MESSAGE_EN : message) { }

        public override void Post()
        {
            if (String.IsNullOrEmpty(this.accessToken = Config.FacebookAccessToken))
            {
                AuthAndUpload(filePath);
            }
            else
            {
                Upload(true);
            }
        }

        void Upload(bool fUsingSavedAccessToken)
        {
            FacebookClient fb = new FacebookClient(this.accessToken);

            // make sure to add event handler for PostCompleted.
            fb.PostCompleted += (o, e) =>
            {
                if (e.Error != null)
                {
                    if (fUsingSavedAccessToken && e.Error is Facebook.FacebookOAuthException)
                    {
                        //the access token is invalid (maybe expired)
                        //Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                        //    {
                        //        Config.FacebookAccessToken = null;
                        //        this.AuthAndUpload(filePath);
                        //    }
                        //));
                        App.Instance.BeginInvoke(new Action(() =>
                            {
                                Config.FacebookAccessToken = null;
                                this.AuthAndUpload(filePath);
                            }
                        ));
                    }
                    else
                    {
                        App.Instance.AddError(Config.FACEBOOK_ERROR, Config.FACEBOOK_TITLE);
                    }
                }
                else
                {
                    App.Instance.AddMessage(Config.FACEBOOK_SUCCEED, Config.FACEBOOK_TITLE);
                }
            };

            var photoDetail = new Dictionary<string, object>();
            photoDetail.Add("message", message);
            photoDetail.Add("source", new FacebookMediaObject
            {
                ContentType = "image/png",
                FileName = filePath
            }.SetValue(File.ReadAllBytes(filePath)));
            fb.PostAsync("me/photos", photoDetail);
            //dynamic parameters = new ExpandoObject();
            //parameters.message = "Posted from Clipoff";
            //parameters.source = new FacebookMediaObject
            //{
            //    ContentType = "image/png",
            //    FileName = filePath
            //}.SetValue(File.ReadAllBytes(filePath));

            //fb.PostAsync("me/photos", parameters);
        }

        bool Auth()
        {
            FacebookLoginDialog fbLoginDialog = new FacebookLoginDialog(AppId, ExtendedPermissions);
            System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(fbLoginDialog);
            fbLoginDialog.ShowDialog();
            if (fbLoginDialog.FacebookOAuthResult == null)
            {
                //The user closed the login dailog manually
                return false;
            }
            else if (fbLoginDialog.FacebookOAuthResult.IsSuccess)
            {
                Config.FacebookAccessToken = this.accessToken = fbLoginDialog.FacebookOAuthResult.AccessToken;
                return true;
            }
            return false;
        }

        void AuthAndUpload(string filePath)
        {
            bool flag = this.Auth();
            if (flag)
            {
                Upload(false);
            }
            else
            {
                //authorization failed
                App.Instance.AddError(Config.AUTHORIZATION_ERROR_EN, Config.FACEBOOK_TITLE);
            }
        }

       
    }
}
