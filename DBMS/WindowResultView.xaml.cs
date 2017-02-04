using System;
using System.Collections.Generic;
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
    /// Interaction logic for WindowResultView.xaml
    /// </summary>
    public partial class WindowResultView : Window
    {
        private Models.DBStructure.Table TableSchema;
        private Dictionary<object, string> ResultRows;
        private DataTable DataSource;
        public WindowResultView(Models.DBStructure.Table tableSchema, Dictionary<object, string> resultRows)
        {
            TableSchema = tableSchema;
            ResultRows = resultRows;
            InitializeComponent();
            InitDataGrid();
            FillDataGrid();
        }

        private void InitDataGrid()
        {
            DataSource = new DataTable();
            if (TableSchema.PrimaryKey == null)
            {
                DataSource.Columns.Add(new DataColumn("Generated PK"));
            }
            foreach (var item in TableSchema.Columns)
            {
                DataSource.Columns.Add(new DataColumn(item.Name));
            }
                        
            dataGridResultView.DataContext = DataSource;            
        }
        private void FillDataGrid()
        {                         
            foreach (var resRow in ResultRows)
            {
                List<object> row = new List<object>();
                row.Add(resRow.Key);
                List<string> columnValues = resRow.Value.Split('|').ToList();
                row = row.Concat(columnValues).ToList();                
                
                DataSource.Rows.Add(row.ToArray());
            }
        }
    }
}
