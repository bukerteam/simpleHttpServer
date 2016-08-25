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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stateObject"></param>
        private static void ProcessHttp(StateObject stateObject)
        {
            var httpRequest = ParseHttpRequest(stateObject);

            var httpResponce = new MyHttpResponse(
                httpRequest, 
                stateObject.WorkSocket,
                ValidHttpVersion);

            httpResponce.CheckHttpVersion();
            httpResponce.CheckPage();
            
            httpResponce.CreateHtml();
            httpResponce.CreateHeader();

            httpResponce.WorkSocket.BeginSend(
                httpResponce.Buffer,
                0,
                httpResponce.Buffer.Length,
                0,
                ProcessHttpCallBack,
                httpResponce);
        }
        

        /// <summary>
        /// parse incoming http request and generate myHttpRequest object with parsing result
        /// </summary>
        /// <param name="stateObject"></param>
        /// <returns></returns>
        private static MyHttpRequest ParseHttpRequest(StateObject stateObject)
        {
            const string methodPattern = @"^([A-Z]+)\s";
            const string uriPattern = @"\s(.+)\sHTTP";
            const string httpVersionPattern = @"\sHTTP/([1,0,\.]+)\r";


            const string responceParamsPattern = "\n((.+): (.+))\r";

            var methodRegex = new Regex(methodPattern);
            var uriRegex = new Regex(uriPattern);
            var httpVersionRegex = new Regex(httpVersionPattern);
            var headersRegex = new Regex(responceParamsPattern);

            var httpRequest = new MyHttpRequest(
                methodRegex.Match(stateObject.ReceivedData).Groups[1].Value,
                uriRegex.Match(stateObject.ReceivedData).Groups[1].Value,
                httpVersionRegex.Match(stateObject.ReceivedData).Groups[1].Value,
                headersRegex.Matches(stateObject.ReceivedData)
                    .Cast<Match>()
                    .Select(reqMatch =>
                        reqMatch.Groups[1].Value.Split(new[] {": "}, StringSplitOptions.None))
                    .Where(dictElGroup =>
                        dictElGroup.Length == 2)
                    .ToDictionary(
                        dictElGroup => dictElGroup[0],
                        dictElGroup => dictElGroup[1]));

            return httpRequest;
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
    }

}