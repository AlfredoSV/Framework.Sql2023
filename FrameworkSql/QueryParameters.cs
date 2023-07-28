using Framework.Sql2023;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrameworkSql
{
    public class QueryParameters
    {
        private List<Parameter> _Parameters =
            new List<Parameter>();

        public List<Parameter> Parameters { get; set; }

 
        public void AddParameter(string parameter, string value)
        {
           this. _Parameters.Add(new Parameter() { ParameterQuery = parameter, Value = value });
        }
    }

    public class Parameter
    {
        public string ParameterQuery { get; set; }
        public string Value { get; set; }
    }

}
