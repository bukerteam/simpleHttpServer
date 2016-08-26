using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SimpleServer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length < 4)
            {
                args = new string[4];

                args[0] = @"D:\Работа\WebServer\files";
                args[1] = 11000.ToString();
                args[2] = "localhost";
                args[3] = "1.1";
            }
            
            var newHttpServerConfiguration = new HttpServerConfiguration
            {
                RootDirectory = args[0],
                ListenPort = (ushort)Convert.ToInt32(args[1]),
                ListenAddress = Dns.GetHostEntry(args[2]).AddressList[0],
                ValidHttpVersion = args[3],
            };

            var simpleHttpServer = new SimpleHttpServer();

            var thread = new Thread(() => 
                simpleHttpServer.Start(newHttpServerConfiguration));

            thread.Start();
            
            WaitForQuit(simpleHttpServer);

            Console.Write("Server stopped!!");
            Console.ReadLine();
        }

        private static void WaitForQuit(SimpleHttpServer simpleHttpServer)
        {
            while (Console.ReadLine() != "q"){}
            simpleHttpServer.Stop();
        }
    }
}
