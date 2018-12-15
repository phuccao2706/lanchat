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
        string username;
        public Client()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            connectClient(); //connect right from the begining
            
        }
      
        private void Form1_Load(object sender, EventArgs e)
        {
            Login loginForm = new Login();
            loginForm.ShowDialog();
            

            if (loginForm.DialogResult == DialogResult.OK)
                username = loginForm.username;

            Text = "Connected as: " + username;

            string[] dataPackage = new string[3];
            dataPackage[0] = "connect";
            dataPackage[1] = "";
            dataPackage[2] = username;
            client.Send(breakDown(dataPackage));

            this.ActiveControl = tbMessage;
        }

        private void bSend_Click(object sender, EventArgs e)
        {
            if(tbMessage.Text != "")
            {
                sendMessage();
                addMessage("You: " + tbMessage.Text);
                tbMessage.Text = "";
            }
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
            {
                IPEndPoint clientIP = client.RemoteEndPoint as IPEndPoint;

                string[] dataPackage = new string[3];
                dataPackage[0] = "message";
                dataPackage[1] = tbMessage.Text;
                dataPackage[2] = username;
                client.Send(breakDown(dataPackage));
            }
        }

        void receiveMessage()
        {
            try
            {
                while (true)
                {
                    byte[] data = new byte[5120000];
                    client.Receive(data);

                    string[] message = (string[])putTogether(data);

                    switch (message[0])
                    {
                        case "message":
                            addMessage(message[1]);
                            break;
                        case "announcement":
                            addMessage("Server: " + message[1]);
                            break;
                        case "kick":
                            string[] dataPackage = new string[3];
                            dataPackage[0] = "kicked";
                            dataPackage[1] = "";
                            dataPackage[2] = username;
                            client.Send(breakDown(dataPackage));
                            Application.Exit();
                            break;
                    }
                }
            }
            catch
            {
                Close();
            }
            
        }

        void addMessage(string message)
        {
            lvMessage.Text += message + "\r\n";
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
            string[] dataPackage = new string[3];
            dataPackage[0] = "left";
            dataPackage[1] = "";
            dataPackage[2] = username;
            client.Send(breakDown(dataPackage));

            disconnectClient();
        }

        private void bImage_Click(object sender, EventArgs e)
        {
            //OpenFileDialog openFileDialog1 = new OpenFileDialog();
            //openFileDialog1.Filter = "Images |*.bmp;*.jpg;*.png;*.gif;*.ico";
            //openFileDialog1.Multiselect = false;
            //openFileDialog1.FileName = "";
            //DialogResult result = openFileDialog1.ShowDialog();
            //if (result == DialogResult.OK)
            //{
            //    Image img = Image.FromFile(openFileDialog1.FileName);
            //    Clipboard.SetImage(img);
            //    lvMessage.Paste(img);
            //    lvMessage.Focus();
            //}
            //else
            //{
            //    lvMessage.Focus();
            //}

        }

        private void lvMessage_TextChanged(object sender, EventArgs e)
        {
            lvMessage.SelectionStart = lvMessage.Text.Length;
            lvMessage.ScrollToCaret();
        }
    }
}
