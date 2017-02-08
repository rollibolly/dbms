using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BerkeleyDB;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using DBMS.Utilities;
using DBMS.Models.DBStructure;
using System.Data;
using DBMS.Models;

namespace DBMS.KVManagement
{
    public class DatabaseMgr
    {
        public BTreeDatabase database;
        private BTreeDatabaseConfig config;
        public string DBFileName { get; set; }

        public DatabaseMgr(string dbFileName)
        {
            DBFileName = dbFileName;
            config = new BTreeDatabaseConfig();
            config.ErrorPrefix = "DBMS";
            config.Duplicates = DuplicatesPolicy.SORTED;
            config.Creation = CreatePolicy.IF_NEEDED;
            config.CacheSize = new CacheInfo(0, 64 * 1024, 1);
            config.PageSize = 8 * 1024;           
        }
        public  DBMSResult Open()
        {                        
            try
            {
                database = BTreeDatabase.Open(DBFileName, config);
                
                return DBMSResult.SUCCESS();
            }
            catch (Exception ex)
            {
                return DBMSResult.FAILURE(ex.Message);
            }            
        }

        public DBMSResult Put(object keyStr, object valueStr)
        {
            try
            {
                DatabaseEntry key = new DatabaseEntry();
                DatabaseEntry value = new DatabaseEntry();
                key.Data = ToByteArray(keyStr);
                value.Data = ToByteArray(valueStr);
                BTreeStats stat = database.Stats();                
                database.Put(key, value);                
                return DBMSResult.SUCCESS();
            }
            catch(Exception ex)
            {
                return DBMSResult.FAILURE(ex.Message);
            }           
        }
        public void Remove(DatabaseEntry key)
        {            
            database.Delete(key);
        }
        public void Remove(object keyObj)
        {
            DatabaseEntry key = new DatabaseEntry();
            key.Data = ToByteArray(keyObj);
            database.Delete(key);
        }
        public DBMSResult Get(object keyObj)        
        {
            try
            {
                DatabaseEntry key = new DatabaseEntry();
                key.Data = ToByteArray(keyObj);
                KeyValuePair<DatabaseEntry, DatabaseEntry> result = database.Get(key);
                return DBMSResult.SUCCESS(FromByteArray<object>(result.Value.Data));
            }
            catch(Exception ex)
            {
                return DBMSResult.FAILURE(ex.Message);
            }            
        }
        public KeyValuePair<DatabaseEntry, DatabaseEntry> GetKV(object keyObj)
        {
            DatabaseEntry key = new DatabaseEntry();
            key.Data = ToByteArray(keyObj);
            return database.Get(key);                      
        }
        public DBMSResult Get<T>(uint? offset, uint? limit)
        {
            BTreeCursor cursor = database.Cursor();
            int counter = 0;
            Dictionary<T, T> resultSet = new Dictionary<T, T>();

            if (offset.HasValue)
            {
                cursor.Move(offset.Value);
            }
            if (limit.HasValue)
            {
                while (counter < limit.Value && cursor.MoveNext())
                {
                    KeyValuePair<DatabaseEntry, DatabaseEntry> row = cursor.Current;
                    resultSet.Add(FromByteArray<T>(row.Key.Data), FromByteArray<T>(row.Value.Data));
                    counter++;
                }
            }
            else
            {
                while (cursor.MoveNext())
                {
                    KeyValuePair<DatabaseEntry, DatabaseEntry> row = cursor.Current;
                    resultSet.Add(FromByteArray<T>(row.Key.Data), FromByteArray<T>(row.Value.Data));                    
                }
            }
            return DBMSResult.SUCCESS(resultSet);
        }
        public List<KeyValuePair<DatabaseEntry, DatabaseEntry>> GetKV(uint? offset, uint? limit)
        {
            BTreeCursor cursor = database.Cursor();
            int counter = 0;
            List<KeyValuePair<DatabaseEntry, DatabaseEntry>> resultSet = new List<KeyValuePair<DatabaseEntry, DatabaseEntry>>();

            if (offset.HasValue)
            {
                cursor.Move(offset.Value);
            }
            if (limit.HasValue)
            {
                while (counter < limit.Value && cursor.MoveNext())
                {
                    KeyValuePair<DatabaseEntry, DatabaseEntry> row = cursor.Current;
                    resultSet.Add(row);
                    counter++;
                }
            }
            else
            {
                while (cursor.MoveNext())
                {
                    KeyValuePair<DatabaseEntry, DatabaseEntry> row = cursor.Current;
                    resultSet.Add(row);
                }
            }
            return resultSet;
        }
        public DBMSResult GetTopN<T>(int n)
        {
            BTreeCursor cursor = database.Cursor();            
            int counter = 0;
            Dictionary<object, T> resultSet = new Dictionary<object, T>();
            while (counter < n && cursor.MoveNext())
            {
                KeyValuePair<DatabaseEntry, DatabaseEntry> row = cursor.Current;
                resultSet.Add(FromByteArray<object>(row.Key.Data), FromByteArray<T>(row.Value.Data));
                counter++;
            }
            return DBMSResult.SUCCESS(resultSet);
        }
        public DBMSResult Close()
        {
            try
            {
                database.Close();
                return DBMSResult.SUCCESS();
            }
            catch(Exception ex)
            {
                return DBMSResult.FAILURE(ex.Message);
            }           
        }

