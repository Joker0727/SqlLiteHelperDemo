using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SqlLiteHelperDemo
{
    public class SQLiteHelper : IDisposable
    {
        private static IFormatProvider format = new System.Globalization.CultureInfo("zh-cn", true);
        private SQLiteConnection MyConn;

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        public SQLiteHelper(string datasource)
        {
            if (!File.Exists(datasource))
            {
                MessageBox.Show("数据异常,请重新安装软件");
                return;
            }
            //连接数据库
            SQLiteConnectionStringBuilder connstr = new SQLiteConnectionStringBuilder();
            connstr.DataSource = datasource;
            //connstr.Version = 3;
            //connstr.Password = password; //设置密码，SQLite ADO.NET实现了数据库密码保护
            MyConn = new SQLiteConnection(connstr.ToString());
        }
        #endregion

        #region 数据操作系列

        #region ExeSql      仅执行Insert与Update语句,返回影响条数
        /// <summary>
        /// 仅执行Insert与Update语句,返回影响条数
        /// </summary>
        /// <param name="sql">SQL语句</param>
        public int ExeSql(string sql)
        {
            if (MyConn.State != ConnectionState.Open) MyConn.Open();
            SQLiteTransaction tx = MyConn.BeginTransaction();
            SQLiteCommand Comm = MyConn.CreateCommand();
            try
            {
                int resultint = -1;
                Comm.CommandText = sql;

                Comm.Transaction = tx;
                resultint = Comm.ExecuteNonQuery();
                tx.Commit();
                return resultint;
            }
            catch (Exception s)
            {
                try
                {
                    tx.Rollback();
                }
                catch {; }
                //  FileHelper.WriteError("ExeSql:"+sql, s);
                return -1;
            }
            finally
            {
                tx.Dispose();
                Comm.Dispose();
                MyConn.Close();
            }
        }
    
        /// <summary>
        /// 仅执行Insert与Update语句,返回影响条数
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="pars">SQL参数名与文件流</param>
        public int ExeSql(string sql, Dictionary<string, byte[]> pars)
        {
            if (MyConn.State != ConnectionState.Open) MyConn.Open();
            SQLiteTransaction tx = MyConn.BeginTransaction();
            SQLiteCommand Comm = MyConn.CreateCommand();
            try
            {
                int resultint = -1;
                Comm.CommandText = sql;
                setPar(Comm, pars);
                Comm.Transaction = tx;
                resultint = Comm.ExecuteNonQuery();
                tx.Commit();
                return resultint;
            }
            catch (Exception s)
            {
                try
                {
                    tx.Rollback();
                }
                catch {; }
                //FileHelper.WriteError("ExeSql:" + sql, s);
                return -1;
            }
            finally
            {
                try
                {
                    tx.Dispose();
                }
                catch {; }
                Comm.Dispose();
                MyConn.Close();
            }
        }

        /// <summary>
        /// 仅执行Insert与Update语句,返回影响条数
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="pars">SQL参数名与文件流</param>
        public int ExeSql(string sql, Dictionary<string, object> pars)
        {
            if (MyConn.State != ConnectionState.Open) MyConn.Open();
            SQLiteTransaction tx = MyConn.BeginTransaction();
            SQLiteCommand Comm = MyConn.CreateCommand();
            try
            {
                int resultint = -1;
                Comm.CommandText = sql;
                setPar(Comm, pars);
                Comm.Transaction = tx;
                resultint = Comm.ExecuteNonQuery();
                tx.Commit();
                return resultint;
            }
            catch (Exception s)
            {
                tx.Rollback();
                // FileHelper.WriteError("ExeSql:" + sql, s);
                return -1;
                //throw new Exception(string.Format(format, "ExeSql异常:\r\n{0};\r\n错误信息:{1};", sql, s.ToString()));
            }
            finally
            {
                tx.Dispose();
                Comm.Dispose();
                MyConn.Close();
            }
        }

        /// <summary>
        /// 仅执行Insert与Update语句,返回刚插入的ID与影响条数
        /// </summary>
        /// <param name="sql">SQL语句</param>
        public int ExeSqlOut(string sql)
        {
            if (MyConn.State != ConnectionState.Open) MyConn.Open();
            SQLiteTransaction tx = MyConn.BeginTransaction();
            SQLiteCommand Comm = MyConn.CreateCommand();
            try
            {
                int outid = -1;
                Comm.CommandText = sql + " ;\r\nselect LAST_INSERT_ROWID()";

                Comm.Transaction = tx;
                outid = Convert.ToInt32(Comm.ExecuteScalar());
                tx.Commit();
                return outid;
            }
            catch (Exception s)
            {
                tx.Rollback();
                //   FileHelper.WriteError("ExeSqlOut:" + sql, s);
                return -1;
            }
            finally
            {
                tx.Dispose();
                Comm.Dispose();
                MyConn.Close();
            }
        }

        /// <summary>
        /// 仅执行Insert,返回刚插入的ID与影响条数
        /// </summary>
        /// <param name="sql">SQL语句</param>
        /// <param name="pars">SQL参数名与文件流</param>
        public int ExeSqlOut(string sql, Dictionary<string, byte[]> pars)
        {
            if (MyConn.State != ConnectionState.Open) MyConn.Open();
            SQLiteTransaction tx = MyConn.BeginTransaction();
            SQLiteCommand Comm = MyConn.CreateCommand();
            try
            {
                int outid = -1;
                Comm.CommandText = sql + " ;select LAST_INSERT_ROWID()";
                setPar(Comm, pars);
                Comm.Transaction = tx;
                outid = Convert.ToInt32(Comm.ExecuteScalar());
                tx.Commit();
                return outid;
            }
            catch (Exception s)
            {
                tx.Rollback();
                //  FileHelper.WriteError("ExeSqlOut:" + sql, s);
                return -1;

                //throw new Exception(string.Format(format, "ExeSqlOut异常:\r\n{0};\r\n错误信息:{1};", sql, s.ToString()));
            }
            finally
            {
                tx.Dispose();
                Comm.Dispose();
                MyConn.Close();
            }
        }
        #endregion

        #region SearchSql       //获取数据  查询语句，返回DataTable
        /// <summary>
        /// 通过执行SQL语句，获取表中数据。
        /// </summary>
        /// <param name="sError">错误信息</param>
        /// <param name="sSQL">执行的SQL语句</param>
        public DataTable SearchSql(string sql)
        {
            if (MyConn.State != ConnectionState.Open) MyConn.Open();
            DataTable dt = new DataTable();
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(sql, MyConn))
                {
                    using (SQLiteDataAdapter dao = new SQLiteDataAdapter(cmd))
                    {
                        dao.Fill(dt);
                    }
                }
            }
            catch (Exception e)
            {
            }
            finally
            {
                MyConn.Close();
            }
            return dt;
        }
        #endregion

        #region RunSql       执行 insert,update,delete
        /// <summary>
        /// 通过执行SQL语句，执行insert,update,delete 动作
        /// </summary>
        /// <param name="sError">错误信息</param>
        /// <param name="sSQL">执行的SQL语句</param>
        /// <param name="bUseTransaction">是否使用事务</param>             
        public bool RunSql(string sql)
        {
            bool iResult = false;
            if (MyConn.State != ConnectionState.Open) MyConn.Open();
            try
            {
                using (SQLiteCommand cmd = new SQLiteCommand(sql, MyConn))
                {
                    iResult = cmd.ExecuteNonQuery() > 0;
                }
            }
            catch (Exception ex)
            {
            }
            finally
            {
                MyConn.Close();
            }
            return iResult;
        }
        #endregion

        //获取数据
        #region GetScalar       获取第一行第一列
        /// <summary>
        /// 获取第一行第一列
        /// </summary>
        /// <param name="par">SQL结构体</param>
        public object GetScalar(string sql)
        {
            if (MyConn.State != ConnectionState.Open) MyConn.Open();
            SQLiteCommand Comm = MyConn.CreateCommand();
            try
            {
                object sobject;
                Comm.CommandText = sql;
                sobject = Comm.ExecuteScalar();
                return sobject;
            }
            catch (Exception s)
            {
                //  FileHelper.WriteError("ExeSqlOut:" + sql, s);
                return null;
            }
            finally
            {
                Comm.Dispose();
                MyConn.Close();
            }
        }
        #endregion

        #region GetRow          获取一行
        /// <summary>
        /// 获取一行数据
        /// </summary>
        /// <param name="par">SQL结构体</param>
        public object[] GetRow(string sql)
        {
            if (MyConn.State != ConnectionState.Open) MyConn.Open();
            SQLiteCommand Comm = MyConn.CreateCommand();
            try
            {
                ArrayList m = null;
                Comm.CommandText = sql;
                SQLiteDataReader dr = Comm.ExecuteReader(CommandBehavior.SingleRow);
                if (dr.Read())
                {
                    m = new ArrayList(20);
                    for (int n = 0; n < dr.FieldCount; n++)
                        m.Add(dr[n]);
                }
                dr.Close();
                return m.ToArray();
            }
            catch (Exception s)
            {
                // FileHelper.WriteError("GetRow:" + sql, s);
                return null;
            }
            finally
            {
                Comm.Dispose();
                MyConn.Close();
            }
        }
        #endregion

        #region GetField        获取一列数据
        /// <summary>
        /// 获取一列数据
        /// </summary>
        /// <param name="par">SQL结构体</param>
        public object[] GetField(string sql)
        {
            if (MyConn.State != ConnectionState.Open) MyConn.Open();
            SQLiteCommand Comm = MyConn.CreateCommand();
            try
            {
                ArrayList m = new ArrayList(20);
                Comm.CommandText = sql;
                SQLiteDataReader dr = Comm.ExecuteReader(CommandBehavior.CloseConnection);
                while (dr.Read())
                    m.Add(dr[0]);
                dr.Close();
                return m.ToArray();
            }
            catch (Exception s)
            {
                // FileHelper.WriteError("GetRow:" + sql, s);
                return null;
            }
            finally
            {
                Comm.Dispose();
                MyConn.Close();
            }
        }
        #endregion

        #region GetArray        获取二维数组
        /// <summary>
        /// 获取二维数组
        /// </summary>
        /// <param name="par">SQL结构体</param>
        public List<object[]> GetArray(string sql)
        {
            if (MyConn.State != ConnectionState.Open) MyConn.Open();
            SQLiteCommand Comm = MyConn.CreateCommand();
            try
            {
                List<object[]> m;
                Comm.CommandText = sql;
                SQLiteDataReader dr = Comm.ExecuteReader(CommandBehavior.CloseConnection);
                m = drtoarray(dr);
                dr.Close();
                return m;
            }
            catch (Exception s)
            {
                // FileHelper.WriteError("GetRow:" + sql, s);
                return null;
            }
            finally
            {
                Comm.Dispose();
                MyConn.Close();
            }
        }
        #endregion

        //一些内部操作
        #region DataReader转成List<object[]>
        /// <summary>
        /// DataReader转成List&lt;object[]&gt;
        /// </summary>
        /// <param name="mydr">传入只读器</param>
        /// <returns></returns>
        /// 传入参数是每个函数中新生成的,或者是参数调进来的
        /// 如果是新生成的,那么变量作用域只存在于父函数体内,因此不会存在共享
        /// 如果是参数调进来的,那么因为该类是实例化的,会为每个对像实例化,所以也不会存在共享
        private static List<object[]> drtoarray(SQLiteDataReader mydr)
        {
            List<object[]> m = new List<object[]>();
            do
            {
                while (mydr.Read())
                {
                    int max = mydr.FieldCount;
                    object[] n = new object[max];
                    for (int i = 0; i < max; i++)
                        n[i] = mydr.GetValue(i);
                    m.Add(n);
                }
            }
            while (mydr.NextResult());
            return m;
        }
        #endregion

        #region 添加内部参数
        /// <summary>
        /// 添加内部参数
        /// </summary>
        /// <param name="Comm">Comm</param>
        /// <param name="pars">参数实体类</param>
        /// 传入参数是每个函数中新生成的,或者是参数调进来的
        /// 如果是新生成的,那么变量作用域只存在于父函数体内,因此不会存在共享
        /// 如果是参数调进来的,那么因为该类是实例化的,会为每个对像实例化,所以也不会存在共享
        private void setPar(SQLiteCommand Comm, Dictionary<string, byte[]> pars)
        {
            if (pars != null && pars.Count > 0) //参数名不为空才要进行到下一步,且parvalue必定会有
            {
                foreach (var par in pars)
                {
                    SQLiteParameter m = new SQLiteParameter(DbType.Binary);
                    m.ParameterName = par.Key;
                    m.Value = par.Value;
                    Comm.Parameters.Add(m);
                }
            }
        }

        /// <summary>
        /// 添加内部参数
        /// </summary>
        /// <param name="Comm">Comm</param>
        /// <param name="pars">参数实体类</param>
        /// 传入参数是每个函数中新生成的,或者是参数调进来的
        /// 如果是新生成的,那么变量作用域只存在于父函数体内,因此不会存在共享
        /// 如果是参数调进来的,那么因为该类是实例化的,会为每个对像实例化,所以也不会存在共享
        private void setPar(SQLiteCommand Comm, Dictionary<string, object> pars)
        {
            if (pars != null && pars.Count > 0) //参数名不为空才要进行到下一步,且parvalue必定会有
            {
                foreach (var par in pars)
                {
                    SQLiteParameter m = new SQLiteParameter(DbType.String);
                    m.ParameterName = par.Key;
                    m.Value = par.Value;
                    Comm.Parameters.Add(m);
                }
            }
        }
        #endregion
        #endregion

        #region 压缩数据库
        public void Compression()
        {
            if (MyConn.State != ConnectionState.Open) MyConn.Open();
            SQLiteCommand cmd = new SQLiteCommand("VACUUM", MyConn);
            cmd.ExecuteNonQuery();
        }
        #endregion

        #region IDisposable 成员
        void IDisposable.Dispose()
        {
            MyConn.Dispose();
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
