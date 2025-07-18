using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DemoHelper
{
    public class MSSQL
    {
        string ConnString = ConfigurationManager.ConnectionStrings["UserDB"].ConnectionString;

        public async Task<DataSet> ExecuteStoredProcedureReturnDS(string SPName,Dictionary<string,string> InPara)
        {
            DataSet ds = new DataSet();
            
            try
            {
                SqlConnection con = new SqlConnection(ConnString);
                SqlCommand cmd = new SqlCommand();
                cmd.Parameters.Clear();
                cmd.CommandText = SPName;
                cmd.Connection = con;
                cmd.CommandTimeout = 60;
                cmd.CommandType = CommandType.StoredProcedure;

                foreach (KeyValuePair<string,string> param in InPara)
                {
                    cmd.Parameters.AddWithValue(param.Key, param.Value);
                }

                SqlDataAdapter da = new SqlDataAdapter();
                da.SelectCommand = cmd;
                await Task.Run(() => da.Fill(ds));
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return ds;
        }

        public async Task ExecuteStoredProcedure(string SPName, Dictionary<string, string> InPara)
        {
            try
            {
                SqlConnection con = new SqlConnection(ConnString);
                SqlCommand cmd = new SqlCommand();
                cmd.Parameters.Clear();
                cmd.CommandText = SPName;
                cmd.Connection = con;
                cmd.CommandTimeout = 60;
                cmd.CommandType = CommandType.StoredProcedure;

                foreach (KeyValuePair<string, string> param in InPara)
                {
                    cmd.Parameters.AddWithValue(param.Key, param.Value);
                }
                con.Open();
                await cmd.ExecuteNonQueryAsync();
                con.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<SqlDataReader> ExecuteStoredProcedureReturnDataReader(string SPName, Dictionary<string, string> InPara)
        {
            SqlDataReader dr = null;
            try
            {
                SqlConnection con = new SqlConnection(ConnString);
                SqlCommand cmd = new SqlCommand();
                cmd.Parameters.Clear();
                cmd.CommandText = SPName;
                cmd.Connection = con;
                cmd.CommandTimeout = 60;
                cmd.CommandType = CommandType.StoredProcedure;

                foreach (KeyValuePair<string, string> param in InPara)
                {
                    cmd.Parameters.AddWithValue(param.Key, param.Value);
                }

                await con.OpenAsync();
                dr = await cmd.ExecuteReaderAsync(CommandBehavior.CloseConnection);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return dr;
        }

        public async Task<object> ExecuteStoredProcedureReturnObject(string SPName, Dictionary<string, string> InPara)
        {
            Object obj = new object();
            try
            {
                SqlConnection con = new SqlConnection(ConnString);
                SqlCommand cmd = new SqlCommand();
                cmd.Parameters.Clear();
                cmd.CommandText = SPName;
                cmd.Connection = con;
                cmd.CommandTimeout = 60;
                cmd.CommandType = CommandType.StoredProcedure;

                foreach (KeyValuePair<string, string> param in InPara)
                {
                    cmd.Parameters.AddWithValue(param.Key, param.Value);
                }

                if (con.State == ConnectionState.Closed)
                {
                    con.Open();
                }
                obj = await cmd.ExecuteScalarAsync();
                con.Close();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return obj;
        }
    }
}
