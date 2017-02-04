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
    /// Interaction logic for WindowCreateTable.xaml
    /// </summary>
    public partial class WindowCreateTable : Window
    {
        public DBMS.Models.DBStructure.Table TableSchema;
                
        private ObservableCollection<Models.DBStructure.TableColumn> TableColumns;
        private ObservableCollection<String> RefTableColumnNames;
        private MainWindow parent;
        public WindowCreateTable(MainWindow p)
        {
            InitializeComponent();
            parent = p; 
            TableColumns = new ObservableCollection<Models.DBStructure.TableColumn>();
            dataGridNewTableColumns.AutoGenerateColumns = false;
            Init();
            dataGridNewTableColumns.ItemsSource = TableColumns;
            TableColumns.CollectionChanged += TableColumns_CollectionChanged;                        
        }
        private void Init()
        {            
            RefTableColumnNames = new ObservableCollection<string>();
            foreach (var table in parent.SelectedDatabase.Tables)
            {   
                foreach (var column in table.Columns)
                {
                    string refCol = string.Format("{0}.{1}", table.TableName, column.Name);
                    RefTableColumnNames.Add(refCol);
                }
            }
            DataGridTextColumn orderColumn = new DataGridTextColumn();
            orderColumn.Header = "Order";
            orderColumn.Binding = new Binding("Order");
            dataGridNewTableColumns.Columns.Add(orderColumn);

            DataGridTextColumn nameColumn = new DataGridTextColumn();
            nameColumn.Header = "Name";
            nameColumn.Binding = new Binding("Name");
            dataGridNewTableColumns.Columns.Add(nameColumn);

            DataGridComboBoxColumn typeColumn = new DataGridComboBoxColumn();
            typeColumn.Header = "Type";
            typeColumn.ItemsSource = Enum.GetValues(typeof(DBMSDataType)).Cast<DBMSDataType>();
            typeColumn.SelectedValueBinding = new Binding("DataType");
            dataGridNewTableColumns.Columns.Add(typeColumn);

            DataGridCheckBoxColumn isPrimaryColumn = new DataGridCheckBoxColumn();
            isPrimaryColumn.Header = "PK";
            isPrimaryColumn.Binding = new Binding("IsPrimaryKey");
            dataGridNewTableColumns.Columns.Add(isPrimaryColumn);

            DataGridCheckBoxColumn isUniqueColumn = new DataGridCheckBoxColumn();
            isUniqueColumn.Header = "Unique";
            isUniqueColumn.Binding = new Binding("IsUnique");
            dataGridNewTableColumns.Columns.Add(isUniqueColumn);

            DataGridComboBoxColumn refTableColumn = new DataGridComboBoxColumn();
            refTableColumn.Header = "Ref Table Column";
            refTableColumn.ItemsSource = RefTableColumnNames;
            refTableColumn.SelectedValueBinding = new Binding("FK");            
            dataGridNewTableColumns.Columns.Add(refTableColumn);            
        }

        private void TableColumns_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (TableColumns.Count == 1)
            {
                TableColumns.Last().Order = 0;
            }
            else
            {
                TableColumns.Last().Order = TableColumns[TableColumns.Count - 2].Order + 1;
            }
        }

        private void btnCreateTableCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            this.Close();
        }
        private string ValidateTableName(string name)
        {
            if (String.IsNullOrEmpty(name))
            {
                return "Table name field is required";
            }
            if (!Char.IsLetter(name[0]))
            {
                return "Table name must begin with a letter";
            }            
            return null;
        }
        private void btnCreateNewTable_Click(object sender, RoutedEventArgs e)
        {
            TableSchema = null;
            if (TableColumns.Count <= 0)
            {
                MessageBox.Show("Cannot create table with 0 column count!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Models.DBStructure.Table table = new Models.DBStructure.Table();
            // TODO: verification if table exists or not            
            table.TableName = textBoxNewTableName.Text;
            string validationMessage = ValidateTableName(table.TableName);
            if (validationMessage != null)
            {
                MessageBox.Show(validationMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            table.Columns = TableColumns.ToList();
            TableSchema = table;
            DialogResult = true;
            this.Close();
        }               
    }
}
