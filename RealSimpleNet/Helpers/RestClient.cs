using System;
using System.Collections.Generic;
using System.Net;
using System.Web.Script.Serialization;

namespace RealSimpleNet.Helpers
{
    public class RestClient
    {
        public RestClient(string endPoint)
        {
            this.EndPoint = endPoint;
        }

        public HttpStatusCode StatusCode;
        public string Data;
        public string Error;
        public Dictionary<string, string> Headers;

        public string EndPoint;
        private JavaScriptSerializer serializer = new JavaScriptSerializer();
        private Http http = new Http();

        /// <summary>
        /// Adds a header to the requests. All requests.
        /// </summary>
        /// <param name="name">The header name</param>
        /// <param name="value">The header value</param>
        public void AddHeader(string name, string value)
        {
            this.http.AddHeader(name, value);
        }
        
        /// <summary>
        /// Returns string data from the RESTFul service
        /// </summary>
        /// <param name="url">The url</param>
        /// <returns></returns>
        public string Get(string url)
        {
            url = EndPoint + url;
            HttpResponse response = http.Get(url, null, Http.ContentTypes.UrlEncoded);
            StatusCode = response.StatusCode;
            Data = response.Data;
            Error = response.ErrorDescription;
            Headers = response.Headers;

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new Exception("Not authorized access");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new Exception("No data found");
            }

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception("Not managed status code " + response.StatusCode.ToString());
            }
            
            return response.Data;
        } // end method Get

        /// <summary>
        /// Returns string a generic object from the RESTFul service
        /// </summary>
        /// <param name="url">The url</param>
        /// <returns></returns>
        public T Get<T>(string url)
        {
            url = EndPoint + url;
            HttpResponse response = http.Get(url, null, Http.ContentTypes.UrlEncoded);
            StatusCode = response.StatusCode;
            Data = response.Data;
            Error = response.ErrorDescription;
            Headers = response.Headers;

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new Exception("Not authorized access");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new Exception("No data found");
            }

            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception("Not managed status code " + response.StatusCode.ToString());
            }

            return serializer.Deserialize<T>(response.Data);
        } // end method Get

        /// <summary>
        /// Make a http post to a rest web service
        /// </summary>
        /// <param name="url">The url, action for the service</param>
        /// <param name="data">The data to post</param>
        /// <returns></returns>
        public string Post(string url, object data)
        {
            url = EndPoint + url;
            HttpResponse response = http.Post(url, data, Http.ContentTypes.Json);
            StatusCode = response.StatusCode;
            Data = response.Data;
            Error = response.ErrorDescription;
            Headers = response.Headers;

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new Exception("Not authorized access");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.UnsupportedMediaType)
            {
                throw new Exception("Bad formed request");
            }

            if (response.StatusCode != System.Net.HttpStatusCode.Created)
            {
                throw new Exception("Not managed status code " + response.StatusCode.ToString());
            }

            string location = response.GetHeader("Location");
            if (string.IsNullOrEmpty(location))
            {
                throw new Exception("No location returned.");
            }

            return location;
        } // end method Post

        /// <summary>
        /// Make a http put to a rest web service
        /// </summary>
        /// <param name="url">The url, action for the service</param>
        /// <param name="data">The data to post</param>
        /// <returns></returns>
        public string Put(string url, object data)
        {
            url = EndPoint + url;
            HttpResponse response = http.Put(url, data, Http.ContentTypes.Json);
            StatusCode = response.StatusCode;
            Data = response.Data;
            Error = response.ErrorDescription;
            Headers = response.Headers;

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new Exception("Not authorized access");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.UnsupportedMediaType)
            {
                throw new Exception("Bad formed request");
            }

            if (response.StatusCode != System.Net.HttpStatusCode.Created)
            {
                throw new Exception("Not managed status code " + response.StatusCode.ToString());
            }

            string location = response.GetHeader("Location");
            if (string.IsNullOrEmpty(location))
            {
                throw new Exception("No location returned.");
            }

            return location;
        } // end method Put

        /// <summary>
        /// Delete a record in RESTFul service
        /// </summary>
        /// <param name="url">The url</param>
        /// <returns></returns>
        public string Delete(string url, object data = null)
        {
            url = EndPoint + url;
            HttpResponse response = http.Delete(url, data, Http.ContentTypes.UrlEncoded);
            StatusCode = response.StatusCode;
            Data = response.Data;
            Error = response.ErrorDescription;
            Headers = response.Headers;

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new Exception("Not authorized access");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new Exception("No data found");
            }

            if (response.StatusCode != System.Net.HttpStatusCode.NoContent)
            {
                throw new Exception("Not managed status code " + response.StatusCode.ToString());
            }

            return response.Data;
        } // end method Get
    } // end class
} // end namespace
