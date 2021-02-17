using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows;
using System.ComponentModel;

namespace MultiscriptRunner.Database
{
    class DatabaseQueryService
    {
        private string connectionString;
        private BackgroundWorker bw;
        public delegate void SqlBatchComplete();
        private SqlBatchComplete jobCompletedDelegate;
        
        public DatabaseQueryService(string connectionString)
        {
            this.connectionString = connectionString;
            bw = new BackgroundWorker();
            bw.RunWorkerCompleted += (sender, args) =>
            {
                if (jobCompletedDelegate != null) jobCompletedDelegate.Invoke();
            };
        }

        private void Bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            throw new NotImplementedException();
        }

        public void addDelegate(SqlBatchComplete dele)
        {
            jobCompletedDelegate = dele;
        }

        public DataTable getDatabaseList()
        {
            List<string> dbNameList = new List<string>();
            DataTable dbTable = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT name from sys.databases", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dbTable.Load(dr);
                    }
                }
            }
            return dbTable;
        }
        public DataTable GetFilteredDatabaseList(string queryStr)
        {
            List<string> dbNameList = new List<string>();
            DataTable dbTable = new DataTable();
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (SqlCommand cmd = new SqlCommand("SELECT name from sys.databases where name like '%" + queryStr + "%'", conn))
                {
                    using (SqlDataReader dr = cmd.ExecuteReader())
                    {
                        dbTable.Load(dr);
                    }
                }
            }
            return dbTable;
        }

        public bool IsJobExecuting()
        {
            return bw.IsBusy;
        }

        public bool CancelJob()
        {
            bw.WorkerSupportsCancellation = true;
            if (!bw.IsBusy) return false;
            bw.CancelAsync();
            return true;
        }

        public void ExecuteDatabaseJob(List<string> dbNames, SqlConnectionStringBuilder connBuilder, string query, DataGrid dg)
        {
            if (!bw.IsBusy)
            {
                bw.WorkerSupportsCancellation = false; //Have to set this to false, otherwise causes performance issues on UI thread.
                DataTable dt = new DataTable();
                Object dtLock = new Object();
                BindingOperations.EnableCollectionSynchronization(dt.DefaultView, dtLock);
                bw.DoWork += (sender, args) =>
                {
                    foreach (string s in dbNames)
                    {
                        if (bw.CancellationPending) break;
                        connBuilder.InitialCatalog = s;
                        using (SqlConnection conn = new SqlConnection(connBuilder.ToString()))
                        {
                            conn.Open();
                            using (SqlCommand cmd = new SqlCommand(query, conn))
                            {
                                try
                                {
                                    using (SqlDataReader dr = cmd.ExecuteReader())
                                    {
                                        if (dt.Columns.Count < 1)
                                        {
                                            dt.Columns.Add("DatabaseName");
                                            dt.Columns.Add("RowsUpdated");
                                            foreach (var columnSchema in dr.GetColumnSchema())
                                            {
                                                dt.Columns.Add(columnSchema.ColumnName);
                                            }
                                            bw.ReportProgress(1);
                                        }
                                        if (!dr.HasRows)
                                        {
                                            DataRow dataRow = dt.NewRow();
                                            dataRow[0] = new string(s);
                                            dataRow[1] = (dr.RecordsAffected == -1 ? 0 : dr.RecordsAffected);
                                            lock (dt)
                                            {
                                                dt.Rows.Add(dataRow);
                                            }
                                        }
                                        while (dr.Read())
                                        {
                                            DataRow dataRow = dt.NewRow();
                                            dataRow[0] = new string(s);
                                            dataRow[1] = dr.RecordsAffected;
                                            for (int i = 0; i < dr.FieldCount; ++i)
                                            {
                                                if (dr.IsDBNull(i))
                                                {

                                                    dataRow[i + 2] = new string("NULL");
                                                }
                                                else dataRow[i + 2] = Convert.ToString(dr.GetValue(i));
                                            }
                                            lock (dt)
                                            {
                                                dt.Rows.Add(dataRow);
                                            }
                                            bw.ReportProgress(1);
                                        }
                                        Console.WriteLine();
                                    }
                                }
                                catch (Exception e)
                                {
                                    MessageBox.Show("Execute query failed: " + e.ToString());
                                }
                            }
                        }
                    }
                };

                bw.ProgressChanged += (sender, args) =>
                {
                    lock (dt)
                    {
                        dg.DataContext = dt.DefaultView;
                    }
                };

                bw.WorkerReportsProgress = true;

                bw.RunWorkerAsync();
            }
        }
    }
}
