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
        public class StateObject
        {
            string _receivedData;
            int _receivedNumber;
            // Client  socket.
            public Socket WorkSocket { get; set; }
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
                        _receivedNumber = Convert.ToInt32(ReceivedData.Replace("q", ""));
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