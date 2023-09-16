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
            _connectionString = SqlStrFramework.Instance.StrConnectionFrameworkSqlServer;
        }

        public List<T> Query(string sql, QueryParameters queryParameters)
        {
            T objectRes = Activator.CreateInstance<T>();
            SqlConnection connection = new SqlConnection(_connectionString);
            List<T> objectResList = Activator.CreateInstance<List<T>>();
            SqlDataReader sqlDataReaderQuery;
            using (connection)
            {
                connection.Open();

                sqlCommand = new SqlCommand(sql, connection);

                if(queryParameters != null)
                {
                    foreach (Parameter param in queryParameters.Parameters)
                    {
                        sqlCommand.Parameters.AddWithValue(param.ParameterQuery, param.Value);
                    }
                }

                sqlDataReaderQuery = sqlCommand.ExecuteReader();

                while (sqlDataReaderQuery.Read())
                {
                    objectRes = MapObjectResultGeneric(sql, queryParameters, sqlDataReaderQuery);
                    objectResList.Add(objectRes);
                }
               
                
            }

            return objectResList;
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

        private T MapObjectResultGeneric(string query, QueryParameters queryParameters ,SqlDataReader sqlDataReaderQuery)
        {

            T objectRes = Activator.CreateInstance<T>();

            PropertyInfo[] properties = typeof(T).GetType().GetProperties();

            TableProps tableProps = GetPropsSql(query,queryParameters);

            foreach (var property in properties)
            {
                if (tableProps.Columns.Any(perty => perty.Name == property.Name))
                {
                    objectRes.GetType().GetProperty(property.Name).SetValue(
                  objectRes,
                  sqlDataReaderQuery.GetValue(sqlDataReaderQuery.GetOrdinal(property.Name)).ToString().Trim(),
                  null);
                }

            }

            return objectRes;
        }

        private TableProps GetPropsSql(string sql, QueryParameters queryParameters)
        {
            TableProps tableProps = new TableProps();
            tableProps.Columns = new List<TableColumns>();
            SqlConnection connection = new SqlConnection(_connectionString);

            string procedureName = "DropPropsTablett";

            string createTable = "Select top 0 tmp.* into tt From("+ sql + ") tmp";

            string sqlInformationColums = @"SELECT TABLE_NAME, COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
                  WHERE INFORMATION_SCHEMA.COLUMNS.TABLE_NAME = 'tt'";

            using (connection)
            {
                connection.Open();

                sqlCommand = new SqlCommand(sql, connection);
                sqlCommand.CommandText = procedureName;
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("query", sql);
                sqlCommand.ExecuteNonQuery();

                sqlCommand.CommandText = createTable;
                sqlCommand.CommandType = System.Data.CommandType.Text;

                if(queryParameters != null)
                {
                    foreach (Parameter param in queryParameters.Parameters)
                    {
                        sqlCommand.Parameters.AddWithValue(param.ParameterQuery, param.Value);
                    }
                }

             
                sqlCommand.ExecuteNonQuery();
                sqlCommand.CommandText = sqlInformationColums;
                sqlDataReader = sqlCommand.ExecuteReader();

                if (sqlDataReader.HasRows)
                {
                    while (sqlDataReader.Read())
                    {
                        tableProps.Columns.Add(new TableColumns() 
                        { Name = sqlDataReader.GetString(1) });
                    }
                }
            }
            return tableProps;
        }

    }
}
