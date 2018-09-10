using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Script.Serialization;

namespace RealSimpleNet.Helpers
{
    public class Http
    {
        private JavaScriptSerializer serializer = new JavaScriptSerializer();
        private Dictionary<string, object> parameters = new Dictionary<string, object>();       
        private Dictionary<string, string> headers = new Dictionary<string, string>();
        private string jsonData;

        public void AddData(object data)
        {
            this.jsonData = serializer.Serialize(data);
        }

        public void AddParameter(string key, object value)
        {
            this.parameters.Add(key, value);
        } // end void AddParameter

        public void ClearParameters()
        {
            this.parameters.Clear();
        } // end void ClearParameters

        public void AddHeader(string key, string value)
        {
            this.headers.Add(key, value);
        } // end void AddParameter

        public void ClearHeaders()
        {
            this.headers.Clear();
        } // end void ClearParameters

        public enum ContentTypes
        {
            Json,
            UrlEncoded
        }

        public enum HttpMethods
        {
            GET,
            POST,
            PUT,
            DELETE,
            HEAD,
            TAGS
        }

        private string GetQueryString(Dictionary<string, object> parameters)
        {
            string queryString = "";
            foreach (string key in parameters.Keys)
            {
                if (queryString.Length > 0)
                {
                    queryString += "&";
                }
                queryString += key + "=" + HttpUtility.UrlEncode(this.parameters[key].ToString());
            }
            return queryString;
        }

        public HttpResponse Get(
            string url,
            object data = null,
            ContentTypes contentType = ContentTypes.UrlEncoded,
            Dictionary<string, string> headers = null)
        {
            return this.Request(url, data, HttpMethods.GET, contentType, headers);
        }

        public HttpResponse Post(
            string url,
            object data = null,
            ContentTypes contentType = ContentTypes.UrlEncoded,
            Dictionary<string,string> headers = null)
        {
            return this.Request(url, data, HttpMethods.POST, contentType, headers);
        }

        public HttpResponse Put(
            string url,
            object data = null,
            ContentTypes contentType = ContentTypes.UrlEncoded,
            Dictionary<string, string> headers = null)
        {
            return this.Request(url, data, HttpMethods.PUT, contentType, headers);
        }

        public HttpResponse Delete(
            string url,
            object data = null,
            ContentTypes contentType = ContentTypes.UrlEncoded,
            Dictionary<string, string> headers = null)
        {
            return this.Request(url, data, HttpMethods.DELETE, contentType, headers);
        }

        private string JsonDateFromMS(long ms)
        {            
            DateTime startTime = new DateTime(1970, 1, 1);
            TimeSpan time = TimeSpan.FromMilliseconds(ms);
            return startTime.Add(time).ToString("yyyy-MM-dd HH:mm:ss");
        }

        private WebClient webClient;
        
        public void DownloadAsync(
            string url, 
            string filePath,
            AsyncCompletedEventHandler onDownloadComplete,
            DownloadProgressChangedEventHandler onDowloadProgressChanged,
            Dictionary<string, string> headers = null )
        {
            webClient = new WebClient();
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(onDownloadComplete);
            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(onDowloadProgressChanged);
            webClient.DownloadFileAsync(new Uri(url), filePath);
        } // end DownloadAsync

        public void Download(string url, string filePath, string user, string password)
        {
            this.Download(url, filePath, null, user, password);
        }

        public void Download(string url, string filePath, Dictionary<string, string> headers = null)
        {
            this.Download(url, filePath, headers);
        } // end function Download

        private void Download(string url, string filePath, Dictionary<string, string> headers = null, string user = null, string password = null)
        {
            WebClient webClient = new WebClient();

            if (headers == null)
            {
                headers = this.headers;
            }

            if (user != null && password != null)
            {
                webClient.UseDefaultCredentials = true;
                webClient.Credentials = new NetworkCredential(user, password);
            }

            foreach (string key in this.headers.Keys)
            {
                webClient.Headers.Add(key, headers[key]);
            }

            webClient.DownloadFile(
                url,
                filePath
            );
        } // end function Download

