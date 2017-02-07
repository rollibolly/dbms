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

    public class WhereClause
    {
        public string LeftValue { get; set; }
        private string op;
        public string Operator
        {
            get { return this.op; }
            set
            {
                op = value;
                switch (op)
                {
                    case "=":
                        OpType = DBMS.Operator.EQ;
                        break;
                    case "<":
                        OpType = DBMS.Operator.LT;
                        break;
                    case ">":
                        OpType = DBMS.Operator.GT;
                        break;
                    case "<=":
                        OpType = DBMS.Operator.LE;
                        break;
                    case ">=":
                        OpType = DBMS.Operator.GE;
                        break;
                    case "<>":
                        OpType = DBMS.Operator.NE;
                        break;
                }
            }
        } // =|<>|<|>|<=|>=
        public string RightValue { get; set; }
        public Operator OpType
        {
            get;set;
        }
        public bool IsColumnRight
        {
            get { return IsColumn(RightValue); }            
        }
        public bool IsColumnLeft
        {
            get { return IsColumn(LeftValue); }
        }
        private bool IsColumn(string value)
        {
            if (value.Split('.').Count() == 2)
                return true;
            return false;
        }
    }

    public class UICommand
    {
        public CommandType Command { get; set; }
        public EntityType Entity { get; set; }
        public string Path { get; set; }

        private List<WhereClause> whereclauses;
        public List<WhereClause> WhereClauses
        {
            get
            {
                if (whereclauses == null)
                    whereclauses = new List<WhereClause>();
                return whereclauses;
            }

            set
            {
                whereclauses = value;
            }
        }    

        private List<string> tablenames;

        public List<string> TableNames
        {
            get
            {
                if (tablenames == null)
                    tablenames = new List<string>();
                return tablenames;
            }
            set { tablenames = value; }
        }

        private List<TableColumn> columns;
        public List<TableColumn> Columns
        {
            get
            {
                if (columns == null)
                    columns = new List<TableColumn>();
                return columns;
            }
            set { columns = value; }
        }
        public string IndexName { get; set; }

        public string ErrorMessage { get; set; }
        public bool Success { get; set; }
        public override string ToString()
        {
            string str = string.Format("Command: {0}\nEntity: {1}\nSuccess: {2}\nTables: {3}\nColumns: {4}\nWhere clause: {5}\nError message: {6}\nDatabase path: {7}\nIndex name: {8}\n",
                Command.ToString(),
                Entity.ToString(),
                Success,
                string.Join(", ", TableNames),
                string.Join(", ", Columns.Select(r => string.Format("Name: {0} | Type: {1} | Is PK: {2} | Is Unique: {3} | Is Null: {4}", r.Name, r.DataType.ToString(), r.IsPrimaryKey, r.IsUnique, r.IsNull))),
                string.Join(", ", WhereClauses.Select(r => string.Format("Left value: {0} | Operator: {1} | Right value: {2}", r.LeftValue, r.Operator, r.RightValue))),
                ErrorMessage,
                Path,
                IndexName
                );            

            return str;
        }
    }
}
