using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DBMS.Utilities
{
    public class DBMSResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public Object Data { get; set; }
        public DBMSResult(bool success = true, object data = null, string message = "")
        {
            Success = success;
            Message = message;
            Data = data;
        }

        public static DBMSResult SUCCESS(object obj = null)
        {
            return new DBMSResult(true, obj);
        }
        public static DBMSResult FAILURE(string message)
        {
            return new DBMSResult(false, null, message);
        }
    }
}
