using DBMS.Models.DBStructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace DBMS.Utilities
{
    public static class SchemaSerializer
    {
        public static DBMSResult LoadDatabases()
        {
            try
            {
                string json = File.ReadAllText("databases.dbms");
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(List<DBMSDatabase>));
                MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
                List<DBMSDatabase> databases = (List<DBMSDatabase>)serializer.ReadObject(stream);
                return DBMSResult.SUCCESS(databases);
            }
            catch(Exception ex)
            {
                return DBMSResult.FAILURE(ex.Message);
            }
        }

        public static DBMSResult SaveDatabases(List<DBMSDatabase> databases)
        {
            try
            {
                string json = ToJson(databases);
                File.WriteAllText("databases.dbms", json);
                return DBMSResult.SUCCESS();
            }
            catch (Exception ex)
            {
                return DBMSResult.FAILURE(ex.Message);
            }
        }

        private static string ToJson<T>(T t)
        {
            MemoryStream stream = new MemoryStream();
            DataContractJsonSerializer ds = new DataContractJsonSerializer(typeof(T));
            DataContractJsonSerializerSettings s = new DataContractJsonSerializerSettings();
            ds.WriteObject(stream, t);
            string jsonString = Encoding.UTF8.GetString(stream.ToArray());
            stream.Close();
            return jsonString;
        }
    }
}
