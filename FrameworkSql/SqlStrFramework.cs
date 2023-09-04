using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkSql
{
    public class SqlStrFramework
    {
        private static SqlStrFramework _instance;

        public static SqlStrFramework Instance { 
            
            get {

                if (_instance is null)
                    _instance = new SqlStrFramework();
                return _instance;
            
            } 
            private set { }
        }

        public string StrConnectionFrameworkSql { get; set; }

    }
}
