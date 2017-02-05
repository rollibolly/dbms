using DBMS.KVManagement;
using DBMS.Utilities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private void btnInsertRows_Click(object sender, RoutedEventArgs e)
        {
            DateTime start = DateTime.Now;
            int rowsAffected = 0;    
            List<string> gridHeaders = dataGridInsertTable.Columns.Select(r => r.Header as string).ToList();
            foreach (DataRow row in table.Rows)
            {
                try
                {
                    Dictionary<string, object> columnsValues = new Dictionary<string, object>();
                    foreach (string header in gridHeaders)
                    {
                        columnsValues.Add(header, row[header]);
                    }
                    rowsAffected += DatabaseMgr.Insert(parent.SelectedDatabase, parent.SelectedTable, columnsValues);
                }
                catch(Exception ex)
                {

                }
            }
            table.Rows.Clear();
            TimeSpan ellapsed = DateTime.Now.Subtract(start);
            statusText.Text = string.Format("Rows affected: {0} Execution time: {1} ms", rowsAffected, ellapsed.Milliseconds);            
        }
    }
}
