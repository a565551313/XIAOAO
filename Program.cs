using System;
using System.Windows.Forms;

namespace 笑傲西游
{
    static class Program
    {
        public static Form1 frm1;
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {



            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            frm1 = new Form1();
            //Application.Run(frm1);


            System.Threading.Mutex mutex = new System.Threading.Mutex(false, "笑傲西游");
            bool Running = !mutex.WaitOne(0, false);
            //if (!Running)
                Application.Run(frm1);
            //else
            //    MessageBox.Show("网关已经启动,不能重复启动");
        }
    }
}
