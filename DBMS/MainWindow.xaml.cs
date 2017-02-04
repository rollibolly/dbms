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
using BerkeleyDB;
using DBMS.KVManagement;
using DBMS.Utilities;
using DBMS.Models.DBStructure;
using System.Collections.ObjectModel;

namespace DBMS
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {        
        public List<DBMSDatabase> CurrentDatabases { get; set; }
        public DBMSDatabase SelectedDatabase { get; set; }
        public Models.DBStructure.Table SelectedTable { get; set; }
        public Models.DBStructure.TableColumn SelectedColumn { get; set; }
        public EntityType SelectedEntityType { get; set; }

        public MainWindow()
        {
            InitializeComponent();                                 
            FillTreeView();          
        }        

        public void FillTreeView()
        {
            DBMSResult result = SchemaSerializer.LoadDatabases();
            CurrentDatabases = null;
            treeView.Items.Clear();
            if (result.Success)
            {
                CurrentDatabases = result.Data as List<DBMSDatabase>;
                foreach (var database in CurrentDatabases)
                {
                    TreeViewItem dbItem = new TreeViewItem();                    
                    dbItem.Header = database.DatabaseName;
                    dbItem.Tag = database;

                    TreeViewItem dbTablesItem = new TreeViewItem();
                    dbTablesItem.Header = "Tables";
                    dbTablesItem.Tag = database.Tables;

                    foreach (var table in database.Tables)
                    {
                        TreeViewItem tableItem = new TreeViewItem();
                        tableItem.Header = table.TableName;
                        tableItem.Tag = table;

                        TreeViewItem columnsItem = new TreeViewItem();
                        columnsItem.Header = "Columns";
                        columnsItem.Tag = table.Columns;
                        foreach (var column in table.Columns)
                        {
                            TreeViewItem columnItem = new TreeViewItem();
                            columnItem.Header = column.Name;
                            columnItem.Tag = column;
                            columnsItem.Items.Add(columnItem);                            
                        }
                        tableItem.Items.Add(columnsItem);

                        dbTablesItem.Items.Add(tableItem);
                    }
                    dbItem.Items.Add(dbTablesItem);
                    treeView.Items.Add(dbItem);
                }
                
            }
            else
            {
                System.Windows.Forms.MessageBox.Show(result.Message, "Error", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
            }
        }

        #region CreateDatabase
        private void btnCreateDatabase_Click(object sender, RoutedEventArgs e)
        {
            WindowCreateDatabase window = new WindowCreateDatabase();
            DBMSDatabase db = null;
            if (window.ShowDialog() == true)
            {
                db = window.DatabaseSchema;
                db.Tables = new List<Models.DBStructure.Table>();
                db.Indexes = new List<Index>();
            }
            List<DBMSDatabase> databases = null;
            DBMSResult res = SchemaSerializer.LoadDatabases();
            if (!res.Success || res.Data == null)
            {
                databases = new List<DBMSDatabase>();
            }
            else
            {
                databases = (List<DBMSDatabase>)res.Data;
            }
            databases.Add(db);
            SchemaSerializer.SaveDatabases(databases);
            FillTreeView();
        }
        
        #endregion
        

        private string DatabaseTypeStr = typeof(DBMSDatabase).ToString();
        private string TablesListTypeStr = typeof(List<Models.DBStructure.Table>).ToString();
        private string TableTypeStr = typeof(Models.DBStructure.Table).ToString();
        private string ColumnListTypeStr = typeof(List<Models.DBStructure.TableColumn>).ToString();
        private string ColumnTypeStr = typeof(Models.DBStructure.TableColumn).ToString();
        private void treeView_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            TreeViewItem selectedItem = treeView.SelectedItem as TreeViewItem;
            if (selectedItem == null)
            {
                return;
            }
            TreeViewItem parent = null;
            switch (Mappings.MapObject(selectedItem.Tag))
            {
                case EntityType.DATABASE:
                    SelectedDatabase = selectedItem.Tag as DBMSDatabase;
                    SelectedTable = null;
                    SelectedColumn = null;
                    SelectedEntityType = EntityType.DATABASE;
                    return;
                case EntityType.TABLE_LIST:
                    parent = selectedItem.Parent as TreeViewItem;
                    SelectedDatabase = parent.Tag as DBMSDatabase;
                    SelectedTable = null;
                    SelectedColumn = null;                    
                    SelectedEntityType = EntityType.TABLE_LIST;
                    return;
                case EntityType.TABLE:
                    parent = (selectedItem.Parent as TreeViewItem).Parent as TreeViewItem;
                    SelectedDatabase = parent.Tag as DBMSDatabase;
                    SelectedTable = selectedItem.Tag as Models.DBStructure.Table;
                    SelectedColumn = null;
                    SelectedEntityType = EntityType.TABLE;
                    return;
                case EntityType.COLUMN_LIST:
                    SelectedEntityType = EntityType.COLUMN_LIST;
                    return;
                case EntityType.COLUMN:
                    SelectedEntityType = EntityType.COLUMN;
                    return;
            }                        
        }        
        
        private void treeView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (treeView.SelectedItem != null)
            {
                ContextMenu ctx = new ContextMenu();
                switch (SelectedEntityType)
                {
                    case EntityType.DATABASE:
                        MenuItem dropItem = new MenuItem();
                        dropItem.Header = String.Format("Drop database {0}", SelectedDatabase.DatabaseName);
                        dropItem.Click += DropEntityItem_Click;
                        ctx.Items.Add(dropItem);
                        ctx.IsOpen = true;
                        break;
                    case EntityType.TABLE_LIST:
                        MenuItem createItem = new MenuItem();
                        createItem.Header = "Create TABLE";
                        createItem.Click += CreateTableItem_Click;
                        ctx.Items.Add(createItem);
                        ctx.IsOpen = true;
                        break;
                    case EntityType.TABLE:
                        MenuItem dropTableItem = new MenuItem();
                        dropTableItem.Header = String.Format("Drop table {0}", SelectedTable.TableName);
                        dropTableItem.Click += DropEntityItem_Click;
                        ctx.Items.Add(dropTableItem);

                        MenuItem insertItem = new MenuItem();
                        insertItem.Header = String.Format("Insert into table {0}", SelectedTable.TableName);
                        insertItem.Click += InsertIntoTableItem_Click;
                        ctx.Items.Add(insertItem);

                        MenuItem createIndexItem = new MenuItem();
                        createIndexItem.Header = String.Format("Create index on table {0}", SelectedTable.TableName);
                        createIndexItem.Click += CreateIndexItem_Click;
                        ctx.Items.Add(createIndexItem);

                        MenuItem selectTopItem = new MenuItem();
                        selectTopItem.Header = String.Format("Select TOP 10 from {0}", SelectedTable.TableName);
                        selectTopItem.Click += SelectTopItem_Click;
                        ctx.Items.Add(selectTopItem);

                        ctx.IsOpen = true;
                        break;
                }                           
            }
        }

        private void SelectTopItem_Click(object sender, RoutedEventArgs e)
        {
            KVManagement.DatabaseMgr mgr = new DatabaseMgr(String.Format("{0}\\{1}", SelectedDatabase.FolderName, SelectedTable.FileName));
            mgr.Open();
            DBMSResult res = mgr.GetTopN<string>(10);
            if (res.Success) {
                Dictionary<object, string> resultSet = res.Data as Dictionary<object, string>;
                WindowResultView window = new DBMS.WindowResultView(SelectedTable, resultSet);
                window.Show();
            }
            mgr.Close();
        }

        private void CreateIndexItem_Click(object sender, RoutedEventArgs e)
        {
            WindowCreateIndex window = new WindowCreateIndex(SelectedTable);
            if (window.ShowDialog() == true)
            {
                CurrentDatabases.FirstOrDefault(r => r.DatabaseName == SelectedDatabase.DatabaseName).Indexes.Add(window.IndexSchema);
                SchemaSerializer.SaveDatabases(CurrentDatabases);
                FillTreeView();
            }

        }

        private void InsertIntoTableItem_Click(object sender, RoutedEventArgs e)
        {
            WindowInsertIntoTable window = new WindowInsertIntoTable(this);
            window.Show();
        }

        private void CreateTableItem_Click(object sender, RoutedEventArgs e)
        {
            WindowCreateTable window = new WindowCreateTable(this);
            if (window.ShowDialog() == true)
            {
                if (SelectedDatabase.Tables.FirstOrDefault(r => r.TableName == window.TableSchema.TableName) != null)
                {
                    MessageBox.Show(String.Format("Table with name: {0} already exists", window.TableSchema.TableName), "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                CurrentDatabases.FirstOrDefault(r => r.DatabaseName == SelectedDatabase.DatabaseName).Tables.Add(window.TableSchema);
                SchemaSerializer.SaveDatabases(CurrentDatabases);
                FillTreeView();
            }            
        }

        private void DropEntityItem_Click(object sender, RoutedEventArgs e)
        {
            switch (SelectedEntityType)
            {
                case EntityType.DATABASE:
                    CurrentDatabases.Remove(SelectedDatabase);
                    SchemaSerializer.SaveDatabases(CurrentDatabases);
                    FillTreeView();
                    break;
                case EntityType.TABLE:
                    CurrentDatabases.FirstOrDefault(r => r.DatabaseName == SelectedDatabase.DisplayName).Tables.Remove(SelectedTable);
                    SchemaSerializer.SaveDatabases(CurrentDatabases);
                    FillTreeView();
                    break;
            }
        }
    }
}
