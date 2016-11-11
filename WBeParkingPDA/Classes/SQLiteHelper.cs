using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.SQLite;

namespace WBeParkingPDA.Classes
{
    internal class SQLiteHelper
    {
        log4net.ILog logger = log4net.LogManager.GetLogger(typeof(SQLiteHelper));

        private SQLiteConnection sql_con;
        private SQLiteCommand sql_cmd;
        private SQLiteDataAdapter DB;
        private DataSet DS = new DataSet();
        private DataTable DT = new DataTable();

        public SQLiteHelper(string dbpath)
        {
            try
            {
                sql_con = new SQLiteConnection(string.Format("Data Source={0};Version=3;New=True;Compress=True;", dbpath));

            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
            }
        }

        public void ExecuteQuery(string txtQuery)
        {

            if (sql_con == null)
                return;

            if (sql_con.State == ConnectionState.Closed)
                sql_con.Open();

            SQLiteTransaction trans = sql_con.BeginTransaction();

            try
            {
                sql_cmd = sql_con.CreateCommand();
                sql_cmd.CommandText = txtQuery;
                sql_cmd.ExecuteNonQuery();
                trans.Commit();
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                trans.Rollback();
            }
            finally
            {
                sql_con.Close();
            }

        }

        private DataTable LoadData()
        {
            SetConnection();
            sql_con.Open();
            sql_cmd = sql_con.CreateCommand();
            string CommandText = "select id, desc from mains";
            DB = new SQLiteDataAdapter(CommandText, sql_con);
            DS.Reset();
            DB.Fill(DS);
            DT = DS.Tables[0];
            Grid.DataSource = DT;
            sql_con.Close();
        }
    }
}
