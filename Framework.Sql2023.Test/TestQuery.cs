using Framework.SqlServer;
using FrameworkSql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Framework.Sql2023.Test
{
    [TestClass]
    public class TestQuery
    {
        [TestMethod]
        public void TestQueryExcute()
        {
            SqlStrFramework.Instance.StrConnectionFrameworkSqlServer = "Server=Alfredo;Database=fkw;User=sa;Password=1007";

            SqlDBQuery<People> query = new SqlDBQuery<People>();

            List<People> peoples = new List<People>();

            peoples = query.Query("Select Name from People", null);

            List<People> peoplesFilter = new List<People>();

            QueryParameters queryParameters = new QueryParameters();

            queryParameters.AddParameter("@name","Jorge");

            peoplesFilter = query.Query("Select Id,Name from People where Name = @name", queryParameters);

            foreach(var ite in peoplesFilter)
            {
                Console.WriteLine(ite.Id + " " + " " + ite.Name);
            }


        }

        
    }

    public class People
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
}
