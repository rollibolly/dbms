﻿using DBMS.Models.DBStructure;
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
                    pattern = @"create " + entity.ToLower() + @"(.*) (on) ([a-zA-Z_]+) \(([a-zA-Z_]+\))";
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

            //var reg = new Regex(@"select (.*) from ([a-z0-9]+,? ?[a-z0-9]+?,? ?[a-z0-9]+?,? ?) ?(where)? ?([a-z0-9]+)? ?(=|<>|<=|>=|<|>)? ?('?[a-z0-9]+.?[0-9]?'?)? ?(and|or)? ?([a-z0-9]+)? ?(=|<>|<=|>=|<|>)? ?('?[a-z0-9]+.?[0-9]?'?)?");
            var reg = new Regex(@"select (.*) from ([a-z0-9]+) ?(where)? ?([a-z0-9]+)? ?(=|<>|<=|>=|<|>)? ?('?[a-z0-9]+.?[0-9]?'?)? ?(and|or)? ?([a-z0-9]+)? ?(=|<>|<=|>=|<|>)? ?('?[a-z0-9]+.?[0-9]?'?)?");
            var colunms = reg.Match(sql).Groups[1].Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            var tables = reg.Match(sql).Groups[2].Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            
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
                string tablename = item.ToString();
                string[] stringSeparators = new string[] { "\r" };
                command.TableNames.Add(tablename.Split(stringSeparators, StringSplitOptions.None).FirstOrDefault().ToString().Trim());
            }

            string s = reg.Match(sql).Groups[3].ToString();

            if (reg.Match(sql).Groups[3].ToString() == "where")
            {
                command.WhereClauses = new List<WhereClause>();

                if (reg.Match(sql).Groups[4].ToString() == "" || reg.Match(sql).Groups[5].ToString() == "" || reg.Match(sql).Groups[6].ToString() == "")
                {
                    command.Success = false;
                    command.ErrorMessage = "Syntax error! Correct format: 'select ColumnName(S) from TableName where ColumnName operator Value' (where operator can be: '=, <>, <, >, <=, >=')";
                }
                else
                {
                    command.Success = true;
                    WhereClause wc = new WhereClause();
                    wc.ClauseType = WhereType.DEFAULT;
                    wc.LeftValue = reg.Match(sql).Groups[4].ToString().Trim();
                    wc.Operator = reg.Match(sql).Groups[5].ToString().Trim();
                    wc.RightValue = reg.Match(sql).Groups[6].ToString().Trim();
                    command.WhereClauses.Add(wc);
                }

                if (reg.Match(sql).Groups[7].ToString() != "")
                {
                    command.Success = true;
                    WhereClause wc = new WhereClause();
                    wc.LeftValue = reg.Match(sql).Groups[8].ToString().Trim();
                    wc.Operator = reg.Match(sql).Groups[9].ToString().Trim();
                    wc.RightValue = reg.Match(sql).Groups[10].ToString().Trim();

                    switch (reg.Match(sql).Groups[7].ToString().Trim())
                    {
                        case "and":
                            wc.ClauseType = WhereType.AND;
                            break;
                        case "or":
                            wc.ClauseType = WhereType.OR;
                            break;
                        default:
                            wc.ClauseType = WhereType.DEFAULT;
                            break;
                    }
                    command.WhereClauses.Add(wc);
                }
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

        public UICommand DeleteQuery(string sqltext)
        {
            UICommand command = new UICommand();
            command.Command = CommandType.DELETE;
            command.Entity = EntityType.TABLE;

            sqltext = sqltext.ToLower();
            //string pattern = @"delete from ([a-z0-9]+) ?(where)? ?([a-z0-9]+)? ?(=|<>|<=|>=|<|>)? ?('?[a-z0-9]+.?[0-9]?'?)?");
            string pattern = @"delete from ([a-z0-9]+) ?(where)? ?([a-z0-9]+)? ?(=|<>|<=|>=|<|>)? ?('?[a-z0-9]+.?[0-9]?'?)? ?(and|or)? ?([a-z0-9]+)? ?(=|<>|<=|>=|<|>)? ?('?[a-z0-9]+.?[0-9]?'?)?";
            Match m = Regex.Match(sqltext, pattern, RegexOptions.Singleline);

            command.Success = true;
            command.TableNames = new List<string>();
            command.TableNames.Add(m.Groups[1].ToString());

            if (m.Groups[2].ToString() == "where")
            {
                command.WhereClauses = new List<WhereClause>();

                if (m.Groups[3].ToString() == "" || m.Groups[4].ToString() == "" || m.Groups[5].ToString() == "")
                {
                    command.Success = false;
                    command.ErrorMessage = "Syntax error! Correct format: 'delete from TableName where ColumnName operator Value' (where operator can be: '=, <>, <, >, <=, >=')";
                }
                else
                {
                    command.Success = true;
                    WhereClause wc = new WhereClause();
                    wc.LeftValue = m.Groups[3].ToString();
                    wc.Operator = m.Groups[4].ToString();
                    wc.RightValue = m.Groups[5].ToString();
                    wc.ClauseType = WhereType.DEFAULT;
                    command.WhereClauses.Add(wc);
                }

                if (m.Groups[6].ToString() != "")
                {
                    command.Success = true;
                    WhereClause wc = new WhereClause();
                    wc.LeftValue = m.Groups[7].ToString().Trim();
                    wc.Operator = m.Groups[8].ToString().Trim();
                    wc.RightValue = m.Groups[9].ToString().Trim();

                    switch (m.Groups[6].ToString().Trim())
                    {
                        case "and":
                            wc.ClauseType = WhereType.AND;
                            break;
                        case "or":
                            wc.ClauseType = WhereType.OR;
                            break;
                        default:
                            wc.ClauseType = WhereType.DEFAULT;
                            break;
                    }
                    command.WhereClauses.Add(wc);
                }

            }

            return command;
        }

        public List<UICommand> InterpretCommand(string sqltext)
        {
            sqltext.Replace('\n', ' ');
            string[] queries = sqltext.Split(';');
            List<UICommand> commandList = new List<UICommand>();
            foreach (string query in queries)
            {
                UICommand command = null;
                string subQuery = query.Trim();
                string[] item = subQuery.Split(' ');

                switch (item[0].ToLower())
                {
                    case "create":
                        command = CreateQuery(sqltext: subQuery, entity: item[1]);
                        break;
                    case "select":
                        command = SelectQuery(sql: subQuery);
                        break;
                    case "insert":
                        command = InsertQuery(insertString: subQuery);
                        break;
                    case "delete":
                        command = DeleteQuery(subQuery);
                        break;
                    case "drop":
                        command = DropTable(sqltext: subQuery, entity: item[1]);
                        break;
                    default:
                        //Console.WriteLine("Incorrect sql syntax!");
                        break;
                }
                if (command != null)
                {
                    commandList.Add(command);
                }
            }
            foreach (UICommand cmd in commandList)
            {
                if (!cmd.Success)
                {
                    throw new Exception(String.Format("Error [{0}]", cmd.ErrorMessage));
                }
            }

            return commandList;
        }
        
    }
}
