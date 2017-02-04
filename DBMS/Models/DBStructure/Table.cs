using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DBMS.Models.DBStructure
{
    

    [DataContract]
    public class Table:DBMSEntity
    {
        [IgnoreDataMember]
        private string _TableName;
        [DataMember]
        public string TableName
        {
            get { return _TableName; }
            set
            {
                _TableName = value;
                this.FileName = String.Format("{0}_dbfile.dbms", value);
                this.DisplayName = _TableName;
            }
        }
        [DataMember]
        public string FileName { get; set; }
        [DataMember]
        public List<TableColumn> Columns { get; set; }

        [IgnoreDataMember]
        public TableColumn PrimaryKey
        {
            get
            {
                return Columns.Where(r => r.IsPrimaryKey == true).FirstOrDefault();
            }
            set { }
        }
        [IgnoreDataMember]
        public List<TableColumn> ForeignKeys
        {
            get
            {
                return Columns.Where(r => r.FK != null).ToList();
            }
            set { }
        }
        [IgnoreDataMember]
        public List<TableColumn> UniqueColumns
        {
            get
            {
                return Columns.Where(r => r.IsUnique).ToList();
            }
            set { }
        } 
    }

    [DataContract]
    public class TableColumn:DBMSEntity
    {
        [IgnoreDataMember]
        private string _Name;
        [DataMember]        
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
                this.DisplayName = _Name;
            }
        }
        [DataMember]
        public UInt32 Order { get; set; }
        [DataMember]
        public DBMSDataType DataType { get; set; }
        [DataMember]
        public UInt32 Length { get; set; }
        [DataMember]
        public bool IsNull { get; set; }
        [DataMember]
        public bool IsPrimaryKey { get; set; }
        [DataMember]
        public bool IsUnique { get; set; }
        [DataMember]
        private string refTable { get; set; }
        [DataMember]
        private string refColumn { get; set; }
        [IgnoreDataMember]
        public string FK
        {
            get
            {
                return string.Format("{0}.{1}", refTable, refColumn);
            }
            set
            {
                string[] arr = value.Split('.');
                if (arr.Count() == 2)
                {
                    refTable = arr[0];
                    refColumn = arr[1];
                }
                else
                {
                    refTable = null;
                    refColumn = null;
                }
            }
        }
    }    
}
