using DBMS.Models.DBStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
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
        public List<string> TableNames { get; set; }
        public List<TableColumn> Columns { get; set; }

        public override string ToString()
        {
            string str = string.Format("Command: {0}\nEntity: {1}\nTables: {2}\nColumns: {3}\n", 
                Command.ToString(),
                Entity.ToString(),
                string.Join(", ", TableNames),
                string.Join(", ", Columns.Select(r => r.Name)));            

            return str;
        }
    }
}
