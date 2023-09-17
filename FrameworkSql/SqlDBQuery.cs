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
        private QueryParameters _queryParameters;
        private string _query;

        public SqlDBQuery()
        {   
            _connectionString = SqlStrFramework.Instance.StrConnectionFrameworkSqlServer;
        }

        public void Query(string query)
        {
            _query = query;
        }

        public List<T> Execute()
        {
            T objectRes = Activator.CreateInstance<T>();
            SqlConnection connection = new SqlConnection(_connectionString);
            List<T> objectResList = Activator.CreateInstance<List<T>>();
            SqlDataReader sqlDataReaderQuery;
            SqlCommand sqlCommand;
            using (connection)
            {
                connection.Open();
                sqlCommand = new SqlCommand(_query, connection);

                if(_queryParameters != null)
                {
                    foreach (Parameter param in _queryParameters.Parameters)
                    {
                        sqlCommand.Parameters.AddWithValue(param.ParameterQuery, param.Value);
                    }
                }

                TableProps tableProps = GetPropsSql();

                sqlDataReaderQuery = sqlCommand.ExecuteReader();

         
                while (sqlDataReaderQuery.Read())
                {
                    objectRes = MapObjectResultGeneric(sqlDataReaderQuery,tableProps);
                    objectResList.Add(objectRes);
                }
               
                
            }

            return objectResList;
        }

        //public IEnumerable<T> GetQueryPagination(string sql, QueryParameters queryParameters)
        //{
        //    List<T> objectRes = Activator.CreateInstance<List<T>>();
        //    SqlConnection connection = new SqlConnection(_connectionString);

        //    using (connection)
        //    {
        //        connection.Open();

        //        sqlCommand = new SqlCommand(sql, connection);

        //        foreach (Parameter param in queryParameters.Parameters)
        //        {
        //            sqlCommand.Parameters.AddWithValue(param.ParameterQuery, param.Value);
        //        }
        //        sqlDataReader = sqlCommand.ExecuteReader();
        //        sqlDataReader.Read();
        //        objectRes = null;// MapObjectResultGeneric(sql, sqlDataReader);
        //    }

        //    return objectRes;
        //}

        private T MapObjectResultGeneric(SqlDataReader sqlDataReaderQuery, TableProps tableProps)
        {

            T objectRes = Activator.CreateInstance<T>();

            PropertyInfo[] properties = objectRes.GetType().GetProperties();

           
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

        private TableProps GetPropsSql()
        {
             SqlDataReader sqlDataReaderGetProps;
            TableProps tableProps = new TableProps();
            tableProps.Columns = new List<TableColumns>();
            SqlConnection connection = new SqlConnection(_connectionString);
            SqlCommand sqlCommand;

            string procedureName = "DropPropsTablett";

            string createTable = "Select top 0 tmp.* into tt From("+ _query + ") tmp";

            string sqlInformationColums = @"SELECT TABLE_NAME, COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
                  WHERE INFORMATION_SCHEMA.COLUMNS.TABLE_NAME = 'tt'";

            using (connection)
            {
                connection.Open();

                sqlCommand = new SqlCommand();
                sqlCommand.Connection = connection;
                sqlCommand.CommandText = procedureName;
                sqlCommand.CommandType = System.Data.CommandType.StoredProcedure;
                sqlCommand.Parameters.AddWithValue("query", _query);
                sqlCommand.ExecuteNonQuery();

                sqlCommand.CommandText = createTable;
                sqlCommand.CommandType = System.Data.CommandType.Text;

                if(_queryParameters != null)
                {
                    foreach (Parameter param in _queryParameters.Parameters)
                    {
                        sqlCommand.Parameters.AddWithValue(param.ParameterQuery, param.Value);
                    }
                }

             
                sqlCommand.ExecuteNonQuery();
                sqlCommand.CommandText = sqlInformationColums;
                sqlDataReaderGetProps = sqlCommand.ExecuteReader();

                if (sqlDataReaderGetProps.HasRows)
                {
                    while (sqlDataReaderGetProps.Read())
                    {
                        tableProps.Columns.Add(new TableColumns() 
                        { Name = sqlDataReaderGetProps.GetString(1) });
                    }
                }
            }
            return tableProps;
        }

        public void AddParameters(QueryParameters queryParameters) {
            _queryParameters = queryParameters;
        }
    }
}
