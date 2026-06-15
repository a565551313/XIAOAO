
using GameServerApp.Common;
using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.OleDb;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace 笑傲西游
{
    
    public partial class Form1 : Form
    {
        static System.Threading.Timer timer = new System.Threading.Timer(TimerCallBack, null, Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan); //构建 Timer
        public BindingList<ClientSocket> AllUser { get; set; }
        private Dictionary<int, ClientSocket> allUserTable;
        //异或因子
        private byte[] xorScale = new byte[] { 99, 77,66, 138, 55, 23, 254, 109, 165, 90, 19, 41, 145, 201, 58, 55, 37, 254, 185, 165, 169, 19, 171, 38, 1, 99, 9, 86, 12, 74, 1, 215, 88, 64, 56, 22, 56 };//.data文件的xor加解密因子
        public Dictionary<int, ClientSocket> AllUserTable
        {
            get { return allUserTable; }
            set { allUserTable = value; }
        }
        private string CDKChoose = "";
        public SynchronizationContext SyncContext = null;
   
        private List<string> CDK = new List<string>();
        public string Notice;
/*        public int flag = 381990860;
        public string key = "FT@!xasd";*/



        //public string key = "fg*565#22@456";

        public int flag = 14138;
        public string 全局ip;
        public byte[] keyArray;
        private string pathFile= "./";
        public bool debug = false;
        public string fgf = "*-*";
        public string fgc = "@+@";

        public int RequestsNumber { get; internal set; }
        public Form1()
        {
            InitializeComponent();
        }
        #region 输出控制台
        public void print(object msg)
        {
            string Tempstirng = "->" + DateTime.Now.ToString() + "    " + msg.ToString() + "\r\n";
            if (Client.ClientMsg.GMID != 0)
                AllUserTable[Client.ClientMsg.GMID].SendMsg(21, Tempstirng);

            textBox1.AppendText(Tempstirng);
        }
        public void OnlineNumber(object msg)
        {
            label4.Text = msg.ToString() + "人";
        }
        public void Status(object msg)
        {
            label6.Text = msg.ToString();
            if (label6.Text == "连接断开")
            {
                Disconnect();
            }
        }
        #endregion
        #region 第一次加载
        private void Form1_Load(object sender, EventArgs e)
        {
            AllUser = new BindingList<ClientSocket>();
            AllUserTable = new Dictionary<int, ClientSocket>();
            //Control.CheckForIllegalCrossThreadCalls = false;
            //keyArray = Encoding.UTF8.GetBytes(key);
            //dataGridView1.AutoGenerateColumns = false;
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            SyncContext = SynchronizationContext.Current;
            dataGridView1.DataSource = AllUser;
            listBox1.SetSelected(0, true);
            listBox2.SetSelected(0, true);
            listBox3.SetSelected(0, true);
            listBox4.SetSelected(0, true);
            if (!File.Exists(@"./ggeserver.exe"))
            {
                checkBox14.Checked = true;
                debug = true;
                pathFile = new DirectoryInfo("../").FullName;//当前应用程序路径的上级目录 //System.AppDomain.CurrentDomain.BaseDirectory;

            }







            if (debug)
                ReadTxtToLst(listBox5, pathFile + @"服务端/封禁IP.txt");
            else
                ReadTxtToLst(listBox5, pathFile + "封禁IP.txt");

            int[] a = new int[]{1125,1501,1214,
                1511,1506,1507,1126,1092,1142,1514,1174,
                1091,1111,1070,1135,1173,1131,1512,1513,1146,1201,1202,1203,1204,1110,1140,
                1001,1193,
                1137,1040,1226,1208,
                1150,1138,1139,1205,1228,1103,1235,1042,1041,1210,1211,1242,1232,1207,1229,1233,
                1114,1231,1218,1221,1920,};
            string[] MapName = new string[]{ "地府","建邺城","昆仑仙境",
            "蟠桃园","东海湾","东海海底","东海岩洞","傲来国","女儿村","花果山","北俱芦洲",
            "长寿郊外","天宫","长寿村","方寸山","大唐境外","狮驼岭","魔王寨","盘丝岭","五庄观","女娲神迹","无名鬼城","小西天","小雷音寺","大唐国境","普陀山",
            "长安城","江南野外",
            "灵台宫","西梁女国","宝象国","朱紫国",
            "凌波城","神木林","无底洞","战神山","碗子山","水帘洞","丝绸之路","解阳山","子母河底","麒麟山","太岁府","须弥东界","比丘国","蓬莱仙岛","波月洞","柳林坡",
            "月宫","蟠桃园","墨家村","墨家禁地","凌云渡"};
            this.listView1.BeginUpdate();   //数据更新，UI暂时挂起，直到EndUpdate绘制控件，可以有效避免闪烁并大大提高加载速度  
            for (int i = 0; i < a.Length; i++)
            {
                string Mapnumber = a[i].ToString();
                listView1.Items.Add(Mapnumber, Mapnumber, i);
                listView1.Items[Mapnumber].SubItems.AddRange(new string[] { MapName[i], "0", "0", "0", "0" });
            }
            string tempPath;
            if (debug)
                tempPath = pathFile + @"服务端/CDK.txt";
            else
                 tempPath =  pathFile + @"CDK.txt";

            var _rstream = new StreamReader(tempPath);
            string lines;
            while ((lines = _rstream.ReadLine()) != null)
            {
                CDK.Add(lines);
            }
            _rstream.Close();


            this.listView1.EndUpdate();  //结束数据处理，UI界面一次性绘制。
            IniFile iniFile;
            if (debug)
              iniFile = new IniFile(@"../服务端/config.ini");
            else
            iniFile = new IniFile(pathFile+@"config.ini");
            textBox25.Text = iniFile.readIni("mainconfig", "key");
            textBox4.Text = iniFile.readIni("mainconfig", "ver");
            textBox17.Text = iniFile.readIni("mainconfig", "ip");
            textBox46.Text = iniFile.readIni("mainconfig", "lv");
            textBox48.Text = iniFile.readIni("mainconfig", "serPort");
            textBox49.Text = iniFile.readIni("mainconfig", "port");
            string gonggao;
            if (debug)
                 gonggao =@"../服务端/公告内容.txt";
            else
             gonggao =pathFile + @"公告内容.txt";

            try
            {
                using (StreamReader sr = new StreamReader(gonggao))
                {
                    string line;
                    // 从文件读取并显示行，直到文件的末尾 
                    while ((line = sr.ReadLine()) != null)
                    {
                        textBox40.AppendText(line + "\r\n");
                    }
                }
            }
            catch (Exception sd)
            {
                print(sd.Message);
            }
            Notice = textBox40.Text;
        }
        private void WriteLstToTxt(ListBox lst, string spath) //listbox 写入txt文件
        {
            int count = lst.Items.Count;
            var _wstream = new StreamWriter(spath);
            for (int i = 0; i < count; i++)
            {
                string data = lst.Items[i].ToString();
                _wstream.Write(data);
                _wstream.WriteLine();
            }
            _wstream.Flush();
            _wstream.Close();
        }
        private void ReadTxtToLst(ListBox lst, string spath) //listbox 读取txt文件
        {
            lst.Items.Clear();
            var _rstream = new StreamReader(spath, System.Text.Encoding.Default);
            string line;
            while ((line = _rstream.ReadLine()) != null)
            {
                lst.Items.Add(line);
            }
            _rstream.Close();
        }
        #endregion
        #region 连接服务端
        private void button1_Click(object sender, EventArgs e)
        {
            if (Server.ServerMsg.state)
            {
                Program.frm1.print(DesEncrypt("10.0.16.11"));


                string key = DesDecrypt(textBox25.Text);
                if (key == null)
                {
                    MessageBox.Show("key不对,请检查后重新输入");
                    return;
                }
                全局ip = key;
                Server.ServerMsg.start(key);
            }
            IniFile iniFile;
            if (debug)
                 iniFile = new IniFile(@"../服务端/config.ini");
            else
             iniFile = new IniFile(pathFile + @"config.ini");
            iniFile.writeIni("mainconfig", "key", textBox25.Text);
            iniFile.writeIni("mainconfig", "ver", textBox4.Text);
            iniFile.writeIni("mainconfig", "ip", textBox17.Text);
            iniFile.writeIni("mainconfig", "lv", textBox46.Text);
            button1.Enabled = false;
            textBox4.ReadOnly = true;
            textBox48.ReadOnly = true;
            textBox49.ReadOnly = true;
            textBox25.ReadOnly = true;
            textBox5.ReadOnly = true;

            RequestsNumber = int.Parse(textBox5.Text);
            timer.Change(TimeSpan.Zero, Timeout.InfiniteTimeSpan);
            if (checkBox4.Checked)
                StartServer();

            ConnectServer();

        }
        #endregion
        #region 清空控制台
        private void button4_Click(object sender, EventArgs e)
        {
            textBox1.Clear();

        }
        #endregion
        #region 联系作者
        private void toolStripStatusLabel4_Click(object sender, EventArgs e)
        {

            System.Diagnostics.Process.Start("tencent://message/?Site=baidu.com&uin=381990860&Menu=yes");
        }
        #endregion
        #region 输入限制数字
        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar < 48 || e.KeyChar > 57)
                e.Handled = true;
            if (e.KeyChar == 8)
                e.Handled = false;
        }
        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar < 48 || e.KeyChar > 57)
                e.Handled = true;
            if (e.KeyChar == 8 || e.KeyChar == 46)
                e.Handled = false;
        }
        #endregion
        #region 取时间戳
        public string GetTimeStamp()
        {
            long a = (DateTime.Now.ToUniversalTime().Ticks - 621355968000000000) / 10000000;
            return a.ToString();

        }
        #endregion
        #region 断开调用函数
        public void Disconnect()
        {
            button1.Enabled = true;
            if (checkBox4.Checked)
            {
                print("连接断开,尝试重连。。。。。。");
                label6.Text = "重新连接中";
                button1.Enabled = false;
            }
        }
        #endregion
        #region 用户表
        public void Adduser(object value)
        {

            ClientSocket ls = (ClientSocket)value;

            if (!AllUserTable.ContainsKey(ClientSocket.id))
            {
                AllUser.Add(ls);
                AllUserTable.Add(ClientSocket.id, ls);
            }

            OnlineNumber(AllUser.Count);
        }

        public void Deluser(object value)
        {
            /*            ClientSocket ls = (ClientSocket)value;
                        if (AllUserTable.ContainsKey(ls.编号))
                        {
                            AllUserTable[ls.编号].clear();
                            AllUserTable.Remove(ls.编号);
                        }*/
            ClientSocket ls = (ClientSocket)value;
            ClientSocket tempclient;
            if (AllUserTable.TryGetValue(ls.编号, out tempclient)) {
                tempclient.clear();
                AllUserTable.Remove(ls.编号);
            }
            AllUser.Remove(ls);
            //Trace.WriteLine("AllUser.Count" + AllUser.Count.ToString());
            OnlineNumber(AllUser.Count);
        }
        #endregion
        #region 启动服务端
        private void ConnectServer()
        {
            Client.ClientMsg.Connect();

        }
        private void StartServer()
        {
            Process[] pro = Process.GetProcesses();//获取已开启的所有进程
            bool qiDong = false;
            //遍历所有查找到的进程

            for (int i = 0; i < pro.Length; i++)
            {
                //判断此进程是否是要查找的进程
                if (pro[i].ProcessName.ToString().ToLower() == "ggeserver")
                {
                    qiDong = true;
                    break;
                }
            }
            if (!qiDong)
            {
                string pathfile;
if (debug)
                 pathfile = @"../服务端/ggeserver.exe";
else
                  pathfile = @"./ggeserver.exe";

                if (!File.Exists(pathfile))
                {
                    MessageBox.Show("找不到服务端文件,请将网关放入服务端同目录下,已经取消自动启动服务端和重连功能！");
                    checkBox3.Checked = false;
                    checkBox4.Checked = false;

                }
                else
                {
                    Process p;//实例化一个Process对象
                    p = Process.Start(@"ggeserver.exe");//要开启的进程（或 要启用的程序），括号内为绝对路径
                }
            }
        }
        #endregion
        #region 开关
        private void Switch_Click(object sender, EventArgs e)
        {
            Client.ClientMsg.SendMsg(1000, ((Button)sender).Text);
        }
        #endregion
        #region 玩家操作处理
        private void button13_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox6.Text))
            {
                MessageBox.Show("玩家ID不能为空！");
                return;
            }

            Client.ClientMsg.SendMsg(1001, "do local ret={[1]=\"" + textBox6.Text + "\",[2]=\"" + listBox1.SelectedItem + "\"} return ret end");
        }
        private void button56_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox39.Text))
            {
                MessageBox.Show("玩家ID不能为空！");
                return;
            }

            Client.ClientMsg.SendMsg(1009, textBox39.Text);
        }
        private void button2_Click(object sender, EventArgs e)
        {
            if (listBox8.SelectedIndex == -1)
            {
                MessageBox.Show("必须选择一个操作方式！");
                return;
            }

            Client.ClientMsg.SendMsg(1009, listBox8.SelectedItem.ToString());
        }
        #endregion
        #region 监听
        private void checkBox5_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox5.Checked)
                Client.ClientMsg.SendMsg(1000, "当前监听开启");
            else
                Client.ClientMsg.SendMsg(1000, "当前监听关闭");
        }

        private void checkBox6_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox6.Checked)
                Client.ClientMsg.SendMsg(1000, "队伍监听开启");
            else
                Client.ClientMsg.SendMsg(1000, "队伍监听关闭");
        }

        private void checkBox7_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox7.Checked)
                Client.ClientMsg.SendMsg(1000, "世界监听开启");
            else
                Client.ClientMsg.SendMsg(1000, "世界监听关闭");
        }

        private void checkBox8_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox8.Checked)
                Client.ClientMsg.SendMsg(1000, "帮派监听开启");
            else
                Client.ClientMsg.SendMsg(1000, "帮派监听关闭");
        }

        private void checkBox10_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox10.Checked)
                Client.ClientMsg.SendMsg(1000, "门派监听开启");
            else
                Client.ClientMsg.SendMsg(1000, "门派监听关闭");
        }

        private void checkBox9_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox9.Checked)
                Client.ClientMsg.SendMsg(1000, "传闻监听开启");
            else
                Client.ClientMsg.SendMsg(1000, "传闻监听关闭");
        }
        #endregion
        #region 发送公告
        private void button23_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox8.Text))
                MessageBox.Show("消息内容不能为空！");
            else
            {
                foreach (var item in AllUser)
                {
                    item.SendMsg(21, "#gm@@/#R/" + textBox8.Text);
                }
            }

        }
        #endregion
        #region 发送广播
        private void button15_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox8.Text))
                MessageBox.Show("消息内容不能为空！");
            else
            {
                foreach (var item in AllUser)
                {
                    item.SendMsg(99997, textBox8.Text);
                }
            }
        }
        #endregion
        #region 全服发送
        private void button42_Click(object sender, EventArgs e)
        {
            if (listBox4.SelectedIndex == -1)
            {
                MessageBox.Show("必须选择一个操作方式！");
                return;
            }

            Client.ClientMsg.SendMsg(1000, listBox4.SelectedItem.ToString());
        }
        #endregion
        #region 调整经验
        private void button39_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox22.Text))
            {
                MessageBox.Show("参数不能为空");
                return;
            }
            Client.ClientMsg.SendMsg(1002, "do local ret={[1]=" + textBox22.Text + ",[2]=\"调整经验\"} return ret end");
        }
        #endregion
        #region 充值业务
        private void button31_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox2.Text) || string.IsNullOrEmpty(textBox35.Text))
            {
                MessageBox.Show("玩家ID和数额不能为空！");
                return;
            }

            Client.ClientMsg.SendMsg(1004, "do local ret={[1]=" + textBox2.Text + ",[2]=\"" + listBox2.SelectedItem + "\",[3]=" + textBox35.Text + "} return ret end");
        }
        #endregion
        #region 赠送称谓
        private void button34_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox3.Text))
            {
                MessageBox.Show("玩家ID不能为空！");
                return;
            }

            Client.ClientMsg.SendMsg(1003, "do local ret={[1]=" + textBox3.Text + ",[2]=\"" + listBox3.SelectedItem + "\"} return ret end");
        }
        #endregion
        #region 定制装备
        private void button26_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox9.Text) || comboBox1.SelectedItem == null || string.IsNullOrEmpty(textBox13.Text) || string.IsNullOrEmpty(textBox12.Text))
            {
                MessageBox.Show("红色标签为必须填写的值！");
                return;
            }
            StringBuilder Msg = new StringBuilder();
            Msg.Append(textBox9.Text);
            Msg.Append("*-*");
            Msg.Append(comboBox1.SelectedItem);
            Msg.Append("*-*");
            Msg.Append(textBox12.Text);
            Msg.Append("*-*");
            if (!(comboBox4.SelectedItem == null))//特效
                Msg.Append(comboBox4.SelectedItem);
            Msg.Append("*-*");
            if (!(comboBox3.SelectedItem == null))//特技
                Msg.Append(comboBox3.SelectedItem);
            Msg.Append("*-*");
            if (radioButton1.Checked)
                Msg.Append("1");
            else if (radioButton2.Checked)
                Msg.Append("2");
            else if (radioButton16.Checked)
                Msg.Append("3");
            Msg.Append("*-*");
            if (!(string.IsNullOrEmpty(comboBox9.Text)))
                Msg.Append(comboBox9.Text);
            Msg.Append("*-*");
            if (!(comboBox2.SelectedItem == null))
                Msg.Append(comboBox2.SelectedItem);
            Msg.Append("=");
            if (!(string.IsNullOrEmpty(textBox10.Text)))
                Msg.Append(textBox10.Text);
            Msg.Append("@");
            if (!(comboBox7.SelectedItem == null))
                Msg.Append(comboBox7.SelectedItem);
            Msg.Append("=");// 三围
            if (!(string.IsNullOrEmpty(textBox11.Text)))
                Msg.Append(textBox11.Text);
            Client.ClientMsg.SendMsg(1005, "do local ret={[1]=" + textBox13.Text + ",[2]=\"" + Msg.ToString() + "\"} return ret end");
        }
        #endregion
        #region CDK
        internal void 兑换CDK(object state)
        {
            var tepMsg = (DataMsgs)(state);
            //判断是否在数据表中
            if (CDK.Contains(tepMsg.文本))
            {

                //删除表数据
                string cs = tepMsg.文本.Substring(0, 5);
                switch (cs)
                {
                    case "CJP00"://普通抽奖
                        Client.ClientMsg.SendMsg(1012, "do local ret={[1]=\"普通抽奖\",[2]=" + tepMsg.数字id + "} return ret end");
                        break;
                    case "CJZ00"://中等抽奖
                        Client.ClientMsg.SendMsg(1012, "do local ret={[1]=\"中等抽奖\",[2]=" + tepMsg.数字id + "} return ret end");
                        break;
                    case "CJJ00"://极品抽奖
                        Client.ClientMsg.SendMsg(1012, "do local ret={[1]=\"极品抽奖\",[2]=" + tepMsg.数字id + "} return ret end");
                        break;
                    case "Y0001"://银子1E
                        Client.ClientMsg.SendMsg(1012, "do local ret={[1]=\"银子1E\",[2]=" + tepMsg.数字id + "} return ret end");
                        break;
                    case "Y0005"://银子5E
                        Client.ClientMsg.SendMsg(1012, "do local ret={[1]=\"银子5E\",[2]=" + tepMsg.数字id + "} return ret end");
                        break;
                    case "Y0010"://银子10E
                        Client.ClientMsg.SendMsg(1012, "do local ret={[1]=\"银子10E\",[2]=" + tepMsg.数字id + "} return ret end");
                        break;
                    case "Y0020"://银子20E
                        Client.ClientMsg.SendMsg(1012, "do local ret={[1]=\"银子20E\",[2]=" + tepMsg.数字id + "} return ret end");
                        break;
                    case "M0006"://仙玉6元
                        Client.ClientMsg.SendMsg(1012, "do local ret={[1]=\"仙玉6元\",[2]=" + tepMsg.数字id + "} return ret end");
                        break;
                    case "M0050"://仙玉50元
                        Client.ClientMsg.SendMsg(1012, "do local ret={[1]=\"仙玉50元\",[2]=" + tepMsg.数字id + "} return ret end");
                        break;
                    case "M0100"://仙玉100元
                        Client.ClientMsg.SendMsg(1012, "do local ret={[1]=\"仙玉100元\",[2]=" + tepMsg.数字id + "} return ret end");
                        break;
                    case "M0500"://仙玉500元
                        Client.ClientMsg.SendMsg(1012, "do local ret={[1]=\"仙玉500元\",[2]=" + tepMsg.数字id + "} return ret end");
                        break;
                    case "M1000"://仙玉1000元
                        Client.ClientMsg.SendMsg(1012, "do local ret={[1]=\"仙玉1000元\",[2]=" + tepMsg.数字id + "} return ret end");
                        break;
                    default:
                        Client.ClientMsg.SendMsg(1011, "do local ret={[1]=\"CDK错误请重新输入\",[2]=" + tepMsg.数字id + "} return ret end");
                        break;
                }
                删除CDK(tepMsg.文本);
            }
            else
                Client.ClientMsg.SendMsg(1011, "do local ret={[1]=\"CDK错误请重新输入\",[2]=" + tepMsg.数字id + "} return ret end");
        }

        private void 删除CDK(string tepMsg)
        {
            for (int i = 0; i < CDK.Count; i++)
            {
                if (CDK[i] == tepMsg)
                {
                    CDK.RemoveAt(i);
                }
            }
            string TmpPath;
if(debug)
             TmpPath = pathFile + @"服务端/CDK.txt";
else
             TmpPath = pathFile +"CDK.txt";

            var _wstream = new StreamWriter(TmpPath);
            for (int i = 0; i < CDK.Count; i++)
            {
                string data = CDK[i];
                _wstream.Write(data);
                _wstream.WriteLine();
            }
            _wstream.Flush();
            _wstream.Close();
        }
        private void button53_Click(object sender, EventArgs e)
        {
            if (CDKChoose == "")
            {
                MessageBox.Show("你当前还没有选择类型");
                return;
            }
            string Msg = ((Button)sender).Text;
            switch (Msg)
            {
                case "生成10张":
                    生成CDK(10);
                    break;
                case "生成100张":
                    生成CDK(100);
                    break;
                case "生成200张":
                    生成CDK(200);
                    break;
                case "提取100张":
                    提取CDK(100);
                    break;
                case "提取10张":
                    提取CDK(10);
                    break;
                default:
                    提取CDK(1);
                    break;
            }
        }
        private void 提取CDK(int count)
        {
            int counts = 0;
            StringBuilder MSG = new StringBuilder();
            for (int i = 0; i < CDK.Count; i++)
            {
                if (CDK[i].StartsWith(CDKChoose))
                {
                    if (counts == 0)
                        textBox23.Text = CDK[i];
                    counts++;
                    MSG.Append(CDK[i]);
                    MSG.Append("\r\n");
                    if (counts >= count)
                    {
                        print(MSG.ToString());
                        return;
                    }
                }
            }
            MessageBox.Show("目前没有对应的CDK请先生成CDK");


        }
        private void 生成CDK(int count)
        {
            string TmpPath;
if(debug)
             TmpPath = pathFile + @"服务端/CDK.txt";
else
             TmpPath = pathFile +"CDK.txt";

            var _wstream = File.AppendText(TmpPath);

            for (int i = 0; i < count; i++)
            {
                string cdkey = CreateAndCheckCode(random, CDKChoose);
                //Console.WriteLine(cdkey);
                _wstream.Write(cdkey);
                _wstream.WriteLine();
                CDK.Add(cdkey);
            }
            _wstream.Flush();
            _wstream.Close();
            MessageBox.Show("生成CDK成功");

        }
        Random random = new Random(~unchecked((int)DateTime.Now.Ticks));
        /// <summary>
        /// 生成CDK
        /// </summary>
        /// <param name="random">random</param>
        /// <param name="code">激活码前缀</param>
        /// <returns>激活码</returns>
        private string CreateAndCheckCode(Random random, string code) // code 激活码前缀
        {
            char[] Pattern = new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', 'q', 'w', 'e', 'r', 't', 'y', 'u', 'i', 'o', 'p', 'a', 's', 'd', 'f', 'g', 'h', 'j', 'k', 'l', 'z', 'x', 'c', 'v', 'b', 'n', 'm' };
            string result = code;
            int n = Pattern.Length;
            for (int i = 0; i < 16; i++)
            {
                int rnd = random.Next(0, n);
                result += Pattern[rnd];
            }
            if (true)//数据库中不存在
            {
                return result;
            }
            else
            {
                return CreateAndCheckCode(random, code);
            }
        }
        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            CDKChoose = ((RadioButton)sender).Tag.ToString();
        }


        #endregion
        #region 定制灵饰
        private void button29_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox20.Text) || comboBox12.SelectedItem == null || comboBox16.SelectedItem == null || comboBox8.SelectedItem == null || string.IsNullOrEmpty(textBox36.Text))
            {
                MessageBox.Show("红色标签为必须填写的值！");
                return;
            }
            StringBuilder Msg = new StringBuilder();
            Msg.Append(comboBox16.SelectedItem);//等级 
            Msg.Append("*-*");
            Msg.Append(comboBox12.SelectedItem);//类型
            Msg.Append("*-*");
            Msg.Append(comboBox8.SelectedItem);//主类型
            Msg.Append("*-*");
            Msg.Append(textBox36.Text);//主属性
            Msg.Append("*-*");
            if (checkBox11.Checked)//超级简易
                Msg.Append("1");
            Msg.Append("*-*");
            if (!(comboBox5.SelectedItem == null))//附加属性1
                Msg.Append(comboBox5.SelectedItem);
            Msg.Append("=");
            if (!(string.IsNullOrEmpty(textBox14.Text)))
                Msg.Append(textBox14.Text);
            Msg.Append("@");

            if (!(comboBox6.SelectedItem == null))//附加属性2
                Msg.Append(comboBox6.SelectedItem);
            Msg.Append("=");
            if (!(string.IsNullOrEmpty(textBox16.Text)))
                Msg.Append(textBox16.Text);
            Msg.Append("@");
            if (!(comboBox10.SelectedItem == null))//附加属性3
                Msg.Append(comboBox10.SelectedItem);
            Msg.Append("=");
            if (!(string.IsNullOrEmpty(textBox18.Text)))
                Msg.Append(textBox18.Text);
            Msg.Append("@");
            if (!(comboBox11.SelectedItem == null))//附加属性4
                Msg.Append(comboBox11.SelectedItem);
            Msg.Append("=");
            if (!(string.IsNullOrEmpty(textBox19.Text)))
            { Msg.Append(textBox19.Text); }


            Client.ClientMsg.SendMsg(1006, "do local ret={[1]=" + textBox20.Text + ",[2]=\"" + Msg.ToString() + "\"} return ret end");
        }
        #endregion
        #region 定制宝宝
        private void button46_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox32.Text) || comboBox15.SelectedItem == null || string.IsNullOrEmpty(textBox33.Text) || string.IsNullOrEmpty(textBox15.Text))
            {
                MessageBox.Show("红色标签为必须填写的值！");
                return;
            }
            StringBuilder Msg = new StringBuilder();
            Msg.Append(textBox33.Text);
            Msg.Append("*-*");
            Msg.Append(textBox15.Text);
            Msg.Append("*-*");
            Msg.Append(comboBox15.SelectedItem);
            Msg.Append("*-*");
            if (!(comboBox14.SelectedItem == null))
                Msg.Append(comboBox14.SelectedItem);
            Msg.Append("*-*");
            if (!(string.IsNullOrEmpty(textBox34.Text)))
                Msg.Append(textBox34.Text);
            Msg.Append("*-*");
            if (!(string.IsNullOrEmpty(textBox26.Text)))
                Msg.Append(textBox26.Text);
            Msg.Append("*-*");
            if (!(string.IsNullOrEmpty(textBox27.Text)))
                Msg.Append(textBox27.Text);
            Msg.Append("*-*");
            if (!(string.IsNullOrEmpty(textBox28.Text)))
                Msg.Append(textBox28.Text);
            Msg.Append("*-*");
            if (!(string.IsNullOrEmpty(textBox29.Text)))
                Msg.Append(textBox29.Text);
            Msg.Append("*-*");
            if (!(string.IsNullOrEmpty(textBox37.Text)))
                Msg.Append(textBox37.Text);
            Msg.Append("*-*");
            if (!(string.IsNullOrEmpty(textBox30.Text)))
                Msg.Append(textBox30.Text);
            Msg.Append("*-*");
            if (!(string.IsNullOrEmpty(textBox31.Text)))
                Msg.Append(textBox31.Text);
            Msg.Append("*-*");

            string strCollected = string.Empty;
            for (int i = 0; i < checkedListBox2.Items.Count; i++)
            {
                if (checkedListBox2.GetItemChecked(i))
                {
                    if (strCollected == string.Empty)
                    {
                        strCollected = checkedListBox2.GetItemText(checkedListBox2.Items[i]);
                    }
                    else
                    {
                        strCollected = strCollected + "@" + checkedListBox2.GetItemText(checkedListBox2.Items[i]);
                    }
                }
            }
            Msg.Append(strCollected);
            Msg.Append("*-*");
            string strCollected1 = string.Empty;
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                if (checkedListBox1.GetItemChecked(i))
                {
                    if (strCollected1 == string.Empty)
                    {
                        strCollected1 = checkedListBox1.GetItemText(checkedListBox1.Items[i]);
                    }
                    else
                    {
                        strCollected1 = strCollected1 + "@" + checkedListBox1.GetItemText(checkedListBox1.Items[i]);
                    }
                }
            }
            Msg.Append(strCollected1);

            Client.ClientMsg.SendMsg(1007, "do local ret={[1]=" + textBox32.Text + ",[2]=\"" + Msg.ToString() + "\"} return ret end");

        }
        #endregion
        #region 时间函数

        static void TimerCallBack(object state)
        {




            Program.frm1.toolStripStatusLabel2.Text = DateTime.Now.ToString("yyyy年MM月dd日-HH:mm:ss");
            if (Program.frm1.checkBox3.Checked)
                Program.frm1.StartServer();
            if (Program.frm1.checkBox4.Checked)
                Program.frm1.ConnectServer();
            var nextTime = DateTime.Now.AddSeconds(1);
            //执行完后,重新设置定时器下次执行时间.
            timer.Change(nextTime.Subtract(DateTime.Now), Timeout.InfiniteTimeSpan);

        }

        #endregion
        #region 封禁系统
        private void button50_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox38.Text))
            {
                MessageBox.Show("封禁信息必须填写的值！");
                return;
            }
            Button lsbutton = (Button)sender;
            switch (lsbutton.Text)
            {
                case "封禁IP":
                    listBox5.Items.Add(textBox38.Text);
                    break;

                case "封禁封包":
                    listBox7.Items.Add(textBox38.Text);
                    break;
            }
        }

        private void button52_Click(object sender, EventArgs e)
        {
            Button lsbutton = (Button)sender;
            switch (lsbutton.Text)
            {
                case "删除封禁IP":
                    if (listBox5.SelectedItems.Count != 0)
                        listBox5.Items.Remove(listBox5.SelectedItem);
                    else
                        MessageBox.Show("请选择需要删除的IP！");
                    break;

                case "删除封禁封包":
                    if (listBox7.SelectedItems.Count != 0)
                        listBox7.Items.Remove(listBox7.SelectedItem);
                    else
                        MessageBox.Show("请选择需要删除的禁封包！");
                    break;
            }
        }
        #endregion
        #region 地图操作
        private void button49_Click_1(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count == 0)
            {
                MessageBox.Show("请选择一个你要操作的地图");
                return;
            }

            string site = listView1.SelectedItems[0].Text;
            string buttonMsg = ((Button)sender).Text;
            if (buttonMsg == "添加假人")
            {
                if (string.IsNullOrEmpty(textBox21.Text))
                {
                    MessageBox.Show("请填写假人数量");
                    return;
                }
                Client.ClientMsg.SendMsg(1008, "do local ret={[1]=\"" + buttonMsg + "\",[2]=" + site + ",[3]=" + textBox21.Text + "} return ret end");
            }
            else
            {
                Client.ClientMsg.SendMsg(1008, "do local ret={[1]=\"" + buttonMsg + "\",[2]=" + site + "} return ret end");
            }
        }
        public void MpaUpdata(object Msg)
        {
            MapUpdata mapUpdata = JsonConvert.DeserializeObject<MapUpdata>((string)Msg);
            listView1.Items[mapUpdata.Number].SubItems[2].Text = mapUpdata.Role;
            listView1.Items[mapUpdata.Number].SubItems[3].Text = mapUpdata.Monster;
            listView1.Items[mapUpdata.Number].SubItems[4].Text = mapUpdata.Model;
            listView1.Items[mapUpdata.Number].SubItems[5].Text = mapUpdata.Stall;
        }
        #endregion
        #region 选择文件
        private void btnSelect_Click(object sender, EventArgs e)
        {
            string key = DesDecrypt(textBox25.Text);
            if (key == null)
            {
                MessageBox.Show("key不对,请检查后重新输入");
                return;
            }
            if (this.openFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.txtFilePath.Text = "";
                foreach (string strName in this.openFileDialog.FileNames)
                {
                    this.txtFilePath.Text += strName + "\r\n";
                }
            }
        }
        #endregion
        #region GetFirstSheetNameFromExcelFileName 获取表格的第一个数据表名称
        /// <summary>
        /// 获取表格的第一个数据表名称
        /// </summary>
        /// <param name="filepath"></param>
        /// <param name="numberSheetID"></param>
        /// <returns></returns>
        public string GetExcelFirstTableName(string excelFileName)
        {
            if (!System.IO.File.Exists(excelFileName))
            {
                return null;
            }
            string tableName = null;
            if (File.Exists(excelFileName))
            {
                using (OleDbConnection conn = new OleDbConnection("Provider=Microsoft.Jet." +
                  "OLEDB.4.0;Extended Properties=\"Excel 8.0\";Data Source=" + excelFileName))
                {
                    conn.Open();
                    DataTable dt = conn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);
                    tableName = dt.Rows[0][2].ToString().Trim();
                }
            }
            return tableName;
        }

        #endregion
        #region ReadData 读取数据
        /// <summary>
        /// 读取数据
        /// </summary>
        /// <param name="path"></param>
        private void ReadData(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            string tableName = GetExcelFirstTableName(path);
            string strConn = "Provider=Microsoft.Jet.OLEDB.4.0;" + "Data Source=" + path + ";" + "Extended Properties='Excel 8.0;HDR=NO;IMEX=1';";
            DataTable dt = null;
            using (OleDbConnection conn = new OleDbConnection(strConn))
            {
                conn.Open();
                string strExcel = "";
                OleDbDataAdapter myCommand = null;
                DataSet ds = null;
                strExcel = string.Format("select * from [{0}]", tableName);
                myCommand = new OleDbDataAdapter(strExcel, strConn);
                ds = new DataSet();
                myCommand.Fill(ds, "table1");
                dt = ds.Tables[0];
                myCommand.Dispose();
            }
            CreateData(path, dt);
        }
        #endregion
        #region 生成加密
        private void btnCreate_Click(object sender, EventArgs e)
        {

            string key = DesDecrypt(textBox25.Text);
            if (key == null)
            {
                MessageBox.Show("key不对,请检查后重新输入");
                return;
            }
            string[] arr = this.txtFilePath.Text.Trim().Split('\r', '\n');
            if (arr.Length > 0)
            {
                for (int i = 0; i < arr.Length; i++)
                {
                    ReadData(arr[i]);
                }
                MessageBox.Show("创建成功");
            }

        }
        #endregion
        #region CreateData 生成加密后的文件
        /// <summary>
        /// 生成加密后的文件
        /// </summary>
        /// <param name="path"></param>
        /// <param name="dt"></param>
        private void CreateData(string path, DataTable dt)
        {
            //数据格式 行数 列数 二维数组每项的值 这里不做判断 都用string存储

            string filePath = path.Substring(0, path.LastIndexOf('\\') + 1);
            string fileFullName = path.Substring(path.LastIndexOf('\\') + 1);
            string fileName = fileFullName.Substring(0, fileFullName.LastIndexOf('.'));

            byte[] buffer = null;
            string[,] dataArr = null;

            using (MMO_MemoryStream ms = new MMO_MemoryStream())
            {
                int row = dt.Rows.Count;
                int columns = dt.Columns.Count;

                dataArr = new string[columns, 3];

                ms.WriteInt(row);
                ms.WriteInt(columns);
                for (int i = 0; i < row; i++)
                {
                    for (int j = 0; j < columns; j++)
                    {
                        if (i < 3)
                        {
                            dataArr[j, i] = dt.Rows[i][j].ToString().Trim();
                        }

                        ms.WriteASCIIString(dt.Rows[i][j].ToString().Trim());
                    }
                }
                buffer = ms.ToArray();
            }

            //------------------
            //第1步：xor加密
            //------------------
            //var strToBytes2 = System.Text.Encoding.Default.GetBytes(textBox25.Text);
            int iScaleLen = xorScale.Length;
            for (int i = 0; i < buffer.Length; i++)
            {
                buffer[i] = (byte)(buffer[i] ^ xorScale[i % iScaleLen]);
            }

            //------------------
            //第2步：压缩
            //------------------
            //压缩后的字节流
            //buffer = ZlibHelper.CompressBytes(buffer);

            //------------------
            //第3步：写入文件
            //------------------
            //FileStream fs = new FileStream(string.Format("{0}{1}{2}", filePath,"..//data//",fileName + ".data"), FileMode.Create);

            FileStream fs;

            if (Program.frm1.debug)
            {
                 fs = new FileStream(string.Format("{0}{1}{2}", filePath, "..//..//服务端//data//", fileName + ".data"), FileMode.Create);
             

               
            }
            else
            {
                 fs = new FileStream(string.Format("{0}{1}{2}", filePath, "..//data//", fileName + ".data"), FileMode.Create);
            }
               

            fs.Write(buffer, 0, buffer.Length);
            fs.Close();
            if (Program.frm1.debug)
                File.Copy(filePath + "..//..//服务端//data//" + fileName + ".data", filePath + "..//..//客户端//data//" + fileName + ".data", true);

            //CreateEntity(filePath, fileName, dataArr);
            //CreateDBModel(filePath, fileName, dataArr);
        }
        /// <summary>
        /// 创建实体
        /// </summary>
        private void CreateEntity(string filePath, string fileName, string[,] dataArr)
        {
            if (dataArr == null) return;

            if (!Directory.Exists(string.Format("{0}Create", filePath)))
            {
                Directory.CreateDirectory(string.Format("{0}Create", filePath));
            }

            if (!Directory.Exists(string.Format("{0}CreateLua", filePath)))
            {
                Directory.CreateDirectory(string.Format("{0}CreateLua", filePath));
            }

            StringBuilder sbr = new StringBuilder();
            sbr.Append("\r\n");
            sbr.Append("//===================================================\r\n");
            sbr.Append("//作    者：Hack-T QQ381990860\r\n");
            sbr.AppendFormat("//创建时间：{0}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sbr.Append("//备    注：此代码为工具生成 请勿手工修改\r\n");
            sbr.Append("//===================================================\r\n");
            sbr.Append("using System.Collections;\r\n");
            sbr.Append("\r\n");
            sbr.Append("/// <summary>\r\n");
            sbr.AppendFormat("/// {0}实体\r\n", fileName);
            sbr.Append("/// </summary>\r\n");
            sbr.AppendFormat("public partial class {0}Entity : AbstractEntity\r\n", fileName);
            sbr.Append("{\r\n");

            for (int i = 0; i < dataArr.GetLength(0); i++)
            {
                if (i == 0) continue;
                sbr.Append("    /// <summary>\r\n");
                sbr.AppendFormat("    /// {0}\r\n", dataArr[i, 2]);
                sbr.Append("    /// </summary>\r\n");
                sbr.AppendFormat("    public {0} {1} {{ get; set; }}\r\n", dataArr[i, 1], dataArr[i, 0]);
                sbr.Append("\r\n");
            }

            sbr.Append("}\r\n");


            using (FileStream fs = new FileStream(string.Format("{0}Create/{1}Entity.cs", filePath, fileName), FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(sbr.ToString());
                }
            }

            //=======================创建Lua的实体
            sbr.Clear();

            sbr.AppendFormat("{0}Entity = {{ ", fileName);

            for (int i = 0; i < dataArr.GetLength(0); i++)
            {

                if (i == dataArr.GetLength(0) - 1)
                {
                    if (dataArr[i, 1].Equals("string", StringComparison.CurrentCultureIgnoreCase))
                    {
                        sbr.AppendFormat("{0} = \"\"", dataArr[i, 0]);
                    }
                    else
                    {
                        sbr.AppendFormat("{0} = 0", dataArr[i, 0]);
                    }
                }
                else
                {
                    if (dataArr[i, 1].Equals("string", StringComparison.CurrentCultureIgnoreCase))
                    {
                        sbr.AppendFormat("{0} = \"\", ", dataArr[i, 0]);
                    }
                    else
                    {
                        sbr.AppendFormat("{0} = 0, ", dataArr[i, 0]);
                    }
                }
            }
            sbr.Append(" }\r\n");

            sbr.Append("\r\n");
            sbr.Append("--这句是重定义元表的索引，就是说有了这句，这个才是一个类\r\n");
            sbr.AppendFormat("{0}Entity.__index = {0}Entity;\r\n", fileName);
            sbr.Append("\r\n");
            sbr.AppendFormat("function {0}Entity.New(", fileName);
            for (int i = 0; i < dataArr.GetLength(0); i++)
            {
                if (i == dataArr.GetLength(0) - 1)
                {
                    sbr.AppendFormat("{0}", dataArr[i, 0]);
                }
                else
                {
                    sbr.AppendFormat("{0}, ", dataArr[i, 0]);
                }
            }
            sbr.Append(")\r\n");
            sbr.Append("    local self = { }; --初始化self\r\n");
            sbr.Append("");
            sbr.AppendFormat("    setmetatable(self, {0}Entity); --将self的元表设定为Class\r\n", fileName);
            sbr.Append("\r\n");
            for (int i = 0; i < dataArr.GetLength(0); i++)
            {
                sbr.AppendFormat("    self.{0} = {0};\r\n", dataArr[i, 0]);
            }
            sbr.Append("\r\n");
            sbr.Append("    return self;\r\n");
            sbr.Append("end");

            using (FileStream fs = new FileStream(string.Format("{0}CreateLua/{1}Entity.lua", filePath, fileName), FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(sbr.ToString());
                }
            }
        }
        private string ChangeTypeName(string type)
        {
            string str = string.Empty;

            switch (type)
            {
                case "int":
                    str = ".ToInt()";
                    break;
                case "long":
                    str = ".ToLong()";
                    break;
                case "float":
                    str = ".ToFloat()";
                    break;
            }

            return str;
        }
        /// <summary>
        /// 创建数据管理类
        /// </summary>
        private void CreateDBModel(string filePath, string fileName, string[,] dataArr)
        {
            if (dataArr == null) return;

            if (!Directory.Exists(string.Format("{0}Create", filePath)))
            {
                Directory.CreateDirectory(string.Format("{0}Create", filePath));
            }

            StringBuilder sbr = new StringBuilder();
            sbr.Append("\r\n");
            sbr.Append("//===================================================\r\n");
            sbr.Append("//作    者： QQ381990860\r\n");
            sbr.AppendFormat("//创建时间：{0}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            sbr.Append("//备    注：此代码为工具生成 请勿手工修改\r\n");
            sbr.Append("//===================================================\r\n");
            sbr.Append("using System.Collections;\r\n");
            sbr.Append("using System.Collections.Generic;\r\n");
            sbr.Append("using System;\r\n");
            sbr.Append("\r\n");
            sbr.Append("/// <summary>\r\n");
            sbr.AppendFormat("/// {0}数据管理\r\n", fileName);
            sbr.Append("/// </summary>\r\n");
            sbr.AppendFormat("public partial class {0}DBModel : AbstractDBModel<{0}DBModel, {0}Entity>\r\n", fileName);
            sbr.Append("{\r\n");
            sbr.Append("    /// <summary>\r\n");
            sbr.Append("    /// 文件名称\r\n");
            sbr.Append("    /// </summary>\r\n");
            sbr.AppendFormat("    protected override string FileName {{ get {{ return \"{0}.data\"; }} }}\r\n", fileName);
            sbr.Append("\r\n");
            sbr.Append("    /// <summary>\r\n");
            sbr.Append("    /// 创建实体\r\n");
            sbr.Append("    /// </summary>\r\n");
            sbr.Append("    /// <param name=\"parse\"></param>\r\n");
            sbr.Append("    /// <returns></returns>\r\n");
            sbr.AppendFormat("    protected override {0}Entity MakeEntity(GameDataTableParser parse)\r\n", fileName);
            sbr.Append("    {\r\n");
            sbr.AppendFormat("        {0}Entity entity = new {0}Entity();\r\n", fileName);

            for (int i = 0; i < dataArr.GetLength(0); i++)
            {
                sbr.AppendFormat("        entity.{0} = parse.GetFieldValue(\"{0}\"){1};\r\n", dataArr[i, 0], ChangeTypeName(dataArr[i, 1]));
            }
            sbr.Append("        return entity;\r\n");
            sbr.Append("    }\r\n");
            sbr.Append("}\r\n");

            using (FileStream fs = new FileStream(string.Format("{0}Create/{1}DBModel.cs", filePath, fileName), FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(sbr.ToString());
                }
            }

            //===============生成lua的DBModel
            sbr.Clear();
            sbr.Append("");

            sbr.AppendFormat("require \"Download/XLuaLogic/Data/Create/{0}Entity\"\r\n", fileName);
            sbr.Append("\r\n");
            sbr.Append("--数据访问\r\n");
            sbr.AppendFormat("{0}DBModel = {{ }}\r\n", fileName);
            sbr.Append("\r\n");
            sbr.AppendFormat("local this = {0}DBModel;\r\n", fileName);
            sbr.Append("\r\n");
            sbr.AppendFormat("local {0}Table = {{ }}; --定义表格\r\n", fileName.ToLower());
            sbr.Append("\r\n");
            sbr.AppendFormat("function {0}DBModel.New()\r\n", fileName);
            sbr.Append("    return this;\r\n");
            sbr.Append("end\r\n");
            sbr.Append("\r\n");
            sbr.AppendFormat("function {0}DBModel.Init()\r\n", fileName);
            sbr.Append("\r\n");
            sbr.Append("    --这里从C#代码中获取一个数组\r\n");
            sbr.Append("\r\n");
            sbr.AppendFormat("    local gameDataTable = CS.LuaHelper.Instance:GetData(\"{0}.data\");\r\n", fileName);
            sbr.Append("");
            sbr.Append("    --表格的前三行是表头 所以获取数据时候 要从 3 开始\r\n");
            sbr.Append("    --print(\"行数\"..gameDataTable.Row);\r\n");
            sbr.Append("    --print(\"列数\"..gameDataTable.Column);\r\n");
            sbr.Append("\r\n");
            sbr.Append("    for i = 3, gameDataTable.Row - 1, 1 do\r\n");
            sbr.AppendFormat("        {0}Table[#{0}Table+1] = {1}Entity.New( ", fileName.ToLower(), fileName);

            for (int i = 0; i < dataArr.GetLength(0); i++)
            {
                if (i == dataArr.GetLength(0) - 1)
                {
                    if (dataArr[i, 1].Equals("string", StringComparison.CurrentCultureIgnoreCase))
                    {
                        sbr.AppendFormat("gameDataTable.Data[i][{0}]", i);
                    }
                    else
                    {
                        sbr.AppendFormat("tonumber(gameDataTable.Data[i][{0}])", i);
                    }
                }
                else
                {
                    if (dataArr[i, 1].Equals("string", StringComparison.CurrentCultureIgnoreCase))
                    {
                        sbr.AppendFormat("gameDataTable.Data[i][{0}], ", i);
                    }
                    else
                    {
                        sbr.AppendFormat("tonumber(gameDataTable.Data[i][{0}]), ", i);
                    }
                }
            }
            sbr.Append(" );\r\n");
            sbr.Append("    end\r\n");
            sbr.Append("\r\n");
            sbr.Append("end\r\n");
            sbr.Append("\r\n");
            sbr.AppendFormat("function {0}DBModel.GetList()\r\n", fileName);
            sbr.AppendFormat("    return {0}Table;\r\n", fileName.ToLower());
            sbr.Append("end");
            sbr.Append("\r\n");
            sbr.Append("\r\n");
            sbr.AppendFormat("function {0}DBModel.GetEntity(id)\r\n", fileName);
            sbr.AppendFormat("    local ret = nil;\r\n");
            sbr.AppendFormat("    for i = 1, #{0}Table, 1 do\r\n", fileName.ToLower());
            sbr.AppendFormat("        if ({0}Table[i].Id == id) then\r\n", fileName.ToLower());
            sbr.AppendFormat("            ret = {0}Table[i];\r\n", fileName.ToLower());
            sbr.AppendFormat("            break;\r\n");
            sbr.AppendFormat("        end\r\n");
            sbr.AppendFormat("    end\r\n");
            sbr.AppendFormat("    return ret;\r\n");
            sbr.AppendFormat("end");

            using (FileStream fs = new FileStream(string.Format("{0}CreateLua/{1}DBModel.lua", filePath, fileName), FileMode.Create))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.Write(sbr.ToString());
                }
            }
        }
        #endregion
        #region 读取文件
        private void btnSelectData_Click(object sender, EventArgs e)
        {

            string key = DesDecrypt(textBox25.Text);
            if (key == null)
            {
                MessageBox.Show("key不对,请检查后重新输入");
                return;
            }
            if (this.openDataFileDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {


                this.txtFileData.Text = "";
                string strPath = this.openDataFileDialog.FileName;

                using (GameDataTableParser parse = new GameDataTableParser(strPath))
                {
                    while (!parse.Eof)
                    {
                        StringBuilder sbr = new StringBuilder();
                        for (int i = 0; i < parse.FieldName.Length; i++)
                        {
                            sbr.AppendFormat("{0} ", parse.GetFieldValue(parse.FieldName[i]));
                        }

                        this.txtFileData.AppendText(sbr.ToString() + "\r\n");
                        parse.Next();
                    }
                }
            }

        }

        #endregion
        #region 密匙
        /// <summary>
        /// 加密字符串
        /// 注意:密钥必须为８位
        /// </summary>
        /// <param name="strText">字符串</param>
        /// <param name="encryptKey">返回加密后的字符串</param>
        public string DesEncrypt(string inputString)
        {
            byte[] byKey = null;
            byte[] IV = { 0x12, 0x34, 0x56, 0x68, 0x90, 0xAB, 0xCD, 0xEF };
            try
            {
                byKey = System.Text.Encoding.UTF8.GetBytes("qq381990860".Substring(0, 8));
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                byte[] inputByteArray = Encoding.UTF8.GetBytes(inputString);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(byKey, IV), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                return Convert.ToBase64String(ms.ToArray());
            }
            catch (System.Exception error)
            {
                //return error.Message;
                return null;
            }
        }
        /// <summary>
        /// 解密字符串
        /// </summary>
        /// <param name="this.inputString">加了密的字符串</param>
        /// <param name="decryptKey">密钥</param>
        /// <param name="decryptKey">返回解密后的字符串</param>
        public string DesDecrypt(string inputString)
        {
            byte[] byKey = null;
            byte[] IV = { 0x12, 0x34, 0x56, 0x68, 0x90, 0xAB, 0xCD, 0xEF };
            byte[] inputByteArray = new Byte[inputString.Length];
            try
            {
                byKey = System.Text.Encoding.UTF8.GetBytes("qq381990860".Substring(0, 8));
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                inputByteArray = Convert.FromBase64String(inputString);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(byKey, IV), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                System.Text.Encoding encoding = new System.Text.UTF8Encoding();
                return encoding.GetString(ms.ToArray());
            }
            catch (System.Exception error)
            {
                //return error.Message;
                return null;
            }
        }
        #endregion

        private void button3_Click(object sender, EventArgs e)
        {
            string gonggao;
if(debug)
             gonggao =@"../服务端/公告内容.txt";
else

             gonggao = pathFile + @"公告内容.txti";

            try
            {
                using (StreamWriter sw = new StreamWriter(gonggao))
                {
                    sw.WriteLine(textBox40.Text);
                }
            }
            catch (Exception sd)
            {

                print(sd.Message);

            }
            Notice = textBox40.Text;
            MessageBox.Show("游戏公告更新成功！");

        }
        private void button37_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox41.Text) || string.IsNullOrEmpty(textBox24.Text))
            {
                MessageBox.Show("玩家ID和物品名称不能为空！");
                return;
            }

            Client.ClientMsg.SendMsg(1010, "do local ret={[1]=" + textBox24.Text + ",[2]=\"" + textBox41.Text + "\""
                + ",[3]=\"" + textBox42.Text + "\""
                + ",[4]=\"" + textBox43.Text + "\""
                + ",[5]=\"" + textBox44.Text + "\""
                + ",[6]=\"" + textBox45.Text +"\"} return ret end");
        }
        private void button65_Click(object sender, EventArgs e)
        {

            if (string.IsNullOrEmpty(textBox17.Text) || textBox25.Text != "qq381990860")
            {
                return;
            }
            textBox25.Text = DesEncrypt(textBox17.Text);

        }
        private void button61_Click(object sender, EventArgs e)
        {

            try
            {
                Process.Start(pathFile + @"服务端源码/服务端模板.sublime-project");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        private void button62_Click(object sender, EventArgs e)
        {

            try
            {
                Process.Start(pathFile + @"客户端源码/游戏模板.sublime-project");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }
        private void button63_Click(object sender, EventArgs e)
        {

            try
            {
                string keyName;
                string keyValue;
                keyName = "MyApp";
                keyValue = "My Application";
                RegistryKey key;
                key = Registry.ClassesRoot.CreateSubKey(keyName);
                key.SetValue("", keyValue);
                key = key.CreateSubKey("shell");
                key = key.CreateSubKey("open");
                key = key.CreateSubKey("command");

                key.SetValue("", pathFile + @"../Sublime Text/sublime_text.exe");

                keyName = ".sublime-project"; //相应后缀
                keyValue = "MyApp";
                key = Registry.ClassesRoot.CreateSubKey(keyName);
                key.SetValue("", keyValue);

                MessageBox.Show("关联成功");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "尝试使用右键管理员打开");
            }


        }
        private void button64_Click(object sender, EventArgs e)
        {

            try
            {
                //创建一个进程
                Process p = new Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.UseShellExecute = false;//是否使用操作系统shell启动
                p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
                p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
                p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
                p.StartInfo.CreateNoWindow = true;//不显示程序窗口
                p.Start();//启动程序
                string strCMD = @"Build\GGEBuild.exe ggegame " + pathFile + @"\客户端源码";
                string strCMD1 = @"Build\GGEBuild.exe ggeserver " + pathFile + @"\服务端源码";
                //向cmd窗口发送输入信息
                p.StandardInput.WriteLine("cd..");
                p.StandardInput.WriteLine("cd..");
                p.StandardInput.WriteLine(strCMD);
                p.StandardInput.WriteLine(strCMD1 + "&exit");
                p.StandardInput.AutoFlush = true;
                //获取cmd窗口的输出信息
                string output = p.StandardOutput.ReadToEnd();
                //等待程序执行完退出进程
                p.WaitForExit();
                p.Close();
                File.Copy(pathFile + @"\客户端源码\compile\g2d.exe", pathFile + @"\客户端\登入器v" + textBox4.Text + ".exe", true);
                File.Copy(pathFile + @"\服务端源码\compile\ggeserver.exe", pathFile + @"\服务端\ggeserver.exe", true);
                MessageBox.Show("编译完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n跟踪;" + ex.StackTrace);
            }

        }

        private void button67_Click(object sender, EventArgs e)
        {

            try
            {
                //创建一个进程
                Process p = new Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.UseShellExecute = false;//是否使用操作系统shell启动
                p.StartInfo.RedirectStandardInput = true;//接受来自调用程序的输入信息
                p.StartInfo.RedirectStandardOutput = true;//由调用程序获取输出信息
                p.StartInfo.RedirectStandardError = true;//重定向标准错误输出
                p.StartInfo.CreateNoWindow = true;//不显示程序窗口
                p.Start();//启动程序
                //向cmd窗口发送输入信息
                p.StandardInput.WriteLine("taskkill/im g2d.exe /f");
                p.StandardInput.WriteLine("taskkill/im ggeserver.exe /f" + "&exit");
                p.StandardInput.AutoFlush = true;
                //获取cmd窗口的输出信息
                string output = p.StandardOutput.ReadToEnd();
                //等待程序执行完退出进程
                p.WaitForExit();
                p.Close();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message + "\r\n跟踪;" + ex.StackTrace);
            }

        }
        private void ranse(string arr, string rsFilePathname)
        {

            if (string.IsNullOrEmpty(arr)) return;


            StringBuilder msgrs = new StringBuilder();
            using (StreamReader sr = new StreamReader(arr))
            {
                string line;

                // 从文件读取并显示行，直到文件的末尾 
                while ((line = sr.ReadLine()) != null)
                {
                    msgrs.Append(line + '\n');
                }
            }

            string pp = msgrs.ToString();
            string[] mm = Regex.Split(pp, "\\s+", RegexOptions.IgnoreCase);

            byte[] buffer = null;
            using (MMO_MemoryStream ms = new MMO_MemoryStream())
            {
                for (int sdi = 0; sdi < mm.Length - 1; sdi++)
                {
                    int sda = int.Parse(mm[sdi]);

                    ms.WriteInt(sda > 1000 ? sda - 1000 : sda);


                }

                buffer = ms.ToArray();
            }
            FileStream fs = new FileStream(@"C:\Users\Administrator\Desktop\染色\" + rsFilePathname + ".wpal", FileMode.Create);
            fs.Write(new byte[] { 0x77, 0x70, 0x61, 0x6c }, 0, 4);
            fs.Write(buffer, 0, buffer.Length);
            fs.Close();

        }

        private void button66_Click(object sender, EventArgs e)
        {

            if (this.openFileDialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string rsFilePath = "";
                List<string> rsFilePathname = new List<string>();
                foreach (string strName in this.openFileDialog1.FileNames)
                {
                    rsFilePath += strName + "\r\n";

                    rsFilePathname.Add(System.IO.Path.GetFileNameWithoutExtension(strName));
                }


                string[] arr = rsFilePath.Trim().Split('\r', '\n');
                if (arr.Length > 0)
                {
                    for (int i = 0; i < arr.Length; i++)
                    {


                        ranse(arr[i], rsFilePathname[i / 2]);

                    }
                    MessageBox.Show("创建成功");
                }


            }


        }

        private void listBox5_SelectedIndexChanged(object sender, EventArgs e)
        {

if(debug)
            WriteLstToTxt(listBox5, pathFile + @"服务端/封禁IP.txt");
else
            WriteLstToTxt(listBox5, pathFile + "封禁IP.txt");


        }

        private void button51_Click(object sender, EventArgs e)
        {
if(debug)
            ReadTxtToLst(listBox5, pathFile + @"服务端/封禁IP.txt");
else
            ReadTxtToLst(listBox5, pathFile + "封禁IP.txt");


        }

 

        private void button5_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox22.Text))
            {
                MessageBox.Show("参数不能为空");
                return;
            }
            IniFile iniFile;
