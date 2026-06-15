using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 笑傲西游
{
    public partial class Form2 : Form
    {
        public string pathFile = string.Empty;
        public Form2()
        {
            InitializeComponent();
        }

        private void Form2_Load(object sender, EventArgs e)
        {
            Console.WriteLine(File.Exists(@"./ggeserver.exe"));
            if (!File.Exists(@"./ggeserver.exe"))
            {
                pathFile = new DirectoryInfo("../").FullName;

            }
            else
            {
                pathFile = "./";
            }

            treeView2.Nodes.Clear();
            if (Directory.Exists(pathFile + "玩家信息") == false)
            {
                MessageBox.Show("玩家信息文件夹不存在");
                this.Close();
                return;
            }
            LoadTree(pathFile + "玩家信息");
        }
        public void LoadTree(string path, TreeNode node = null)

        {
            
            string[] dirs = Directory.GetDirectories(path);//获取子目录

            foreach (string dir in dirs)

            {
         
                TreeNode node1 = new TreeNode(Path.GetFileName(dir));

                //TreeNode node1 = new TreeNode(dir);//文件所有路径

                if (node == null)

                {

                    treeView2.Nodes.Add(node1);

                }

                else

                {

                    node.Nodes.Add(node1);

                }

                if (Directory.GetDirectories(dir).Length > 1 )
                {
                   
                    LoadTree(dir, node1);
                }
            }
        }
    }
}