        #region DataTransformation
        /*
            Useful inner functions for data transformation
            Ex: from and to byte[] (Because Berkely DB uses byte[] as his key or value Data) 
        */
        public static byte[] ToByteArray(object obj)
        {
            if (obj == null)
                return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        public static T FromByteArray<T>(byte[] data)
        {
            if (data == null)
                return default(T);
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream(data))
            {
                object obj = bf.Deserialize(ms);
                return (T)obj;
            }
        }
        #endregion

        public static int ExecuteCommand(DBMSDatabase dbSchema, UICommand command, out DbmsTableResult resultTable)
        {
            List<DBMSDatabase> databases = null;
            resultTable = null;
            DBMSResult commandResult = null;
            switch (command.Command)
            {
                case Utilities.CommandType.CREATE:
                    switch (command.Entity)
                    {
                        case Utilities.EntityType.DATABASE:                                                        
                            commandResult = SchemaSerializer.LoadDatabases();
                            if (!commandResult.Success || commandResult.Data == null)
                            {
                                databases = new List<DBMSDatabase>();
                            }
                            else
                            {
                                databases = (List<DBMSDatabase>)commandResult.Data;
                            }
                            DBMSDatabase newDB = new DBMSDatabase();
                            newDB.DatabaseName = command.TableNames[0];
                            newDB.FolderName = command.Path;
                            databases.Add(newDB);
                            SchemaSerializer.SaveDatabases(databases);
                            return 1;
                        case Utilities.EntityType.INDEX:
                            commandResult = SchemaSerializer.LoadDatabases();
                            if (!commandResult.Success || commandResult.Data == null)
                            {
                                databases = new List<DBMSDatabase>();
                            }
                            else
                            {
                                databases = (List<DBMSDatabase>)commandResult.Data;
                            }
                            Table tableForIndex = dbSchema.Tables.Where(r => r.TableName == command.TableNames[0]).First();
                            Index newIndex = new Index();
                            newIndex.Filename = command.IndexName;
                            newIndex.RefColumn = tableForIndex.Columns.Where(r => r.Name == command.Columns[0].Name).First();
                            newIndex.RefTable = tableForIndex;
                            databases.Where(r => r.DatabaseName == dbSchema.DatabaseName).First().Indexes.Add(newIndex);
                            SchemaSerializer.SaveDatabases(databases);
                            return 0;
                        case Utilities.EntityType.TABLE:
                            commandResult = SchemaSerializer.LoadDatabases();
                            if (!commandResult.Success || commandResult.Data == null)
                            {
                                databases = new List<DBMSDatabase>();
                            }
                            else
                            {
                                databases = (List<DBMSDatabase>)commandResult.Data;
                            }
                            Table newTable = new Table();
                            newTable.TableName = command.TableNames[0];
                            newTable.Columns = new List<TableColumn>();
                            foreach (var item in command.Columns)
                            {
                                newTable.Columns.Add(item);                                
                            }
                            foreach(var item in newTable.Columns)
                            {
                                if (item.IsUnique) // TODO find solutions for FK
                                {
                                    Index index = new Index();
                                    index.Filename = string.Format("{0}.index.dbms", Guid.NewGuid().ToString());
                                    index.RefColumn = item;
                                    index.RefTable = newTable;
                                    databases.Where(r => r.DatabaseName == dbSchema.DatabaseName).FirstOrDefault().Indexes.Add(index);
                                }
                            }
                            databases.Where(r => r.DatabaseName == dbSchema.DatabaseName).FirstOrDefault().Tables.Add(newTable);
                            SchemaSerializer.SaveDatabases(databases);
                            return 0;
                        default:
                            return -1;
                    }
                case Utilities.CommandType.DROP:
                    switch (command.Entity)
                    {
                        case Utilities.EntityType.DATABASE:
                            commandResult = SchemaSerializer.LoadDatabases();                            
                            if (!commandResult.Success || commandResult.Data == null)
                            {
                                databases = new List<DBMSDatabase>();
                            }
                            else
                            {
                                databases = (List<DBMSDatabase>)commandResult.Data;
                            }
                            DBMSDatabase dbForRemove = databases.Where(r => r.DatabaseName == command.TableNames[0]).FirstOrDefault();
                            if (dbForRemove != null)
                            {
                                databases.Remove(dbForRemove);
                            }
                            SchemaSerializer.SaveDatabases(databases);
                            return 1;
                        case Utilities.EntityType.INDEX:
                            return 0;
                        case Utilities.EntityType.TABLE:
                            commandResult = SchemaSerializer.LoadDatabases();
                            if (!commandResult.Success || commandResult.Data == null)
                            {
                                databases = new List<DBMSDatabase>();
                            }
                            else
                            {
                                databases = (List<DBMSDatabase>)commandResult.Data;
                            }
                            Table tblForRemove = databases.Where(r => r.DatabaseName == dbSchema.DatabaseName).FirstOrDefault().Tables.Where(r => r.TableName == command.TableNames[0]).FirstOrDefault();
                            databases.Where(r => r.DatabaseName == dbSchema.DatabaseName).FirstOrDefault().Tables.Remove(tblForRemove);
                            string tblFilnemae = String.Format("{0}\\{1}", dbSchema.FolderName, tblForRemove.FileName);
                            if (File.Exists(tblFilnemae))
                            {
                                File.Delete(tblFilnemae);
                            }
                            SchemaSerializer.SaveDatabases(databases);
                            return 0;
                        default:
                            return -1;
                    }               
                case Utilities.CommandType.INSERT:
                    Dictionary<string, object> columnValues = new Dictionary<string, object>();
                    foreach (var item in command.Columns)
                    {
                        columnValues.Add(item.Name, item.Value);
                    }
                    Table tableSchema = dbSchema.Tables.Where(r => r.TableName == command.TableNames[0].Trim()).FirstOrDefault();
                    return DatabaseMgr.Insert(dbSchema, tableSchema, columnValues);
                case Utilities.CommandType.DELETE:
                    commandResult = SchemaSerializer.LoadDatabases();
                    if (!commandResult.Success || commandResult.Data == null)
                    {
                        databases = new List<DBMSDatabase>();
                    }
                    else
                    {
                        databases = (List<DBMSDatabase>)commandResult.Data;
                    }                    
                    Table tblForDelete = databases.Where(r => r.DatabaseName == dbSchema.DatabaseName).FirstOrDefault().Tables.Where(r => r.TableName == command.TableNames[0]).FirstOrDefault();
                    int ra = DatabaseMgr.Delete(dbSchema, tblForDelete, command.WhereClauses);
                    resultTable = new DbmsTableResult();
                    resultTable.Headers.Add("Result");
                    object[] res = resultTable.NewRow();
                    res[0] = string.Format("Rows affected {0}", ra);
                    resultTable.Rows.Add(res);
                    return 0;
                case Utilities.CommandType.SELECT:
                    List<string> columnNames = command.Columns.Select(r => r.Name).ToList();
                    Table tblForSelect = dbSchema.Tables.Where(r => r.TableName == command.TableNames[0]).FirstOrDefault();
                    resultTable = SelectWhere(dbSchema, tblForSelect, columnNames, command.WhereClauses);
                    //resultTable = Select(dbSchema, tblForSelect, columnNames, null, null);
                    return 0;
                case Utilities.CommandType.UPDATE:
                    return 0;
                default:
                    return -1;
            }
        }

        
        public static bool CheckOp(string left, string right, Operator op, Type type)
        {            
            if (type == typeof(int))
            {
                int lVal = Int32.Parse(left);
                int rVal = Int32.Parse(right);
                switch (op)
                {
                    case Operator.EQ:
                        return lVal == rVal;                        
                    case Operator.LT:
                        return lVal < rVal;                        
                    case Operator.GT:
                        return lVal > rVal;
                    case Operator.LE:
                        return lVal <= rVal;
                    case Operator.GE:
                        return lVal >= rVal;
                    case Operator.NE:
                        return lVal != rVal;
                    default:
                        break;
                }
            }
            if (type == typeof(double))
            {
                double lVal = Double.Parse(left);
                double rVal = Double.Parse(right);
                switch (op)
                {
                    case Operator.EQ:
                        return lVal == rVal;
                    case Operator.LT:
                        return lVal < rVal;
                    case Operator.GT:
                        return lVal > rVal;
                    case Operator.LE:
                        return lVal <= rVal;
                    case Operator.GE:
                        return lVal >= rVal;
                    case Operator.NE:
                        return lVal != rVal;
                    default:
                        break;
                }
            }
            if (type == typeof(string))
            {
                string lVal = left;
                string rVal = right;
                int eq = lVal.CompareTo(rVal);
                switch (op)
                {
                    
                    case Operator.EQ:
                        return lVal == rVal;
                    case Operator.LT:
                        return eq < 0;
                    case Operator.GT:
                        return eq > 0;
                    case Operator.LE:                        
                        return eq <= 0;
                    case Operator.GE:
                        return eq >= 0;
                    case Operator.NE:
                        return eq != 0;
                    default:
                        break;                    
                }
            }
            return false;
        }

