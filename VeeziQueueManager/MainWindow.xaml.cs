using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MultiscriptRunner.Database;
using System.Data;
using System.Collections;
using MS.Internal;

namespace MultiscriptRunner
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        DatabaseConnector databaseConnectionWindow;
        DatabaseQueryService databaseQueryService;
        public MainWindow()
        {
            InitializeComponent();
            InitalizeDatabaseConnection();
        }


        void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            databaseConnectionWindow.DestroryWindow();
        }

        public void InitalizeDatabaseConnection()
        {
            this.Hide();
            if(databaseConnectionWindow == null) databaseConnectionWindow = new DatabaseConnector();
            databaseConnectionWindow.Show();
            databaseConnectionWindow.AddDelegate(() =>
            {
                if (databaseConnectionWindow.connectionString == null) this.Close();
                else
                {
                    this.Show();
                    databaseQueryService = new DatabaseQueryService(databaseConnectionWindow.connectionString);
                    var dbList = databaseQueryService.getDatabaseList();
                    ConnectionGrid.DataContext = dbList;
                }
                return 1;
            });
        }

        public List<string> GetDatabaseListFromView(DataGrid grid)
        {
            IList drv = grid.SelectedItems as IList;
            List<string> dbList = new List<string>();
            foreach (DataRowView p in drv)
            {
                dbList.Add(p.Row.ItemArray[0].ToString());
            }

            return dbList;
        }

        private void SelectAllBackOfficeDb_Click(object sender, RoutedEventArgs e)
        {
            ConnectionGrid.DataContext = databaseQueryService.GetFilteredDatabaseList("BackOffice");
        }

        private void SelectAllVgcDb_Click(object sender, RoutedEventArgs e)
        {
            ConnectionGrid.DataContext = databaseQueryService.GetFilteredDatabaseList("VeeziVGC");
        }

        private void SelectAllLoyaltyDb_Click(object sender, RoutedEventArgs e)
        {
            ConnectionGrid.DataContext = databaseQueryService.GetFilteredDatabaseList("VeeziLoyalty");
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            ConnectionGrid.SelectAll();
        }

        private void ResetFilter_Click(object sender, RoutedEventArgs e)
        {
            ConnectionGrid.DataContext = databaseQueryService.getDatabaseList();
        }

        private void ExecuteQueryButton_Click(object sender, RoutedEventArgs e)
        {
            var dbList = GetDatabaseListFromView(ConnectionGrid);
            MessageBox.Show(dbList.Count + " Databases to execute: " + Environment.NewLine + string.Join(";", dbList));
            databaseQueryService.ExecuteDatabaseJob(dbList, databaseConnectionWindow.ConnectionStrBuilder, QueryBox.Text, ResultGrid);
        }

        private void SwitchServer_Click(object sender, RoutedEventArgs e)
        {
            this.Hide();
            databaseConnectionWindow.Show();
        }
    }
}
