using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

//
// Reuse the code from https://psycodedeveloper.wordpress.com/2013/04/02/how-to-download-a-file-with-httpclient-in-c/
//
namespace TT.XBRLProcessor
{
    /// <summary>Download and save a file asynchronously using HttpClient.</summary>
    public static class WebUtils
    {
        public static Task<Stream> DownloadAsync(string requestUri, string filename)
        {
            if (requestUri == null)
            {
                throw new ArgumentNullException("requestUri");
            }

            return  DownloadAsync(new Uri(requestUri) , filename);
        }

       
        public static async Task<Stream> DownloadAsync(Uri requestUri, string filename)
        {
 /*           if (filename == null)
                throw new ArgumentNullException("filename");
 */

            Stream contentStream = null;
            Stream savedStream = null;

            if (filename == null)
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
                    {
                        contentStream = await (await httpClient.SendAsync(request)).Content.ReadAsStreamAsync();
                    }
                }
            }
            else
            {
                using (var httpClient = new HttpClient())
                {
                    using (var request = new HttpRequestMessage(HttpMethod.Get, requestUri))
                    {
                        contentStream = await (await httpClient.SendAsync(request)).Content.ReadAsStreamAsync();
                        using (savedStream = new FileStream(filename, FileMode.Create, FileAccess.Write, FileShare.None, XBRLConstants.LargeBufferSize, true))
                        {
                            await contentStream.CopyToAsync(savedStream, XBRLConstants.LargeBufferSize);
                        }
                    }
                }
            }

            return contentStream;
        }
    }
}
