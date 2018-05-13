﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Web.Script.Serialization;

namespace kinGUI
{
    class ClientConn
    {
        private static ClientConn singletonClient;

        private TcpClient client;

        public event EventHandler<CommandRecievedEventArgs> OnCommandRecieved;

        //private readonly string ip = "192.168.5.180";
        private readonly string ip = "127.0.0.1";
        private readonly int port = 8888;

        public static ClientConn Instance
        {
            get
            {
                if (singletonClient == null)
                {
                    singletonClient = new ClientConn();
                }
                return singletonClient;
            }
        }

        private ClientConn()
        {
            try
            {
                IPEndPoint ep = new IPEndPoint(IPAddress.Parse(ip), port);
                this.client = new TcpClient();
                this.client.Connect(ep);
                this.ReadMesagge();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message.ToString());
            }
        }

        public void sendMessage(string msg)
        {
            new Task(() =>
            {
                using (NetworkStream stream = client.GetStream())
                using (StreamWriter writer = new StreamWriter(stream))
                {
                    //string args = JsonConvert.SerializeObject(e);
                    writer.Write(msg);
                }
            }).Start();
        }

        public void ReadMesagge()
        {
            new Task(() =>
            {
                string arg;
                //byte[] message = new byte[4096];
                //int bytesRead;
                NetworkStream stream = this.client.GetStream();
                StreamReader reader = new StreamReader(stream);

                while (client.Connected)
                {
                    //bytesRead = stream.Read(message, 0, 4096);
                    //arg = Encoding.ASCII.GetString(message, 0, 4096);
                    arg = reader.ReadLine();
                  //  stream.Flush();

                    var serializer = new JavaScriptSerializer();
                    CommandRecievedEventArgs e = serializer.Deserialize<CommandRecievedEventArgs>(arg);

                    OnCommandRecieved?.Invoke(this, e);
                }


            }).Start();
        }

        public void close()
        {
            //this.client.Shutdown(SocketShutdown.Both);
            this.client.Close();
        }
    }
}
