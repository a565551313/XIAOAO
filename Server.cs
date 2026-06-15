using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace 笑傲西游
{
    public class Server
    {
        private Socket m_SocketServer;
        private static Server serverMsg;
        public bool state = true;
        public static Server ServerMsg
        {
            get { return serverMsg ?? (serverMsg = new Server()); }
        }
        private Server()
        {

        }
        public void start(string m_ServerIP)
        {
            m_SocketServer = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            m_SocketServer.Bind(new IPEndPoint(IPAddress.Parse(m_ServerIP), Convert.ToInt32(Program.frm1.textBox49.Text)));
            m_SocketServer.Listen(1000);
            //Program.frm1.全局ip = m_SocketServer.LocalEndPoint.ToString();
            Program.frm1.print(string.Format("启动网关:{0}成功", Program.frm1.全局ip));
            state = false;
            Thread mThread = new Thread(ListenClientCallback);
            mThread.IsBackground = true;
            mThread.Start();
        }

        internal static string MapPath(string v)
        {
            throw new NotImplementedException();
        }

        private void ListenClientCallback()
        {
            while (true)
            {

                Socket socket = m_SocketServer.Accept();
                string lsIP = ((IPEndPoint)socket.RemoteEndPoint).Address.ToString();
                if (!Client.start)
                {
                    Program.frm1.SyncContext.Post(Program.frm1.print, "还未连接到服务器:" + lsIP);
                }
                else if (Program.frm1.listBox5.Items.Contains(lsIP))
                {

                }
                else
                {
                    if (Program.frm1.checkBox13.Checked)
                    {
                        Program.frm1.listBox5.Items.Add(((IPEndPoint)socket.RemoteEndPoint).Address.ToString());

                    }
                    else {
                        if (Program.frm1.checkBox12.Checked)
                        {
                        
                            Program.frm1.SyncContext.Post(Program.frm1.print, string.Format("客户端:{0}连接服务器", socket.RemoteEndPoint.ToString()));
                        }
                        ClientSocket.id++;
                        Program.frm1.SyncContext.Post(Program.frm1.Adduser, new ClientSocket(socket));
                    }
      
                }
            }
        }
    }
}
