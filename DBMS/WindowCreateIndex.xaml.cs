using DBMS.Models.DBStructure;
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
    /// Interaction logic for WindowCreateIndex.xaml
    /// </summary>
    public partial class WindowCreateIndex : Window
    {
        private DBMS.Models.DBStructure.Table table;
        public Index IndexSchema = null;
        public WindowCreateIndex(Models.DBStructure.Table t)
        {
            InitializeComponent();
            table = t;
            labelTableName.Content = table.TableName;
            foreach (var item in table.Columns)
            {
                listBoxClumns.Items.Add(item);
            }            
        }

        private void btnCreateIndex_Click(object sender, RoutedEventArgs e)
        {
            if (listBoxClumns.SelectedItem != null)
            {
                IndexSchema = new Index();
                IndexSchema.RefTable = table;
                IndexSchema.RefColumn = listBoxClumns.SelectedItem as Models.DBStructure.TableColumn;
                IndexSchema.Filename = string.Format("{0}.index.dbms", Guid.NewGuid().ToString());
                DialogResult = true;
                this.Close();
            }
            else
            {
                MessageBox.Show("Select a column first!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        }
    }
}
