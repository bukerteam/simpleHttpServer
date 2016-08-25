using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace SimpleServer
{
    public class AsyncSocketListener
    {
        /// <summary>
        /// 
        /// </summary>
        private static readonly ManualResetEventSlim AllDone  = new ManualResetEventSlim(false);


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
            var workSocket = ((Socket)ar.AsyncState).EndAccept(ar);
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

            var stateObject = (StateObject)ar.AsyncState;
            var readBytesCount = stateObject.WorkSocket.EndReceive(ar);

            if (readBytesCount > 0)
            {
                stateObject.StringBuilder.Append(
                    Encoding.ASCII.GetString(
                        stateObject.Buffer, 
                        0, 
                        readBytesCount));

                if (stateObject.ReceivedData.IndexOf("q", StringComparison.Ordinal) > -1)
                {
                    Console.WriteLine("Read {0} bytes from socket. \n Data : {1}",
                        stateObject.ReceivedData.Length, 
                        stateObject.ReceivedData);

                    var Html = "<html><body><h1>It works!</h1></body></html>";
                    var Str = "HTTP/1.1 200 OK\nContent-type: text/html\nContent-Length:" + Html.Length.ToString() + "\n\n" + Html;

                    var bStr = Encoding.ASCII.GetBytes(Str);

                    stateObject.WorkSocket.Send(bStr);

                    //SendFiles(stateObject);

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
        private static void SendFileCallback(IAsyncResult ar)
        {
            var stateObject = (StateObject)ar.AsyncState;
            stateObject.WorkSocket.EndSendFile(ar);

            Console.WriteLine("Send file named {0}.txt",
                stateObject.ReceivedNumber);

            stateObject.WorkSocket.Shutdown(SocketShutdown.Both);
            stateObject.WorkSocket.Close();
        }

        /// <summary>
        /// send files to client. sending file name depends on receive data
        /// </summary>
        /// <param name="stateObject"></param>
        private static void SendFiles(StateObject stateObject)
        {
            try
            {
                if (stateObject.ReceivedNumber > 0)
                {

                    var fileName = SelectFileFromStorge(stateObject.ReceivedNumber);
                    stateObject.WorkSocket.BeginSendFile(
                        fileName,
                        SendFileCallback,
                        stateObject);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="receivedNumber"></param>
        /// <returns></returns>
        private static string SelectFileFromStorge(int receivedNumber)
        {
            return Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory.Replace(@"\bin\Debug", @"\files"),
                string.Format("{0}.txt", receivedNumber));
            
        }

        public class StateObject
        {
            string _receivedData;
            int _receivedNumber;
            // Client  socket.
            public Socket WorkSocket { get;set; }
            // Size of receive buffer.
            public const int BufferSize = 1024;
            // Receive buffer.
            public byte[] Buffer { get; set; }
            public StringBuilder StringBuilder { get; set; }

            public string ReceivedData
            {
                get { return _receivedData ?? (_receivedData = StringBuilder.ToString()); }
            }
            public int ReceivedNumber
            {
                get
                {
                    if (_receivedNumber != -1) 
                        return _receivedNumber;

                    try
                    {
                        _receivedNumber = Convert.ToInt32(ReceivedData.Replace("q",""));
                        return _receivedNumber;
                    }
                    catch (Exception)
                    {
                        _receivedNumber = 0;
                        return _receivedNumber;
                    }
                }
            }
            public StateObject()
            {
                _receivedNumber = -1;
                _receivedData = null;
                StringBuilder = new StringBuilder();
                Buffer = new byte[BufferSize];
            }

        }
    }
}