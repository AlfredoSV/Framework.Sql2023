using System;
using System.Collections.Generic;

namespace Framework.SqlServer
{

    public class TableProps
    {
        public string Name { get; set; }

        public List<TableColumns> Columns { get; set; }

    }
}
