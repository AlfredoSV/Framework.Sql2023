﻿using Framework.SqlServer;
using FrameworkSql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework.Sql2023.Test
{
    [TestClass]
    public class TestSqlDB
    {

        public TestSqlDB()
        {
            SqlStrFramework.Instance.StrConnectionFrameworkSqlServer = "Server=Alfredo;Database=fkw;User=sa;Password=1007";
        }


        [TestMethod]
        public void SelectList_Test()
        {
            SqlDB<Escritor> sqlDB = new SqlDB<Escritor>();

            IEnumerable<Escritor> escritores = sqlDB.SelectList();

            Assert.IsTrue(escritores.Any());

            Assert.IsNotNull(escritores);

        }

        [TestMethod]
        public void Delete_Test()
        {

        }

    }

    class Escritor
    {
        public string Id { get; set; }
        public string Nombre { get; set; }
        public string Apellidos { get; set; }
        public string Direccion { get; set; }
    }
}
