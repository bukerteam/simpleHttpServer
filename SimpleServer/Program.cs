using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace SimpleServer
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            try
            {
                var newHttpServerConfiguration = new HttpServerConfiguration
                {
                    RootDirectory = args[0],
                    ListenPort = (ushort) Convert.ToInt32(args[1]),
                    HostNames = new string[100],
                    ListenAddress = Dns.GetHostEntry(args[2]).AddressList[0],
                    ValidHttpVersion = args[3],
                };

                SocketServer.StartListening(newHttpServerConfiguration);
            }
            catch (Exception)
            {
                
                Console.Write("check bat file");
            }

            
        }
    }
}
