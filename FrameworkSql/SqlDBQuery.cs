using FrameworkSql;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;

namespace Framework.SqlServer
{
    public sealed class SqlDBQuery<T>
    {
        
        private readonly string _connectionString;
        private SqlCommand sqlCommand;
        private SqlDataReader sqlDataReader;

        public SqlDBQuery()
        {   
            _connectionString = SqlStrFramework.Instance.StrConnectionFrameworkSql;
        }

        public T Query(string sql, QueryParameters queryParameters)
        {
            T objectRes = Activator.CreateInstance<T>();
            SqlConnection connection = new SqlConnection(_connectionString);

            using (connection)
            {
                connection.Open();

                sqlCommand = new SqlCommand(sql, connection);

                foreach (Parameter param in queryParameters.Parameters)
                {
                    sqlCommand.Parameters.AddWithValue(param.ParameterQuery, param.Value);
                }
                sqlDataReader = sqlCommand.ExecuteReader();
                sqlDataReader.Read();
                objectRes = MapObjectResultGeneric(sql, sqlDataReader);
            }

            return objectRes;
        }

        public IEnumerable<T> GetQueryPagination(string sql, QueryParameters queryParameters)
        {
            List<T> objectRes = Activator.CreateInstance<List<T>>();
            SqlConnection connection = new SqlConnection(_connectionString);

            using (connection)
            {
                connection.Open();

                sqlCommand = new SqlCommand(sql, connection);

                foreach (Parameter param in queryParameters.Parameters)
                {
                    sqlCommand.Parameters.AddWithValue(param.ParameterQuery, param.Value);
                }
                sqlDataReader = sqlCommand.ExecuteReader();
                sqlDataReader.Read();
                objectRes = null;// MapObjectResultGeneric(sql, sqlDataReader);
            }

            return objectRes;
        }


        private T MapObjectResultGeneric(string query, SqlDataReader res)
        {

            T objectRes = Activator.CreateInstance<T>();

            PropertyInfo[] properties = objectRes.GetType().GetProperties();

            TableProps tableProps = GetPropsSql(query);

            foreach (var property in properties)
            {
                if (tableProps.Columns.Any(perty => perty.Name == property.Name))
                {
                    objectRes.GetType().GetProperty(property.Name).SetValue(
                  objectRes,
                  res.GetValue(res.GetOrdinal(property.Name)).ToString().Trim(),
                  null);
                }

            }

            return objectRes;
        }

        private TableProps GetPropsSql(string sql)
        {
            TableProps tableProps = new TableProps();
            SqlConnection connection = new SqlConnection(_connectionString);

            string procedureName = @"GetPropsQuery";

            using (connection)
            {
                connection.Open();

                sqlCommand = new SqlCommand(sql, connection);
                sqlCommand.CommandText = procedureName;
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("query", sql);
                sqlDataReader = sqlCommand.ExecuteReader();
                tableProps.Columns = new List<TableColumns>();

                if (sqlDataReader.HasRows)
                {
                    while (sqlDataReader.Read())
                    {
                        tableProps.Columns.Add(new TableColumns() { Name = sqlDataReader.GetString(1) });
                    }
                }
            }
            return tableProps;
        }

    }
}
