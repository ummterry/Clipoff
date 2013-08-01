using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Security.Cryptography;
using System.Net;
using System.Web;
using System.Collections.Specialized;

namespace Clipoff.Utility
{
    public class RenrenClient
    {
        public RenrenClient(string clientID, string clientSecret, string accessToken = "", string refreshToken = "")
        {
            this.ClientID = clientID;
            this.ClientSecret = clientSecret;
            this.AccessToken = accessToken;
            this.RefreshToken = refreshToken;
        }

        public string AuthorizationUri
        {
            get
            {
                return String.Format("https://graph.renren.com/oauth/authorize?response_type=code&client_id={0}&redirect_uri=http://graph.renren.com/oauth/login_success.html&scope=photo_upload&display=popup",
                    ClientID);
            }
        }

        public string AccessToken { get; private set; }
        public string RefreshToken { get; private set; }

        public RenrenException LastError { get; private set; }
        public string LastErrorMessage { get; private set; }

        string authorizationCode;
        string ClientID;
        string ClientSecret;

        public bool RefreshAccessToken()
        {
            if (String.IsNullOrEmpty(RefreshToken))
            {
                return false;
            }
            NameValueCollection parameters = new NameValueCollection()
            {
                {"grant_type", "refresh_token"},
                {"refresh_token", RefreshToken},
                {"client_id", ClientID},
                {"client_secret", ClientSecret}
            };
            try
            {
                string responseData = this.HttpPost("https://graph.renren.com/oauth/token", parameters);
                JObject jo = JObject.Parse(responseData);
                if (jo["refresh_token"] != null)
                {
                    RefreshToken = jo["refresh_token"].ToString();
                }
                return (jo["access_token"] != null && !String.IsNullOrEmpty(AccessToken = jo["access_token"].ToString()));
            }
            catch
            {
                return false;
            }
        }

        public bool GetAccessToken()
        {
            if (String.IsNullOrEmpty(authorizationCode))
            {
                return false;
            }
            NameValueCollection parameters = new NameValueCollection()
            {
                {"grant_type", "authorization_code"},
                {"code", authorizationCode},
                {"client_id", ClientID},
                {"client_secret", ClientSecret},
                {"redirect_uri", "http://graph.renren.com/oauth/login_success.html"}
            };
            try
            {
                string responseData = this.HttpPost("https://graph.renren.com/oauth/token", parameters);
                JObject jo = JObject.Parse(responseData);
                if (jo["refresh_token"] != null)
                {
                    RefreshToken = jo["refresh_token"].ToString();
                }
                return (jo["access_token"] != null && !String.IsNullOrEmpty(AccessToken = jo["access_token"].ToString()));
            }
            catch
            {
                return false;
            }
        }

        public bool TryParseAuthorizationCode(string uri)
        {
            if (String.IsNullOrEmpty(uri))
            {
                return false;
            }
            int startIndex = 0, length = 0;
            string sPattern = "?code=";
            if ((startIndex = uri.IndexOf(sPattern)) > 0
                && (length = uri.Length - startIndex - sPattern.Length) > 0)
            {
                this.authorizationCode = HttpUtility.UrlDecode(uri.Substring(startIndex + sPattern.Length, length));
                return true;
            }
            return false;
        }

        //public bool TryParseCallbackUri(string uri)
        //{
        //    if (String.IsNullOrEmpty(uri))
        //    {
        //        return false;
        //    }
        //    int startIndex = 0, endIndex = 0;
        //    string sPattern = "#access_token=";
        //    if ((startIndex = uri.IndexOf(sPattern)) > 0
        //        && (endIndex = uri.IndexOf('&')) > 0
        //        && endIndex > startIndex)
        //    {
        //        this.AccessToken = HttpUtility.UrlDecode(uri.Substring(startIndex + sPattern.Length, endIndex - startIndex - sPattern.Length));

        //        return true;
        //    }
        //    return false;
        //}

        public bool PostPhoto(string filePath, string caption)
        {
            if (String.IsNullOrEmpty(AccessToken))
            {
                this.LastError = RenrenException.InvalidToken;
                return false;
            }
            if (String.IsNullOrEmpty(filePath)
                || !File.Exists(filePath))
            {
                return false;
            }
            caption = caption == null ? "" : caption;

            List<APIParameter> paramList = new List<APIParameter>() 
            {
                new APIParameter("method", "photos.upload"),
                new APIParameter("call_id", DateTime.Now.Millisecond.ToString()),
                new APIParameter("v", "1.0"),
                new APIParameter("access_token", AccessToken),
                new APIParameter("format", "json"),
                new APIParameter("caption", caption)
            };
            string sig = CalSig(paramList);
            if (String.IsNullOrEmpty(sig))
            {
                return false;
            }
            paramList.Add(new APIParameter("sig", sig));

            try
            {
                string responseData = HttpPost("http://api.renren.com/restserver.do", paramList, filePath);
                JObject jo = JObject.Parse(responseData);
                if (jo["pid"] != null && !String.IsNullOrEmpty(jo["pid"].ToString()))
                {
                    return true;
                }
                else
                {
                    this.SetLastError(jo);
                    return false;
                }
            }
            catch
            {
                return false;
            }
        }

        void SetLastError(JObject jo)
        {
            int error_code = 0;
            if (jo == null || jo["error_code"] == null || !int.TryParse(jo["error_code"].ToString(), out error_code))
            {
                return;
            }
            if (error_code == 200 || error_code == 201 || error_code == 202 || error_code == 2001)
            {
                this.LastError = RenrenException.InvalidToken;
            }
            else if (error_code == 2002)
            {
                this.LastError = RenrenException.ExpiredToken;
            }
            else if (error_code == 300 || error_code == 10511)
            {
                this.LastError = RenrenException.InvalidPhoto;
            }
            else if (jo["error_msg"] != null && !String.IsNullOrEmpty(jo["error_msg"].ToString()))
            {
                this.LastError = RenrenException.InvalidOperation;
                this.LastErrorMessage = jo["error_msg"].ToString();
            }
        }

