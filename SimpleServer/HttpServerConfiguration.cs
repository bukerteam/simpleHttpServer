using System;
using System.Collections.Generic;
using System.Net;

namespace SimpleServer
{
    public class HttpServerConfiguration
    {
        public string RootDirectory { get; set; }
        public ushort ListenPort { get; set; }
        public IPAddress ListenAddress { get; set; }
        public List<string> HostNames { get; set; }
        public string ValidHttpVersion { get; set; }


        /// <summary>
        /// create an instance of HttpServerConfiguration and fill Valid Host names
        /// </summary>
        public HttpServerConfiguration()
        {
            HostNames = new List<string>
            {
                "176.112.216.155",
                "127.0.0.1",
                "localhost",
            };
        }
    }
}