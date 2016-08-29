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
    public partial class SimpleHttpServer: IHttpServer
    {
        private bool _isClosed;

        private Socket ServerSocket { get; set; }

        public HttpServerConfiguration HttpServerConfiguration { get; private set; }

        //public const string ValidHttpVersion = "1.1";
        //public const string MainPath = @"D:\Работа\WebServer\files";
        //public const string Host = "localhost";
        //public const int Port = 11000;

        /// <summary>
        /// 
        /// </summary>
        private readonly ManualResetEventSlim _allDone = new ManualResetEventSlim(false);


        /// <summary>
        /// create server(listenig-socket), based on httpServerConfiguration, and start listen in async mode
        /// </summary>
        public void Start(HttpServerConfiguration httpServerConfiguration)
        {
            HttpServerConfiguration = httpServerConfiguration;
            
            var ipEndPoint = new IPEndPoint(
                HttpServerConfiguration.ListenAddress,
                HttpServerConfiguration.ListenPort);

           ServerSocket = new Socket(
                HttpServerConfiguration.ListenAddress.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            // bind socket to endpoint and start listen
            try
            {
                ServerSocket.Bind(ipEndPoint);
                ServerSocket.Listen(10);

                // start listen
                while (!_isClosed)
                {
                    _allDone.Reset();

                    Console.WriteLine("Wait for connection to Port {0}", ipEndPoint);

                    ServerSocket.BeginAccept(
                        AcceptCallbak,
                        ServerSocket);

                    _allDone.Wait();
                }
                
                ServerSocket.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        /// <summary>
        /// stop created server and sockets
        /// </summary>
        public void Stop()
        {
            _isClosed = true;
            _allDone.Set();
        }

        /// <summary>
        /// callbak method for Accept method 
        /// </summary>
        /// <param name="ar"></param>
        private void AcceptCallbak(IAsyncResult ar)
        {
            _allDone.Set();
            if (_isClosed) return;
            var workSocket = ((Socket) ar.AsyncState).EndAccept(ar);
            var httpRequest = new MyHttpRequest(
                workSocket,
                HttpServerConfiguration);
            

            workSocket.BeginReceive(
                httpRequest.Buffer,
                0,
                MyHttpRequest.BufferSize,
                0,
                ReceiveCallback,
                httpRequest);
        }

        /// <summary>
        /// callbak method for Receive method 
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveCallback(IAsyncResult ar)
        {
            if (_isClosed) return;
            var httpRequest = (MyHttpRequest)ar.AsyncState;
            var readBytesCount = httpRequest.WorkSocket.EndReceive(ar);

            if (readBytesCount > 0)
            {
                httpRequest.StringBuilder.Append(
                    Encoding.ASCII.GetString(
                        httpRequest.Buffer,
                        0,
                        readBytesCount));

                if (httpRequest.ReceivedData.IndexOf("\r\n\r\n", StringComparison.Ordinal) > -1)
                {
                    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                        httpRequest.ReceivedData.Length,
                        httpRequest.ReceivedData);

                    ProcessHttpRequest(httpRequest);
                }
                else
                {
                    httpRequest.WorkSocket.BeginReceive(
                        httpRequest.Buffer,
                        0,
                        MyHttpRequest.BufferSize,
                        0,
                        ReceiveCallback,
                        httpRequest);
                }
            }
        }

        /// <summary>
        /// callbak method for ProcessHttp method 
        /// </summary>
        /// <param name="ar"></param>
        private void ProcessHttpCallBack(IAsyncResult ar)
        {
            if (_isClosed) return;
            var httpResponce = (MyHttpResponse) ar.AsyncState;
            httpResponce.HttpRequest.WorkSocket.EndSend(ar);

            Console.WriteLine("Send responce with header: \n{0}",
                httpResponce.Header);

            httpResponce.HttpRequest.WorkSocket.Shutdown(SocketShutdown.Both);
            httpResponce.HttpRequest.WorkSocket.Close();
        }
    }
}