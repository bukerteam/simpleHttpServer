using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace SimpleServer
{
    public partial class SimpleHttpServer
    {

        /// <summary>
        /// http Request arguments
        /// </summary>
        public class MyHttpRequest
        {
            public HttpServerConfiguration HttpServerConfiguration { get; set; }
            public string Method { get; private set; }
            public Uri Uri { get; set; }
            public string HttpVersion { get; private set; }
            public Dictionary<string, string> HeadersDict { get; set; }

            string _receivedData;
            /// <summary>
            /// client work socket
            /// </summary>
            public Socket WorkSocket { get; set; }
            /// <summary>
            /// Size of receive buffer.
            /// </summary>
            public const int BufferSize = 1024;
            /// <summary>
            /// Receive buffer
            /// </summary>
            public byte[] Buffer { get; set; }

            /// <summary>
            /// builder for get correct request
            /// </summary>
            public StringBuilder StringBuilder { get; set; }

            /// <summary>
            /// get received data from client as a string
            /// </summary>
            public string ReceivedData
            {
                get { return _receivedData ?? (_receivedData = StringBuilder.ToString()); }
            }

            /// <summary>
            /// create instance of MyHttpRequest based on client work socket
            /// </summary>
            /// <param name="socket"></param>
            /// <param name="httpServerConfiguration"></param>
            public MyHttpRequest(Socket socket, 
                HttpServerConfiguration httpServerConfiguration)
            {
                HttpServerConfiguration = httpServerConfiguration;
                WorkSocket = socket;
                StringBuilder = new StringBuilder();
                Buffer = new byte[BufferSize];
            }

            /// <summary>
            /// parse incoming http request and fill HttpRequest object with parsing result
            /// </summary>
            /// <returns></returns>
            public void ParseHttpRequest()
            {
                #region patterns

                //const string methodPattern = @"^([A-Z]+)\s";
                //const string uriPattern = @"\s/(.+)\sHTTP";
                //const string httpVersionPattern = @"\sHTTP/([1,0,\.]+)\r";
                //const string responceHeadersPattern = "\n((.+): (.+))\r";

                #endregion
                
                var requestParts = ReceivedData.Split(new[] {"\r\n"}, StringSplitOptions.None);
                var requestStringParts = requestParts[0].Split(' ');

                Method = requestStringParts[0];
                var uri = requestStringParts[1];
                if (uri.StartsWith("/"))
                    uri = string.Format(@"localhost{0}", uri);


                Uri = new Uri(string.Format(@"http://{0}", uri));
                HttpVersion = requestStringParts[2];

                if (Uri.AbsolutePath == "/")
                {
                    Uri = new Uri(string.Format(@"http://{0}/{1}",
                        Uri.Host,
                        "/index.html"));
                }

                HeadersDict = new Dictionary<string, string>();
                for (var i = 1; i < requestParts.Length; i++)
                {
                    var headerPair =
                        requestParts[i].Split(new string[1] {": "}, StringSplitOptions.None);
                    if (headerPair.Length == 2)
                    {
                        HeadersDict.Add(headerPair[0], headerPair[1]);
                    }

                }
            }
        }
    }
}