using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMS.Models
{
    public class DbmsTableResult
    {
        private List<string> headers;
        private List<object[]> values;

        public List<string> Headers
        {
            get
            {
                if (headers == null)
                    headers = new List<string>();
                return headers;
            }
            set { headers = value; }
        }
        public List<object[]> Rows
        {
            get
            {
                if (values == null)
                    values = new List<object[]>();
                return values;
            }
            set { values = value; }
        }
        public int RowCount
        {
            get { return Rows.Count; }
        }
        public int ColumnCount
        {
            get { return headers.Count; }
        }
        public object[] NewRow()
        {
            return new object[ColumnCount];
        }
        public List<int> IndexesOfHeader(string header)
        {
            int index = 0;
            List<int> indexes = new List<int>();
            foreach (string str in headers)
            {
                if (header == str)
                    indexes.Add(index);
                index++;
            }
            return indexes;
        }
    }
}
