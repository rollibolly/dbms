using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace DBMS.Models.DBStructure
{
    [DataContract]
    public class Index : DBMSEntity
    {
        [DataMember]
        public Table RefTable { get; set; }
        [DataMember]
        public TableColumn RefColumn { get; set; }

        private string filename = null;
        [DataMember]
        public string Filename
        {
            get
            {
                return filename;
            }
            set
            {
                filename = value;
            }
        }
    }
}
