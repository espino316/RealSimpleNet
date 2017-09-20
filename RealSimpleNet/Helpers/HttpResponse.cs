using System.Collections.Generic;
using System.Net;
using System.Web.Script.Serialization;

namespace RealSimpleNet.Helpers
{
    public class HttpResponse
    {
        public string Data;
        public string ErrorDescription;
        public string ContentType;
        public HttpStatusCode StatusCode;
        public Dictionary<string, string> Headers = new Dictionary<string, string>();

        public string GetHeader(string name)
        {
            return this.Headers[name];
        }

        public T Deserialize<T>()
        {
            JavaScriptSerializer serializer = new JavaScriptSerializer();
            return serializer.Deserialize<T>(this.Data);
        }
    }
}
