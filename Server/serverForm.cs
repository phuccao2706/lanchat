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

namespace Server
{
    public partial class serverForm : Form
    {
        public serverForm()
        {
            InitializeComponent();
            CheckForIllegalCrossThreadCalls = false;
            createServer();
        }

        IPEndPoint IP;
        Socket server;
        List<Socket> clientList;

        void createServer()
        {
            clientList = new List<Socket>();
            IP = new IPEndPoint(IPAddress.Any, 6969);
            server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);

            server.Bind(IP);

            Thread watingOnClients = new Thread(() => {
                try
                {
                    while (true)
                    {
                        server.Listen(100);
                        Socket client = server.Accept();
                        clientList.Add(client);

                        Thread receive = new Thread(receiveMessage);
                        receive.IsBackground = true;
                        receive.Start(client);
                    }
                }
                catch
                {
                    IP = new IPEndPoint(IPAddress.Any, 6969);
                    server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                }
                
            });
            watingOnClients.IsBackground = true;
            watingOnClients.Start();
        }

        void disconnectServer()
        {
            server.Close();
        }

        void sendMessage(Socket client)
        {
            if (tbMessage.Text != string.Empty)
                client.Send(breakDown(tbMessage.Text));
        }

        void receiveMessage(object obj)
        {
            Socket client = obj as Socket;
            try
            {
                while (true)
                {
                    byte[] data = new byte[5120000];
                    client.Receive(data);

                    string message = (string)putTogether(data);
                    addMessage(message);

                    foreach(Socket item in clientList) //send the message to other clients
                    {
                        if(item != client) //except for the one who send the message
                        {
                            item.Send(breakDown(message));
                        }
                    }
                }
            }
            catch
            {
                clientList.Remove(client);
                client.Close();
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

        private void serverForm_Load(object sender, EventArgs e)
        {
            this.ActiveControl = tbMessage;
        }

        private void serverForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            disconnectServer();
        }

        private void bSend_Click(object sender, EventArgs e)
        {
            foreach (Socket item in clientList)
            {
                sendMessage(item);
            }
            addMessage(tbMessage.Text);
            tbMessage.Text = "";
        }
    }
}
