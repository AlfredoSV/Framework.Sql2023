using FrameworkSql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Framework.Sql2023.Test
{
    [TestClass]
    public class TestGenerateSql
    {
        [TestMethod]
        [Ignore]
        public void Test_GetPropsSql()
        {
            SqlDB<People> sqlDB = new SqlDB<People>("server=ALFREDO ; database=fkw ; integrated security = true");
            People people = new People();
           
            //TableProps sql = sqlDB.GetPropsSql("select * from People");

            //Assert.IsTrue(sql.Columns.Count == 2);
            //Assert.IsTrue(sql.Name.Equals("People"));

        }

        [TestMethod]
        public void Test_ExecuteSql()
        {
            //SqlDB<People> sqlDB = new SqlDB<People>("server=ALFREDO ; database=fkw ; integrated security = true");
            //People people = new People();

            //People sql = sqlDB.Query("select 1 as age");

        }
    }

    class People
    {
        [Id]
        public string Id { get; set; }

        public string Name { get; set; }
    }
}
