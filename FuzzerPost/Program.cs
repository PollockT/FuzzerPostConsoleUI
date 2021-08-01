using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace FuzzerPost
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] requestLines = File.ReadAllLines(args[0]);
            string[] parms = requestLines[requestLines.Length - 1].Split('&');
            string host = string.Empty;
            StringBuilder requestBuilder = new StringBuilder();

            foreach (string line in requestLines)
            {
                if (line.StartsWith("Host:"))
                {
                    host = line.Split(' ')[1].Replace("\r", string.Empty);
                    requestBuilder.Append(line + "\n");
                }
                string request = requestBuilder.ToString() + "\r\n";
                Console.WriteLine(request);
                IPEndPoint rhost = new IPEndPoint(IPAddress.Parse(host), 80);
                foreach (string parm in parms)
                {
                    using (Socket socket = new Socket(AddressFamily.InterNetwork,
                                                SocketType.Stream,
                                                ProtocolType.Tcp))
                    {
                        socket.Connect(rhost);
                        string val = parm.Split('=')[1];
                        string req = request.Replace("=" + val, "=" + val + "'");

                        byte[] reqBytes = Encoding.ASCII.GetBytes(req);
                        socket.Send(reqBytes);

                        byte[] buffer = new byte[socket.ReceiveBufferSize];

                        socket.Receive(buffer);
                        string response = Encoding.ASCII.GetString(buffer);
                        if (response.Contains("error in your SQL syntax"))
                        {
                            Console.WriteLine("Parameter " + parm + " seems vulnerable");
                            Console.Write(" to SQL injection with value: " + val + "'");
                        }
                    }
                }
            }            
        }
    }
}
