using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DBMS.Models.DBStructure
{
    [DataContract]
    public class DBMSDatabase:DBMSEntity
    {
        [DataMember]
        public string FolderName { get; set; }
        [IgnoreDataMember]
        private string _DatabaseName;
        [DataMember]
        public string DatabaseName
        {
            get
            {
                return _DatabaseName;
            }
            set
            {
                _DatabaseName = value;
                this.DisplayName = _DatabaseName;
            }
        }
        [DataMember]
        public List<Table> Tables { get; set; }
        [DataMember]
        public List<Index> Indexes
        {
            get
            {
                if (this.indexses == null)
                    this.indexses = new List<Index>();
                return indexses;
            }
            set
            {
                this.indexses = value;
            }
        }


        [IgnoreDataMember]
        private List<Index> indexses;
    }
}