        /// <summary>
        /// post an HTTP request with parameters
        /// </summary>
        /// <param name="url"></param>
        /// <param name="parameterList"></param>
        /// <exception cref="">may throw exception</exception>
        /// <returns>the response (Encoded in UTF8)</returns>
        string HttpPost(string url, NameValueCollection parameterList)
        {
            using (WebClient client = new WebClient())
            {
                byte[] ret = client.UploadValues(url, parameterList);
                if (ret != null) return System.Text.Encoding.UTF8.GetString(ret);
            }
            return "";
        }

        /// <summary>
        /// post an HTTP request with parameters and file
        /// </summary>
        /// <param name="url"></param>
        /// <param name="parameterList"></param>
        /// <param name="filePath"></param>
        /// <exception cref="">may throw exception</exception>
        /// <returns>the response (Encoded in UTF8)</returns>
        string HttpPost(string url, List<APIParameter> parameterList, string filePath)
        {
            string boundary = "SoMeTeXtWeWiLlNeVeRsEe";

            HttpWebRequest webRequest = WebRequest.Create(url) as HttpWebRequest;
            webRequest.ServicePoint.Expect100Continue = false;
            webRequest.Timeout = 300000;
            webRequest.ContentType = "multipart/form-data;charset=utf-8;boundary=" + boundary;
            webRequest.Method = "POST";
            webRequest.KeepAlive = true;
            webRequest.Credentials = CredentialCache.DefaultCredentials;

            Stream memStream = new MemoryStream();

            byte[] beginBoundary = Encoding.UTF8.GetBytes("\r\n--" + boundary + "\r\n");
            byte[] endBoundary = Encoding.UTF8.GetBytes("\r\n--" + boundary + "--\r\n");

            string formdataTemplate = "Content-Disposition: form-data; name=\"{0}\"\r\n\r\n{1}";
            foreach (APIParameter param in parameterList)
            {
                memStream.Write(beginBoundary, 0, beginBoundary.Length);

                byte[] formitembytes = Encoding.UTF8.GetBytes(string.Format(formdataTemplate, param.Name, param.Value));
                memStream.Write(formitembytes, 0, formitembytes.Length);
            }


            string fileName = Path.GetFileName(filePath);
            string contentType = GetContentType(fileName);
            memStream.Write(beginBoundary, 0, beginBoundary.Length);
            byte[] headerbytes = System.Text.Encoding.UTF8.GetBytes(
                string.Format("Content-Disposition: form-data; name=\"upload\"; filename=\"{0}\"\r\nContent-Type: \"{1}\"\r\n\r\n",
                fileName, GetContentType(fileName)));
            memStream.Write(headerbytes, 0, headerbytes.Length);

            byte[] buffer;
            memStream.Write((buffer = File.ReadAllBytes(filePath)), 0, buffer.Length);

            memStream.Write(endBoundary, 0, endBoundary.Length);

            webRequest.ContentLength = memStream.Length;
            memStream.Position = 0;
            byte[] tempBuffer = new byte[memStream.Length];
            memStream.Read(tempBuffer, 0, tempBuffer.Length);
            memStream.Close();
            using (Stream s = webRequest.GetRequestStream())
            {
                s.Write(tempBuffer, 0, tempBuffer.Length);
            }

            using (StreamReader sr = new StreamReader(webRequest.GetResponse().GetResponseStream()))
            {
                return sr.ReadToEnd();
            }

        }

        string GetContentType(string fileName)
        {
            string contentType = "application/octetstream";
            string ext = Path.GetExtension(fileName).ToLower();
            Microsoft.Win32.RegistryKey registryKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(ext);

            if (registryKey != null && registryKey.GetValue("Content Type") != null)
            {
                contentType = registryKey.GetValue("Content Type").ToString();
            }

            return contentType;
        }

        string CalSig(List<APIParameter> paras)
        {
            paras.Sort(new ParameterComparer());
            StringBuilder sbList = new StringBuilder();
            foreach (APIParameter para in paras)
            {
                sbList.AppendFormat("{0}={1}", para.Name, para.Value);
            }
            sbList.Append(ClientSecret);
            return MD5Encrpt(sbList.ToString());
        }

        string MD5Encrpt(string plainText)
        {
            MD5 md5Hash = MD5.Create();
            byte[] data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(plainText));
            StringBuilder sbList = new StringBuilder();
            foreach (byte d in data)
            {
                sbList.Append(d.ToString("x2"));
            }
            return sbList.ToString();
        }

        class APIParameter
        {
            private string name = null;
            private string value = null;

            public APIParameter(string name, string value)
            {
                this.name = name;
                this.value = value;
            }

            public string Name
            {
                get { return name; }
            }

            public string Value
            {
                get { return value; }
            }
        }

        class ParameterComparer : IComparer<APIParameter>
        {
            public int Compare(APIParameter x, APIParameter y)
            {
                if (x.Name == y.Name)
                {
                    return string.Compare(x.Value, y.Value);
                }
                else
                {
                    return string.Compare(x.Name, y.Name);
                }
            }
        }

        public enum RenrenException
        {
            Unknown = 0,
            InvalidToken,
            ExpiredToken,
            InvalidPhoto,
            InvalidOperation
        }
    }
}
