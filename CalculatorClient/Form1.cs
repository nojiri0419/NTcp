using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

using NTcp;

namespace CalculatorClient
{
    public partial class Form1 : Form
    {
        private NTcpClient client;

        public Form1()
        {
            InitializeComponent();

            Font f1 = textBox1.Font;
            textBox1.Font = new Font(FontFamily.GenericMonospace, f1.Size);

            Font f2 = textBox1.Font;
            textBox2.Font = new Font(FontFamily.GenericMonospace, f2.Size);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                client = new NTcpClient("127.0.0.1", 12345);
                client.Open();
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
                client.Close();
                client = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                string command = textBox1.Text;
                if (!String.IsNullOrEmpty(command))
                {
                    client.SendBytes(Encoding.Default.GetBytes(command));
                    textBox2.AppendText("sent.: " + command + Environment.NewLine);

                    Thread.Sleep(100);

                    byte[][] bbb = client.ReceiveBytes();
                    foreach (byte[] bb in bbb)
                    {
                        string response = Encoding.Default.GetString(bb);
                        textBox2.AppendText("received.: " + response + Environment.NewLine);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
