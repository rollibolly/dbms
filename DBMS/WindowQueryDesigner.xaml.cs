using DBMS.Models.DBStructure;
using DBMS.Utilities;
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
using System.Windows.Shapes;

namespace DBMS
{
    /// <summary>
    /// Interaction logic for WindowQueryDesigner.xaml
    /// </summary>
    public partial class WindowQueryDesigner : Window
    {
        List<DBMSDatabase> databases;
        List<Models.DBStructure.Table> tables;
            
        public WindowQueryDesigner()
        {
            InitializeComponent();
        }

        private void btnAddTable_Click(object sender, RoutedEventArgs e)
        {

        }

        private void comboBoxTables_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string dbName = comboBoxDBs.SelectedItem.ToString();
            FillComboTables(dbName);
        }
        
        public void FillComboTables(string dbName)
        {
            tables = new List<Models.DBStructure.Table>();
            
            foreach (DBMSDatabase d in databases)
            {
                if (d.DatabaseName == dbName)
                {
                    tables = d.Tables;
                    break;
                }
            }

            comboBoxTables.ItemsSource = tables.Select(r => r.TableName).ToList();
        }

        private void btnAddTable_Click_1(object sender, RoutedEventArgs e)
        {
            string dbName = comboBoxDBs.SelectedItem.ToString();
            string tableName = comboBoxTables.SelectedItem.ToString();
            List<Models.DBStructure.TableColumn> columns = new List<Models.DBStructure.TableColumn>();

            foreach (var d in tables)
            {
                if (d.TableName == tableName)
                {
                    columns = d.Columns;
                    break;
                }
            }

            textBlockTables.Text += "\nTable name:\n" + tableName + "\nColumn names:\n";

            foreach (var item in columns)
            {
                textBlockTables.Text += item.Name + "\n";
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            DBMSResult res = SchemaSerializer.LoadDatabases();
            databases = res.Data as List<DBMSDatabase>;
            List<string> dbNames = new List<string>();

            foreach (DBMSDatabase d in databases)
            {
                dbNames.Add(d.DatabaseName);
            }

            this.comboBoxDBs.ItemsSource = dbNames;
        }
    }
}
