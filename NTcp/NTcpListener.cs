using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
//using System.Threading.Tasks;

namespace NTcp
{
    public class NTcpListener : NTcpBase
    {
        public delegate void StartedDelegate();
        public delegate void StoppedDelegate();
        public delegate byte[] ReceivedDelegate(byte[] command);
        public delegate void ConnectedDelegate(EndPoint localEndPoint, EndPoint remoteEndPoint);
        public delegate void DisconnectedDelegate(EndPoint localEndPoint, EndPoint remoteEndPoint);

        public event StartedDelegate OnStarted;
        public event StoppedDelegate OnStopped;
        public event ReceivedDelegate OnReceived;
        public event ConnectedDelegate OnConnected;
        public event DisconnectedDelegate OnDisconnected;

        private IList<TcpClient> clientList;

        private IPEndPoint ipEndPoint;
        private TcpListener listener;
        private string terminator;
        private int interval;

        private bool alive;
        private Thread runner;
        private Thread acceptor;

        public NTcpListener(int port)
            : this(port, "\n")
        {
        }

        public NTcpListener(int port, string terminator)
            : this(port, terminator, 100)
        {
        }

        public NTcpListener(int port, string terminator, int interval)
            : this(IPAddress.Any, port, terminator, interval)
        {
        }

        public NTcpListener(IPAddress ipAddress, int port)
            : this(ipAddress, port, "\n")
        {
        }

        public NTcpListener(IPAddress ipAddress, int port, string terminator)
            : this(ipAddress, port, terminator, 100)
        {
        }

        public NTcpListener(IPAddress ipAddress, int port, string terminator, int interval)
            : this(new IPEndPoint(ipAddress, port), terminator, interval)
        {
        }

        public NTcpListener(IPEndPoint ipEndPoint, string terminator, int interval)
        {
            this.clientList = null;

            this.ipEndPoint = ipEndPoint;
            this.listener   = null;
            this.terminator = terminator;
            this.interval = interval;

            this.alive    = false;
            this.runner   = null;
            this.acceptor = null;
        }

        public void Start()
        {
            clientList = new List<TcpClient>();

            listener = new TcpListener(ipEndPoint);
            listener.Start();

            alive = true;

            runner = new Thread(new ThreadStart(Run));
            runner.IsBackground = true;
            runner.Name = "Tcp Listener Runner";
            runner.Start();
            while (!runner.IsAlive)
            {
                Thread.Sleep(10);
            }

            acceptor = new Thread(new ThreadStart(Accept));
            acceptor.IsBackground = true;
            acceptor.Name = "Tcp Listener Acceptor";
            acceptor.Start();
            while (!acceptor.IsAlive)
            {
                Thread.Sleep(10);
            }

            if (OnStarted != null)
            {
                OnStarted.Invoke();
            }
        }

        public void Stop()
        {
            alive = false;

            acceptor.Abort();

            if (listener != null)
            {
                listener.Stop();
                listener = null;
            }

            while (runner.IsAlive)
            {
                Thread.Sleep(10);
            }

            lock (clientList)
            {
                foreach (TcpClient client in clientList)
                {
                    client.Close();
                }
            }

            if (OnStopped != null)
            {
                OnStopped.Invoke();
            }
        }

        private void Run()
        {
            while (alive)
            {
                try
                {
                    lock (clientList)
                    {
                        int index = 0;
                        while (index < clientList.Count)
                        {
                            if (!IsConnected(clientList[index]))
                            {
                                if (OnDisconnected != null)
                                {
                                    OnDisconnected.Invoke(clientList[index].Client.LocalEndPoint, clientList[index].Client.RemoteEndPoint);
                                }
                                clientList.RemoveAt(index);
                            }
                            else
                            {
                                index++;
                            }
                        }

                        foreach (TcpClient client in clientList)
                        {
                            NetworkStream stream = client.GetStream();
                            if (stream != null)
                            {
                                byte[][] commands = ReceiveBytes(stream);
                                if (OnReceived != null)
                                {
                                    foreach (byte[] command in commands)
                                    {
                                        byte[] response = OnReceived.Invoke(command);
                                        if (response != null && response.Length > 0)
                                        {
                                            SendBytes(stream, response, terminator);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
#if DEBUG
                    Console.WriteLine(ex);
#endif
                }
                finally
                {
                    Thread.Sleep(interval);
                }
            }
        }

        private void Accept()
        {
            while (alive)
            {
                try
                {
#if DEBUG
                    Console.WriteLine("DEBUG: accepting...");
#endif

                    TcpClient client = listener.AcceptTcpClient();
#if DEBUG
                    Console.WriteLine("DEBUG: accepted.");
#endif
                    if (OnConnected != null)
                    {
                        OnConnected.Invoke(client.Client.LocalEndPoint, client.Client.RemoteEndPoint);
                    }

                    lock (clientList)
                    {
                        clientList.Add(client);
                    }
                }
                catch (ThreadAbortException ex)
                {
#if DEBUG
                    Console.WriteLine("WARN: " + ex);
#endif
                }
                catch (Exception ex)
                {
#if DEBUG
                    Console.WriteLine(ex);
#endif
                }
            }
        }
    }
}
