using FrameworkSql;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Reflection;

namespace FrameworkSqlTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            People prop = new People();

            SqlDB<People> sqlDB = new SqlDB<People>("server=ALFREDO ; database=fkw ; integrated security = true");

            /* List<People> res = sqlDB.SelectList();

            foreach (People item in res)
            {
                Console.WriteLine($"{item.Id}=>{item.Name}");
                
            }

            Console.WriteLine("//////////////////////////////////");

            People people = sqlDB.Select();

            Console.WriteLine($"{people.Id} => {people.Name}");

            Console.ReadLine();

            Console.WriteLine("//////////////////////////////////");
            */
            People peopleIn = new People()
            {
                Id = "38785",
                Name
                = "PrueInsert"
            };

            foreach (PropertyInfo pro in peopleIn.GetType().GetProperties())
            {
                Console.WriteLine(pro.PropertyType.Name);
                if (pro.CustomAttributes.Count() > 0)
                    Console.WriteLine(pro.CustomAttributes.First().AttributeType.Name);
            }

            Console.Read();
        }
    }
}