        public static List<KeyValuePair<DatabaseEntry, DatabaseEntry>> Where(WhereClause clause, Table tableDef, DatabaseMgr tableMgr, Dictionary<string, DatabaseMgr> indexManagers, List<KeyValuePair<DatabaseEntry, DatabaseEntry>> subResult)
        {
            List<KeyValuePair<DatabaseEntry, DatabaseEntry>> resultRows = new List<KeyValuePair<DatabaseEntry, DatabaseEntry>>();
            List<TableColumn> tableCols = tableDef.Columns.OrderBy(r => r.Order).ToList();
            TableColumn column = tableCols.Where(r => r.Name == clause.LeftValue).FirstOrDefault();
            KeyValuePair<DatabaseEntry, DatabaseEntry> row;
            if (indexManagers.ContainsKey(clause.LeftValue))
            {
                // Use the index file
                DatabaseMgr indexFile = indexManagers[clause.LeftValue];
                indexFile.Open();                
                if (clause.OpType == Operator.EQ)
                {
                    row = indexFile.GetKV(clause.RightValue);
                    
                    resultRows.Add(tableMgr.database.Get(row.Value));                    
                    return resultRows;
                }
                else
                {
                    if (subResult.Count > 0)
                    {
                        BTreeCursor cursor = indexFile.database.Cursor();
                        while (cursor.MoveNext())
                        {
                            string value = DatabaseMgr.FromByteArray<string>(cursor.Current.Key.Data);
                            Type type = null;
                            if (column.DataType == DBMSDataType.INTEGER) { type = typeof(int); }
                            if (column.DataType == DBMSDataType.FLOAT) { type = typeof(double); }
                            if (column.DataType == DBMSDataType.STRING) { type = typeof(string); }
                            if (DatabaseMgr.CheckOp(value, clause.RightValue, clause.OpType, type))
                            {
                                KeyValuePair<DatabaseEntry, DatabaseEntry> subRes = tableMgr.database.Get(cursor.Current.Value);
                                foreach (KeyValuePair<DatabaseEntry, DatabaseEntry> item in subResult)
                                {
                                    string a = DatabaseMgr.FromByteArray<string>(item.Key.Data);
                                    string b = DatabaseMgr.FromByteArray<string>(cursor.Current.Value.Data);
                                    if (a == b)
                                    {
                                        resultRows.Add(subRes);
                                        break;
                                    }
                                }                                
                            }
                        }
                        cursor.Close();

                        /*
                        foreach (KeyValuePair<DatabaseEntry, DatabaseEntry> current in subResult)
                        {
                            //string indexedColValue = GetColumnValueFrom(tableDef, clause.LeftValue, current);
                            //KeyValuePair<DatabaseEntry, DatabaseEntry> foundEntry = indexFile.GetKV(indexedColValue);
                            string value = DatabaseMgr.FromByteArray<string>(current.Key.Data);
                            Type type = null;
                            if (column.DataType == DBMSDataType.INTEGER) { type = typeof(int); }
                            if (column.DataType == DBMSDataType.FLOAT) { type = typeof(double); }
                            if (column.DataType == DBMSDataType.STRING) { type = typeof(string); }
                            if (DatabaseMgr.CheckOp(value, clause.RightValue, clause.OpType, type))
                            {
                                KeyValuePair<DatabaseEntry, DatabaseEntry> subRes = tableMgr.database.Get(current.Value);
                                if (subResult.Contains(subRes))
                                {
                                    resultRows.Add(subRes);
                                }
                            }
                        }*/
                    }
                    else
                    {
                        BTreeCursor cursor = indexFile.database.Cursor();
                        while (cursor.MoveNext())
                        {
                            string value = DatabaseMgr.FromByteArray<string>(cursor.Current.Key.Data);
                            Type type = null;
                            if (column.DataType == DBMSDataType.INTEGER) { type = typeof(int); }
                            if (column.DataType == DBMSDataType.FLOAT) { type = typeof(double); }
                            if (column.DataType == DBMSDataType.STRING) { type = typeof(string); }
                            if (DatabaseMgr.CheckOp(value, clause.RightValue, clause.OpType, type))
                            {
                                KeyValuePair<DatabaseEntry, DatabaseEntry> subRes = tableMgr.database.Get(cursor.Current.Value);
                                resultRows.Add(subRes);
                            }
                        }
                        cursor.Close();
                    }
                    
                }
                //indexFile.Close();
            }
            else
            {            
                // If the Where condition left value is a Primary key    
                if (column.IsPrimaryKey)
                {
                    if (clause.OpType == Operator.EQ)
                    {
                        row = tableMgr.GetKV(clause.RightValue);
                        resultRows.Add(row);
                        //tableMgr.Close();
                        return resultRows;
                    }
                    else
                    {
                        if (subResult.Count > 0)
                        {
                            resultRows = SearchNonCursorNonIndexPK(tableMgr, tableCols, column, clause, subResult);                            
                        }
                        else
                        {
                            resultRows = SearchCursorNonIndexPK(tableMgr, tableCols, column, clause);                            
                        }
                    }
                }
                else
                {
                    if (subResult.Count > 0)
                    {
                        resultRows = SearchNonCursorNonIndexNonPK(tableMgr, tableDef, tableCols, column, clause, subResult);
                    }
                    else
                    {
                        resultRows = SearchCursorNonIndexNonPK(tableMgr, tableDef, tableCols, column, clause);
                    }                    
                }
            }
            return resultRows;
        }
        public static List<KeyValuePair<DatabaseEntry, DatabaseEntry>> SearchCursorNonIndexPK(DatabaseMgr tableMgr, List<TableColumn> tableCols, TableColumn column, WhereClause clause)
        {
            List<KeyValuePair<DatabaseEntry, DatabaseEntry>> resultRows = new List<KeyValuePair<DatabaseEntry, DatabaseEntry>>();
            BTreeCursor cursor = tableMgr.database.Cursor();
            while (cursor.MoveNext())
            {
                string value = DatabaseMgr.FromByteArray<string>(cursor.Current.Key.Data);
                Type type = null;
                if (column.DataType == DBMSDataType.INTEGER) { type = typeof(int); }
                if (column.DataType == DBMSDataType.FLOAT) { type = typeof(double); }
                if (column.DataType == DBMSDataType.STRING) { type = typeof(string); }
                if (DatabaseMgr.CheckOp(value, clause.RightValue, clause.OpType, type))
                {
                    resultRows.Add(new KeyValuePair<DatabaseEntry, DatabaseEntry>(cursor.Current.Key, cursor.Current.Value));
                }
            }
            cursor.Close();
            return resultRows;
        }
        public static List<KeyValuePair<DatabaseEntry, DatabaseEntry>> SearchNonCursorNonIndexPK(DatabaseMgr tableMgr, List<TableColumn> tableCols, TableColumn column, WhereClause clause, List<KeyValuePair<DatabaseEntry, DatabaseEntry>> subResult)
        {
            List<KeyValuePair<DatabaseEntry, DatabaseEntry>> resultRows = new List<KeyValuePair<DatabaseEntry, DatabaseEntry>>();
            foreach (KeyValuePair<DatabaseEntry,DatabaseEntry> current in subResult)
            {
                string value = DatabaseMgr.FromByteArray<string>(current.Key.Data);
                Type type = null;
                if (column.DataType == DBMSDataType.INTEGER) { type = typeof(int); }
                if (column.DataType == DBMSDataType.FLOAT) { type = typeof(double); }
                if (column.DataType == DBMSDataType.STRING) { type = typeof(string); }
                if (DatabaseMgr.CheckOp(value, clause.RightValue, clause.OpType, type))
                {
                    resultRows.Add(new KeyValuePair<DatabaseEntry, DatabaseEntry>(current.Key, current.Value));
                }
            }            
            return resultRows;
        }
        public static List<KeyValuePair<DatabaseEntry, DatabaseEntry>> SearchCursorNonIndexNonPK(DatabaseMgr tableMgr, Table tableDef, List<TableColumn> tableCols, TableColumn column, WhereClause clause)
        {
            List<KeyValuePair<DatabaseEntry, DatabaseEntry>> resultRows = new List<KeyValuePair<DatabaseEntry, DatabaseEntry>>();
            BTreeCursor cursor = tableMgr.database.Cursor();
            bool hasPk = (tableDef.PrimaryKey != null ? true : false);
            while (cursor.MoveNext())
            {
                string key = DatabaseMgr.FromByteArray<string>(cursor.Current.Key.Data);
                string values = DatabaseMgr.FromByteArray<string>(cursor.Current.Value.Data);
                string[] rowArray = new string[tableCols.Count];

                string[] valuesArr = values.Split('|');
                if (hasPk)
                {
                    rowArray[tableDef.PrimaryKey.Order] = key;
                    int i = 0;
                    for (int ind = 0; ind < tableCols.Count; ++ind)
                    {
                        if (rowArray[ind] == null)
                        {
                            rowArray[ind] = valuesArr[i];
                            i++;
                        }
                    }
                }
                else
                {
                    rowArray = valuesArr;
                }
                Type type = null;
                if (column.DataType == DBMSDataType.INTEGER) { type = typeof(int); }
                if (column.DataType == DBMSDataType.FLOAT) { type = typeof(double); }
                if (column.DataType == DBMSDataType.STRING) { type = typeof(string); }
                if (DatabaseMgr.CheckOp(rowArray[column.Order], clause.RightValue, clause.OpType, type))
                {
                    resultRows.Add(new KeyValuePair<DatabaseEntry, DatabaseEntry>(cursor.Current.Key, cursor.Current.Value));
                }
            }
            cursor.Close();
            return resultRows;
        }

