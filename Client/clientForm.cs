using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Client
{
    public partial class Client : Form
    {
        public Client()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            connectClient(); //connect right from the begining
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.ActiveControl = tbMessage;
        }

        private void bSend_Click(object sender, EventArgs e)
        {
            sendMessage();
            addMessage(tbMessage.Text);
            tbMessage.Text = "";
        }

        IPEndPoint IP;
        Socket client;

        void connectClient()
        {
            IP = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 6969);
            client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

            try
            {
                client.Connect(IP);
            }
            catch
            {
                MessageBox.Show("Can't connect!","Error",MessageBoxButtons.OK);
                return;
            }

            Thread Listen = new Thread(receiveMessage);
            Listen.IsBackground = true;
            Listen.Start();

        }

        void disconnectClient()
        {
            client.Close();
        }

        void sendMessage()
        {
            if(tbMessage.Text != string.Empty)
                client.Send(breakDown(tbMessage.Text));
        }

        void receiveMessage()
        {
            try
            {
                while (true)
                {
                    byte[] data = new byte[5120000];
                    client.Receive(data);

                    string message = (string)putTogether(data);
                    addMessage(message);
                }
            }
            catch
            {
                Close();
            }
            
        }

        void addMessage(string message)
        {
            lvMessage.Items.Add(new ListViewItem() { Text = message });
        }

        //breakdown an object into a byte array
        byte[] breakDown(object obj)
        {
            MemoryStream stream = new MemoryStream();
            BinaryFormatter formatter = new BinaryFormatter();

            formatter.Serialize(stream, obj);
            return stream.ToArray();
        }

        //put together byte array to form an object
        object putTogether(byte[] data)
        {
            MemoryStream stream = new MemoryStream(data);
            BinaryFormatter formatter = new BinaryFormatter();

            return formatter.Deserialize(stream);
        }

        private void Client_FormClosed(object sender, FormClosedEventArgs e)
        {
            disconnectClient();
        }
    }
}
