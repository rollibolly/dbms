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
    /// Interaction logic for WindowCreateDatabase.xaml
    /// </summary>
    public partial class WindowCreateDatabase : Window
    {
        public DBMSDatabase DatabaseSchema { get; set; }
        public WindowCreateDatabase()
        {
            InitializeComponent();
        }

        private void btnBrowseDatabaseLocation_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.FolderBrowserDialog dialog = new System.Windows.Forms.FolderBrowserDialog();
            System.Windows.Forms.DialogResult result = dialog.ShowDialog();
            if (!String.IsNullOrWhiteSpace(dialog.SelectedPath))
            {
                btnBrowseDatabaseLocation.Content = dialog.SelectedPath;
            }
        }

        private void btnCreateDatabase_Click(object sender, RoutedEventArgs e)
        {
            string location = btnBrowseDatabaseLocation.Content.ToString();
            if (string.IsNullOrEmpty(location) || location == "Browse")
            {
                MessageBox.Show("Database location is invalid!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            string name = textBoxDatabaseName.Text;
            if (string.IsNullOrEmpty(name))
            {
                MessageBox.Show("Database name is invalid!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            DatabaseSchema = new DBMSDatabase();
            DatabaseSchema.DatabaseName = name;
            DatabaseSchema.FolderName = location;
            this.DialogResult = true;
            this.Close();
        }
    }
}
