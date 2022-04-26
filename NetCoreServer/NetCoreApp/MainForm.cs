using System;
using System.Diagnostics;
using System.Windows.Forms;

namespace WinFormsApp1
{
    public partial class MainForm : Form
    {
        // 跨类调用单例
        static MainForm _instance;
        public static MainForm Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new MainForm();
                    RegisterHandler();
                }
                return _instance;
            }
        }

        public MainForm()
        {
            InitializeComponent();

            this.SizeChanged += new EventHandler(Form1_SizeChanged);
            this.notifyIcon1.MouseClick += new MouseEventHandler(notifyIcon1_MouseClick);
            this.notifyIcon1.DoubleClick += new EventHandler(notifyIcon1_DoubleClick);

            //Debug.Print("count---" + this.notifyIcon1.ContextMenuStrip.Items.Count);
            //Debug.Print("0---" + this.notifyIcon1.ContextMenuStrip.Items[0].Name);
            //Debug.Print("1---" + this.notifyIcon1.ContextMenuStrip.Items[1].Name);
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            switch (this.WindowState)
            {
                case FormWindowState.Normal:
                    Debug.Print("还原窗口");
                    break;
                case FormWindowState.Minimized:
                    this.Hide(); //隐藏主窗体
                    this.notifyIcon1.Visible = true; //让notifyIcon1图标显示
                    Debug.Print("最小化");
                    break;
                case FormWindowState.Maximized:
                    Debug.Print("最大化");
                    break;
            }
        }

        // 点击托管图标时触发
        private void notifyIcon1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) //当鼠标点击为左键时
            {
                Debug.Print("左键点击");

                //this.Show(); //显示主窗体
                //this.WindowState = FormWindowState.Normal; //主窗体的大小为默认
            }
            else if (e.Button == MouseButtons.Right)
            {
                Debug.Print("右键点击");
            }
        }

        // 双击托管图标时触发
        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            Debug.Print("双击");

            if (this.Visible)
            {
                this.Hide();
            }
            else
            {
                /*
                //还原窗体显示
                this.WindowState = FormWindowState.Normal;
                //激活窗体并给予它焦点
                this.Activate();
                //任务栏区显示图标
                this.ShowInTaskbar = true;
                //托盘区图标隐藏
                this.notifyIcon1.Visible = false;
                */
                this.Show();
                this.WindowState = FormWindowState.Normal;
            }
        }

        private void ConnectToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var box = MessageBox.Show("更新完成，是否立即重启？", "退出", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (box == DialogResult.OK)
            {
                Application.Restart(); //必须是Form
            }
        }

        private void ShowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Normal;
        }

        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var box = MessageBox.Show("是否确认退出程序？", "退出", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            Debug.Print($"{box}");
            if (box == DialogResult.OK)
            {
                //this.Dispose(); //关闭所有的线程
                //this.Close();
                Application.Exit();
            }
        }
    }
}