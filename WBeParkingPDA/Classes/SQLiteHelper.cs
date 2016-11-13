using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SQLite;
using System.IO;

namespace WBeParkingPDA.Classes
{
    internal class SQLiteHelper
    {
        log4net.ILog logger = log4net.LogManager.GetLogger(typeof(SQLiteHelper));

        private SQLiteConnection sql_con;
        private SQLiteCommand sql_cmd;
        private SQLiteDataAdapter DB;

        private string connectionString = "";

        public SQLiteHelper(string dbpath)
        {
            try
            {
                if (File.Exists(dbpath) == false)
                {
                    SQLiteConnection.CreateFile(dbpath);

                }

                connectionString = string.Format("Data Source={0};", dbpath);

                sql_con = new SQLiteConnection(connectionString);

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw ex;
            }
        }

        public void openConnection()
        {
            try
            {
                if (sql_con.State == ConnectionState.Closed)
                    sql_con.Open();
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw ex;
            }
        }
        public void closeConnection()
        {
            try
            {
                if (sql_con != null)
                {
                    sql_con.Close();
                    sql_con.Dispose();
                    sql_con = null;
                }

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw ex;
            }
        }

        public SQLiteDataReader select(SQLiteCommand QueryCMD)
        {

            try
            {

                this.openConnection();
                QueryCMD.Connection = sql_con;
                SQLiteDataReader SR = QueryCMD.ExecuteReader(CommandBehavior.CloseConnection);
                QueryCMD.Dispose();

                return SR;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw ex;
            }
        }

        public void select(SQLiteCommand QueryCMD, DataTable DT)
        {


            try
            {

                openConnection();
                QueryCMD.Connection = sql_con;

                SQLiteDataAdapter AD = new SQLiteDataAdapter(QueryCMD);

                AD.Fill(DT);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw ex;
            }
            finally
            {
                closeConnection();
            }
        }

        public DataTable select(string tsql, params object[] sqlparamters)
        {
            DataSet ds = null;
            DataTable dt = null;
            SQLiteTransaction st = null;
            try
            {
                ds = new DataSet("WBSystem");
                st = transActionBegin();
                sql_cmd = sql_con.CreateCommand();
                sql_cmd.CommandText = tsql;
                sql_cmd.CommandType = CommandType.Text;
                sql_cmd.Transaction = st;

                for (int i = 0; i < sqlparamters.Length; i++)
                {
                    SQLiteParameter p = new SQLiteParameter(string.Format("@p{0}", i), sqlparamters[i]);
                    sql_cmd.Parameters.Add(p);
                }

                DB = new SQLiteDataAdapter(sql_cmd);
                DB.Fill(ds);

                if (ds != null && ds.Tables.Count == 1)
                {
                    dt = ds.Tables[0];
                }
               
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
            }
            finally
            {
                transActionClose(st);
            }

            return dt;
        }

        public void execute(string tsql, params object[] sqlparamters)
        {
            SQLiteTransaction st = null;
            try
            {
                
                st = transActionBegin();
                sql_cmd = sql_con.CreateCommand();
                sql_cmd.CommandText = tsql;
                sql_cmd.CommandType = CommandType.Text;
                sql_cmd.Transaction = st;

                for (int i = 0; i < sqlparamters.Length; i++)
                {
                    SQLiteParameter p = new SQLiteParameter(string.Format("@p{0}", i), sqlparamters[i]);
                    sql_cmd.Parameters.Add(p);
                }
               
                sql_cmd.ExecuteNonQuery();                
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
            }
            finally
            {
                transActionClose(st);
            }

        }
        public void execute(SQLiteCommand sqlcmd)
        {


            try
            {
                openConnection();
                sqlcmd.Connection = sql_con;
                sqlcmd.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw ex;
            }
            finally
            {
                closeConnection();
            }
        }


        public void execute(SQLiteCommand[] sqlCmd)
        {
            SQLiteTransaction st = null;


            try
            {
                st = transActionBegin();

                for (int i = 0; i < sqlCmd.Length; i++)
                {
                    transActionExecute(sqlCmd[i], st);
                }

                transActionClose(st);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                st.Rollback();
                throw ex;
            }
            finally
            {
                if (sql_con != null && sql_con.State == ConnectionState.Open)
                    closeConnection();
            }
        }

        /*
         * 以下為 transaction
         * 
         */
        public SQLiteTransaction transActionBegin()
        {
            // SqlConnection conn = new SqlConnection();
            openConnection();
            SQLiteTransaction sTran = sql_con.BeginTransaction();
            return sTran;
        }

        public void transActionExecute(SQLiteCommand cmd, SQLiteTransaction cTrans)
        {
            cmd.Transaction = cTrans;
            cmd.Connection = cTrans.Connection;
            cmd.ExecuteNonQuery();
        }

        public SQLiteDataReader transActionExecute_DataReader(SQLiteCommand cmd, SQLiteTransaction cTrans)
        {
            cmd.Transaction = cTrans;
            cmd.Connection = cTrans.Connection;
            return cmd.ExecuteReader();
        }

        public void transActionClose(SQLiteTransaction sTran)
        {
            sql_con = sTran.Connection;

            try
            {
                sTran.Commit();
            }
            catch (Exception ex)
            {
                sTran.Rollback();
                throw ex;
            }
            finally
            {
                closeConnection();
            }
        }
    }
}
