using DBMS.Models.DBStructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DBMS.Utilities
{
    public class CommandInterpreter
    {
        public UICommand CreateQuery(string sqltext, string entity)
        {
            UICommand command = new UICommand();
            string pattern = "";
            int id = 0;

            command.Command = CommandType.CREATE;

            switch (entity.ToLower())
            {
                case "table":
                    command.Entity = EntityType.TABLE;
                    pattern = @"create " + entity.ToLower() + @" (.+?) \((.*\))";
                    id = 1;
                    command.Success = true;
                    break;
                case "index":
                    command.Entity = EntityType.INDEX;
                    pattern = @"create " + entity.ToLower() + @"(.*) (on) ([a-zA-Z]+) \(([a-zA-Z]+\))";
                    id = 2;
                    command.Success = true;
                    break;
                case "database":
                    command.Entity = EntityType.DATABASE;
                    pattern = @"create " + entity.ToLower() + @" ([a-zA-Z]+) (.*)";
                    id = 3;
                    command.Success = true;
                    break;
                default:
                    command.Success = false;
                    break;
            }

            if (command.Success)
            {
                if (id == 3)
                {
                    Match m1 = Regex.Match(sqltext.ToLower(), pattern, RegexOptions.Singleline);

                    command.Path = m1.Groups[2].ToString().Trim();
                    command.TableNames = new List<string>();
                    command.TableNames.Add(m1.Groups[1].ToString().Trim());
                    command.Columns = new List<TableColumn>();

                    TableColumn tableColumn = new TableColumn();
                    tableColumn.Name = new String(m1.Groups[3].ToString().Where(Char.IsLetter).ToArray());// m1.Groups[3].ToString().Trim();
                    command.Columns.Add(tableColumn);
                }

                if (id == 2)
                {
                    Match m1 = Regex.Match(sqltext.ToLower(), pattern, RegexOptions.Singleline);

                    command.IndexName = m1.Groups[1].ToString().Trim();
                    command.TableNames = new List<string>();
                    command.TableNames.Add(m1.Groups[3].ToString().Trim());
                    command.Columns = new List<TableColumn>();

                    TableColumn tableColumn = new TableColumn();
                    tableColumn.Name = new String(m1.Groups[4].ToString().Where(Char.IsLetter).ToArray());// m1.Groups[3].ToString().Trim();
                    command.Columns.Add(tableColumn);
                }

                if (id == 1)
                {
                    string pattern2 = @"(.+?) ([a-zA-Z]+)( primary key| unique)?( not null)?(,|\))";

                    Match m = Regex.Match(sqltext.ToLower(), pattern, RegexOptions.Singleline);

                    string name = m.Groups[1].ToString().Trim();
                    
                    command.TableNames = new List<string>();
                    command.TableNames.Add(name);
                    command.Columns = new List<TableColumn>();

                    foreach (Match mm in Regex.Matches(m.Groups[2].ToString(), pattern2))
                    {
                        TableColumn tableColumn = new TableColumn();

                        tableColumn.Name = mm.Groups[1].ToString().Trim();

                        switch (mm.Groups[2].ToString())
                        {
                            case "string":
                                tableColumn.DataType = DBMSDataType.STRING;
                                break;
                            case "integer":
                                tableColumn.DataType = DBMSDataType.INTEGER;
                                break;
                            case "boolean":
                                tableColumn.DataType = DBMSDataType.BOOLEAN;
                                break;
                            case "float":
                                tableColumn.DataType = DBMSDataType.FLOAT;
                                break;
                        }

                        switch (mm.Groups[3].ToString())
                        {
                            case " primary key":
                                tableColumn.IsPrimaryKey = true;
                                tableColumn.IsUnique = false;
                                break;
                            case " unique":
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
                }
            }
            else
            {
                command.TableNames = new List<string>();
                command.Columns = new List<TableColumn>();
                command.ErrorMessage = "Invalid syntax! Please use: 'create table TableName (ColumnName Type ...) / create database DatabaseName / create index on TableName (ColumnName)'";
            }

            return command;
        }

        public UICommand SelectQuery(string sql)
        {
            UICommand command = new UICommand();
            command.Success = true;
            command.Command = CommandType.SELECT;

            //var reg = new Regex(@"select(.*?)(?<!\w*"")FROM(?!\w*?"")(.*?)");
            var reg = new Regex(@"select (.*) from (.*)");
            var colunms = reg.Match(sql).Groups[1].Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var tables = reg.Match(sql).Groups[2].Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            //var where = reg.Match(sql).Groups[4].Value.Split(new string[] { "&&", "||", "AND", "OR" }, StringSplitOptions.None);

            command.Columns = new List<TableColumn>();

            foreach (var item in colunms)
            {
                TableColumn column = new TableColumn();
                column.Name = item.ToString().Trim();   
                command.Columns.Add(column);
            }

            command.TableNames = new List<string>();

            foreach (var item in tables)
            {
                char c = '\\';
                command.TableNames.Add(item.ToString().Split(c).FirstOrDefault());
            }

            return command;
        }

        public UICommand InsertQuery(string insertString)
        {
            UICommand command = new UICommand();
            command.Success = true;
            command.Command = CommandType.INSERT;
            insertString = insertString.ToLower();

            IEnumerable<string> inserts = insertString.Split(new[] { Environment.NewLine }, StringSplitOptions.None);
            string firstInsert = inserts.First();
            int tableIndex = firstInsert.IndexOf("insert into ") + "insert into ".Length;

            command.TableNames = new List<string>();
            string table = firstInsert.Substring(tableIndex, firstInsert.IndexOf("(", tableIndex) - tableIndex);

            command.TableNames.Add(table.Trim());

            var regex = new System.Text.RegularExpressions.Regex(@"\(([^)]+)\)", System.Text.RegularExpressions.RegexOptions.Compiled);
            string[] columns = regex.Matches(firstInsert)[0].Value.Split(new char[] { ',', ')', '(' }, StringSplitOptions.RemoveEmptyEntries);

            IEnumerable<string> values = inserts.Select(sql => regex.Matches(sql)[1].Value);
            string[] vals = values.First().Split(new char[] { ',', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);

            command.Columns = new List<TableColumn>();

            for (int i = 0; i < columns.Length; i++)
            {
                TableColumn column = new TableColumn();
                column.Name = columns[i].ToString().Trim();
                column.Value = vals[i].ToString().Trim();
                command.Columns.Add(column);
            }

            return command;
        }

        public UICommand DropTable(string sqltext, string entity)
        {
            UICommand command = new UICommand();
            sqltext = sqltext.ToLower();
            string pattern = @"drop " + entity.ToLower() + @" (.*)";
            command.Command = CommandType.DROP;

            switch (entity.ToLower())
            {
                case "table":
                    command.Entity = EntityType.TABLE;
                    command.Success = true;
                    break;
                case "database":
                    command.Entity = EntityType.DATABASE;
                    command.Success = true;
                    break;
                case "index":
                    command.Entity = EntityType.INDEX;
                    command.Success = true;
                    break;
                default:
                    command.Success = false;
                    break;
            }

            if (!command.Success)
            {
                command.TableNames = new List<string>();
                command.Columns = new List<TableColumn>();
                command.ErrorMessage = "Invalid syntax! Please use: 'drop table/database/index name'";
            }
            else
            { 
                Match m = Regex.Match(sqltext, pattern, RegexOptions.Singleline);
                command.TableNames = new List<string>();
                string tablename = m.Groups[1].ToString();
                string[] stringSeparators = new string[] { "\r\n" };
                command.TableNames.Add(tablename.Split(stringSeparators, StringSplitOptions.None).FirstOrDefault().ToString());
                
                command.Columns = new List<TableColumn>();
            }
            return command;
        }

        public UICommand InterpretCommand(string sqltext)
        {
            UICommand command = new UICommand();
            string[] item = sqltext.Split(' ');

            switch (item[0].ToLower())
            {
                case "create":
                    command = CreateQuery(sqltext: sqltext, entity: item[1]);
                    break;
                case "select":
                    command = SelectQuery(sql: sqltext);
                    break;
                case "insert":
                    command = InsertQuery(insertString: sqltext);
                    break;
                case "delete":

                    break;
                case "drop":
                    command =  DropTable(sqltext: sqltext, entity: item[1]);
                    break;
                default:
                    //Console.WriteLine("Incorrect sql syntax!");
                    break;
            }

            return command;
        }
    }
}
