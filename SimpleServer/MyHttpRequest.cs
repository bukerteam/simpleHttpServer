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
            public string Uri { get; set; }
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
                const string methodPattern = @"^([A-Z]+)\s";
                const string uriPattern = @"\s/(.+)\sHTTP";
                const string httpVersionPattern = @"\sHTTP/([1,0,\.]+)\r";


                const string responceParamsPattern = "\n((.+): (.+))\r";

                var methodRegex = new Regex(methodPattern);
                var uriRegex = new Regex(uriPattern);
                var httpVersionRegex = new Regex(httpVersionPattern);
                var headersRegex = new Regex(responceParamsPattern);

                Method = methodRegex.Match(ReceivedData).Groups[1].Value;
                Uri = uriRegex.Match(ReceivedData).Groups[1].Value;

                if (Uri == "")
                {
                    Uri = "index.html";
                }


                HttpVersion = httpVersionRegex.Match(ReceivedData).Groups[1].Value;
                HeadersDict =   
                    headersRegex.Matches(ReceivedData)
                        .Cast<Match>()
                        .Select(reqMatch =>
                            reqMatch.Groups[1].Value.Split(new[] { ": " }, StringSplitOptions.None))
                        .Where(dictElGroup =>
                            dictElGroup.Length == 2)
                        .ToDictionary(
                            dictElGroup => dictElGroup[0],
                            dictElGroup => dictElGroup[1]);
            }
        }
    }
}