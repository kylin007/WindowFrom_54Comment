using CefSharp.WinForms;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        DataTable datatable_user = new DataTable();
        DataTable datatable_linshiComm = new DataTable();
        SqlDataAdapter dapt = new SqlDataAdapter();
        SqlDataAdapter dapt2 = new SqlDataAdapter();
        SqlConnection conn = new SqlConnection("Data Source=.;Initial Catalog=54Comment;Integrated Security=True");

        public Form1()
        {
            InitializeComponent();
            //必须进行初始化，否则就出来页面啦。
            CefSharp.Cef.Initialize();

            //实例化控件
            ChromiumWebBrowser wb = new ChromiumWebBrowser("http://www.hi-54.com");
            //设置停靠方式
            wb.Dock = DockStyle.Fill;

            wb.LifeSpanHandler = new OpenPageSelf();
            //加入到当前窗体中
            //this.Controls.Add(wb);
            groupBox1.Controls.Add(wb);
        }

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
            
            conn.Open();
            string sql = "select * from [User]";
            dapt = new SqlDataAdapter(sql, conn);
            dapt.Fill(datatable_user);
            dataGridView1.DataSource = datatable_user;

            sql = "select * from [TemporaryTable]";
            dapt2 = new SqlDataAdapter(sql, conn);
            dapt2.Fill(datatable_linshiComm);
            dataGridView2.DataSource = datatable_linshiComm;
            conn.Close();
        }

        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            //int count = dataGridView1.SelectedRows.Count;
            int a_Index = dataGridView1.CurrentRow.Index;
            string str = dataGridView1.Rows[a_Index].Cells[0].Value.ToString();
            //MessageBox.Show("str");
        }

        private void button1_Click(object sender, EventArgs e)
        {
           
        }

        private void button3_Click(object sender, EventArgs e)
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
                MessageBox.Show("格式出现异常，请注意格式");
            }
            
        }
    }
}
