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
        /// <summary>
        /// main method
        /// </summary>
        /// <param name="args"></param>
        private static void Main(string[] args)
        {
            var listenAdress = IPAddress.Parse("127.0.0.1");
            if (args.Length < 4)
            {
                args = new string[4];

                
                if (true)
                {
                    args[0] = @"D:\Работа\IFF\код\C#(S#)\work\SimpleClientServer\SimpleServer\SimpleServer\bootstrap-3.3.7\docs";
                    args[1] = 11000.ToString();
                    listenAdress = IPAddress.Parse("127.0.0.1");

                }
                else
                {
                    args[0] = @"C:\data\files";
                    args[1] = 80.ToString();
                    listenAdress = IPAddress.Parse("176.112.216.155");
                }

                args[2] = "localhost";
                args[3] = "1.1";
            }

            //temp ip for testWebServer (flops.ru)
            //var listenAdress = IPAddress.Parse("176.112.216.155");
            

            var newHttpServerConfiguration = new HttpServerConfiguration
            {
                RootDirectory = args[0],
                ListenPort = (ushort)Convert.ToInt32(args[1]),
                ListenAddress = listenAdress, //Dns.GetHostEntry(args[2]).AddressList[1],
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

        /// <summary>
        /// wait for user press 'q' char
        /// </summary>
        /// <param name="simpleHttpServer"></param>
        private static void WaitForQuit(SimpleHttpServer simpleHttpServer)
        {
            Console.Write("\nFor exit press 'q' char...\n");
            while (Console.ReadLine() != "q")
            {
                Console.Write("\nFor exit press 'q' char...\n");
            }
            simpleHttpServer.Stop();
        }
    }
}
