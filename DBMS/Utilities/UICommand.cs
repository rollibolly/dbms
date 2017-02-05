using DBMS.Models.DBStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMS.Utilities
{
    public enum CommandType
    {
        CREATE,
        DROP,
        SELECT,
        INSERT,
        UPDATE,
        DELETE
    }

    public enum EntityType
    {
        TABLE,
        INDEX,
        DATABASE
    }

    public class UICommand
    {
        public CommandType Command { get; set; }
        public EntityType Entity { get; set; }
        public string Path { get; set; }
        public string IndexName { get; set; }
        public List<string> TableNames { get; set; }
        public List<TableColumn> Columns { get; set; }
        public string ErrorMessage { get; set; }
        public bool Success { get; set; }
        public override string ToString()
        {
            string str = string.Format("Command: {0}\nEntity: {1}\nSuccess: {2}\nTables: {3}\nColumns: {4}\nError message: {5}\nDatabase path: {6}\nIndex name: {7}\n", 
                Command.ToString(),
                Entity.ToString(),
                Success,
                string.Join(", ", TableNames),
                string.Join(", ", Columns.Select(r => string.Format("Name: {0} | Type: {1} | Is PK: {2} | Is Unique: {3} | Is Null: {4}", r.Name, r.DataType.ToString(), r.IsPrimaryKey, r.IsUnique, r.IsNull))),
                ErrorMessage,
                Path,
                IndexName
                );            

            return str;
        }
    }
}