        public static List<KeyValuePair<DatabaseEntry, DatabaseEntry>> SearchNonCursorNonIndexNonPK(DatabaseMgr tableMgr, Table tableDef, List<TableColumn> tableCols, TableColumn column, WhereClause clause, List<KeyValuePair<DatabaseEntry, DatabaseEntry>> subResult)
        {
            List<KeyValuePair<DatabaseEntry, DatabaseEntry>> resultRows = new List<KeyValuePair<DatabaseEntry, DatabaseEntry>>();
            
            bool hasPk = (tableDef.PrimaryKey != null ? true : false);
            foreach (KeyValuePair<DatabaseEntry, DatabaseEntry> current in subResult)
            {
                string key = DatabaseMgr.FromByteArray<string>(current.Key.Data);
                string values = DatabaseMgr.FromByteArray<string>(current.Value.Data);
                string[] rowArray = new string[tableCols.Count];

                string[] valuesArr = values.Split('|');
                if (hasPk)
                {
                    rowArray[tableDef.PrimaryKey.Order] = key;
                    int i = 0;
                    for (int ind = 0; ind < tableCols.Count; ++ind)
                    {
                        if (rowArray[ind] == null)
                        {
                            rowArray[ind] = valuesArr[i];
                            i++;
                        }
                    }
                }
                else
                {
                    rowArray = valuesArr;
                }
                Type type = null;
                if (column.DataType == DBMSDataType.INTEGER) { type = typeof(int); }
                if (column.DataType == DBMSDataType.FLOAT) { type = typeof(double); }
                if (column.DataType == DBMSDataType.STRING) { type = typeof(string); }
                if (DatabaseMgr.CheckOp(rowArray[column.Order], clause.RightValue, clause.OpType, type))
                {
                    resultRows.Add(new KeyValuePair<DatabaseEntry, DatabaseEntry>(current.Key, current.Value));
                }
            }            
            return resultRows;
        }
        public static Dictionary<string,string> GetColumnValuesArrayFrom(Table tableDef, KeyValuePair<DatabaseEntry, DatabaseEntry> data)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            int columnsCount = tableDef.Columns.Count;            
            
