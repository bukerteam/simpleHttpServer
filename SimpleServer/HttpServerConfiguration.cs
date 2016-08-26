using System;
using System.Net;

namespace SimpleServer
{
    public class HttpServerConfiguration
    {
        public string RootDirectory { get; set; }
        public ushort ListenPort { get; set; }
        public IPAddress ListenAddress { get; set; }
        public string[] HostNames { get; set; }
        public string ValidHttpVersion { get; set; }
    }
}