using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;
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
            this.title.AutoSize = true;
            this.title.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.title.Location = new System.Drawing.Point(180, 150);
            this.title.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.title.Name = "title";
            this.title.Size = new System.Drawing.Size(118, 20);
            this.title.TabIndex = 0;
            this.title.Text = "NetCoreServer";
            this.title.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // connectBtn
            // 
            this.connectBtn.Location = new System.Drawing.Point(180, 250);
            this.connectBtn.Margin = new System.Windows.Forms.Padding(4);
            this.connectBtn.Name = "connectBtn";
            this.connectBtn.Size = new System.Drawing.Size(120, 30);
            this.connectBtn.TabIndex = 1;
            this.connectBtn.Text = "连接";
            this.connectBtn.UseVisualStyleBackColor = true;
            this.connectBtn.Click += new System.EventHandler(this.DB_Click);
            // 
            // startServerBtn
            // 
            this.startServerBtn.Location = new System.Drawing.Point(180, 400);
            this.startServerBtn.Margin = new System.Windows.Forms.Padding(4);
            this.startServerBtn.Name = "startServerBtn";
            this.startServerBtn.Size = new System.Drawing.Size(120, 30);
            this.startServerBtn.TabIndex = 2;
            this.startServerBtn.Text = "Start Server";
            this.startServerBtn.UseVisualStyleBackColor = true;
            this.startServerBtn.Click += new System.EventHandler(this.StartServer_Click);
            // 
            // stopServerBtn
            // 
            this.stopServerBtn.Location = new System.Drawing.Point(180, 450);
            this.stopServerBtn.Name = "stopServerBtn";
            this.stopServerBtn.Size = new System.Drawing.Size(120, 30);
            this.stopServerBtn.TabIndex = 3;
            this.stopServerBtn.Text = "Stop Server";
            this.stopServerBtn.UseVisualStyleBackColor = true;
            this.stopServerBtn.Click += new System.EventHandler(this.StopServer_Click);
            // 
            // onlineNumBtn
            // 
            this.onlineNumBtn.Location = new System.Drawing.Point(180, 500);
            this.onlineNumBtn.Name = "onlineNumBtn";
            this.onlineNumBtn.Size = new System.Drawing.Size(120, 30);
            this.onlineNumBtn.TabIndex = 4;
            this.onlineNumBtn.Text = "在线人数";
            this.onlineNumBtn.UseVisualStyleBackColor = true;
            this.onlineNumBtn.Click += new System.EventHandler(this.OnlineNum_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::NetCoreServer.Properties.Resources.red_light_x32;
            this.pictureBox1.Location = new System.Drawing.Point(228, 330);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(32, 32);
            this.pictureBox1.TabIndex = 5;
            this.pictureBox1.TabStop = false;
            // 
            // logText
            // 
            this.logText.AutoSize = true;
            this.logText.Location = new System.Drawing.Point(100, 550);
            this.logText.Name = "logText";
            this.logText.Size = new System.Drawing.Size(48, 20);
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
            this.contextMenuStrip1.Size = new System.Drawing.Size(129, 76);
            // 
            // showToolStripMenuItem
            // 
            this.showToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.maxToolStripMenuItem,
            this.windowToolStripMenuItem});
            this.showToolStripMenuItem.Name = "showToolStripMenuItem";
            this.showToolStripMenuItem.Size = new System.Drawing.Size(128, 24);
            this.showToolStripMenuItem.Text = "显示";
            this.showToolStripMenuItem.Click += new System.EventHandler(this.ShowToolStripMenuItem_Click);
            // 
            // maxToolStripMenuItem
            // 
            this.maxToolStripMenuItem.Name = "maxToolStripMenuItem";
            this.maxToolStripMenuItem.Size = new System.Drawing.Size(152, 26);
            this.maxToolStripMenuItem.Text = "Max";
            // 
            // windowToolStripMenuItem
            // 
            this.windowToolStripMenuItem.Name = "windowToolStripMenuItem";
            this.windowToolStripMenuItem.Size = new System.Drawing.Size(152, 26);
            this.windowToolStripMenuItem.Text = "Window";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(128, 24);
            this.exitToolStripMenuItem.Text = "退出";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.ExitToolStripMenuItem_Click);
            // 
            // connectToolStripMenuItem
            // 
            this.connectToolStripMenuItem.Name = "connectToolStripMenuItem";
            this.connectToolStripMenuItem.Size = new System.Drawing.Size(128, 24);
            this.connectToolStripMenuItem.Text = "连接DB";
            this.connectToolStripMenuItem.Click += new System.EventHandler(this.ConnectToolStripMenuItem_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(480, 720);
            this.Controls.Add(this.logText);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.onlineNumBtn);
            this.Controls.Add(this.stopServerBtn);
            this.Controls.Add(this.startServerBtn);
            this.Controls.Add(this.connectBtn);
            this.Controls.Add(this.title);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainForm";
            this.Text = "Winform+NetCore3.1+MongoDB";
            this.SizeChanged += new System.EventHandler(this.Form1_SizeChanged);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private async void DB_Click(object sender, System.EventArgs e)
        {
            //await MySQLTool.MultiSQL();
            await MySQLTool.TestQuery();

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

        // 重启
        void Restart()
        {
            Application.Restart();
            Environment.Exit(0);
        }
        void RegisterHandler()
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            //AppDomain.CurrentDomain.FirstChanceException += CurrentDomain_FirstChanceException;
            //AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;
            Application.ThreadException += new ThreadExceptionEventHandler(Application_ThreadException);
        }
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = (Exception)e.ExceptionObject;
            Console.WriteLine("MyHandler caught : " + ex.Message);
            Console.WriteLine("trace: {0}", ex.StackTrace);
            Console.WriteLine("Runtime terminating: {0}", e.IsTerminating);
        }
        private void CurrentDomain_FirstChanceException(object sender, System.Runtime.ExceptionServices.FirstChanceExceptionEventArgs e)
        {
            throw new NotImplementedException();
        }
        private void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }
        static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            /*
            var fromAddress = new MailAddress("your Gmail address", "Your name");
            var toAddress = new MailAddress("email address where you want to receive reports", "Your name");
            const string fromPassword = "your password";
            const string subject = "exception report";
            Exception exception = e.Exception;
            string body = exception.Message + "\n" + exception.Data + "\n" + exception.StackTrace + "\n" + exception.Source;

            var smtp = new SmtpClient
            {
                Host = "smtp.gmail.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
            };
            using (var message = new MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                //You can also use SendAsync method instead of Send so your application begin invoking instead of waiting for send mail to complete. SendAsync(MailMessage, Object) :- Sends the specified e-mail message to an SMTP server for delivery. This method does not block the calling thread and allows the caller to pass an object to the method that is invoked when the operation completes. 
                smtp.Send(message);
            }*/

            /*
            Exception ex = default(Exception);
            ex = e.Exception;
            ILog log = LogManager.GetLogger(typeof(Program)); //Log4NET
            log.Error(ex.Message + "\n" + ex.StackTrace);*/
        }
        public static void WriteLog(Exception ex)
        {
            /*
            string errorTime = "异常时间：" + DateTime.Now.ToString();
            //string errorAddress = "异常地址：" + HttpContext.Current.Request.Url.ToString();
            string errorInfo = "异常信息：" + ex.Message;
            string errorSource = "错误源：" + ex.Source;
            string errorType = "运行类型：" + ex.GetType();
            string errorFunction = "异常函数：" + ex.TargetSite;
            string errorTrace = "堆栈信息：" + ex.StackTrace;
            HttpContext.Current.Server.ClearError();
            System.IO.StreamWriter writer = null;
            try
            {
                //写入日志
                string path = string.Empty;
                path = HttpContext.Current.Server.MapPath("~/ErrorLogs/");
                //不存在则创建错误日志文件夹
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                path += string.Format(@"\{0}.txt", DateTime.Now.ToString("yyyy-MM-dd"));

                writer = !System.IO.File.Exists(path) ? System.IO.File.CreateText(path) : System.IO.File.AppendText(path); //判断文件是否存在，如果不存在则创建，存在则添加
                writer.WriteLine("用户IP:" + HttpContext.Current.Request.UserHostAddress);
                writer.WriteLine(errorTime);
                writer.WriteLine(errorAddress);
                writer.WriteLine(errorInfo);
                writer.WriteLine(errorSource);
                writer.WriteLine(errorType);
                writer.WriteLine(errorFunction);
                writer.WriteLine(errorTrace);
                writer.WriteLine("********************************************************************************************");
            }
            finally
            {
                if (writer != null)
                {
                    writer.Close();
                }
            }*/
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