            string key = DatabaseMgr.FromByteArray<string>(data.Key.Data);
            string values = DatabaseMgr.FromByteArray<string>(data.Value.Data);
            string[] rowArray = new string[columnsCount];

            string[] valuesArr = values.Split('|');
            if (tableDef.PrimaryKey != null)
            {
                //rowArray[tableDef.PrimaryKey.Order] = key;
                result.Add(tableDef.PrimaryKey.Name, key);
                int i = 0;
                for (int ind = 0; ind < columnsCount; ++ind)
                {
                    if (tableDef.PrimaryKey.Order != ind)
                    //if (rowArray[ind] == null)
                    {
                        //rowArray[ind] = valuesArr[i];
                        result.Add(tableDef.Columns.Where(r => r.Order == ind).First().Name, valuesArr[i]);
                        i++;
                    }
                }
            }
            else
            {
                foreach (TableColumn item in tableDef.Columns)
                {
                    result.Add(item.Name, valuesArr[item.Order]);
                }
                //rowArray = valuesArr;
            }
            return result;
        }
        public static string GetColumnValueFrom(Table tableDef, string column, KeyValuePair<DatabaseEntry, DatabaseEntry> data)
        {
            return GetColumnValuesArrayFrom(tableDef, data)[column];            
        }

        public static int Delete(DBMSDatabase dbSchema, Table tableSchema, List<WhereClause> clauses, DatabaseMgr prevRes = null)
        {
            int affectedRows = 0;
            DatabaseMgr tableMgr = new DatabaseMgr(String.Format("{0}\\{1}", dbSchema.FolderName, tableSchema.FileName));
            tableMgr.Open();
            // If there are no conditions than burn the whole table
            if (clauses == null || clauses.Count() == 0)
            {
                BTreeCursor cursor = tableMgr.database.Cursor();
                while (cursor.MoveNext())
                {
                    tableMgr.Remove(cursor.Current.Key);
                    affectedRows++;
                }
                cursor.Close();
                tableMgr.Close();
                return affectedRows;
            }            

            // First get all index files for this table
            List<Index> tableIndexes = dbSchema.Indexes.Where(r => r.RefTable.TableName == tableSchema.TableName).ToList();
            Dictionary<string, DatabaseMgr> indexManagers = new Dictionary<string, DatabaseMgr>();
            foreach (Index index in tableIndexes)
            {
                indexManagers.Add(index.RefColumn.Name, new DatabaseMgr(String.Format("{0}\\{1}", dbSchema.FolderName, index.Filename)));
            }

            // Determining PK if any
            string pkName = null;
            TableColumn pk = tableSchema.PrimaryKey;
            if (pk != null)
            {
                pkName = tableSchema.PrimaryKey.Name;
            }
            List<KeyValuePair<DatabaseEntry, DatabaseEntry>> subResult = new List<KeyValuePair<DatabaseEntry, DatabaseEntry>>();
            while (clauses.Count > 0)
            {
                WhereClause clause = clauses.First();
                clauses.Remove(clause);
                subResult = DatabaseMgr.Where(clause, tableSchema, tableMgr, indexManagers, subResult);
                if (clauses.Count == 0)
                {
                    if (indexManagers != null && indexManagers.Count > 0)
                    {
                        foreach (DatabaseMgr indexMgr in indexManagers.Values)
                        {
                            indexMgr.Open();
                        }
                        foreach (KeyValuePair<DatabaseEntry, DatabaseEntry> entry in subResult)
                        {
                            Dictionary<string, string> valuesDict = GetColumnValuesArrayFrom(tableSchema, entry);
                            foreach (KeyValuePair<string, DatabaseMgr> indexMgr in indexManagers)
                            {
                                indexMgr.Value.Remove(valuesDict[indexMgr.Key]);
                            }
                            tableMgr.database.Delete(entry.Key);
                        }
                    }
                    else
                    {
                        foreach (KeyValuePair<DatabaseEntry, DatabaseEntry> entry in subResult)
                        {
                            tableMgr.database.Delete(entry.Key);
                        }
                    }
                    break;
                }
            }
            foreach (DatabaseMgr indexMgr in indexManagers.Values)
            {
                indexMgr.Close();
            }
            tableMgr.Close();
            return affectedRows;
        }

