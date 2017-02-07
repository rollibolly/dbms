using DBMS.Models.DBStructure;
using DBMS.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        ObservableCollection<Models.DBStructure.Table> availableTables;
        ObservableCollection<Models.DBStructure.TableColumn> availableColumns;

        ObservableCollection<Models.DBStructure.Table> selectedTables;
        ObservableCollection<Models.DBStructure.TableColumn> selectedColumns;

        private ObservableCollection<WhereClause> whereClauses;

        public WindowQueryDesigner()
        {
            InitializeComponent();
            selectedTables = new ObservableCollection<Models.DBStructure.Table>();
            selectedColumns = new ObservableCollection<Models.DBStructure.TableColumn>();
            listBoxSelectedTables.ItemsSource = selectedTables;
            listBoxSelectedColumns.ItemsSource = selectedColumns;

            whereClauses = new ObservableCollection<WhereClause>();
            whereClauses.Add(new WhereClause { LeftValue = "col1", Operator = "=", RightValue = "10" });
            whereClauses.Add(new WhereClause { LeftValue = "col2", Operator = "=", RightValue = "10" });
            whereClauses.Add(new WhereClause { LeftValue = "col3", Operator = "=", RightValue = "10" });
            whereClauses.Add(new WhereClause { LeftValue = "col4", Operator = "=", RightValue = "10" });
            listBoxWhere.ItemsSource = whereClauses;
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
            DBMSDatabase selectedDatabase = databases.Where(r => r.DatabaseName == dbName).FirstOrDefault();
            availableTables = new ObservableCollection<Models.DBStructure.Table>(selectedDatabase.Tables);
            availableColumns = new ObservableCollection<Models.DBStructure.TableColumn>();
            foreach (Models.DBStructure.Table tbl in availableTables)
            {
                tbl.SetColumnsFullName();                
            }
            listBoxAvailableTables.ItemsSource = availableTables;
            listBoxAvailableColumns.ItemsSource = availableColumns;

            //comboBoxTables.ItemsSource = tables.Select(r => r.TableName).ToList();
        }

        private void btnAddTable_Click_1(object sender, RoutedEventArgs e)
        {
            string dbName = comboBoxDBs.SelectedItem.ToString();
            string tableName = comboBoxTables.SelectedItem.ToString();
            List<Models.DBStructure.TableColumn> columns = new List<Models.DBStructure.TableColumn>();

            foreach (var d in availableTables)
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

        private void listBoxAvailableTables_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBox lb = sender as ListBox;
            Models.DBStructure.Table selectedTable = lb.SelectedItem as Models.DBStructure.Table;
            if (selectedTable != null)
            {
                selectedTables.Add(selectedTable);
                availableTables.Remove(selectedTable);
                foreach (Models.DBStructure.TableColumn column in selectedTable.Columns)
                {
                    availableColumns.Add(column);
                }
            }
        }
    }
}
