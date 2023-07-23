using FrameworkSql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Framework.Sql2023.Test
{
    [TestClass]
    public class TestGenerateSql
    {
        [TestMethod]
        public void Test_GenerateSqlUpdate()
        {
            SqlDB<People> sqlDB = new SqlDB<People>("server=ALFREDO ; database=fkw ; integrated security = true");
            People people = new People();
            string res = sqlDB.GenerateSqlUpdate();
        }
    }

    class People
    {
        [Id]
        public Guid Id { get; set; }

        public string Name { get; set; }
    }
}
