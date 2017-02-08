using DBMS.KVManagement;
using DBMS.Models;
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
        public UICommand resultCommand;

        public WindowQueryDesigner()
        {
            InitializeComponent();
            selectedTables = new ObservableCollection<Models.DBStructure.Table>();
            selectedColumns = new ObservableCollection<Models.DBStructure.TableColumn>();
            listBoxSelectedTables.ItemsSource = selectedTables;
            listBoxSelectedColumns.ItemsSource = selectedColumns;
            resultCommand = new UICommand();

            whereClauses = new ObservableCollection<WhereClause>();
            listBoxWhere.ItemsSource = whereClauses;
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
            this.comboBoxOperator.ItemsSource = Enum.GetValues(typeof(Operator)).Cast<Operator>();
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

        private void listBoxSelectedTables_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBox lb = sender as ListBox;
            Models.DBStructure.Table selectedTable = lb.SelectedItem as Models.DBStructure.Table;

            if (selectedTable != null)
            {
                selectedTables.Remove(selectedTable);
                availableTables.Add(selectedTable);
                
                foreach (Models.DBStructure.TableColumn column in selectedTable.Columns)
                {
                    availableColumns.Remove(column);
                }

                foreach (Models.DBStructure.TableColumn column in selectedTable.Columns)
                {
                    selectedColumns.Remove(column);
                }
            }
        }

        private void listBoxAvailableColumns_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBox lb = sender as ListBox;
            Models.DBStructure.TableColumn selectedColumn = lb.SelectedItem as Models.DBStructure.TableColumn;

            if (selectedColumn != null)
            {
                selectedColumns.Add(selectedColumn);
                availableColumns.Remove(selectedColumn);
            }

            this.comboBoxLeftValue.ItemsSource = selectedColumns;
            this.comboBoxRightValue.ItemsSource = selectedColumns;
        }

        private void listBoxSelectedColumns_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBox lb = sender as ListBox;
            Models.DBStructure.TableColumn selectedColumn = lb.SelectedItem as Models.DBStructure.TableColumn;

            if (selectedColumn != null)
            {
                selectedColumns.Remove(selectedColumn);
                availableColumns.Add(selectedColumn);
            }

            this.comboBoxLeftValue.ItemsSource = selectedColumns;
            this.comboBoxRightValue.ItemsSource = selectedColumns;
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            WhereClause wc = new WhereClause();
            Models.DBStructure.TableColumn rightValCol = comboBoxRightValue.SelectedItem as Models.DBStructure.TableColumn; ;

            if (comboBoxLeftValue.SelectedIndex == -1 || comboBoxOperator.SelectedIndex == -1)
            {
                MessageBox.Show("First you must select an item from the combos!");
                return;
            }
            else
            {
                if (whereClauses.Count() == 0)
                {
                    wc.LeftValue = (comboBoxLeftValue.SelectedItem as Models.DBStructure.TableColumn).Name;
                    wc.OpType = (Operator)Enum.Parse(typeof(Operator), comboBoxOperator.SelectedItem.ToString());
                    rightValCol = comboBoxRightValue.SelectedItem as Models.DBStructure.TableColumn;
                }
                else
                {
                    if ((radiobtnAND.IsChecked == false && radiobtnOR.IsChecked == true) || (radiobtnAND.IsChecked == true && radiobtnOR.IsChecked == false))
                    {
                        wc.LeftValue = (comboBoxLeftValue.SelectedItem as Models.DBStructure.TableColumn).Name;
                        wc.OpType = (Operator)Enum.Parse(typeof(Operator), comboBoxOperator.SelectedItem.ToString());
                        rightValCol = comboBoxRightValue.SelectedItem as Models.DBStructure.TableColumn;
                    }
                    else
                    {
                        MessageBox.Show("Please select AND / OR to the where statement!");
                        return;
                    }
                }
            }

            if (rightValCol != null)
            {
                wc.RightValue = rightValCol.ColumnFullName;
            }
            else
            {
                wc.RightValue = textBoxValue.Text;
            }

            if (radiobtnAND.IsChecked == true)
            {
                wc.ClauseType = WhereType.AND;
            }
            else
            {
                if (radiobtnOR.IsChecked == true)
                {
                    wc.ClauseType = WhereType.OR;
                }
                else
                {
                    wc.ClauseType = WhereType.DEFAULT;
                }
            }
            
            whereClauses.Add(wc);

            this.comboBoxLeftValue.SelectedIndex = -1;
            this.comboBoxRightValue.SelectedIndex = -1;
            this.comboBoxOperator.SelectedIndex = -1;
            this.textBoxValue.Text = "";
        }

        private void textBoxValue_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty(textBoxValue.Text))
            {
                comboBoxRightValue.IsEnabled = false;
                comboBoxRightValue.SelectedIndex = -1;
            }
            else
            {
                comboBoxRightValue.IsEnabled = true;
            }
        }

        private void btnViewQuery_Click(object sender, RoutedEventArgs e)
        {
            textBoxViewQuery.Text = "SELECT ";
            textBoxViewQuery.Text += String.Join(", ", selectedColumns.Select(r => r.ColumnFullName));
            textBoxViewQuery.Text += "\nFROM ";
            textBoxViewQuery.Text += String.Join(",", selectedTables.Select(r => r.TableName));
            
            if (whereClauses.Count != 0)
            {
                textBoxViewQuery.Text += "\nWHERE ";
                textBoxViewQuery.Text += String.Join(" ", whereClauses.Select(r => string.Format("{0} {1} {2} {3}", r.ClauseType == WhereType.DEFAULT ? "":r.ClauseType.ToString(), r.LeftValue, r.Operator, r.RightValue)));
            }
        }

        private void btnExecute_Click(object sender, RoutedEventArgs e)
        {
            resultCommand.Command = CommandType.SELECT;
            resultCommand.Entity = Utilities.EntityType.TABLE;
            resultCommand.TableNames = selectedTables.Select(r => r.TableName).ToList();
            resultCommand.Columns = selectedColumns.ToList();
            resultCommand.WhereClauses = whereClauses.ToList();

            DateTime start = DateTime.Now;
            try
            {
                DbmsTableResult resTable = null;
                if (resultCommand != null)
                {
                        MessageBox.Show(resultCommand.ToString());
                        DatabaseMgr.ExecuteCommand(comboBoxDBs.SelectedItem as DBMSDatabase, resultCommand, out resTable); 
                }
                if (resTable != null)
                    FillResultView(resTable);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            TimeSpan ellapsed = DateTime.Now.Subtract(start);
            textBlockTime.Text = string.Format("Execution duration: {0} ms", ellapsed.Milliseconds);
        }

        private void FillResultView(DbmsTableResult resTable)
        {
            dataGridSelectResult.Items.Clear();
            dataGridSelectResult.Columns.Clear();
            int i = 0;
            foreach (string item in resTable.Headers)
            {
                DataGridTextColumn col = new DataGridTextColumn();
                col.Header = item;
                col.Binding = new Binding(string.Format("[{0}]", i));
                dataGridSelectResult.Columns.Add(col);
                i++;
            }

            foreach (object[] resRow in resTable.Rows)
            {
                dataGridSelectResult.Items.Add(resRow);
            }
        }
    }
}
