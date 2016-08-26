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
    public partial class SocketServer
    {

        /// <summary>
        /// process incoming http request subject to stateObject (contain data
        /// </summary>
        /// <param name="httpRequest"></param>
        private static void ProcessHttpRequest(MyHttpRequest httpRequest)
        {
            httpRequest.ParseHttpRequest();

            var httpResponce = new MyHttpResponse(httpRequest);

            httpResponce.CheckHttpVersion();
            httpResponce.CheckPageExistence();
            
            httpResponce.SpecifyContent();
            httpResponce.ReadContentFile();
            httpResponce.CreateHeader();

            httpResponce.HttpRequest.WorkSocket.BeginSend(
                httpResponce.Buffer,
                0,
                httpResponce.Buffer.Length,
                0,
                ProcessHttpCallBack,
                httpResponce);
        }
    }

}