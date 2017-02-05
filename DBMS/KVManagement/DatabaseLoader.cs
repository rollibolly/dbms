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

namespace DBMS.KVManagement
{
    public class DatabaseMgr
    {
        private BTreeDatabase database;
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
        public DBMSResult GetTopN<T>(int n)
        {
            BTreeCursor cursor = database.Cursor();            
            int counter = 0;
            Dictionary<object, T> resultSet = new Dictionary<object, T>();
            while (counter < n++ && cursor.MoveNext())
            {
                KeyValuePair<DatabaseEntry, DatabaseEntry> row = cursor.Current;
                resultSet.Add(FromByteArray<object>(row.Key.Data), FromByteArray<T>(row.Value.Data));                
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
        private byte[] ToByteArray(object obj)
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

        private T FromByteArray<T>(byte[] data)
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
            return rowsAffected;
        }
    }
}
