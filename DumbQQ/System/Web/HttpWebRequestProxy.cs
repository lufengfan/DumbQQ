using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace System.Web
{
    public class HttpWebRequestProxy
    {
        private HttpWebRequest request;

        public IDictionary<string, object> RawHeaders
        {
            get
            {
                WebHeaderCollection headers = this.request.Headers;
                return headers.AllKeys.ToDictionary(
                    (key => key),
                    (key => (object)headers[key])
                );
            }
        }

        public HttpWebRequestProxy(HttpWebRequest request)
        {
            this.request = request ?? throw new ArgumentNullException(nameof(request));
        }

        public static implicit operator HttpWebRequestProxy(HttpWebRequest request)
        {
            return new HttpWebRequestProxy(request ?? throw new ArgumentNullException(nameof(request)));
        }

        public static implicit operator HttpWebRequest(HttpWebRequestProxy requestProxy)
        {
            return (requestProxy ?? throw new ArgumentNullException(nameof(requestProxy))).request;
        }

        public void AddExtraHeader(string header, object value)
        {
            if (value != null && !this.request.Headers.AllKeys.Contains(header))
            {
                this.request.Headers.Add(header, (value ?? string.Empty).ToString());
            }
        }
    }
}
