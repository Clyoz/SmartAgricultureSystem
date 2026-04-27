using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;

namespace SmartAgricultureSystem.Helpers
{

    /// <summary>
    /// 数据库辅助类
    /// </summary>
    public class DBHelper
    {
        //获取数据库连接字符串
        public static string ConnStr = System.Configuration.ConfigurationManager.ConnectionStrings["SmartAgricultureDB"].ToString();

        #region 增、删、改（普通的SQL语句）
        /// <summary>
        /// 增、删、改（普通的SQL语句）
        /// </summary>
        /// <param name="sql">普通的SQL语句</param>
        /// <returns>返回的受影响的行数/returns>  
        public static int CUD(string sql, Dictionary<string, object> parameters)
        {

            SqlConnection con = null;
            try
            {
                //创建数据库连接对象
                con = new SqlConnection(ConnStr);
                //打开数据库连接
                con.Open();
                //创建命令对象
                SqlCommand com = new SqlCommand(sql, con);
                int index = com.ExecuteNonQuery();
                return index;

            }
            catch (Exception ex)
            {

                return -1;
            }
            finally
            {
                con.Close();
            }
        }

        #endregion

        #region 增、删、改（参数化SQL语句）
        /// <summary>
        /// 增、删、改（参数化SQL语句）
        /// </summary>
        /// <param name="sql">参数化SQL语句</param>
        /// <returns>返回的受影响的行数/returns>
        public static int CUD(string sql, List<SqlParameter> sqlParams)
        {
            SqlConnection con = null;
            try
            {
                //创建数据库连接对象
                con = new SqlConnection(ConnStr);
                //打开数据库连接
                con.Open();
                //创建命令对象
                SqlCommand com = new SqlCommand(sql, con);
                //ToArray()用于将集合转换为数组
                com.Parameters.AddRange(sqlParams.ToArray());
                int index = com.ExecuteNonQuery();
                return index;

            }
            catch (Exception ex)
            {
                return -1;
            }
            finally
            {
                con.Close();
            }
        }

        #endregion

        #region  执行查询操作，返回SqlDataReader对象
        /// <summary>
        /// 执行查询操作，返回SqlDataReader对象
        /// </summary>
        /// <param name="sql">普通的SQL语句</param>
        /// <returns>返回的SqlDataReader对象</returns>

        public static SqlDataReader GetRead(string sql)
        {
            Console.WriteLine(ConnStr);
            SqlConnection con = null;
            try
            {
                con = new SqlConnection(ConnStr);
                con.Open();
                SqlCommand com = new SqlCommand(sql, con);
                //CommandBehavior.CloseConnection用于设置当SqlDataReader关闭时会自动关闭连接
                SqlDataReader dr = com.ExecuteReader(CommandBehavior.CloseConnection);

                //SqlDataReader dr = com.ExecuteReader();
                return dr;

            }
            catch (Exception ex)
            {

                return null;

            }
            //finally
            //{
            //    con.Close();
            //}


        }
        #endregion


        #region  执行查询操作，返回SqlDataReader对象(参数化SQL语句)
        /// <summary>
        /// 执行查询操作，返回SqlDataReader对象(参数化SQL语句)
        /// </summary>
        /// <param name="sql">参数化SQL语句</param>
        /// <returns>返回的SqlDataReader对象</returns>

        public static SqlDataReader GetRead(string sql, List<SqlParameter> parameters)
        {
            SqlConnection con = null;
            try
            {
                con = new SqlConnection(ConnStr);
                con.Open();
                SqlCommand com = new SqlCommand(sql, con);
                com.Parameters.AddRange(parameters.ToArray());
                //CommandBehavior.CloseConnection用于设置当SqlDataReader关闭时会自动关闭连接
                SqlDataReader dr = com.ExecuteReader(CommandBehavior.CloseConnection);
                return dr;

            }
            catch (Exception ex)
            {

                return null;
            }
            //finally
            //{
            //    con.Close();
            //}
        }
        #endregion


        #region 执行查询操作，返回DataTable对象(普通SQL语句)
        /// <summary>
        /// 执行查询操作，返回DataTable对象(普通SQL语句)
        /// </summary>
        /// <param name="sql">普通sql语句</param>
        /// <returns>返回DataTable</returns>
        public static DataTable GetDateTable(string sql)
        {
            SqlConnection con = null;
            try
            {
                con = new SqlConnection(ConnStr);
                con.Open();
                SqlDataAdapter da = new SqlDataAdapter(sql, con);
                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;


            }
            catch (Exception ex)
            {

                return null;
            }
            finally
            {
                con.Close();
            }

        }
        #endregion


        #region 执行查询操作，返回DataTable对象(参数化SQL语句)
        /// <summary>
        /// 执行查询操作，返回DataTable对象(参数化SQL语句)
        /// </summary>
        /// <param name="sql">带@参数化SQL语句</param>
        /// <returns>返回DataTable</returns>
        public static DataTable GetDateTable(string sql, List<SqlParameter> parameters)
        {
            SqlConnection con = null;
            try
            {
                con = new SqlConnection(ConnStr);
                con.Open();
                SqlCommand com = new SqlCommand(sql, con);
                com.Parameters.AddRange(parameters.ToArray());
                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = com;

                DataTable dt = new DataTable();
                da.Fill(dt);
                return dt;


            }
            catch (Exception ex)
            {

                return null;
            }
            finally
            {
                con.Close();
            }

        }
        #endregion


        #region 执行查询语句，返回第一行第一个单元格中的数据(普通SQL语句)
        /// <summary>
        /// 执行查询语句，返回第一行第一个单元格中的数据
        /// </summary>
        /// <param name="sql">普通的SQL语句</param>
        /// <returns>返回第一行第一个单元格中的数据</returns>
        public static object GetScalar(string sql)
        {

            SqlConnection con = null;
            try
            {
                con = new SqlConnection(ConnStr);
                con.Open();
                SqlCommand com = new SqlCommand(sql, con);
                object result = com.ExecuteScalar();
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                con.Close();
            }
        }
        #endregion


        #region 执行查询语句，返回第一行第一个单元格中的数据(参数化SQL语句)
        /// <summary>
        /// 执行查询语句，返回第一行第一个单元格中的数据(参数化SQL语句)
        /// </summary>
        /// <param name="sql">参数化SQL语句</param>
        /// <returns>返回第一行第一个单元格中的数据</returns>
        public static object GetScalar(string sql, List<SqlParameter> parameters)
        {

            SqlConnection con = null;
            try
            {
                con = new SqlConnection(ConnStr);
                con.Open();
                SqlCommand com = new SqlCommand(sql, con);
                com.Parameters.AddRange(parameters.ToArray());
                object result = com.ExecuteScalar();
                return result;
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                con.Close();
            }
        }
        #endregion
    }
}
