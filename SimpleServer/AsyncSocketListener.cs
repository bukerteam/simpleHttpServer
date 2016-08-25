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

        public const string ValidHttpVersion = "1.1";


        /// <summary>
        /// 
        /// </summary>
        private static readonly ManualResetEventSlim AllDone = new ManualResetEventSlim(false);


        /// <summary>
        /// create listenig-socket and listen in async mode
        /// </summary>
        public static void StartListening()
        {
            // Устанавливаем для сокета локальную конечную точку
            var ipHost = Dns.GetHostEntry("localhost");
            var ipAddr = ipHost.AddressList[0];
            var ipEndPoint = new IPEndPoint(ipAddr, 11000);

            // Создаем сокет Tcp/Ip
            var serverSocket = new Socket(
                ipAddr.AddressFamily,
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

                    Console.WriteLine("Wait for connection to port {0}", ipEndPoint);

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
            var stateObject = new StateObject
            {
                WorkSocket = workSocket
            };

            workSocket.BeginReceive(
                stateObject.Buffer,
                0,
                StateObject.BufferSize,
                0,
                ReceiveCallback,
                stateObject);
        }

        private static void ReceiveCallback(IAsyncResult ar)
        {

            var stateObject = (StateObject) ar.AsyncState;
            var readBytesCount = stateObject.WorkSocket.EndReceive(ar);

            if (readBytesCount > 0)
            {
                stateObject.StringBuilder.Append(
                    Encoding.ASCII.GetString(
                        stateObject.Buffer,
                        0,
                        readBytesCount));

                if (stateObject.ReceivedData.IndexOf("\r\n\r\n", StringComparison.Ordinal) > -1)
                {
                    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                        stateObject.ReceivedData.Length,
                        stateObject.ReceivedData);

                    ProcessHttp(stateObject);
                }
                else
                {
                    stateObject.WorkSocket.BeginReceive(
                        stateObject.Buffer,
                        0,
                        StateObject.BufferSize,
                        0,
                        ReceiveCallback,
                        stateObject);
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
            httpResponce.WorkSocket.EndSend(ar);

            Console.WriteLine("Send responce");

            httpResponce.WorkSocket.Shutdown(SocketShutdown.Both);
            httpResponce.WorkSocket.Close();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ar"></param>
        private static void SendFileCallback(IAsyncResult ar)
        {
            var stateObject = (StateObject) ar.AsyncState;
            stateObject.WorkSocket.EndSendFile(ar);

            Console.WriteLine("Send file named {0}.txt",
                stateObject.ReceivedNumber);

            stateObject.WorkSocket.Shutdown(SocketShutdown.Both);
            stateObject.WorkSocket.Close();
        }
        
    }

}