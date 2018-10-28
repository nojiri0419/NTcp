using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
//using System.Threading.Tasks;

namespace NTcp
{
    public class NTcpClient : NTcpBase
    {
        private IPEndPoint ipEndPoint;
        private string terminator;
        private TcpClient client;

        public NTcpClient(string hostname, int port)
            : this(hostname, port, "\n")
        {
        }

        public NTcpClient(string hostname, int port, string terminator)
            : this(IPAddress.Parse(hostname), port, terminator)
        {
        }

        public NTcpClient(IPAddress ipAddress, int port)
            : this(ipAddress, port, "\n")
        {
        }

        public NTcpClient(IPAddress ipAddress, int port, string terminator)
            : this(new IPEndPoint(ipAddress, port), terminator)
        {
        }

        public NTcpClient(IPEndPoint ipEndPoint)
            : this(ipEndPoint, "\n")
        {
        }

        public NTcpClient(IPEndPoint ipEndPoint, string terminator)
        {
            this.ipEndPoint = ipEndPoint;
            this.terminator = terminator;
        }

        public void Open()
        {
            client = new TcpClient();
            client.Connect(ipEndPoint);

#if DEBUG
            Console.WriteLine("DEBUG: open.");
#endif
        }

        public void Close()
        {
            NetworkStream stream = client.GetStream();
            if (stream != null)
            {
                stream.Close();
            }
            client.Close();
            client = null;

#if DEBUG
            Console.WriteLine("DEBUG: close.");
#endif
        }

        public void SendBytes(byte[] command)
        {
            if (!IsConnected(client))
            {
                Close();
                Open();
            }

            NetworkStream stream = client.GetStream();
            if (stream != null)
            {
                SendBytes(stream, command, terminator);

#if DEBUG
                Console.WriteLine("DEBUG: send.: " + ToString(command));
#endif
            }
        }

        public byte[][] ReceiveBytes()
        {
            if (!IsConnected(client))
            {
                Close();
                Open();
            }

            byte[][] responses = new byte[0][];
            NetworkStream stream = client.GetStream();
            if (stream != null)
            {
                responses = ReceiveBytes(stream);
#if DEBUG
                foreach (byte[] response in responses)
                {
                    Console.WriteLine("DEBUG: recv.: " + ToString(response));
                }
#endif
            }
            return responses;
        }
    }
}