if (debug)
              iniFile = new IniFile(@"../服务端/config.ini");
else

             iniFile = new IniFile(pathFile + @"config.ini");

            
            iniFile.writeIni("mainconfig", "lv", textBox46.Text);
            Client.ClientMsg.SendMsg(1002, "do local ret={[1]=" + textBox46.Text + ",[2]=\"限制等级\"} return ret end");

        }

        private void button57_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox3.Text))
            {
                MessageBox.Show("玩家ID不能为空！");
                return;
            }
            if (string.IsNullOrEmpty(textBox47.Text))
            {
                MessageBox.Show("自定义不能为空！");
                return;
            }
            Client.ClientMsg.SendMsg(1003, "do local ret={[1]=" + textBox3.Text + ",[2]=\"" + textBox47.Text + "\"} return ret end");
        }

        private void button71_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox56.Text) || comboBox22.SelectedItem == null || comboBox13.SelectedItem == null || string.IsNullOrEmpty(textBox51.Text) || string.IsNullOrEmpty(textBox50.Text))
            {
                MessageBox.Show("红色标签为必须填写的值！");
                return;
            }
           //  附加类型, 附加属性, 三围)
            StringBuilder Msg = new StringBuilder();
            Msg.Append(textBox56.Text);//等级
            Msg.Append("*-*");
            Msg.Append(comboBox22.SelectedItem);//类型
            Msg.Append("*-*");
            Msg.Append(comboBox13.SelectedItem);// 主属性
            Msg.Append("*-*");
            Msg.Append(textBox51.Text);//主数值
            Msg.Append("*-*");

            if (!(comboBox19.SelectedItem == null))//附加类型
                Msg.Append(comboBox19.SelectedItem);
            Msg.Append("*-*");
            if (!(string.IsNullOrEmpty(textBox54.Text)))//附加属性
                Msg.Append(textBox54.Text);
            Msg.Append("*-*");

            if (!(comboBox21.SelectedItem == null))
                Msg.Append(comboBox21.SelectedItem);
            Msg.Append("=");
            if (!(string.IsNullOrEmpty(textBox57.Text)))
                Msg.Append(textBox57.Text);
            Msg.Append("@");
            if (!(comboBox20.SelectedItem == null))
                Msg.Append(comboBox20.SelectedItem);
            Msg.Append("=");// 三围
            if (!(string.IsNullOrEmpty(textBox55.Text)))
                Msg.Append(textBox55.Text);
            Client.ClientMsg.SendMsg(1013, "do local ret={[1]=" + textBox50.Text + ",[2]=\"" + Msg.ToString() + "\"} return ret end");
        }

        private void button72_Click(object sender, EventArgs e)
        {
            Form2 f2 = new Form2();
            f2.Show();
        }

        private void button73_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox1.Items.Count; i++)
            {
                checkedListBox1.SetItemChecked(i, false);
            }
        }

        private void checkBox14_CheckedChanged(object sender, EventArgs e)
        {


            if (checkBox14.Checked)
                debug = true;
            else
                debug = false;

            if (debug)
                pathFile = new DirectoryInfo("../").FullName;//当前应用程序路径的上级目录 //System.AppDomain.CurrentDomain.BaseDirectory;
            else
                pathFile = "./";

        }

        private void button75_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBox41.Text) )
            {
                MessageBox.Show("物品名称不能为空！");
                return;
            }

            Client.ClientMsg.SendMsg(1010, "do local ret={[1]=\"全服\",[2]=\"" + textBox41.Text + "\""
                + ",[3]=\"" + textBox42.Text + "\""
                + ",[4]=\"" + textBox43.Text + "\""
                + ",[5]=\"" + textBox44.Text + "\""
                + ",[6]=\"" + textBox45.Text + "\"} return ret end");
        }
    }

}
