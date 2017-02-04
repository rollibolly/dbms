using DBMS.KVManagement;
using DBMS.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
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
    /// Interaction logic for WindowInsertIntoTable.xaml
    /// </summary>
    public partial class WindowInsertIntoTable : Window
    {
        private MainWindow parent;
        private DataTable table;
        public WindowInsertIntoTable(MainWindow parent)
        {
            InitializeComponent();
            this.parent = parent;
            InitGrid();            
        }        

        private void InitGrid()
        {
            table = new DataTable();
            foreach (var item in parent.SelectedTable.Columns)
            {
                table.Columns.Add(new DataColumn(item.Name));
            }
            foreach (DataColumn item in table.Columns)
            {
                dataGridInsertTable.Columns.Add(
                    new DataGridTextColumn
                    {
                        Header = item.ColumnName,
                        Binding = new Binding(string.Format("[{0}]", item.ColumnName))
                    });
            }
            dataGridInsertTable.DataContext = table;
            
            
        }

        private void btnAddRowForGridInsertTable_Click(object sender, RoutedEventArgs e)
        {
            table.Rows.Add(table.NewRow());
        }

        private void btnInsertRows_Click(object sender, RoutedEventArgs e)
        {
            DatabaseMgr mgr = new DatabaseMgr(String.Format("{0}\\{1}",parent.SelectedDatabase.FolderName,parent.SelectedTable.FileName));
            DBMSResult res = mgr.Open();
            if (!res.Success)
            {
                MessageBox.Show(res.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            if (parent.SelectedTable.PrimaryKey != null)
            {
                Dictionary<string, int> headers = dataGridInsertTable.Columns.Select(r => r).ToDictionary(r =>  r.Header as string, r => r.DisplayIndex);
                int indexOfPK = headers[parent.SelectedTable.PrimaryKey.Name];
                foreach (DataRow row in table.Rows)
                {
                    
                    List<string> values = new List<string>();
                    for (int i = 0; i < row.ItemArray.Count(); ++i)
                    {
                        if (i != indexOfPK)
                        {
                            values.Add(row.ItemArray[i] as string);
                        }
                    }

                    res = mgr.Put(row[parent.SelectedTable.PrimaryKey.Name] as string, String.Join("|", values));
                    if (!res.Success)
                    {
                        MessageBox.Show(res.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
            }
            else
            {                
                foreach (DataRow row in table.Rows)
                {

                    List<string> values = new List<string>();
                    for (int i = 0; i < row.ItemArray.Count(); ++i)
                    {
                        values.Add(row.ItemArray[i] as string);
                    }

                    res = mgr.Put(Guid.NewGuid().ToString(), String.Join("|", values));
                    if (!res.Success)
                    {
                        MessageBox.Show(res.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
            }
            res = mgr.Close();
            if (!res.Success)
            {
                MessageBox.Show(res.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
    }
}
