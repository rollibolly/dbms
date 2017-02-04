using DBMS.Models.DBStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Utilities
{
    public class CommandInterpreter
    {
        public UICommand CreateQuery(string sqltext, string entity)
        {
            UICommand command = new UICommand();
            string pattern = "";

            command.Command = CommandType.CREATE;

            switch (entity)
            {
                case "TABLE":
                    command.Entity = EntityType.TABLE;
                    pattern = @"CREATE " + entity + @" (.+?) \((.*\))";
                    break;
                case "INDEX":
                    command.Entity = EntityType.INDEX;
                    pattern = @"CREATE " + entity + @" ([a-zA-Z]+)";
                    break;
                case "DATABASE":
                    command.Entity = EntityType.DATABASE;
                    pattern = @"CREATE " + entity + @" ([a-zA-Z]+)";
                    break;
            }

            string pattern2 = @"(.+?) ([a-zA-Z]+)( PRIMARY KEY| UNIQUE)?( NOT NULL)?(,|\))";

            Match m = Regex.Match(sqltext, pattern, RegexOptions.Singleline);
       
            string name = m.Groups[1].ToString();

            command.TableNames = new List<string>();
            command.TableNames.Add(name);
            command.Columns = new List<TableColumn>();
            
            foreach (Match mm in Regex.Matches(m.Groups[2].ToString(), pattern2))
            {
                TableColumn tableColumn = new TableColumn();
            
                tableColumn.Name = mm.Groups[1].ToString().Trim();
                
                switch (mm.Groups[2].ToString()) 
                {
                    case "STRING":
                        tableColumn.DataType = DBMSDataType.STRING;
                        break;
                    case "INTEGER":
                        tableColumn.DataType = DBMSDataType.INTEGER;
                        break;
                    case "BOOLEAN":
                        tableColumn.DataType = DBMSDataType.BOOLEAN;
                        break;
                    case "FLOAT":
                        tableColumn.DataType = DBMSDataType.FLOAT;
                        break;
                }
            
                switch (mm.Groups[3].ToString()) 
                {
                    case " PRIMARY KEY":
                        tableColumn.IsPrimaryKey = true;
                        tableColumn.IsUnique = false;
                        break;
                    case " UNIQUE":
                        tableColumn.IsPrimaryKey = false;
                        tableColumn.IsUnique = true;
                        break;
                    default:
                        tableColumn.IsPrimaryKey = false;
                        tableColumn.IsUnique = false;
                        break;
                }
            
                tableColumn.IsNull = (mm.Groups[4].ToString().Length == 0);
            
                command.Columns.Add(tableColumn);
            }

            return command;
        }

        public UICommand SelectQuery(string sql)
        {
            UICommand command = new UICommand();

            command.Command = CommandType.SELECT;

            var reg = new Regex(@"(?is)SELECT(.*?)(?<!\w*"")FROM(?!\w*?"")(.*?)(?=WHERE|ORDER|$)");
            var colunms = reg.Match(sql).Groups[1].Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var tables = reg.Match(sql).Groups[2].Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            var where = reg.Match(sql).Groups[4].Value.Split(new string[] { "&&", "||", "AND", "OR" }, StringSplitOptions.None);

            command.Columns = new List<TableColumn>();

            foreach (var item in colunms)
            {
                TableColumn column = new TableColumn();
                column.Name = item.ToString();
                
                command.Columns.Add(column);
            }

            foreach (var item in tables)
            {
                command.TableNames = new List<string>();
                command.TableNames.Add(item.ToString());
            }

            return command;
        }

        public UICommand InsertQuery(string insertString)
        {
            UICommand command = new UICommand();

            command.Command = CommandType.INSERT;

            IEnumerable<string> inserts = insertString.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            string firstInsert = inserts.First();
            int tableIndex = firstInsert.IndexOf("INSERT INTO ") + "INSERT INTO ".Length;

            command.TableNames = new List<string>();
            string table = firstInsert.Substring(tableIndex, firstInsert.IndexOf("(", tableIndex) - tableIndex);

            command.TableNames.Add(table);

            var regex = new System.Text.RegularExpressions.Regex(@"\(([^)]+)\)", System.Text.RegularExpressions.RegexOptions.Compiled);
            string[] columns = regex.Matches(firstInsert)[0].Value.Split(new char[] { ',', ')', '(' }, StringSplitOptions.RemoveEmptyEntries);

            IEnumerable<string> values = inserts.Select(sql => regex.Matches(sql)[1].Value);
            string[] vals = values.First().Split(new char[] { ',', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);

            command.Columns = new List<TableColumn>();

            for (int i = 0; i < columns.Length; i++)
            {
                TableColumn column = new TableColumn();
                column.Name = columns[i].ToString();
                column.Value = vals[i].ToString();
                command.Columns.Add(column);
            }

            return command;
        }

        public UICommand InterpretCommand(string sqltext)
        {
            UICommand command = new UICommand();
            string[] item = sqltext.Split(' ');

            switch (item[0])
            {
                case "CREATE":
                    command = CreateQuery(sqltext: sqltext, entity: item[1]);
                    break;
                case "SELECT":
                    command = SelectQuery(sql: sqltext);
                    break;
                case "INSERT":
                    command = InsertQuery(insertString: sqltext);
                    break;
                case "DELETE":

                    break;
                case "DROP":
                   // DropTable(sqltext);
                    break;
                default:
                    Console.WriteLine("Incorrect sql syntax!");
                    break;
            }

            return command;
        }
    }
}
