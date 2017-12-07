using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace System.Web
{
    internal sealed class HttpWebResponseProxy
    {
        private HttpWebResponse response;

        public string RawText
        {
            get
            {
                string text = string.Empty;

                Encoding encoding = string.IsNullOrEmpty(this.response.CharacterSet) ? Encoding.UTF8 : Encoding.GetEncoding(this.response.CharacterSet);
                using (StreamReader reader = new StreamReader(this.response.GetResponseStream()))
                {
                    text = reader.ReadToEnd();
                }

                return text;
            }
        }

        public HttpWebResponseProxy(HttpWebResponse response)
        {
            this.response = response ?? throw new ArgumentNullException(nameof(response));
        }

        public static implicit operator HttpWebResponseProxy(HttpWebResponse response)
        {
            return new HttpWebResponseProxy(response ?? throw new ArgumentNullException(nameof(response)));
        }

        public static implicit operator HttpWebResponse(HttpWebResponseProxy responseProxy)
        {
            return (responseProxy ?? throw new ArgumentNullException(nameof(responseProxy))).response;
        }
    }
}
