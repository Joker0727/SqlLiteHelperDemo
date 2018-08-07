using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SqlLiteHelperDemo
{
    public partial class Form1 : Form
    {
        public static string basePath = AppDomain.CurrentDomain.BaseDirectory;
        public static string sqlitePath = AppDomain.CurrentDomain.BaseDirectory+ @"sqlite.db";
        public SQLiteHelper sqlLiteHelper = null;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
          
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (!File.Exists(sqlitePath))
            {
                MessageBox.Show("数据故障！", "WeChat");
                return;
            }
            sqlLiteHelper = new SQLiteHelper(sqlitePath);

            string sql = string.Format("insert into wechat (number,sex) values ('{0}','{1}')","11111111111","未知");

            int qwe = sqlLiteHelper.ExeSqlOut(sql);            
        }
    }
}
