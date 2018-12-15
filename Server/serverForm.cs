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

            var button = new ToolStripButton("kick");
            button.Click += kickToolStripMenuItem_Click;
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
            {
                string[] dataPackage = new string[2];
                dataPackage[0] = "message";
                dataPackage[1] = "Server: " + tbMessage.Text;
                client.Send(breakDown(dataPackage));
            }
                
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

                    string[] message = (string[])putTogether(data);
                    IPEndPoint clientIP = client.RemoteEndPoint as IPEndPoint;

                    switch (message[0])
                    {
                        case "message":
                            addMessage(message[2] + " (" + clientIP.ToString() + "): " + message[1]);
                            foreach (Socket item in clientList) //send the message to other clients
                            {
                                if (item != client) //except for the one who send the message
                                {
                                    string[] dataPackage = new string[2];
                                    dataPackage[0] = "message";
                                    dataPackage[1] = message[2] + " (" +  clientIP.ToString() + "): " + message[1];
                                    item.Send(breakDown(dataPackage));
                                }
                            }
                            
                            break;
                        case "left":
                            addMessage(">>" + message[2] + " (" + clientIP.ToString() + ") just left!");
                            int temp = clientList.IndexOf(client);
                            lvClient.Items.RemoveAt(temp);
                            clientList.RemoveAt(temp);

                            foreach (Socket item in clientList) //send the message to other clients
                            {
                                if (item != client) //except for the one who send the message
                                {
                                    string[] dataPackage = new string[2];
                                    dataPackage[0] = "announcement";
                                    dataPackage[1] = message[2] + " (" + clientIP.ToString() + ") just left!";
                                    item.Send(breakDown(dataPackage));
                                }
                            }
                            break;
                        case "connect":
                            addMessage(">>" + message[2] + "(" + clientIP.ToString() + ") just connected!");

                            
                            loadListClients(message[2], clientIP, client);

                            foreach (Socket item in clientList) //send the message to other clients
                            {
                                if (item != client) //except for the one who send the message
                                {
                                    string[] dataPackage = new string[2];
                                    dataPackage[0] = "announcement";
                                    dataPackage[1] = message[2] + " (" + clientIP.ToString() + ") just connected!";
                                    item.Send(breakDown(dataPackage));
                                }
                            }
                            break;
                        case "kicked":
                            addMessage(message[2] + " (" + clientIP.ToString() + ") has been kicked");
                            foreach (Socket item in clientList) //send the message to other clients
                            {
                                if (item != client) //except for the one who send the message
                                {
                                    string[] dataPackage = new string[2];
                                    dataPackage[0] = "message";
                                    dataPackage[1] = message[2] + " has been kicked";
                                    item.Send(breakDown(dataPackage));
                                }
                            }
                            break;
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
            lvMessage.AppendText(message + "\r\n");
        }

        void addBoldMessage(string message)
        {

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
            addMessage(">> Server: " + tbMessage.Text);
            if (tbMessage.Text != "")
            {
                foreach (Socket item in clientList)
                {
                    sendMessage(item);
                }
                tbMessage.Text = "";
            }
            
        }

        void loadListClients(string username, IPEndPoint clientIP, Socket client)
        {
            string[] subItems = new string[3];
            
            subItems[1] = username;
            subItems[2] = clientIP.ToString()/*.Split(':')[0]*/;

            ListViewItem lvi = new ListViewItem(subItems);
            lvi.Tag = client;

            lvClient.Items.Add(lvi);
        }

        private void lvClient_ColumnWidthChanging(object sender, ColumnWidthChangingEventArgs e)
        {
            //e.Cancel = true;
            //e.NewWidth = lvClient.Columns[e.ColumnIndex].Width;
        }

        private void lvMessage_TextChanged(object sender, EventArgs e)
        {
            lvMessage.SelectionStart = lvMessage.Text.Length;
            lvMessage.ScrollToCaret();
        }
        

        private void kickToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //if (lvClient.SelectedItems.Count > 0)
            //{
            //    int temp = lvClient.Items.IndexOf(lvClient.SelectedItems[0]);
            //    Socket client = clientList.ElementAt(temp);
            //    client.Send(breakDown("something hihi"));
            //}
            if (lvClient.SelectedIndices.Count > 0)
            {
                int temp = lvClient.SelectedIndices[0];
                Socket client = clientList.ElementAt(temp);

                string[] dataPackage = new string[2];
                dataPackage[0] = "kick";
                dataPackage[1] = "";
                client.Send(breakDown(dataPackage));
            }
            
        }
    }
}
