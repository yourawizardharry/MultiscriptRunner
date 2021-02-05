using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Data;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;

namespace MultiscriptRunner
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class DatabaseConnector : Window
    {
        public string connectionString { get; set; }
        public SqlConnectionStringBuilder ConnectionStrBuilder {get; set; }
        private NewDelegate dele;
        public DatabaseConnector()
        {
            InitializeComponent();
            this.Hide();
        }

        public delegate int NewDelegate();

        public void AddDelegate(NewDelegate newDelegate)
        {
            dele = newDelegate;
        }

        public void DestroryWindow()
        {
            dele = null;
            this.Close();
        }

        public void InitalizeDbConnection(object sender, RoutedEventArgs e)
        {
            ConnectionStrBuilder = new SqlConnectionStringBuilder();
            ConnectionStrBuilder.DataSource = dbURL.Text;
            ConnectionStrBuilder.UserID = dbUser.Text;
            ConnectionStrBuilder.Password = dbPassword.Password;

            try
            {
                ValidateDbCreds(ConnectionStrBuilder.ToString());
                connectionString = ConnectionStrBuilder.ToString();
                MessageBox.Show(this, "Database connection established.", "Connected");
                this.Hide();
                dele.Invoke();
            }
            catch(SqlException ex)
            {
                MessageBox.Show(this, "Connection Error: " + ex.Message, "Failed");
            }
        }

        public bool ValidateDbCreds(string dbConnStr) 
        {
            using(SqlConnection conn = new SqlConnection(dbConnStr))
            {
                conn.Open();
            }
            return true;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if(dele!=null) dele.Invoke();
        }
    }
}