        public HttpResponse Request(                       
            string url,
            object data = null,
            HttpMethods method = HttpMethods.GET,
            ContentTypes contentType = ContentTypes.UrlEncoded,
            Dictionary<string, string> headers = null
        ) {
            try
            {
                JavaScriptSerializer serializer = new JavaScriptSerializer();
                string requestData = "";

                if (data == null && this.parameters != null && contentType == ContentTypes.UrlEncoded)
                {
                    requestData = this.GetQueryString(this.parameters);
                }

                if (data != null && contentType == ContentTypes.UrlEncoded)
                {
                    this.parameters = (Dictionary<string, object>)data;
                    requestData = this.GetQueryString(this.parameters);
                }

                if (data == null && !string.IsNullOrEmpty(this.jsonData) && contentType == ContentTypes.Json)
                {
                    requestData = this.jsonData;
                }

                if (data != null && contentType == ContentTypes.Json)
                {                    
                    requestData = serializer.Serialize(data);
                    string pattern = "Date\\(\\d*\\)";
                    pattern = "\\\\\\/Date\\(\\d*\\)\\\\\\/";
                    Regex regex = new Regex(pattern);
                    foreach (Match m in regex.Matches(requestData))
                    {
                        string dateStr = m.Value;
                        dateStr = dateStr.Replace("\\/Date(", "").Replace(")\\/", "");
                        dateStr = JsonDateFromMS(Convert.ToInt64(dateStr));
                        requestData
                            = requestData.Replace(m.Value, dateStr);
                    }
                }

                if (headers == null)
                {
                    headers = this.headers;
                }

                WebRequest request;
                Stream dataStream;
                if (method == HttpMethods.GET)
                {
                    if (!string.IsNullOrEmpty(requestData))
                    {
                        url += "?" + requestData;
                    }

                    request = WebRequest.Create(url);                    
                    request.Method = method.ToString();
                    foreach (string key in headers.Keys)
                    {
                        request.Headers.Add(key, headers[key]);
                    }
                }
                else
                {
                    request = WebRequest.Create(url);
                    request.Method = method.ToString();
                    foreach (string key in headers.Keys)
                    {
                        request.Headers.Add(key, headers[key]);
                    }

                    byte[] byteArray = Encoding.UTF8.GetBytes(requestData);
                    if (contentType == ContentTypes.Json)
                    {
                        request.ContentType = "application/json; charset=UTF-8";                        
                    }
                    else if (contentType == ContentTypes.UrlEncoded)
                    {
                        request.ContentType = "application/x-www-form-urlencoded";
                    }

                    request.ContentLength = byteArray.Length;
                    dataStream = request.GetRequestStream();
                    dataStream.Write(byteArray, 0, byteArray.Length);
                    dataStream.Close();
                }
                
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                HttpResponse httpResponse = new HttpResponse();
                httpResponse.StatusCode = response.StatusCode;
                httpResponse.ContentType = response.ContentType;

                foreach (var header in response.Headers)
                {
                    httpResponse.Headers.Add(
                        (string)header,
                        response.Headers.Get((string)header).ToString()
                    );
                }

                dataStream = response.GetResponseStream();
                StreamReader reader = new StreamReader(dataStream);
                string responseFromServer = reader.ReadToEnd();
                httpResponse.Data = responseFromServer;
                
                reader.Close();
                dataStream.Close();
                response.Close();

                this.ClearHeaders();
                this.ClearParameters();
                return httpResponse;
            }
            catch (WebException webEx)
            {
                Console.Write(webEx);
                HttpResponse httpResponse = new HttpResponse();
                HttpWebResponse response = (HttpWebResponse)webEx.Response;
                if (response != null)
                {
                    httpResponse.StatusCode = response.StatusCode;
                    Stream dataStream = response.GetResponseStream();
                    StreamReader reader = new StreamReader(dataStream);
                    string responseFromServer = reader.ReadToEnd();
                    httpResponse.ErrorDescription = responseFromServer;
                    return httpResponse;
                }
                throw webEx;
            }
            catch (Exception ex)
            {
                Console.Write(ex);
                throw ex;
            }
        } // end post
    } // end class
} // end namespace
