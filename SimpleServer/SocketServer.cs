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

        public static HttpServerConfiguration HttpServerConfiguration { get; private set; }

        //public const string ValidHttpVersion = "1.1";
        //public const string MainPath = @"D:\Работа\WebServer\files";
        //public const string Host = "localhost";
        //public const int Port = 11000;

        /// <summary>
        /// 
        /// </summary>
        private static readonly ManualResetEventSlim AllDone = new ManualResetEventSlim(false);


        /// <summary>
        /// create listenig-socket and listen in async mode
        /// </summary>
        public static void StartListening(HttpServerConfiguration httpServerConfiguration)
        {
            // Устанавливаем для сокета локальную конечную точку
            HttpServerConfiguration = httpServerConfiguration;
            
            var ipEndPoint = new IPEndPoint(
                HttpServerConfiguration.ListenAddress,
                HttpServerConfiguration.ListenPort);

            // Создаем сокет Tcp/Ip
            var serverSocket = new Socket(
                HttpServerConfiguration.ListenAddress.AddressFamily,
                SocketType.Stream,
                ProtocolType.Tcp);

            // Назначаем сокет локальной конечной точке и слушаем входящие сокеты
            try
            {
                serverSocket.Bind(ipEndPoint);
                serverSocket.Listen(10);

                // Начинаем слушать соединения
                while (true)
                {
                    AllDone.Reset();

                    Console.WriteLine("Wait for connection to Port {0}", ipEndPoint);

                    serverSocket.BeginAccept(
                        AcceptCallbak,
                        serverSocket);


                    AllDone.Wait();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally
            {
                Console.ReadLine();
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ar"></param>
        private static void AcceptCallbak(IAsyncResult ar)
        {
            AllDone.Set();
            var workSocket = ((Socket) ar.AsyncState).EndAccept(ar);
            var httpRequest = new MyHttpRequest(workSocket);
            

            workSocket.BeginReceive(
                httpRequest.Buffer,
                0,
                MyHttpRequest.BufferSize,
                0,
                ReceiveCallback,
                httpRequest);
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {

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
        /// 
        /// </summary>
        /// <param name="ar"></param>
        private static void ProcessHttpCallBack(IAsyncResult ar)
        {
            var httpResponce = (MyHttpResponse) ar.AsyncState;
            httpResponce.HttpRequest.WorkSocket.EndSend(ar);

            Console.WriteLine("Send responce with header: \n{0}",
                httpResponce.Header);

            httpResponce.HttpRequest.WorkSocket.Shutdown(SocketShutdown.Both);
            httpResponce.HttpRequest.WorkSocket.Close();
        }
    }
}