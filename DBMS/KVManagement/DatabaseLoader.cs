using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BerkeleyDB;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using DBMS.Utilities;

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
    }
}
