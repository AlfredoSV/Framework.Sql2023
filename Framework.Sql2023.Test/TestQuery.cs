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
        [Ignore]
        [TestMethod]
        public void TestQueryExcute()
        {
            SqlStrFramework.Instance.StrConnectionFrameworkSqlServer = "Server=Alfredo;Database=fkw;User=sa;Password=1007";

            SqlDBQuery<People> query = new SqlDBQuery<People>();

            List<People> peoples = new List<People>();

            query.Query("Select 1 as Id, Name from People");

            peoples = query.Execute();

            List <People> peoplesFilter = new List<People>();

            QueryParameters queryParameters = new QueryParameters();

            queryParameters.AddParameter("@name","Jorge");

            
            query.Query("Select Id,Name from People where Name = @name");
            query.AddParameters(queryParameters);
            peoplesFilter = query.Execute();

            foreach (var ite in peoplesFilter)
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
