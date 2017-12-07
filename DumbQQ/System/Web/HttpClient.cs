using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

namespace System.Web
{
    public class HttpClient
    {
        public HttpWebRequest Request { get; private set; }
        public HttpWebResponse Response { get; private set; }
        public bool PersistCookies { get; internal set; }
        public bool ThrowExceptionOnHttpError { get; internal set; }
        public bool StreamResponse { get; internal set; }
        public string UserAgent { get; internal set; }
        public bool KeepAlive { get; internal set; }
        public object Accept { get; internal set; }
        public bool AllowAutoRedirect { get; internal set; }
        public string Expect { get; internal set; }
        public string Referer { get; internal set; }

        [Obsolete("可能引发错误。", true)]
        public HttpWebResponse Delete(string uri, object query = null)
        {
            this.InitRequest(uri, HttpMethod.DELETE, query);
            return this.ProcessRequest();
        }

        public HttpWebResponse Get(string url, object query = null)
        {
            this.InitRequest(url, HttpMethod.GET, query);
            return this.ProcessRequest();
        }

        [Obsolete("可能引发错误。", true)]
        public HttpWebResponse Head(string uri, object query = null)
        {
            this.InitRequest(uri, HttpMethod.HEAD, query);
            return this.ProcessRequest();
        }

        [Obsolete("可能引发错误。", true)]
        public virtual HttpWebResponse Patch(string uri, object data, string contentType, object query = null)
        {
            this.InitRequest(uri, HttpMethod.PATCH, query);
            this.InitData(data, contentType);
            return this.ProcessRequest();
        }

        public HttpWebResponse Post(string uri, object data, string contentType, object query = null)
        {
            this.InitRequest(uri, HttpMethod.POST, query);
            this.InitData(data, contentType);
            return this.ProcessRequest();
        }

        [Obsolete("可能引发错误。", true)]
        public virtual HttpWebResponse Put(string uri, object data, string contentType, object query = null)
        {
            this.InitRequest(uri, HttpMethod.PUT, query);
            this.InitData(data, contentType);
            return this.ProcessRequest();
        }

        private HttpWebResponse ProcessRequest()
        {
            if (!this.PersistCookies && this.Request.CookieContainer != null)
                this.Request.CookieContainer = new CookieContainer();

            HttpWebResponse response = (HttpWebResponse)this.Request.GetResponse();
            this.Response = response;

            if (this.ThrowExceptionOnHttpError && this.IsHttpError())
                throw new HttpException((int)this.Response.StatusCode, this.Response.StatusDescription);

            return this.Response;
        }

        private bool IsHttpError()
        {
            int num = ((int)this.Response.StatusCode / (int)HttpStatusCode.Continue);
            return num == 4 || num == 5;
        }

        public string ComposeQuery(object data)
        {
            if (data == null) return string.Empty;

            return this.ComposeQuery(data, data.GetType());
        }

        public string ComposeQuery<TData>(TData data)
        {
            if (data == null) return string.Empty;

            return this.ComposeQuery(data, typeof(TData));
        }

        private string ComposeQuery(object data, Type type)
        {
            string queryStr = string.Join("&",
                   (
                       from fi in type.GetFields(
                           System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                       let attributes = fi.GetCustomAttributes(true)
                       where !attributes.OfType<NonSerializedAttribute>().Any()
                       let defaultValueAttr = attributes.OfType<DefaultValueAttribute>().FirstOrDefault()
                       let hasDefaultValue = defaultValueAttr != null
                       let defaultValue = hasDefaultValue ? defaultValueAttr.Value : null
                       let exactValue = fi.GetValue(data)
                       select string.Format("{0}={1}",
                           fi.Name,
                           HttpUtility.UrlEncode(((
                               (hasDefaultValue && object.Equals(defaultValue, exactValue))
                                   ? null : exactValue
                           ) ?? string.Empty).ToString())
                       )
                   ).ToArray()
               );
            if (string.IsNullOrEmpty(queryStr))
                return string.Empty;
            else
                return queryStr.PadLeft(1, '?');
        }

        private void InitRequest(string uri, HttpMethod method, object query)
        {
            this.Request = (HttpWebRequest)WebRequest.Create(uri + this.ComposeQuery(query));
            this.Request.UserAgent = this.UserAgent;
            this.Request.Expect = this.Expect;
            this.Request.KeepAlive = this.KeepAlive;
            this.Request.AllowAutoRedirect = this.AllowAutoRedirect;
            this.Request.Method = Enum.GetName(typeof(HttpMethod), method);
            this.Request.ContentType = "application/x-www-form-urlencoded";
            this.Request.Referer = this.Referer;
            this.Request.CookieContainer = new CookieContainer();
        }

        [Obsolete("忽略了参数 contentType 。")]
        private void InitData(object data, string contentType)
        {
            if (data != null)
            {
                this.Request.ContentType = contentType;

                byte[] bytes = Encoding.UTF8.GetBytes((data ?? string.Empty).ToString());
                if (bytes.Length != 0)
                {
                    this.Request.ContentLength = bytes.LongLength;
                }
                using (Stream stream = this.Request.GetRequestStream())
                {
                    stream.Write(bytes, 0, bytes.Length);
                }
            }
        }
    }
}
