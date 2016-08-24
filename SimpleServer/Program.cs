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
            AsyncSocketListener.StartListening();
        }
    }
}
