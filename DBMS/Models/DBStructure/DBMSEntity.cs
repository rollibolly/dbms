using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DBMS.Models.DBStructure
{
    public enum EntityType
    {
        COLUMN,
        COLUMN_LIST,
        TABLE,
        TABLE_LIST,
        DATABASE,
        DATABASE_LIST,
        UNKNOWN
    }

    public static class Mappings
    {
        private static string DatabaseTypeStr = typeof(DBMSDatabase).ToString();
        private static string TablesListTypeStr = typeof(List<Models.DBStructure.Table>).ToString();
        private static string TableTypeStr = typeof(Models.DBStructure.Table).ToString();
        private static string ColumnListTypeStr = typeof(List<Models.DBStructure.TableColumn>).ToString();
        private static string ColumnTypeStr = typeof(Models.DBStructure.TableColumn).ToString();
        public static EntityType MapObject(object obj)
        {
            string objTypeStr = obj.GetType().ToString();

            // Database selected
            if (objTypeStr == DatabaseTypeStr)
            {                
                return EntityType.DATABASE;
            }

            // Tables list selected
            if (objTypeStr == TablesListTypeStr)
            {                
                return EntityType.TABLE_LIST;
            }

            // Table selected
            if (objTypeStr == TableTypeStr)
            {                
                return EntityType.TABLE;
            }

            // Columns List selected
            if (objTypeStr == ColumnListTypeStr)
            {
                return EntityType.COLUMN_LIST;
            }

            // Column selected
            if (objTypeStr == ColumnTypeStr)
            {
                return EntityType.COLUMN;
            }
            return EntityType.UNKNOWN;
        }
    }

    [DataContract]
    public abstract class DBMSEntity
    {
        [DataMember]
        public string DisplayName { get; set; }
    }
}
