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
    public partial class AsyncSocketListener
    {

        /// <summary>
        /// http Request arguments
        /// </summary>
        public class MyHttpRequest
        {

            public string Method { get; private set; }
            public string Uri { get; private set; }
            public string HttpVersion { get; private set; }
            public Dictionary<string, string> HeadersDict { get; set; }

            public MyHttpRequest(
                string method,
                string uri,
                string httpVersion,
                Dictionary<string, string> headersDict)
            {
                Method = method;
                Uri = uri;
                HttpVersion = httpVersion;
                HeadersDict = headersDict;
            }
        }
    }
}