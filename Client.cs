using GameServerApp.Common;
using LuaTableToCsSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace 笑傲西游
{
    class Client
    {
 

        public static bool start;
        //接受数据包的缓存区
        private byte[] m_ReceiveBuffer = new byte[4096];
        //接受数据包的缓存数据流
        private MMO_MemoryStream m_ReceiveMS = new MMO_MemoryStream();
        private Socket clientSoket;
        //接收消息队列
        private Queue<byte[]> m_ReceiveQueue = new Queue<byte[]>();

        //检查发送队列的委托
        private Action m_CheckSendQueue;
        //发送消息队列
        private Queue<byte[]> m_SendQueue = new Queue<byte[]>();
        private static Client clientMsg;
        private Thread m_ReveiveThread;

        public static Client ClientMsg
        {
            get { return clientMsg ?? (clientMsg = new Client()); }
            set { clientMsg = value; }
        }

        public int GMID { get; internal set; }

        private Client()
        {

        }
        #region 连接Soket服务器
        /// <summary>
        /// 连接服务器
        /// </summary>
        public void Connect()
        {
            //u3d关闭   if (clientSoket != null && clientSoket.Connected) return; Client.shutdown（SocketShutdown.Both） Client.Close（）
            //如果clientSoket存在 并且 处于连接状态 返回

            if (clientSoket != null && clientSoket.Connected) return;

            try
            {
                clientSoket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
 if(Program.frm1.debug)
                clientSoket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), Convert.ToInt32(Program.frm1.textBox48.Text)));
else

                clientSoket.Connect(new IPEndPoint(IPAddress.Parse("127.0.0.1"), Convert.ToInt32(Program.frm1.textBox48.Text)));


                m_CheckSendQueue = OnCheckSendQueueCallBack;
                m_ReveiveThread = new Thread(ReceiveMsg);
                m_ReveiveThread.IsBackground = true;
                m_ReveiveThread.Start();
                if (Program.frm1.checkBox12.Checked)
                    Program.frm1.SyncContext.Post(Program.frm1.print, string.Format("连接到{0}服务器成功", clientSoket.RemoteEndPoint));
                start = true;
                Program.frm1.SyncContext.Post(Program.frm1.Status, "连接成功");
            }
            catch (Exception ex)
            {
                if (Program.frm1.checkBox12.Checked)
                    Program.frm1.SyncContext.Post(Program.frm1.print, string.Format("连接失败:原因{0}", ex.Message));
                Program.frm1.SyncContext.Post(Program.frm1.Status, "连接断开");
                start = false;
            }
        }
        #endregion
        #region  检查队列的委托回调
        /// <summary>
        /// 检查队列的委托回调
        /// </summary>
        private void OnCheckSendQueueCallBack()
        {
            lock (m_SendQueue)
            {
                //如果队列中有数据包 就发送数据包
                if (m_SendQueue.Count > 0)
                {
                    Send(m_SendQueue.Dequeue());
                }
            }
        }
        #endregion
        #region 封装Buffer
        /// <summary>
        /// 封装Buffer
        /// </summary>
        /// <param name="data">需要处理的Buff</param>
        /// <returns>封装好的Buff</returns>
        private byte[] MakeData(int num, string msg)
        {

            //Trace.Write(msg);
            byte[] retBuffer = null;
            using (MMO_MemoryStream ms = new MMO_MemoryStream())
            {
                ms.WriteInt(Program.frm1.flag);
                ms.WriteInt(0);

                byte[] numbers = BitConverter.GetBytes(1);
                byte[] number = BitConverter.GetBytes(num);
                byte[] msgByte = Encoding.Default.GetBytes(msg);
                byte[] resArr = new byte[numbers.Length + number.Length + msgByte.Length];
                ms.WriteInt(resArr.Length);
                numbers.CopyTo(resArr, 0);
                number.CopyTo(resArr, numbers.Length);
                msgByte.CopyTo(resArr, number.Length + numbers.Length);

/*                int k = 0;
                byte[] decodeBuffer = new byte[resArr.Length];
                for (int i = 0; i < resArr.Length; i++)
                {
                    decodeBuffer[i] = (byte)(resArr[i] ^ Program.frm1.keyArray[k]);
                    k++;
                    if (k == Program.frm1.keyArray.Length)
                    {
                        k = 0;
                    }
                }*/
                ms.Write(resArr, 0, resArr.Length);
                retBuffer = ms.ToArray();
            }
            return retBuffer;
        }
        #endregion
        #region send发送数据包到服务器
        /// <summary>
        /// 真正的发送数据包到服务器
        /// </summary>
        /// <param name="buffer"></param>
        private void Send(byte[] buffer)
        {
            try
            {
                clientSoket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, SendCallBack, clientSoket);
            }
            catch (Exception ex)
            {
                if (Program.frm1.checkBox12.Checked)
                    Program.frm1.SyncContext.Post(Program.frm1.print, string.Format("连接失败:原因{0}", ex.Message));
                Program.frm1.SyncContext.Post(Program.frm1.Status, "连接断开");
                start = false;
            }

        }
        #endregion
        #region 发送数据包的回调
        /// <summary>
        /// 发送数据包的回调
        /// </summary>
        /// <param name="ar"></param>
        private void SendCallBack(IAsyncResult ar)
        {
            clientSoket.EndSend(ar);
            //继续检查队列
            OnCheckSendQueueCallBack();
        }
        #endregion
        #region 发送消息 把消息加入队列
        /// <summary>
        ///发送消息 把消息加入队列
        /// </summary>
        /// <param name="msg">包体</param>
        public void SendMsg(int num, string msg)
        {

            //Trace.WriteLine(num.ToString() + "_" + msg);
            //得到封装好的数据包
            byte[] senBuffer = MakeData(num, msg);
            lock (m_SendQueue)
            {
                //把数据包加入队列
                m_SendQueue.Enqueue(senBuffer);
                //启动委托
                m_CheckSendQueue.BeginInvoke(null, null);
            }


        }
        #endregion
        #region 接收数据
        /// <summary>
        /// 接收数据
        /// </summary>
        private void ReceiveMsg()
        {
            //异步接收数据
            clientSoket.BeginReceive(m_ReceiveBuffer, 0, m_ReceiveBuffer.Length, SocketFlags.None, ReceiveCalBback, clientSoket);
        }
        /// <summary>
        /// 接收数据回调
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveCalBback(IAsyncResult ar)
        {
            string rsck = "";
            try
            {
                int len = clientSoket.EndReceive(ar);
                if (len > 0)
                {
                    //已经接收到数据
                    //把接收到的数据 写入缓冲区的尾部 
                    m_ReceiveMS.Position = m_ReceiveMS.Length;
                    //把指定长度的字节 写入数据流中
                    m_ReceiveMS.Write(m_ReceiveBuffer, 0, len);
                    //判读流中包体的长度
                    if (m_ReceiveMS.Length > 2)
                    {
                        //进行循环 拆分数据包  粘包处理
                        while (true)
                        {
                            //把数据流指针放到0处
                            m_ReceiveMS.Position = 0;
                            //计算包体的长度
                            //ushort msgLen = m_ReceiveMS.ReadUShort();
                            int flag = m_ReceiveMS.ReadInt();//判断识别14138
                            int seq = m_ReceiveMS.ReadInt();//判断包序号
                            int msgLen = m_ReceiveMS.ReadInt();//识别包体长度
                            if (flag == Program.frm1.flag)
                            {
                                //计算总包的长度
                                int countMsgLen = 12 + msgLen;
                                //判读流中数据 是否完整
                                if (m_ReceiveMS.Length >= countMsgLen)
                                {
                                    byte[] buffer = new byte[msgLen];
                                    m_ReceiveMS.Position = 12;
                                    m_ReceiveMS.Read(buffer, 0, msgLen);
                                    //解密数据
/*                                    int k = 0;
                                    byte[] decodeBuffer = new byte[buffer.Length];
                                    for (int i = 0; i < buffer.Length; i++)
                                    {

                                        decodeBuffer[i] = (byte)(buffer[i] ^ Program.frm1.keyArray[k]);
                                        k++;
                                        if (k == Program.frm1.keyArray.Length)
                                        {
                                            k = 0;
                                        }
                                    }*/
                                    using (MMO_MemoryStream ms2 = new MMO_MemoryStream(buffer))
                                    {
                                        ms2.Position = 4;
                                        int num = ms2.ReadInt();
                                        int numID = ms2.ReadInt();
                                        string msg = ms2.ReadDefaultString(msgLen - 12);
                                        rsck = num.ToString() + "\n\t";
                                        rsck = rsck + numID.ToString() + "\n\t";
                                        rsck = rsck + msg + "\n\t";
                                        DataProcessing(num, numID, msg);
                                    }
                                    //检查剩余包
                                    int remainLen = (int)m_ReceiveMS.Length - countMsgLen;
                                    if (remainLen > 0)
                                    {
                                        m_ReceiveMS.Position = countMsgLen;
                                        byte[] remainBuffer = new byte[remainLen];
                                        m_ReceiveMS.Position = countMsgLen;
                                        m_ReceiveMS.Read(remainBuffer, 0, remainLen);
                                        //清空数据流
                                        m_ReceiveMS.Position = 0;
                                        m_ReceiveMS.SetLength(0);
                                        //把剩余数据重新写入数据流
                                        m_ReceiveMS.Write(remainBuffer, 0, remainBuffer.Length);
                                        remainBuffer = null;
                                    }
                                    else
                                    {
                                        //刚好一个完整包
                                        //清空数据流
                                        m_ReceiveMS.Position = 0;
                                        m_ReceiveMS.SetLength(0);
                                        break;
                                    }
                                }
                                else
                                {
                                    //没收到完整包
                                    break;
                                }

                            }
                            else
                            {
                                //没收到完整包
                                break;
                            }
                        }
                    }
                    //进行下一次接收数据包
                    ReceiveMsg();
                }
                else
                {
                    //客户端断开连接
                    if (Program.frm1.checkBox12.Checked)
                        Program.frm1.SyncContext.Post(Program.frm1.print, "与服务器断开连接");
                    Program.frm1.SyncContext.Post(Program.frm1.Status, "连接断开");
                    start = false;


                }
            }
            catch (Exception ex)
            {
                //客户端断开连接
                if (Program.frm1.checkBox12.Checked)
                    Program.frm1.SyncContext.Post(Program.frm1.print, string.Format("与服务器断开连接1,原因{0}", ex.Message));
                Program.frm1.SyncContext.Post(Program.frm1.print, rsck);
                Program.frm1.SyncContext.Post(Program.frm1.Status, "连接断开");
                start = false;


            }

        }

        #endregion
        #region 数据处理
        private void DataProcessing(int num, int ID, string msg)
        {
            //Trace.WriteLine("ssssssssssssssssssssss");
            Trace.WriteLine("ID："+ID.ToString());
            Trace.WriteLine(num.ToString() + "_" + msg);
/*            if (num == 991) //发送网关消息
            {
                Program.frm1.SyncContext.Post(Program.frm1.print, msg);
                return;
            }
            else if (num == 992) //聊天信息
            {
                Program.frm1.textBox7.AppendText(msg + "\r\n");
                return;
            }*/
/*            else if (num == 993) //添加假人
            {
                if (Program.frm1.checkBox12.Checked)
                    Program.frm1.SyncContext.Post(Program.frm1.MpaUpdata, msg);
               
                return;
            }*/
            if (Program.frm1.checkBox1.Checked)
            {
                Program.frm1.SyncContext.Post(Program.frm1.print, string.Format("服务端:序号={0}id={1}封包={2}", num, ID, msg));
            }
            if (false) { 
            
            }
            /*            if (num == 381990) //广播消息
                        {
                            for (int i = 0; i < Program.frm1.AllUser.Count; i++)
                            {
                                Program.frm1.AllUser[i].SendMsg(21, msg);
                            }

                        }
                        else if (num == 99997)
                        {

                            for (int i = 0; i < Program.frm1.AllUser.Count; i++)
                            {
                                Program.frm1.AllUser[i].SendMsg(num, msg);
                            }
                        }
                        else if (num == 990)
                        {
                            //if (Program.frm1.AllUserTable.ContainsKey(Client.ClientMsg.GMID))
                            //{
                            //    Program.frm1.AllUserTable[Client.ClientMsg.GMID].SendMsg(num, msg);

                            //}

                        }*/
            else
            {
                switch (num)
                {
                    /*                    case 30:
                                            Usermsg usermsg = JsonConvert.DeserializeObject<Usermsg>(msg);
                                            if (Program.frm1.AllUserTable.ContainsKey(ID))
                                            {
                                                Program.frm1.AllUserTable[ID].ID = usermsg.id;
                                                Program.frm1.AllUserTable[ID].账号 = usermsg.User;
                                                Program.frm1.AllUserTable[ID].名称 = usermsg.Name;
                                                Program.frm1.AllUserTable[ID].状态 = ClientSocket.State.登入游戏;
                                            }
                                            break;*/
                    case 997:
                        Trace.WriteLine(msg.Substring(13, msg.Length - 13 - 15));
                        SharpluaTable lua = new SharpluaTable();
                        var dic = lua.Parse(msg.Substring(13, msg.Length - 13 - 15));
                        SharpluaTable lua1 = new SharpluaTable();
                        //var dic1 = lua1.Parse(dic["内容"]);
                        /*                        Trace.WriteLine(dic1["id"]);
                                                Trace.WriteLine(dic1["账号"]);
                                                Trace.WriteLine(dic1["名称"]);*/
                        if (dic.ContainsKey("内容")) {
                            var dic1 = lua1.Parse(dic["内容"]);
                            var mc = "";
                            if (dic1.ContainsKey("名称")) {
                                mc = dic1["名称"];
                            }
                            if (Program.frm1.AllUserTable.ContainsKey(ID))
                            {
                                //Program.frm1.AllUserTable[ID].ID = int.Parse(dic1["id"]);
                                //Program.frm1.AllUserTable[ID].账号 = dic1["账号"];
                                Program.frm1.AllUserTable[ID].名称 = mc;
                                Program.frm1.AllUserTable[ID].状态 = ClientSocket.State.登入游戏;
                            }
                        }
                        break;
                    case 998:
                        if (Program.frm1.AllUserTable.ContainsKey(ID))
                        {
                            Program.frm1.SyncContext.Post(Program.frm1.print, string.Format("客户端:{0}断开连接,原因{1}", Program.frm1.AllUserTable[ID].IP, "服务端请求"));
                            if (Program.frm1.AllUserTable.ContainsKey(ID))
                            {
                                Program.frm1.AllUserTable[ID].断开方式 = "服务端";
                                Program.frm1.AllUserTable[ID].SendMsg(998, msg);
                            }
                            //Server.ServerMsg.AllUser.Remove(Server.ServerMsg.AllUserTable[Msg.id]);
                            //Server.ServerMsg.AllUserTable.Remove(Msg.id);

                        }
                        //Trace.WriteLine("服务端人："+Program.frm1.AllUser.Count.ToString());
                        //Program.frm1.SyncContext.Post(Program.frm1.OnlineNumber, Program.frm1.AllUser.Count);
                        break;

                    /*                    case 99999:
                                            if (Program.frm1.AllUserTable.ContainsKey(ID))
                                            {
                                                Program.frm1.SyncContext.Post(Program.frm1.print, string.Format("客户端:{0}断开连接,原因{1}", Program.frm1.AllUserTable[ID].IP, "服务端请求"));
                                                //Server.ServerMsg.AllUser.Remove(Server.ServerMsg.AllUserTable[Msg.id]);
                                                //Server.ServerMsg.AllUserTable.Remove(Msg.id);

                                            }

                                            Program.frm1.SyncContext.Post(Program.frm1.OnlineNumber, Program.frm1.AllUser.Count);
                                            break;
                    */
                    default:
                        /*                        if (Program.frm1.AllUserTable.ContainsKey(ID))
                                                {
                                                    Trace.WriteLine(num.ToString() + "_" + msg);
                                                    Program.frm1.AllUserTable[ID].SendMsg(num, msg);
                                                }*/
                        ClientSocket tempclient;
                        if (Program.frm1.AllUserTable.TryGetValue(ID,out tempclient))
                        {
                            Trace.WriteLine(num.ToString() + "_" + msg);
                            tempclient.SendMsg(num, msg);
                            //Program.frm1.AllUserTable[ID].SendMsg(num, msg);
                        }
                        break;
                }
            }
        }
        #endregion
    }
}
