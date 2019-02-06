using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Cache;
using System.Text;
using System.Xml;
using System.Threading.Tasks;

namespace TT.XBRLProcessor
{
    //
    // Using the code from https://msdn.microsoft.com/en-us/library/bb669135(v=vs.100).aspx
    // How to use XmlUrlResolver https://msdn.microsoft.com/en-us/library/hebb1a8k(v=vs.100).aspx
    //
    public class UrlCachingResolver : XmlUrlResolver
    {
        bool enableHttpCaching;
        ICredentials credentials;

        //resolve resources from cache (if possible) when enableHttpCaching is set to true
        //resolve resources from source when enableHttpcaching is set to false 
        public UrlCachingResolver(bool enableHttpCaching)
        {
            this.enableHttpCaching = enableHttpCaching;
        }

        public override ICredentials Credentials
        {
            set
            {
                credentials = value;
                base.Credentials = value;
            }
        }

        internal async Task<Stream> MyGetEntityAsync(Uri absoluteUri, string role, Type ofObjectToReturn)
        {
            /*
                        WebRequest webReq = WebRequest.Create(absoluteUri);
                          webReq.CachePolicy = new HttpRequestCachePolicy(HttpRequestCacheLevel.Default);
                          if (credentials != null)
                          {
                              webReq.Credentials = credentials;
                          }
                          WebResponse resp = webReq.GetResponse();
                          return resp.GetResponseStream();
           */
            // Task<Stream> t = WebUtils.DownloadAsync(absoluteUri, null);

            // t.GetAwaiter();

            // return t.Result;

            var result = await WebUtils.DownloadAsync(absoluteUri, null);

            return result;
        }
        public override object GetEntity(Uri absoluteUri, string role, Type ofObjectToReturn)
        {
            if (absoluteUri == null)
            {
                throw new ArgumentNullException("absoluteUri");
            }
            //resolve resources from cache (if possible)
            if (absoluteUri.Scheme == "http" /* && enableHttpCaching */ && (ofObjectToReturn == null || ofObjectToReturn == typeof(Stream)))
            {
                var result = MyGetEntityAsync(absoluteUri, role, ofObjectToReturn);

                return result;
            }
            //otherwise use the default behavior of the XmlUrlResolver class (resolve resources from source)
            else
            {
                return base.GetEntity(absoluteUri, role, ofObjectToReturn);
            }
        }
    }
}
