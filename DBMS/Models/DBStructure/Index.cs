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
        public Table RefTable { get; set; }
        public TableColumn RefColumn { get; set; }

        private string filename = null;
        public string Filename
        {
            get
            {
                if (string.IsNullOrEmpty(filename))
                {
                    if (!string.IsNullOrEmpty(RefTable.TableName) && !string.IsNullOrEmpty(RefColumn.Name))
                    {
                        filename = string.Format("index_{0}_{1}.{2}.dbms", 
                            Guid.NewGuid().ToString(), 
                            RefTable.TableName,
                            RefColumn.Name);
                    }
                }
                return filename;
            }
            set { }
        }
    }
}
