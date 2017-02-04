using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace DBMS.Utilities
{
    public class DBMSResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Object Data { get; set; }
        public DBMSResult(bool success = true, object data = null, string message = "")
        {
            Success = success;
            Message = message;
            Data = data;
        }

        public static DBMSResult SUCCESS(object obj = null)
        {
            return new DBMSResult(true, obj);
        }
        public static DBMSResult FAILURE(string message)
        {
            return new DBMSResult(false, null, message);
        }

        public void DataToDataTable(Models.DBStructure.Table tableSchema, DataTable resultTable)
        {                    
            try
            {
                resultTable.Columns.Clear();
                if (tableSchema.PrimaryKey == null)
                {
                    resultTable.Columns.Add(new DataColumn("Generated PK"));
                }
                foreach (var item in tableSchema.Columns)
                {
                    resultTable.Columns.Add(new DataColumn(item.Name));
                }
                Dictionary<object, string> resultSet = this.Data as Dictionary<object, string>;
                foreach (var resRow in resultSet)
                {
                    List<object> row = new List<object>();
                    row.Add(resRow.Key);
                    List<string> columnValues = resRow.Value.Split('|').ToList();
                    row = row.Concat(columnValues).ToList();

                    resultTable.Rows.Add(row.ToArray());
                }
            }
            catch (Exception ex)
            {
                resultTable = null;
                // TODO
            }            
        }
    }
}
