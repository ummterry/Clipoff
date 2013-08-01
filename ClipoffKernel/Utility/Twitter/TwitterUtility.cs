using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using TweetSharp;
using TweetSharp.Model;
using Hammock;
using Hammock.Authentication.OAuth;

//using LinqToTwitter;

namespace Clipoff.Utility
{
    ////LinqToTwitter Method
    //class TwitterUtility
    //{
    //    TwitterLoginDialog loginDialog;
    //    public bool Post(string filePath)
    //    {
    //        loginDialog = new TwitterLoginDialog(filePath);
    //        loginDialog.ShowDialog();


    //        return true;
    //    }
    //}

    //TweetSharp methods

    class TwitterUtility : PostBase
    {
        const string ConsumerKey = "n7FWAsu59mGsKsQMEXequg";
        const string ConsumerSecret = "qIOfWd4UxMBdqMRpiXcGkgXgbY5C3l5O0bRrIqVpYw";
        string token, tokenSecret;
        bool fUsingSavedToken = true;

        public TwitterUtility(string filePath, string message)
            : base(filePath, String.IsNullOrEmpty(message) ? Config.DEFAULT_MESSAGE_EN : message) { }

        public override void Post()
        {
            if ((String.IsNullOrEmpty(token = Config.TwitterAccessToken)
                || String.IsNullOrEmpty(tokenSecret = Config.TwitterTokenSecret))
                && !this.Auth())
            {
                App.Instance.AddError(Config.AUTHORIZATION_ERROR_EN, Config.TWITTER_TITLE);
                return;
            }
            else
            {
                //Authorize with the saved access token and secret
                TwitterClientInfo twitterClientInfo = new TwitterClientInfo();
                twitterClientInfo.ConsumerKey = ConsumerKey;
                twitterClientInfo.ConsumerSecret = ConsumerSecret;
                TwitterService twitterService = new TwitterService(twitterClientInfo);
                //twitterService.AuthenticateWith(token, tokenSecret);
            }

            try
            {
                this.PostTweet();
            }
            catch (Exception)
            {
                App.Instance.AddError(Config.TWITTER_ERROR, Config.TWITTER_TITLE);
            }
        }

        bool Auth()
        {
            this.fUsingSavedToken = false;
            TwitterClientInfo twitterClientInfo = new TwitterClientInfo();
            twitterClientInfo.ConsumerKey = ConsumerKey;
            twitterClientInfo.ConsumerSecret = ConsumerSecret;
            TwitterService twitterService = new TwitterService(twitterClientInfo);
            TwitterLoginDialog loginDialog = new TwitterLoginDialog(twitterService);
            System.Windows.Forms.Integration.ElementHost.EnableModelessKeyboardInterop(loginDialog);//fuck..
            if (loginDialog.ShowDialog() == true)
            {
                OAuthAccessToken accessToken = loginDialog.accessToken;
                try
                {
                    twitterService.AuthenticateWith(
                        Config.TwitterAccessToken = this.token = accessToken.Token,
                        Config.TwitterTokenSecret = this.tokenSecret = accessToken.TokenSecret);
                    return true;
                }
                catch
                {
                }
            }
            return false;
        }
        void PostTweet()
        {
            var credentials = new OAuthCredentials
            {
                Type = OAuthType.ProtectedResource,
                SignatureMethod = OAuthSignatureMethod.HmacSha1,
                ParameterHandling = OAuthParameterHandling.HttpAuthorizationHeader,
                ConsumerKey = ConsumerKey,
                ConsumerSecret = ConsumerSecret,
                Token = token,
                TokenSecret = tokenSecret,
                Version = "1.0a"
            };


            RestClient restClient = new RestClient
            {
                Authority = "https://upload.twitter.com",
                //HasElevatedPermissions = true,
                Credentials = credentials,
                Method = Hammock.Web.WebMethod.Post
            };
            RestRequest restRequest = new RestRequest
            {
                Path = "1/statuses/update_with_media.json"
            };

            restRequest.AddParameter("status", message);
            restRequest.AddFile("media[]", (new FileInfo(filePath)).Name, filePath, "image/png");

            restClient.BeginRequest(restRequest, new RestCallback(PostTweetRequestCallback));
        }


        void PostTweetRequestCallback(RestRequest request, Hammock.RestResponse response, object obj)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                if (fUsingSavedToken)
                {
                    App.Instance.BeginInvoke(new Action(() =>
                        {
                            Config.TwitterAccessToken = null;
                            Config.TwitterTokenSecret = null;
                            if (this.Auth())
                            {
                                this.PostTweet();
                            }
                            else
                            {
                                App.Instance.AddError(Config.AUTHORIZATION_ERROR_EN, Config.TWITTER_TITLE);
                            }
                        }
                    ));
                    return;
                }
                App.Instance.AddError(Config.AUTHORIZATION_ERROR_EN, Config.TWITTER_TITLE);
            }
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                App.Instance.AddMessage(Config.TWITTER_SUCCEED, Config.TWITTER_TITLE);
            }
            else
            {
                App.Instance.AddError(Config.TWITTER_ERROR, Config.TWITTER_TITLE);
            }
        }
    }

}