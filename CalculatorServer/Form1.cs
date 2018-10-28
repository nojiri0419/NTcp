using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

using NTcp;

using CalculatorCore;

namespace CalculatorServer
{
    public partial class Form1 : Form
    {
        private Calculator calculator;
        private NTcpListener listener;

        private delegate void AppendTextDelegate(string text);

        public Form1()
        {
            InitializeComponent();

            Font f1 = textBox1.Font;
            textBox1.Font = new Font(FontFamily.GenericMonospace, f1.Size);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                calculator = new Calculator();

                listener = new NTcpListener(12345);
                listener.OnStarted      += OnStarted;
                listener.OnStopped      += OnStopped;
                listener.OnReceived     += OnReceived;
                listener.OnConnected    += OnConnected;
                listener.OnDisconnected += OnDisconnected;
                listener.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            try
            {
                listener.Stop();
                listener = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void OnStarted()
        {
            AppendText("started." + Environment.NewLine);
        }

        private void OnStopped()
        {
            AppendText("stopped." + Environment.NewLine);
        }

        private byte[] OnReceived(byte[] command)
        {
            string s1 = Encoding.Default.GetString(command);
            AppendText("received.: " + s1 + Environment.NewLine);

            string s2;
            MatchCollection matches = Regex.Matches(s1, "^(\\d+)( *)([+-])( *)(\\d+)$");
            if (matches.Count == 1)
            {
                GroupCollection groups = matches[0].Groups;
                int x = Int32.Parse(groups[1].Value);
                int y = Int32.Parse(groups[5].Value);
                if (groups[3].Value == "+")
                {
                    int z = calculator.Add(x, y);
                    s2 = z.ToString();
                }
                else if (groups[3].Value == "-")
                {
                    int z = calculator.Subtract(x, y);
                    s2 = z.ToString();
                }
                else
                {
                    s2 = "ERROR";
                }
            }
            else
            {
                s2 = "ERROR";
            }
            byte[] response = Encoding.Default.GetBytes(s2);
            AppendText("sent.: " + s2 + Environment.NewLine);

            return response;
        }

        private void OnConnected(EndPoint localEndPoint, EndPoint remoteEndPoint)
        {
            AppendText("connected.: local=" + localEndPoint + ",remote=" + remoteEndPoint + Environment.NewLine);
        }

        private void OnDisconnected(EndPoint localEndPoint, EndPoint remoteEndPoint)
        {
            AppendText("disconnected.: local=" + localEndPoint + ",remote=" + remoteEndPoint + Environment.NewLine);
        }

        private void AppendText(string text)
        {
            if (InvokeRequired)
            {
                Invoke(new AppendTextDelegate(AppendText), text);
            }
            else
            {
                textBox1.AppendText(text);
            }
        }
    }
}
