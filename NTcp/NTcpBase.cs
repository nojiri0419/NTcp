using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
//using System.Threading.Tasks;

namespace NTcp
{
    public class NTcpBase
    {
        protected bool IsConnected(TcpClient client)
        {
            return client.Connected && IsConnected(client.Client);
        }

        protected bool IsConnected(Socket socket)
        {
            return !(socket.Poll(1000, SelectMode.SelectRead) && socket.Available == 0);
        }

        protected void SendBytes(NetworkStream stream, byte[] buffer, string terminator)
        {
            using (MemoryStream memory = new MemoryStream())
            {
                byte[] bb1 = buffer;
                memory.Write(bb1, 0, bb1.Length);

                byte[] bb2 = Encoding.Default.GetBytes(terminator);
                memory.Write(bb2, 0, bb2.Length);

                byte[] bb3 = memory.ToArray();
                stream.Write(bb3, 0, bb3.Length);
            }
        }

        protected byte[][] ReceiveBytes(NetworkStream stream)
        {
            return ReceiveBytes(stream, 1024);
        }

        protected byte[][] ReceiveBytes(NetworkStream stream, int bufferSize)
        {
            byte[] buf = new byte[0];
            using (MemoryStream memory = new MemoryStream())
            {
                int length = 0;
                byte[] bb = new byte[bufferSize];
                while (stream.DataAvailable)
                {
                    length = stream.Read(bb, 0, bb.Length);
                    if (length > 0)
                    {
                        memory.Write(bb, 0, length);
                    }
                }
                buf = memory.ToArray();
            }

            IList<IList<byte>> listOfList = new List<IList<byte>>();
            IList<byte> list = null;
            foreach (byte b in buf)
            {
                if (list == null)
                {
                    list = new List<byte>();
                    listOfList.Add(list);
                }

                if (b == 0x0d || b == 0x0a)
                {
                    list = null;
                }
                else
                {
                    list.Add(b);
                }
            }
            return
                listOfList
                    .Where(o => o.Count > 0)
                    .Select(o => o.ToArray())
                    .ToArray();
        }

        protected string ToString(byte[] buffer)
        {
            StringBuilder sb = new StringBuilder();
            if (buffer != null)
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(' ');
                    }
                    sb.Append(buffer[i].ToString("x2"));
                }
            }
            return sb.ToString();
        }
    }
}
