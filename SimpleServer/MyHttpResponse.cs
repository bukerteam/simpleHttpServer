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
        /// http response params
        /// </summary>
        public class MyHttpResponse
        {
            private string _html;
            private byte[] _buffer;
            private string _stringBuffer;
            public HttpResponseAnswerCodes HttpRequestAnswerCode { get; set; }

            public string HttpVersion { get; set; } 

            public Dictionary<string, string> HeadersDict { get; set; }

            public string Header
            {
                get
                {
                    return HeadersDict
                        .Aggregate("", (cum, headersPair) =>
                            string
                                .Format("{0}\n{1}: {2}\r", 
                                    cum, 
                                    headersPair.Key, 
                                    headersPair.Value)
                                .TrimStart('\n')
                                .TrimEnd('\r'));
                    
                }
            }

            public MyHttpRequest HttpRequest { get; private set; }

            public const int BufferSize = 1024;
            public Socket WorkSocket { get; set; }

            public string Html
            {
                get
                {
                    return _html ??
                           (_html = "<html><body><h1>Not ready yet, please wait...</h1></body></html>");
                }
                set { _html = value; }
            }

            public string StringBuffer
            {
                get
                {
                    return _stringBuffer ?? (_stringBuffer = 
                        string.Format("HTTP/{0} {1} {2} \n{3}\n\n{4}",
                            HttpVersion,
                            (int)HttpRequestAnswerCode,
                            HttpRequestAnswerCode.ToString().Replace("_", " "),
                            Header,
                            Html));
                }
            }

            public byte[] Buffer
            {
                get
                {
                    return _buffer ?? (_buffer = Encoding.ASCII.GetBytes(StringBuffer));
                }
            }

            public MyHttpResponse(MyHttpRequest httpRequest, Socket socket, string httpVersion)
            {
                HttpVersion = httpVersion;
                HttpRequest = httpRequest;
                WorkSocket = socket;

                _html = null;
                _buffer = null;
                HttpRequestAnswerCode = HttpResponseAnswerCodes.OK;
                HeadersDict = new Dictionary<string, string>();
            }

            public void CreateHeader()
            {

                HeadersDict.Add(
                    "Date", string.Format("{0:R}" ,DateTime.Now));

                HeadersDict.Add(
                    "Content-Length", Html.Length.ToString());

                 HeadersDict.Add(
                    "Content-type", System.Net.Mime.MediaTypeNames.Text.Html);

                switch (HttpRequestAnswerCode)
                {
                    case HttpResponseAnswerCodes.OK:
                    {
                        break;
                    }
                    case HttpResponseAnswerCodes.Not_Found:
                    {
                        break;
                    }
                    case HttpResponseAnswerCodes.HTTP_Version_Not_Supported:
                    {
                        break;
                    }
                }

                HeadersDict.Add(
                    "Last-Modified", string.Format("{0:R}", DateTime.Now));
            }

            /// <summary>
            /// 
            /// </summary>
            public void CreateHtml()
            {
                switch (HttpRequestAnswerCode)
                {
                    case HttpResponseAnswerCodes.OK:
                    {
                        Html = GetPage();
                        break;
                    }
                    case HttpResponseAnswerCodes.Not_Found:
                    {
                        Html = "<html><body><h1>Error 404: File not found</h1></body></html>";
                        break;
                    }
                    case HttpResponseAnswerCodes.HTTP_Version_Not_Supported:
                    {
                        Html = "<html><body><h1>Error 505: Version Not Supported</h1></body></html>";
                        break;
                    }
                }
            }

            /// <summary>
            /// 
            /// </summary>
            /// <returns></returns>
            private string GetPage()
            {
                return "<html><body><h1>This is valid page!</h1></body></html>";
            }

            /// <summary>
            /// check and validate http request, and get relevant http code for it
            /// </summary>
            public void CheckHttpVersion()
            {
                if (HttpRequestAnswerCode == HttpResponseAnswerCodes.OK)
                {
                    HttpRequestAnswerCode = 
                        HttpRequest.HttpVersion != HttpVersion ? 
                        HttpResponseAnswerCodes.HTTP_Version_Not_Supported : 
                        HttpRequestAnswerCode;
                }
            }

            /// <summary>
            /// check existing require page, and get relevant http code for it
            /// </summary>
            public void CheckPage()
            {
                if (HttpRequestAnswerCode == HttpResponseAnswerCodes.OK)
                {
                }
            }

            //TODO: заменить на HttpStatusCode
            public enum HttpResponseAnswerCodes
            {
                OK = 200,
                Not_Found = 404,
                HTTP_Version_Not_Supported = 505,
            }
        }
    }
}