        public static DbmsTableResult AddToTableResult(DatabaseEntry keyEntry, DatabaseEntry valueEntry, Table tableSchema, List<string> columns, DbmsTableResult resultTable)
        {
            string pkName = null;
            int pkOrder = 0;
            if (tableSchema.PrimaryKey != null)
            {
                pkName = tableSchema.PrimaryKey.Name;
                pkOrder = (int)tableSchema.PrimaryKey.Order;
            }
            List<string> orderedColumnNames = tableSchema.Columns.OrderBy(r => r.Order).Select(r => r.Name).ToList();            
                      
            
            string key = FromByteArray<string>(keyEntry.Data);
            string value = FromByteArray<string>(valueEntry.Data);                
            string[] values = value.Split('|');
            object[] dataRow = resultTable.NewRow();
            int index = 0;
            foreach (string orderedColumn in orderedColumnNames)
            {

                if (columns.Contains(orderedColumn) || columns.Contains("*"))
                {
                    List<int> indexesList = resultTable.IndexesOfHeader(orderedColumn);
                    if (orderedColumn == pkName)
                    {
                        AddToObjectRow(dataRow, indexesList, key);
                    }
                    else
                    {
                        AddToObjectRow(dataRow, indexesList, values[index]);
                    }
                }
                if (pkName != null && pkName != orderedColumn)
                {
                    index++;
                }
            }
            resultTable.Rows.Add(dataRow);
                        
            return resultTable;
        }

