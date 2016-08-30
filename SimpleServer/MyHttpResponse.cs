using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
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
        /// http response params
        /// </summary>
        public class MyHttpResponse
        {
            private byte[] _buffer;

            /// <summary>
            /// dictionary of mime type according to file extension
            /// </summary>
            private static readonly Dictionary<string, string> MimeContentType = new Dictionary<string, string> 
            {
                { ".text", "text/plain" },
                { ".js", "text/javascript" },
                { ".css", "text/css" },
                { ".html", "text/html" },
                { ".htm", "text/html" },
                { ".png", "image/png" },
                { ".ico", "image/x-icon" },
                { ".gif", "image/gif" },
                { ".bmp", "image/bmp" },
                { ".jpg", "image/jpeg" },
                { ".pdf", "application/pdf" }
            };

            /// <summary>
            /// client http request, which this responce is based on
            /// </summary>
            public MyHttpRequest HttpRequest { get; private set; }

            /// <summary>
            /// response code - it differ from HttpStatusCode class because contain "_" chars
            /// </summary>
            public HttpResponseCodes HttpResponseCode { get; set; }
           
            /// <summary>
            /// dictionary with headers pairs
            /// </summary>
            private Dictionary<string, string> HeadersDict { get; set; }

            /// <summary>
            /// header of MyHttpResponse based on HeadersDict
            /// </summary>
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

            /// <summary>
            /// stream with read content from require file
            /// </summary>
            private MemoryStream ContentStream { get; set; }

            /// <summary>
            /// final buffer with byte array, with is base of http response
            /// </summary>
            public byte[] Buffer
            {
                get
                {
                    if (_buffer != null) return _buffer;
                    var bufferMemoryStream = new MemoryStream();

                    var contentMemoryStream = GetContentPartOfBuffer();
                    var headerMemoryStream = GetHeaderPartOfBuffer();

                    headerMemoryStream.CopyTo(bufferMemoryStream);
                    contentMemoryStream.CopyTo(bufferMemoryStream);

                    _buffer = bufferMemoryStream.ToArray();

                    return _buffer;
                }
            }

            /// <summary>
            /// create instance of MyHttpResponse based on httpRequest (cause of this response)
            /// </summary>
            /// <param name="httpRequest"></param>
            public MyHttpResponse(MyHttpRequest httpRequest)
            {
                HttpRequest = httpRequest;
                ContentStream = new MemoryStream();

                _buffer = null;
                HttpResponseCode = HttpResponseCodes.OK;
                HeadersDict = new Dictionary<string, string>();
            }

            /// <summary>
            /// create headers for response
            /// </summary>
            public void CreateHeader()
            {

                HeadersDict.Add(
                    "Date", string.Format("{0:R}" ,DateTime.Now));

                HeadersDict.Add(
                    "Content-Length", ContentStream.Length.ToString());

                HeadersDict.Add(
                    "Connection", "keep-alive");

                HeadersDict.Add(
                    "Last-Modified", string.Format("{0:R}", 
                        File.GetLastWriteTime(
                            Path.Combine(
                                HttpRequest.HttpServerConfiguration.RootDirectory,
                                HttpRequest.Uri))));


                if (HttpRequest.HeadersDict.ContainsKey("Accept-Encoding") &&
                    HttpRequest.HeadersDict["Accept-Encoding"].Contains("gzip"))
                {
                    HeadersDict.Add(
                        "Content-Encoding", "gzip");
                }

                 SpecifyMimeType();
            }

            /// <summary>
            /// specify content according to HttpResponseCodes, if HttpResponseCodes is not OK
            /// </summary>
            public void SpecifyContent()
            {
                if (HttpResponseCode!= HttpResponseCodes.OK)
                {
                    HttpRequest.Uri = string.Format("{0}.html", (int)HttpResponseCode);
                }
            }

            /// <summary>
            /// directly read and save content file to stream
            /// </summary>
            public void ReadContentFile()
            {
                var fs = new FileStream(
                    Path.Combine(
                        HttpRequest.HttpServerConfiguration.RootDirectory, 
                        HttpRequest.Uri), 
                    FileMode.Open);

                fs.CopyTo(ContentStream);
                fs.Close();
                
            }

            /// <summary>
            /// check if request method is GET
            /// </summary>
            public void CheckMethod()
            {
                if (HttpResponseCode == HttpResponseCodes.OK)
                {
                    HttpResponseCode =
                        HttpRequest.Method != "GET" ?
                        HttpResponseCodes.Forbidden :
                        HttpResponseCode;
                }
            }


            /// <summary>
            /// check and validate http version request, and get relevant HttpResponseCodes for it
            /// </summary>
            public void CheckHttpVersion()
            {
                if (HttpResponseCode == HttpResponseCodes.OK)
                {
                    HttpResponseCode =
                        HttpRequest.HttpVersion != HttpRequest.HttpServerConfiguration.ValidHttpVersion ? 
                        HttpResponseCodes.HTTP_Version_Not_Supported : 
                        HttpResponseCode;
                }
            }

            /// <summary>
            /// check existing required page, and get relevant HttpResponseCodes code for it
            /// </summary>
            public void CheckPageExistence()
            {
                if (HttpResponseCode == HttpResponseCodes.OK)
                {
                    HttpResponseCode = !File.Exists(
                        Path.Combine(
                            HttpRequest.HttpServerConfiguration.RootDirectory, 
                            HttpRequest.Uri))
                        ? HttpResponseCodes.Not_Found
                        : HttpResponseCode;
                }
            }

            /// <summary>
            /// check and validate incomint host - HttpServerConfiguration.HostNames must contain it
            /// </summary>
            public void CheckHost()
            {
                if (HttpResponseCode == HttpResponseCodes.OK)
                {
                    HttpResponseCode = !HttpRequest.HttpServerConfiguration.HostNames
                        .Contains(HttpRequest.HeadersDict["Host"].Split(':')[0])
                        ? HttpResponseCodes.Forbidden
                        : HttpResponseCode;
                }
            }


            /// <summary>
            /// get MemoryStream - part of response buffer, responsible for content
            /// </summary>
            /// <returns></returns>
            private MemoryStream GetContentPartOfBuffer()
            {
                ContentStream.Position = 0;

                var contentMemoryStream = new MemoryStream();

                if (HeadersDict.ContainsKey("Content-Encoding") &&
                    HeadersDict["Content-Encoding"].Contains("gzip"))
                {

                    var gZipStream = new GZipStream(
                        contentMemoryStream,
                        CompressionMode.Compress,
                        true);

                    ContentStream.CopyTo(gZipStream);
                    gZipStream.Close();

                    HeadersDict["Content-Length"] = contentMemoryStream.Length.ToString();
                }
                else
                {
                    ContentStream.CopyTo(contentMemoryStream);
                }

                contentMemoryStream.Position = 0;

                return contentMemoryStream;
            }

            /// <summary>
            /// get MemoryStream - part of response buffer, responsible for header
            /// </summary>
            /// <returns></returns>
            private MemoryStream GetHeaderPartOfBuffer()
            {
                var headerMemoryStream = new MemoryStream(
                    Encoding.ASCII.GetBytes(
                        string.Format("HTTP/{0} {1} {2} \n{3}\n\n",
                            HttpRequest.HttpServerConfiguration.ValidHttpVersion,
                            (int)HttpResponseCode,
                            HttpResponseCode.ToString().Replace("_", " "),
                            Header)));

                return headerMemoryStream;
            }

            /// <summary>
            /// specify mime type depends on file extension
            /// </summary>
            private void SpecifyMimeType()
            {
                const string mimePattern = @"\.[^\.]+$";
                string mimeType;

                if (!MimeContentType
                    .TryGetValue(
                        Regex.Match(HttpRequest.Uri, mimePattern).Value, 
                        out mimeType))
                    mimeType = "text/plain";

                HeadersDict.Add("Content-type", mimeType);
            }


            //TODO: придумать как заменить на HttpStatusCode
            /// <summary>
            /// custom response code - differ from HttpStatusCode, because contain "_" char in names
            /// </summary>
            public enum HttpResponseCodes
            {
                OK = 200,
                Bad_Request = 400,
                Forbidden = 403, 
                Not_Found = 404,
                HTTP_Version_Not_Supported = 505,

            }
        }
    }
}