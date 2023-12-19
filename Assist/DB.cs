using Rabit.Info;
using MySql.Data.MySqlClient;
using System;
using System.Data;
using Rabit.Models;

namespace Rabit.Assist
{
    public class DB
    {
        #region single
        public static DataTable SelectSingle(string query)
        {
            DataTable dt = new DataTable();

            using (MySqlConnection con = new MySqlConnection(Conf.IConfig["db"] + ";sslmode=None;ConnectionTimeout=100"))
            {
                con.Open();

                using (MySqlCommand cmd = new MySqlCommand(query.ToString(), con))
                {
                    using (MySqlDataAdapter sda = new MySqlDataAdapter())
                    {
                        sda.SelectCommand = cmd;
                        cmd.CommandTimeout = 180;

                        using (DataSet ds = new DataSet())
                        {
                            sda.Fill(ds);
                            dt = ds.Tables[0];
                        }
                    }
                }
            }
            return dt;
        }
        #endregion

        #region multi
        public static DataTable[] SelectMulti(string[] query)
        {
            DataTable[] dt = new DataTable[query.Length];

            using (MySqlConnection con = new MySqlConnection(Conf.IConfig["db"] + ";sslmode=None;ConnectionTimeout=100"))
            {
                con.Open();

                using (MySqlCommand cmd = new MySqlCommand(string.Concat(query), con))
                {
                    using (MySqlDataAdapter sda = new MySqlDataAdapter())
                    {
                        sda.SelectCommand = cmd;
                        cmd.CommandTimeout = 180;

                        using (DataSet ds = new DataSet())
                        {
                            sda.Fill(ds);
                            for (int i = 0; i < ds.Tables.Count; i++)
                            {
                                dt[i] = ds.Tables[i];
                            }
                        }
                    }
                }
            }

            return dt;
        }
        #endregion

        #region execute
        public static void Execute(string query)
        {
            if (string.IsNullOrEmpty(query)) return;

            try
            {
                using (MySqlConnection con = new MySqlConnection(Conf.IConfig["db"] + ";sslmode=None;ConnectionTimeout=100"))
                {
                    con.Open();
                    new MySqlCommand(query, con).ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Conf.ILog.Error(ex.ToString());
            }
        }
        #endregion

        #region 전일 주식정보
        public static void GetLastdayInfo()
        {
            string query = "SELECT a.code, a.volume, a.capital, a.isalert FROM tb_stock_sise AS a WHERE a.date=(SELECT date FROM tb_stock_sise GROUP BY date ORDER BY date DESC LIMIT 1);";
            var dt = SelectSingle(query);

            for (int i = 0; i < dt.Rows.Count; i++)
            {
                var row = dt.Rows[i].ItemArray;

                string code = (string)row[0];
                long volume = (long)row[1];
                long cap    = (long)row[2];

                Data.Items.TryGetValue(code, out Item im);
                if (im == null) continue;

                if (cap < 1000 || volume < 30000 || !string.IsNullOrEmpty((string)row[3])) Data.Items.Remove(code);                
            }
        }
        #endregion
    }
}