        public static DbmsTableResult SelectWhere(DBMSDatabase dbSchema, Table tableSchema, List<string> columns, List<WhereClause> clauses, DatabaseMgr prevRes = null)
        {
            DbmsTableResult resultTable = new DbmsTableResult();                        
            List<string> orderedColumnNames = tableSchema.Columns.OrderBy(r => r.Order).Select(r => r.Name).ToList();
            foreach (string item in columns)
            {
                if (item == "*")
                {
                    foreach (string orderedColumn in orderedColumnNames)
                    {
                        resultTable.Headers.Add(orderedColumn);
                    }
                }
                else
                {
                    resultTable.Headers.Add(item);
                }
            }
            
            DatabaseMgr tableMgr = new DatabaseMgr(String.Format("{0}\\{1}", dbSchema.FolderName, tableSchema.FileName));
            tableMgr.Open();
            // If there are no conditions than burn the whole table
            if (clauses == null || clauses.Count() == 0)
            {
                BTreeCursor cursor = tableMgr.database.Cursor();
                while (cursor.MoveNext())
                {
                    resultTable = AddToTableResult(cursor.Current.Key, cursor.Current.Value, tableSchema, columns, resultTable);
                }
                cursor.Close();
                tableMgr.Close();
                return resultTable;             
            }

            // First get all index files for this table
            List<Index> tableIndexes = dbSchema.Indexes.Where(r => r.RefTable.TableName == tableSchema.TableName).ToList();
            Dictionary<string, DatabaseMgr> indexManagers = new Dictionary<string, DatabaseMgr>();
            foreach (Index index in tableIndexes)
            {
                indexManagers.Add(index.RefColumn.Name, new DatabaseMgr(String.Format("{0}\\{1}", dbSchema.FolderName, index.Filename)));
            }

            // Determining PK if any
            string pkName = null;
            TableColumn pk = tableSchema.PrimaryKey;
            if (pk != null)
            {
                pkName = tableSchema.PrimaryKey.Name;
            }
            List<KeyValuePair<DatabaseEntry, DatabaseEntry>> subResult = new List<KeyValuePair<DatabaseEntry, DatabaseEntry>>();
            while (clauses.Count > 0)
            {
                WhereClause clause = clauses.First();
                clauses.Remove(clause);
                if (clause.ClauseType == WhereType.AND || clause.ClauseType == WhereType.DEFAULT)
                {
                    subResult = DatabaseMgr.Where(clause, tableSchema, tableMgr, indexManagers, subResult);
                }
                else
                {
                    List<KeyValuePair<DatabaseEntry, DatabaseEntry>> orResult = DatabaseMgr.Where(clause, tableSchema, tableMgr, indexManagers, null);
                    subResult = subResult.Union(orResult).ToList();
                }
                
                if (clauses.Count == 0)
                {
                    
                    foreach (KeyValuePair<DatabaseEntry, DatabaseEntry> entry in subResult)
                    {
                    resultTable = AddToTableResult(entry.Key, entry.Value, tableSchema, columns, resultTable);                        
                    }                    
                    break;
                }
            }
            foreach (DatabaseMgr indexMgr in indexManagers.Values)
            {
                indexMgr.Close();
            }
            tableMgr.Close();
            //return affectedRows;
            return resultTable;
        }

