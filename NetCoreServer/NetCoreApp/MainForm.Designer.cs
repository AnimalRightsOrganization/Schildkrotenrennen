using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using NetCoreServer;
using NetCoreServer.Utils;

namespace WinFormsApp1
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            this.notifyIcon1.Dispose(); //清理托盘图标
            TcpChatServer.TCPChatServer.Stop(); //执行速度较慢

            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.title = new System.Windows.Forms.Label();
            this.connectBtn = new System.Windows.Forms.Button();
            this.startServerBtn = new System.Windows.Forms.Button();
            this.stopServerBtn = new System.Windows.Forms.Button();
            this.onlineNumBtn = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.logText = new System.Windows.Forms.Label();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.showToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.maxToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.windowToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.connectToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // title
            // 
            this.title.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.title.Location = new System.Drawing.Point(170, 180);
            this.title.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.title.Name = "title";
            this.title.Size = new System.Drawing.Size(140, 30);
            this.title.TabIndex = 0;
            this.title.Text = "NetCoreServer";
            this.title.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // connectBtn
            // 
            this.connectBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.connectBtn.Location = new System.Drawing.Point(180, 300);
            this.connectBtn.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.connectBtn.Name = "connectBtn";
            this.connectBtn.Size = new System.Drawing.Size(120, 30);
            this.connectBtn.TabIndex = 1;
            this.connectBtn.Text = "测试1";
            this.connectBtn.UseVisualStyleBackColor = true;
            this.connectBtn.Click += new System.EventHandler(this.Test_Click);
            // 
            // startServerBtn
            // 
            this.startServerBtn.Location = new System.Drawing.Point(180, 450);
            this.startServerBtn.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.startServerBtn.Name = "startServerBtn";
            this.startServerBtn.Size = new System.Drawing.Size(120, 30);
            this.startServerBtn.TabIndex = 2;
            this.startServerBtn.Text = "Start Server";
            this.startServerBtn.UseVisualStyleBackColor = true;
            this.startServerBtn.Click += new System.EventHandler(this.StartServer_Click);
            // 
            // stopServerBtn
            // 
            this.stopServerBtn.Location = new System.Drawing.Point(180, 500);
            this.stopServerBtn.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.stopServerBtn.Name = "stopServerBtn";
            this.stopServerBtn.Size = new System.Drawing.Size(120, 30);
            this.stopServerBtn.TabIndex = 3;
            this.stopServerBtn.Text = "Stop Server";
            this.stopServerBtn.UseVisualStyleBackColor = true;
            this.stopServerBtn.Click += new System.EventHandler(this.StopServer_Click);
            // 
            // onlineNumBtn
            // 
            this.onlineNumBtn.Location = new System.Drawing.Point(180, 560);
            this.onlineNumBtn.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.onlineNumBtn.Name = "onlineNumBtn";
            this.onlineNumBtn.Size = new System.Drawing.Size(120, 30);
            this.onlineNumBtn.TabIndex = 4;
            this.onlineNumBtn.Text = "在线人数";
            this.onlineNumBtn.UseVisualStyleBackColor = true;
            this.onlineNumBtn.Click += new System.EventHandler(this.OnlineNum_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.AccessibleRole = System.Windows.Forms.AccessibleRole.OutlineButton;
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.Image = global::NetCoreServer.Properties.Resources.red_light_x32;
            this.pictureBox1.Location = new System.Drawing.Point(220, 360);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(0);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(40, 40);
            this.pictureBox1.TabIndex = 5;
            this.pictureBox1.TabStop = false;
            // 
            // logText
            // 
            this.logText.AutoSize = true;
            this.logText.Location = new System.Drawing.Point(15, 11);
            this.logText.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.logText.Name = "logText";
            this.logText.Size = new System.Drawing.Size(56, 24);
            this.logText.TabIndex = 6;
            this.logText.Text = "NULL";
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "notifyIcon1";
            this.notifyIcon1.Visible = true;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showToolStripMenuItem,
            this.exitToolStripMenuItem,
            this.connectToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(142, 94);
            // 
            // showToolStripMenuItem
            // 
            this.showToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.maxToolStripMenuItem,
            this.windowToolStripMenuItem});
            this.showToolStripMenuItem.Name = "showToolStripMenuItem";
            this.showToolStripMenuItem.Size = new System.Drawing.Size(141, 30);
            this.showToolStripMenuItem.Text = "显示";
            this.showToolStripMenuItem.Click += new System.EventHandler(this.ShowToolStripMenuItem_Click);
            // 
            // maxToolStripMenuItem
            // 
            this.maxToolStripMenuItem.Name = "maxToolStripMenuItem";
            this.maxToolStripMenuItem.Size = new System.Drawing.Size(181, 34);
            this.maxToolStripMenuItem.Text = "Max";
            // 
            // windowToolStripMenuItem
            // 
            this.windowToolStripMenuItem.Name = "windowToolStripMenuItem";
            this.windowToolStripMenuItem.Size = new System.Drawing.Size(181, 34);
            this.windowToolStripMenuItem.Text = "Window";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(141, 30);
            this.exitToolStripMenuItem.Text = "退出";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItem_Click);
            // 
            // connectToolStripMenuItem
            // 
            this.connectToolStripMenuItem.Name = "connectToolStripMenuItem";
            this.connectToolStripMenuItem.Size = new System.Drawing.Size(141, 30);
            this.connectToolStripMenuItem.Text = "连接DB";
            this.connectToolStripMenuItem.Click += new System.EventHandler(this.ConnectToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(478, 694);
            this.Controls.Add(this.logText);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.onlineNumBtn);
            this.Controls.Add(this.stopServerBtn);
            this.Controls.Add(this.startServerBtn);
            this.Controls.Add(this.connectBtn);
            this.Controls.Add(this.title);
            this.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.Name = "MainForm";
            this.Text = "Winform+NetCore3.1+MongoDB";
            this.SizeChanged += new System.EventHandler(this.Form1_SizeChanged);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private void Test_Click(object sender, System.EventArgs e)
        {
            //await MySQLTool.MultiSQL();
            //await MySQLTool.TestQuery();

            //var result = await MySQLTool.CheckLogin("lala", "123456");
            //Debug.Print($"lala: {result}");
            //var result2 = await MySQLTool.CheckLogin("test1", "123456");
            //Debug.Print($"test2: {result2}");
            //var result3 = await MySQLTool.CheckUserExist("test1", "123456");
            //Debug.Print($"test3: {result3}");
            //var result4 = await MySQLTool.Register("test3", "123456");
            //Debug.Print($"test4: {result4}");
            //int result5 = Convert.ToInt32(await MySQLTool.CheckUserExist("test3", "123456"));
            //Debug.Print($"test5: {result5}");
            //UserInfo result6 = (await MySQLTool.GetUserInfo("test3", "123456"));
            //if (result6 == null)
            //{
            //    Debug.Print($"result6: 用户名或密码错误");
            //    return;
            //}
            //Debug.Print($"result6: {result6.nickname}");
            PrintMap();
        }

        private async void StartServer_Click(object sender, System.EventArgs e)
        {
            // 检查数据库连接是否正常
            bool db = await MySQLTool.IsConnected();
            if (db == false)
            {
                string error_message = "无法启动服务器，SQL连接失败";
                RefreshLogs(error_message);
                Debug.Print(error_message);
                return;
            }

            TcpChatServer.TCPChatServer.Run();

            this.pictureBox1.Image = global::NetCoreServer.Properties.Resources.green_light_x32;
        }

        private void StopServer_Click(object sender, EventArgs e)
        {
            TcpChatServer.TCPChatServer.Stop();

            this.pictureBox1.Image = global::NetCoreServer.Properties.Resources.red_light_x32;
        }

        private void OnlineNum_Click(object sender, EventArgs e)
        {
            var server = TcpChatServer.TCPChatServer.server;
            if (server == null)
            {
                Debug.Print("server is not started!");
                return;
            }
            //string log1 = $"服务器状态={server.IsStarted}, 连接数={server.ConnectedSessions}";
            //string log2 = $"在线人数={TcpChatServer.TCPChatServer.m_PlayerManager.Count}";
            //string log3 = $"{server.Endpoint.Address}:{server.Endpoint.Port}";
            //this.logText.Text = $"{log1}\n{log2}\n{log3}";

            string content = string.Empty;
            foreach (var player in TcpChatServer.TCPChatServer.m_PlayerManager.GetPlayersAll())
            {
                //content += $"({player.ToString()})\n";
                content += $"{player.UserName}: #{player.RoomId}, #{player.SeatId}, 状态:{player.Status}\n";
            }
            this.logText.Text = content;
        }

        #endregion

        #region Refresh UI in Thread

        private void InvokeUI(Action a)
        {
            this.BeginInvoke(new MethodInvoker(a));
        }

        public void RefreshPlayerNum()
        {
            var num = TcpChatServer.TCPChatServer.m_PlayerManager.Count;
            InvokeUI(() => {
                this.logText.Text = $"在线人数={num}";
            });
        }

        public void RefreshRoomNum()
        {
            var num = TcpChatServer.TCPChatServer.m_RoomManager.Count;
            InvokeUI(() => {
                this.logText.Text = $"房间数={num}";
            });
        }

        public void RefreshLogs(string logs)
        {
            InvokeUI(() => {
                this.logText.Text += $"\n{logs}";
            });
        }

        public void PrintMap()
        {
            string logText = string.Empty;
            var roomManager = TcpChatServer.TCPChatServer.m_RoomManager;
            if (roomManager == null || roomManager.Count == 0)
            {
                logText = "未初始化或未创建房间";
            }
            else
            {
                var room = TcpChatServer.TCPChatServer.m_RoomManager.GetServerRoom(1);
                Debug.Print($"房间内人数={room.m_PlayerDic.Count}");
                logText = room.PrintMap();
            }
            InvokeUI(() => {
                this.logText.Text = logText;
            });
        }

        // 重启
        static void Restart()
        {
            //关闭应用程序并立即启动一个新实例。
            //调用Restart最常见的原因是使用或UpdateAsync方法启动通过ClickOnceUpdate下载的应用程序的新版本。
            //Application.Restart();
            //终止此进程，并将退出代码返回到操作系统。
            //exitCode。返回到操作系统的退出代码。 使用 0（零）指示处理已成功完成。
            //Environment.Exit(0);
            //Environment.Exit(1);

            //Debug.Print($"{Application.ExecutablePath}-------"); //是dll
            //System.Diagnostics.Process.Start(Application.ExecutablePath);
            string path = @"C:\Users\Administrator\Desktop\Turtle\NetCoreServer\NetCoreApp\bin\Debug\netcoreapp3.1\NetCoreServer.exe";
            System.Diagnostics.Process.Start(path);
            Application.Exit();
        }
        static void RegisterHandler()
        {
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
        }
        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Exception ex = e.Exception;
            string body = $"ThreadException: Message: {ex.Message}\nData: {ex.Data}\nTrace: {ex.StackTrace}\nSource: {ex.Source}";
            WriteLog(body);
        }
        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            string body = $"UnhandledException Message: {ex.Message}\nTrace: {ex.StackTrace}\nRuntime terminating: {e.IsTerminating}";
            WriteLog(body);
        }
        public static void WriteLog(string err_message)
        {
            string root = @$"C:\Users\Administrator\Desktop\";

            string fileName = $"{System.DateTime.Now.ToString("yyyy_MM_dd_hh_mm_ss")}.txt"; //2022_04_27_03_19_40
            //Debug.Print(fileName);

            string filePath = Path.Combine(root, fileName);
            File.WriteAllText(filePath, err_message);
        }

        #endregion

        private Label title;
        private Button connectBtn;
        private Button startServerBtn;
        private Button stopServerBtn;
        private Button onlineNumBtn;
        private PictureBox pictureBox1;
        private Label logText;
        private NotifyIcon notifyIcon1;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem showToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private ToolStripMenuItem maxToolStripMenuItem;
        private ToolStripMenuItem windowToolStripMenuItem;
        private ToolStripMenuItem connectToolStripMenuItem;
    }
}