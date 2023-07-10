using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkSql
{

    public class TableProps
    {
        public string Name { get; set; }

        public List<TableColumns> Columns { get; set; }

    }
}
