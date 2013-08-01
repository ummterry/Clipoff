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
using TweetSharp;
using TweetSharp.Model;
//using LinqToTwitter;

namespace Clipoff.Utility
{
    ////LinqToTwitter
    //public partial class TwitterLoginDialog : Window
    //{
    //    public TwitterLoginDialog(string filePath)
    //    {
    //        this.filePath = filePath;
    //        InitializeComponent();
    //    }
    //    const string ConsumerKey = "n7FWAsu59mGsKsQMEXequg";
    //    const string ConsumerSecret = "qIOfWd4UxMBdqMRpiXcGkgXgbY5C3l5O0bRrIqVpYw";
    //    PinAuthorizer auth;
    //    string filePath;


    //    public void StartAuthorization(String authUri)
    //    {
    //        this.Show();
    //        webBrowser.Navigate(new Uri(authUri));
    //    }


    //    private void Window_Loaded(object sender, RoutedEventArgs e)
    //    {
    //        auth = new PinAuthorizer
    //        {
    //            Credentials = new InMemoryCredentials
    //            {
    //                ConsumerKey = ConsumerKey,
    //                ConsumerSecret = ConsumerSecret
    //            },
    //            UseCompression = true,
    //            GoToTwitterAuthorization = pageLink => Dispatcher.BeginInvoke((Action)(() =>
    //            {
    //                webBrowser.Navigate(new Uri(pageLink));
    //            })),
    //            //GoToTwitterAuthorization = pageLink => webBrowser.Navigate(new Uri(pageLink)),
    //            //GetPin = () => 
    //            //    {
    //            //        return this.txtPin.Text.Trim();
    //            //    }
    //        };
    //        auth.BeginAuthorize(resp => Dispatcher.BeginInvoke((Action)(() =>
    //        {
    //            switch (resp.Status)
    //            {
    //                case TwitterErrorStatus.Success:
    //                    break;
    //                case TwitterErrorStatus.TwitterApiError:
    //                case TwitterErrorStatus.RequestProcessingException:
    //                    MessageBox.Show(resp.Error.ToString(), resp.Message, MessageBoxButton.OK);
    //                    break;
    //            }
    //        })));
    //    }

    //    private void btnOK_Click(object sender, RoutedEventArgs e)
    //    {
    //        auth.CompleteAuthorize(
    //            this.txtPin.Text,
    //            completeResp => Dispatcher.BeginInvoke((Action)(() =>
    //            {
    //                switch (completeResp.Status)
    //                {
    //                    case TwitterErrorStatus.Success:
    //                        String accessToken = auth.Credentials.OAuthToken;
    //                        String accessTokenSecret = auth.Credentials.AccessToken;
    //                        this.Post();
    //                        break;
    //                    case TwitterErrorStatus.TwitterApiError:
    //                    case TwitterErrorStatus.RequestProcessingException:
    //                        MessageBox.Show(
    //                            completeResp.Error.ToString(),
    //                            completeResp.Message,
    //                            MessageBoxButton.OK);
    //                        break;
    //                }
    //            })));

    //        this.Close();
    //    }

    //    void Post()
    //    {
    //        try
    //        {
    //            //auth.Authorize();
    //            var mediaItems =
    //            new List<Media>
    //                {
    //                    new Media
    //                    {
    //                        Data = Utilities.GetFileBytes(filePath),
    //                        FileName = (new FileInfo(filePath)).Name,
    //                        ContentType = MediaContentType.Png
    //                    }
    //                };
    //            TwitterContext twitterCtx = new TwitterContext(auth);
    //            Status tweetStatus = twitterCtx.TweetWithMedia("Posted from Clipoff", false, mediaItems);
    //            MessageBox.Show("Picture Uploaded Successfully");
    //        }
    //        catch (Exception)
    //        {
    //            MessageBox.Show("Failed to Upload");
    //        }
    //        finally
    //        {
                
    //        }
    //    }

    //}

    //TweetSharp methods
    
    public partial class TwitterLoginDialog : Window
    {
        public TwitterLoginDialog(TwitterService twitterService)
        {
            this.twitterService = twitterService;
            InitializeComponent();
        }

        TwitterService twitterService;
        OAuthRequestToken requestToken;
        public OAuthAccessToken accessToken;


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            requestToken = twitterService.GetRequestToken();
            Uri authUri = twitterService.GetAuthorizationUri(requestToken);
            webBrowser.Navigate(authUri);
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            accessToken = twitterService.GetAccessToken(requestToken, this.txtPin.Text.Trim());
            this.DialogResult = true;
            this.Close();
        }
    }
}