        public static DbmsTableResult Select(DBMSDatabase dbSchema, Table tableSchema, List<string> columns, uint? offset, uint? limit)
        {
            DbmsTableResult resultTable = new DbmsTableResult();
            DatabaseMgr mgr = new DatabaseMgr(String.Format("{0}\\{1}", dbSchema.FolderName, tableSchema.FileName));
            mgr.Open();
            List<string> orderedColumnNames = tableSchema.Columns.OrderBy(r => r.Order).Select(r => r.Name).ToList();
            foreach (string item in columns)
            {
                if (item == "*")
                {
                    foreach (string orderedColumn in orderedColumnNames)
                    {
                        resultTable.Headers.Add(orderedColumn);
                    }
                }
                else
                {
                    resultTable.Headers.Add(item);
                }
            }
            string pkName = null;
            int pkOrder = 0;
            if (tableSchema.PrimaryKey != null)
            {
                pkName = tableSchema.PrimaryKey.Name;
                pkOrder = (int)tableSchema.PrimaryKey.Order;
            }
            List<KeyValuePair<DatabaseEntry, DatabaseEntry>> result = mgr.GetKV(offset, limit);
            //resultSet.Add(FromByteArray<T>(row.Key.Data), FromByteArray<T>(row.Value.Data));
            //Dictionary<string, string> resultList = result.Data as Dictionary<string, string>;
            foreach (var row in result)
            {
                resultTable = AddToTableResult(row.Key, row.Value, tableSchema, columns, resultTable);
                /*
                string key = row.Key;
                string[] values = row.Value.Split('|');
                object[] dataRow = resultTable.NewRow();
                int index = 0;
                foreach (string orderedColumn in orderedColumnNames)
                {
                    
                    if (columns.Contains(orderedColumn) || columns.Contains("*"))
                    {
                        List<int> indexesList = resultTable.IndexesOfHeader(orderedColumn);
                        if (orderedColumn == pkName)
                        {
                            AddToObjectRow(dataRow, indexesList, key);                            
                        }
                        else
                        {
                            AddToObjectRow(dataRow, indexesList, values[index]);                            
                        }
                    }
                    if (pkName != null && pkName != orderedColumn)
                    {
                        index++;
                    }
                }
                resultTable.Rows.Add(dataRow);
                */
            }
            mgr.Close();
            return resultTable;
        }
        private static object[] AddToObjectRow(object[] row, List<int> indexes, object value)
        {
            foreach (int index in indexes)
            {
                row[index] = value;
            }
            return row;
        }
        public static int Insert(DBMSDatabase dbSchema, Table tableSchema, Dictionary<string, object> columnValues)
        {
            return Insert(dbSchema, tableSchema, new List<Dictionary<string, object>> { columnValues });
        }
        public static int Insert(DBMSDatabase dbSchema, Table tableSchema, List<Dictionary<string, object>> columnValues)
        {
            int rowsAffected = 0;
            DatabaseMgr mgr = new DatabaseMgr(String.Format("{0}\\{1}", dbSchema.FolderName, tableSchema.FileName));
            DBMSResult res = mgr.Open();            
            if (!res.Success)
            {
                throw new IOException("Can not open database file");
            }

            // Opening index managers if any
            List<Index> indexListForTable = dbSchema.Indexes.Where(r => r.RefTable.FileName == tableSchema.FileName).ToList();
            Dictionary<string, DatabaseMgr> indexManagers = new Dictionary<string, DatabaseMgr>();
            foreach (Index index in indexListForTable)
            {
                DatabaseMgr indexManager = new DatabaseMgr(String.Format("{0}\\{1}", dbSchema.FolderName, index.Filename));
                res = indexManager.Open();
                if (!res.Success)
                    throw new IOException(String.Format("Can not open index file {0}", index.Filename));
                indexManagers.Add(index.RefColumn.Name, indexManager);
            }

            // Buildin the key and value strings for berkley db
            List<string> orderedColumns = tableSchema.Columns.OrderBy(r => r.Order).Where(r => !r.IsPrimaryKey).Select(r => r.Name).ToList();

            foreach (Dictionary<string, object> row in columnValues)
            {
                List<string> valuesList = new List<string>();

                string keyStr = null;
                string valueStr = null;
                if (tableSchema.PrimaryKey == null)
                {
                    keyStr = Guid.NewGuid().ToString();
                }
                else
                {
                    keyStr = row[tableSchema.PrimaryKey.Name].ToString();
                    row.Remove(tableSchema.PrimaryKey.Name);
                }

                foreach (string columnName in orderedColumns)
                {
                    if (row.ContainsKey(columnName))
                    {
                        valuesList.Add(row[columnName].ToString());
                    }
                    else
                    {
                        valuesList.Add("");
                    }
                    
                    if (indexManagers.ContainsKey(columnName))
                    {
                        string indexKey = row[columnName].ToString();
                        string indexValue = keyStr;
                        res = indexManagers[columnName].Put(indexKey, indexValue);
                        if (!res.Success)
                        {
                            throw new ArgumentException(string.Format("Can not insert into index file for column [{0}] key: [{1}] value: [{2}]", columnName, indexKey, indexValue));
                        }
                    }
                }
                valueStr = string.Join("|", valuesList);
                res = mgr.Put(keyStr, valueStr);
                if (!res.Success)
                {
                    throw new ArgumentException(string.Format("Can not insert Key: [{0}] Value: [{1}]", keyStr, valueStr));                    
                }
                rowsAffected++;
            }
            res = mgr.Close();
            if (!res.Success)
            {
                throw new IOException("Can not close database file");
            }
            foreach (var item in indexManagers)
            {
                res = item.Value.Close();
                if (!res.Success)
                {
                    throw new IOException("Can not close index file");
                }
            }
            return rowsAffected;
        }
    }
}
