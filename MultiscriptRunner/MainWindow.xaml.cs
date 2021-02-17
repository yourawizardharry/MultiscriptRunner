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
using Microsoft.Win32;
using System.IO;

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
            if (databaseConnectionWindow == null) databaseConnectionWindow = new DatabaseConnector();
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

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            ConnectionGrid.SelectAll();
        }

        private void ResetFilter_Click(object sender, RoutedEventArgs e)
        {
            DbFilterBox.Text = "Filter by name..";
            ConnectionGrid.DataContext = databaseQueryService.getDatabaseList();
        }

        private void ExecuteQueryButton_Click(object sender, RoutedEventArgs e)
        {
            if (!databaseQueryService.IsJobExecuting())
            {
                var dbList = GetDatabaseListFromView(ConnectionGrid);
                if ((MessageBox.Show(dbList.Count + " Databases to execute: " + Environment.NewLine + string.Join(";", dbList), "Confirmation", MessageBoxButton.YesNo)) == MessageBoxResult.Yes)
                {
                    databaseQueryService.addDelegate(() =>
                    {
                        MessageBox.Show("Sql batch completed", "Message");
                    });
                    databaseQueryService.ExecuteDatabaseJob(dbList, databaseConnectionWindow.ConnectionStrBuilder, QueryBox.Text, ResultGrid);
                }
            }
            else MessageBox.Show("Current job is still executing, please wait..");
        }

        private void SwitchServer_Click(object sender, RoutedEventArgs e)
        {
            databaseQueryService.CancelJob();
            this.Hide();
            databaseConnectionWindow.Show();
        }

        private void DataGridToCSV(DataGrid dg, string path)
        {
            dg.SelectAllCells();
            dg.ClipboardCopyMode = DataGridClipboardCopyMode.IncludeHeader;
            ApplicationCommands.Copy.Execute(null, dg);
            ResultGrid.UnselectAllCells();
            File.AppendAllText(path, (string)System.Windows.Clipboard.GetData(System.Windows.DataFormats.CommaSeparatedValue), UnicodeEncoding.UTF8);
        }
        

        private void OutputToCSV(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "CSV|*.csv"
            };
            if (saveFileDialog.ShowDialog() == true)
            {
                DataGridToCSV(ResultGrid, saveFileDialog.FileName);
            }
            
        }

        private void QueryBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if(QueryBox.Text == "Enter Query Here..")
            {
                QueryBox.Text = "";
            }
        }

        private void QueryBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (QueryBox.Text == "")
            {
                QueryBox.Text = "Enter Query Here..";
            }

        }

        private void AbortQuery_ButtonClick(object sender, RoutedEventArgs e)
        {
            if (databaseQueryService.CancelJob()) MessageBox.Show("Query Cancelled", "Message");
            else MessageBox.Show("No Queries are Currently Running", "Message");
        }

        private void DbNameFilter_GotFocus(object sender, RoutedEventArgs e)
        {
            if (DbFilterBox.Text == "Filter by name..") DbFilterBox.Text = "";
        }

        private void DbNameFilter_LostFocus(object sender, RoutedEventArgs e)
        {
            if (DbFilterBox.Text == "") DbFilterBox.Text = "Filter by name..";
        }

        private void DbFilterBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (databaseQueryService != null)
            {
                if (DbFilterBox.Text != "" && DbFilterBox.Text != "Filter by name..") ConnectionGrid.DataContext = databaseQueryService.GetFilteredDatabaseList(DbFilterBox.Text);
                else ConnectionGrid.DataContext = databaseQueryService.getDatabaseList();
            }
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            ConnectionGrid.Height = e.NewSize.Height - 150;
            QueryBox.Height = e.NewSize.Height - 125;
        }
    }
}
