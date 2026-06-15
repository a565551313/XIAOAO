namespace 笑傲西游
{
    /// <summary>
    /// 登录
    /// </summary>
    public class Login
    {
        /// <summary>
        /// 
        /// </summary>
        public string 账号 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string 密码 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string 卡洛 { get; set; }
    }
    public class Root
    {
        /// <summary>
        /// 
        /// </summary>
        public string 密码 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string 账号 { get; set; }
        public string ip { get; set; }
        public int id { get; set; }
        public int 编号 { get; set; }
    }
    public class vVerify
    {
        /// <summary>
        /// 
        /// </summary>
        public string 空了 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string 皮皮 { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string 版本 { get; set; }
    }
    public class RegisterRole
    {
        public int 序号 { get; set; }
        public int 编号 { get; set; }
        public string 账号 { get; set; }
        public string 名称 { get; set; }
        public string 密码 { get; set; }

        public int id { get; set; }
        public string ip { get; set; }
    }

    public class Register//注册
    {

        public string b { get; set; }
        public string d { get; set; }
        public string c { get; set; }

        public int a { get; set; }
        public string e { get; set; }
    }
    public class DataMsg
    {
        public int b { get; set; }
        public string f { get; set; }
        public string g { get; set; }
        public int id { get; set; }
        public int d { get; set; }
        public int c { get; set; }
        public int a { get; set; }
        public int e { get; set; }
    }
    public class Message
    {
        public string text { get; set; }
        public int id { get; set; }
    }
    public class Usermsg
    {

        public string User { get; set; }
        public string Name { get; set; }
        public int id { get; set; }
    }
    public class MapUpdata
    {
        /// <summary>
        /// 
        /// </summary>
        public string Stall { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Monster { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Model { get; set; }
        /// <summary>
        /// 
        /// </summary>
        public string Role { get; set; }
        public string Number { get; set; }
    }
    public class DataMsgs
    {

        public string 文本 { get; set; }
        public string ip { get; set; }
        public int 数字id { get; set; }
        public int 连接id { get; set; }
        public int d { get; set; }
        public int 序号 { get; set; }
        public string 编号 { get; set; }
        public int 参数 { get; set; }

        public DataMsgs(string ip, int 数字id, int 连接id, int 序号, int 参数, string 文本, string 编号)
        {
            this.ip = ip;
            this.数字id = 数字id;
            this.连接id = 连接id;
            this.序号 = 序号;
            this.参数 = 参数;
            this.文本 = 文本;
            this.编号 = 编号;
        }

    }


}
