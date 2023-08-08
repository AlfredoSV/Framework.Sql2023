using System.Collections.Generic;

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
