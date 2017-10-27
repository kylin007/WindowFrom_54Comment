using CefSharp;
using CefSharp.WinForms;
using Ivony.Html;
using Ivony.Html.Parser;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        DataTable datatable_user = new DataTable();//用户表
        DataTable datatable_linshiComm = new DataTable();//临时评论表
        static DataTable datatable_DBComm = new DataTable();//所有评论
        SqlDataAdapter dapt = new SqlDataAdapter();//用户表
        SqlDataAdapter dapt2 = new SqlDataAdapter();//评论表
        SqlConnection conn = new SqlConnection("Data Source=10.1.51.21;Initial Catalog=54Comment;User ID=sa;Password=123456");///数据库链接语句

        Database_ database = new Database_();//引用数据库操作
        ChromiumWebBrowser wb = new ChromiumWebBrowser();//web窗体控件
        int DBCommentNumber = 0;//数据库剩余条数

        public Form1()
        {
            InitializeComponent();

        }

        /// <summary>
        /// 窗体加载时执行的方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO:  这行代码将数据加载到表“_54CommentDataSet2.User”中。您可以根据需要移动或删除它。
            //this.userTableAdapter1.Fill(this._54CommentDataSet2.User);
            // TODO:  这行代码将数据加载到表“_54CommentDataSet1.TemporaryTable”中。您可以根据需要移动或删除它。
            //this.temporaryTableTableAdapter.Fill(this._54CommentDataSet1.TemporaryTable);
            // TODO:  这行代码将数据加载到表“_54CommentDataSet.User”中。您可以根据需要移动或删除它。
            //this.userTableAdapter.Fill(this._54CommentDataSet.User);
            //WebView webView = new WebView();
            //webView.Address = "http://www.baidu.com";
            //webView.Dock = DockStyle.Fill;
            //this.Controls.Add(webView);

            //必须进行初始化，否则就出来页面啦。
            CefSharp.Cef.Initialize();

            //实例化控件
            wb = new ChromiumWebBrowser("http://www.hi-54.com");
            //设置停靠方式
            wb.Dock = DockStyle.Fill;

            wb.LifeSpanHandler = new OpenPageSelf();

            //wb.IWebBrowser = new IWebBrowser_();
            //加入到当前窗体中
            //this.Controls.Add(wb);
            groupBox1.Controls.Add(wb);

            string sql = "DELETE FROM [TemporaryTable]";
            database.deletesql(sql);

            UpDateDBCommentCount();

            UpdataComment();

            UpdataUser();

            UpdateDBComm();
            
        }

        /// <summary>
        /// 点击用户生成评论
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)//生成对应用户的评论
        {
            string url = wb.Address.ToString();

            if (!url.Contains("http://www.hi-54.com/a-"))
            {
                MessageBox.Show("请跳转至文章页面 ，再生成评论");
                return;
            }

            if (datatable_DBComm.Rows.Count == 0)
            {
                MessageBox.Show("常用评论为空，请前往右下角添加");
                return;
            }
            //同步数据
            SqlCommandBuilder builder = new SqlCommandBuilder(dapt2);
            dapt2.Update(datatable_linshiComm);
            
            Random ran = new Random();

            int a_Index = dataGridView1.CurrentRow.Index;
            int UserId = int.Parse(dataGridView1.Rows[a_Index].Cells[0].Value.ToString());
            string UserName = dataGridView1.Rows[a_Index].Cells[1].Value.ToString();

            int index = ran.Next(0, datatable_DBComm.Rows.Count);
            string CommentText = datatable_DBComm.Rows[index]["CommentText"].ToString();

            DateTime CommentTime = returnTime();

            string sql = string.Format("INSERT INTO [TemporaryTable] VALUES ('{0}','{1}','{2}','{3}')", UserId, UserName, CommentText, CommentTime);
            if (database.InsertInTo(sql))
            {
                UpdataComment();
            }            
        }

        /// <summary>
        /// 随机生成若干条评论
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button1_Click(object sender, EventArgs e)//随机若干条生成评论
        {

            string url = wb.Address.ToString();

            if (!url.Contains("http://www.hi-54.com/a-"))
            {
                MessageBox.Show("请跳转至文章页面 ，再生成评论");
                return;
            }

            if (datatable_DBComm.Rows.Count == 0)
            {
                MessageBox.Show("常用评论为空，请前往右下角添加");
                return;
            }
            if (datatable_user.Rows.Count == 0)
            {
                MessageBox.Show("用户列表无数据，请在用户列表添加数据并保存");
                return;
            }

            //同步数据
            SqlCommandBuilder builder = new SqlCommandBuilder(dapt2);
            dapt2.Update(datatable_linshiComm);

            Random ran = new Random();
            int index_count = ran.Next(1, 11);
            bool isNumber = true;
            try
            {
                var richTextBox_val = richTextBox1.Text;
                index_count = int.Parse(richTextBox1.ToString());
                
            }
            catch {
                isNumber = false;
            }

            int seccesscount = 0;
            for (int i = 0; i < index_count; i++)
            {
                int index = ran.Next(0, datatable_user.Rows.Count);
                string UserName = datatable_user.Rows[index]["UserName"].ToString();
                int UserId = int.Parse(datatable_user.Rows[index]["UserId"].ToString());

                index = ran.Next(0, datatable_DBComm.Rows.Count);
                string CommentText = datatable_DBComm.Rows[index]["CommentText"].ToString();

                DateTime CommentTime = returnTime();

                string sql = string.Format("INSERT INTO [TemporaryTable] VALUES ('{0}','{1}','{2}','{3}')", UserId, UserName, CommentText, CommentTime);
                if (database.InsertInTo(sql))
                {
                    seccesscount++;
                }       
            }
            if (!isNumber)
            {
                MessageBox.Show("文本框输入数字格式错误，随机生成" + index_count + "条评论");
            }
            else if (seccesscount == index_count)
            {
                MessageBox.Show("成功生成" + index_count + "条评论");
            }
            else
            {
                MessageBox.Show("出现未知错误 仅生成" + seccesscount + "条评论");
            }
            UpdataComment();

        }

        /// <summary>
        /// 同步用户表信息至数据库
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)//同步User数据
        {
            try
            {
                SqlCommandBuilder builder = new SqlCommandBuilder(dapt);
                dapt.Update(datatable_user);
                builder = new SqlCommandBuilder(dapt2);
                dapt2.Update(datatable_linshiComm);

                MessageBox.Show("成功");
            }
            catch
            {
                UpdataUser();
                MessageBox.Show("格式出现异常，请注意格式");
            }
            
        }

        /// <summary>
        /// 更新评论
        /// </summary>
        public void UpdataComment()//更新评论
        {
            conn.Open();
            string sql = "select Id,UserId,UserName,CommentText,CommentTime from [TemporaryTable]";
            dapt2 = new SqlDataAdapter(sql, conn);
            datatable_linshiComm = new DataTable();
            dapt2.Fill(datatable_linshiComm);
            dataGridView2.DataSource = datatable_linshiComm;
            dataGridView2.Columns[0].Visible = false;
            dataGridView2.Columns[1].Visible = false;
            //dataGridView2.Columns[0].Width = -10;
            dataGridView2.Columns[2].Width = 60;
            dataGridView2.Columns[3].Width = 195;
            dataGridView2.Columns[4].Width = 120;
            conn.Close();
        }

        /// <summary>
        /// /更新用户
        /// </summary>
        public void UpdataUser()//更新用户
        {
            conn.Open();
            string sql = "select * from [User]";
            dapt = new SqlDataAdapter(sql, conn);
            datatable_user = new DataTable();
            dapt.Fill(datatable_user);
            dataGridView1.DataSource = datatable_user;

            dataGridView1.Columns[0].Visible = false;
            conn.Close();
        }

        /// <summary>
        /// 返回一个时间
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public DateTime returnTime()//返回随机时间
        {
            DateTime dt_now = DateTime.Now;
            DateTime dt_page = returnPageTime();

            DateTime dateTimeMin = Convert.ToDateTime(dt_page);
            DateTime dateTimeMax = Convert.ToDateTime(dt_now);
            dateTimeMax = dateTimeMax.AddDays(3);

            TimeSpan ts = dateTimeMax - dateTimeMin;
            DateTime rTime = DateTime.Now;
            do
            {
                Random r = new Random();
                //int t1 = r.Next(0, (int)ts.TotalDays);
                int t2 = r.Next(0, (int)ts.TotalHours);
                //int t3 = r.Next(0, (int)ts.TotalMinutes);
                //int t4 = r.Next(0, (int)ts.TotalDays);

                DateTime newDT = dateTimeMin.Add(new TimeSpan(0, t2, 0, 0));

                Random random = new Random((int)(DateTime.Now.Ticks));

                int hour = random.Next(7, 22);
                int minute = random.Next(0, 60);
                int second = random.Next(0, 60);
                string tempStr = string.Format("{0} {1}:{2}:{3}", newDT.ToString("yyyy-MM-dd"), hour, minute, second);
                rTime = Convert.ToDateTime(tempStr);
            }
            while (rTime >= dateTimeMax || rTime <= dateTimeMin);


            return rTime;
        }

        /// <summary>
        /// 返回页面时间
        /// </summary>
        /// <returns></returns>
        public DateTime returnPageTime ()//返回页面时间
        {
            wb.Forward();
            var task1 = wb.GetSourceAsync();
            task1.Wait();
            string html = task1.Result;

            var documenthtml = new JumonyParser().Parse(html);

            string time = documenthtml.GetElementById("source").FindFirst(".time").InnerText().ToString();
            return DateTime.Parse(time);
        }

        /// <summary>
        /// 窗口关闭 执行删除临时表
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)//窗口关闭执行事件
        {
            string sql = "DELETE FROM [TemporaryTable]";
            database.deletesql(sql);
            return;
        }

        /// <summary>
        /// 提交按钮实现
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)//提交数据到数据库
        {
            try
            {
                SqlCommandBuilder builder = new SqlCommandBuilder(dapt2);
                dapt2.Update(datatable_linshiComm);
            }
            catch
            {
                UpdataComment();
                MessageBox.Show("评论生成出现异常， 请注意格式");
                return;
            }

            string url = wb.Address.ToString();

            if (!url.Contains("http://www.hi-54.com/a-"))
            {
                MessageBox.Show("请选择评论的文章，并进入详情页");
                return;
            }

            int PageId = int.Parse(url.Split('-')[2].ToString());

            int seccesscount = 0;
            int nowinsert = 0;

            for (int i = 0; i < datatable_linshiComm.Rows.Count; i++)
            {
                int UserId = int.Parse(datatable_linshiComm.Rows[i]["UserId"].ToString());
                string UserName = datatable_linshiComm.Rows[i]["UserName"].ToString();
                string CommentText = datatable_linshiComm.Rows[i]["CommentText"].ToString();
                string comm_Time = datatable_linshiComm.Rows[i]["CommentTime"].ToString();
                if (comm_Time.Length > 19)
                {
                    comm_Time = comm_Time.Substring(0, 19);
                }
                DateTime CommentTime = DateTime.Parse(comm_Time);
                //comm_Time = comm_Time.Replace('-', '/');

                var timestamp = CommentTime.Ticks;
                var now_time = DateTime.Now.Ticks;

                if (timestamp > now_time)
                {
                    //存入数据库

                    DateTime dt_now = DateTime.Now;
                    string sql = string.Format("INSERT INTO [CreationComment] VALUES ('{0}','{1}','{2}','{3}','{4}','{5}')", UserId, PageId, UserName, CommentText, CommentTime, dt_now);
                    if (database.InsertInTo(sql))
                    {
                        seccesscount++;
                    }
                }
                else
                {
                    nowinsert++;
                    //接口提交
                    PostData(UserId, PageId, CommentText, CommentTime);
                }
            }

            UpDateDBCommentCount();

            if ((seccesscount+nowinsert)!=datatable_linshiComm.Rows.Count) {
                MessageBox.Show(nowinsert + "条数据成功发布\n" + seccesscount + "条数据因大于系统时间存入数据库等待执行\n" + (datatable_linshiComm.Rows.Count-(seccesscount+nowinsert)) + "条插入数据库失败\n");
            }
            else {
                MessageBox.Show(nowinsert + "条数据成功发布\n" + seccesscount + "条数据因大于系统时间存入数据库等待执行\n");
            }

            string sql_ = "DELETE FROM [TemporaryTable]";
            database.deletesql(sql_);
            UpdataComment();
            
        }

        /// <summary>
        /// 常见评论编辑按钮
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            CommonComment comment = new CommonComment();
            comment.Show();
        }

        /// <summary>
        /// 弹框提示：双击空白处即可修改
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void dataGridView1_CellBeginEdit(object sender, DataGridViewCellEventArgs e)
        {
            MessageBox.Show("双击空白处即可修改单元格内容");
        }

        /// <summary>
        /// /更新DBtable的数据
        /// </summary>
        public static void UpdateDBComm()//更新DBtable的数据
        {
            string sql = "select * from [CommentText]";
            DataTable datatable_DBComm_1 = new DataTable();
            //datatable_DBComm.Clear();
            datatable_DBComm = new Database_().GetTableBySql(sql);
        }

        /// <summary>
        /// 时钟执行 1000毫秒间隔
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void timer1_Tick(object sender, EventArgs e)
        {

            DateTime dt = DateTime.Now;
            string sql = "select * from [CreationComment] where CommentTime <='" + dt + "'";
            DataTable dt_comm = database.GetTableBySql(sql);
            for (int i = 0; i < dt_comm.Rows.Count; i++)
            {
                int UserId = int.Parse(dt_comm.Rows[i]["UserId"].ToString());
                int PageId = int.Parse(dt_comm.Rows[i]["PageId"].ToString());
                string UserName = dt_comm.Rows[i]["UserName"].ToString();
                string Comment = dt_comm.Rows[i]["Comment"].ToString();
                DateTime CommentTime = DateTime.Parse(dt_comm.Rows[i]["CommentTime"].ToString());
                int Id = int.Parse(dt_comm.Rows[i]["Id"].ToString());

                try
                {
                    ////执行API接口
                    //PostData(UserId, PageId, Comment, CommentTime);
                    ////删除数据
                    //string sql_delete = "DELETE FROM [CreationComment] WHERE Id = " + Id;
                    //database.deletesql(sql_delete);

                    DBCommentNumber--;
                    this.Invoke(new Action(() =>
                    {
                        label4.Text = DBCommentNumber.ToString();
                    }));
                    break;
                }
                catch { }
                
            }

            
        }

        /// <summary>
        /// 上传数据
        /// </summary>
        /// <param name="UserId"></param>
        /// <param name="PageId"></param>
        /// <param name="UserName"></param>
        /// <param name="Comment"></param>
        /// <param name="CommentTime"></param>
        public void PostData(int UserId, int PageId, string Comment, DateTime CommentTime)
        {
            HttpWebRequest wReq = (HttpWebRequest)WebRequest.Create("http://localhost:37831/api/Values");
            
            wReq.Method = "Post";

            wReq.ContentType = "application/json";


            byte[] data = Encoding.Default.GetBytes("{\"UserId\":\"" + UserId + "\",\"PageId\":\"" + PageId + "\",\"Comment\":\"" + Comment + "\",\"CommentTime\":\"" + CommentTime + "\"}");
            wReq.ContentLength = data.Length;
            Stream reqStream = wReq.GetRequestStream();
            reqStream.Write(data, 0, data.Length);
            reqStream.Close();
            using (StreamReader sr = new StreamReader(wReq.GetResponse().GetResponseStream()))
            {
                string result = sr.ReadToEnd();
            } 
        }

        /// <summary>
        /// 跳转首页按钮实现
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button5_Click(object sender, EventArgs e)
        {
            //wb.Address = "http://www.hi-54.com";
            this.wb.Load("http://www.hi-54.com");

        }

        /// <summary>
        /// 更新数据库位发布评论条数
        /// </summary>
        public void UpDateDBCommentCount()
        {
            string sql = "select count(*) from CreationComment";
            DBCommentNumber = database.selectCount(sql);

            this.Invoke(new Action(() =>
            {
                label4.Text = DBCommentNumber.ToString();
            }));
        }

        /// <summary>
        /// 添加双击托盘图标事件（双击显示窗口）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                //还原窗体显示    
                WindowState = FormWindowState.Normal;
                //激活窗体并给予它焦点
                this.Activate();
                //任务栏区显示图标
                this.ShowInTaskbar = true;
                //托盘区图标隐藏
                notifyIcon1.Visible = false;
            }
        }

        /// <summary>
        /// 判断是否最小化,然后显示托盘
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            //判断是否选择的是最小化按钮
            if (WindowState == FormWindowState.Minimized)
            {
                //隐藏任务栏区图标
                this.ShowInTaskbar = false;
                //图标显示在托盘区
                notifyIcon1.Visible = true;
            }
        }

        /// <summary>
        /// 确认是否退出
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (MessageBox.Show("是否退出程序？\n确定则退出程序，取消则最小化至托盘", "退出", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                // 关闭所有的线程
                this.Dispose();
                this.Close();
            }
            else
            {
                e.Cancel = true;
                this.WindowState = FormWindowState.Minimized;
            } 
        }


    }
}
