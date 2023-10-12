using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using System.Linq;
using FrameworkSql;

namespace Framework.SqlServer
{
    public class SqlDB<T>
    {
        #region Properties and Contructor
        private string _connectionStr;

        public SqlDB()
        {
            _connectionStr = SqlStrFramework.Instance.StrConnectionFrameworkSqlServer;
        }
        #endregion

        #region Public methods      

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Enumerable<T></returns>
        public IEnumerable<T> SelectList()
        {
            List<T> objectResList = Activator.CreateInstance<List<T>>();
            T objectRes = Activator.CreateInstance<T>();
            string sql = GenerateSqlSelect(objectRes);
            SqlConnection sqlConnection = new SqlConnection(_connectionStr);
            SqlDataReader sqlDataReade;
            using (sqlConnection)
            {
                sqlConnection.Open();

                SqlCommand sqlCommand = new SqlCommand(sql, sqlConnection);
                sqlDataReade = sqlCommand.ExecuteReader();

                while (sqlDataReade.Read())
                {
                    objectRes = MapObjectResult(sqlDataReade);
                    objectResList.Add(objectRes);
                }

            }

            return objectResList;
        }

        public T Select()
        {
            T objectRes = Activator.CreateInstance<T>();
            string sql = GenerateSqlSelect(objectRes);
            SqlConnection sqlConnection = new SqlConnection(_connectionStr);
            using (sqlConnection)
            {
                sqlConnection.Open();

                SqlCommand sqlCommand = new SqlCommand(sql, sqlConnection);
                SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
                sqlDataReader.Read();
                objectRes = MapObjectResult(sqlDataReader);
            }

            return objectRes;
        }

        public int Insert(T obj)
        {
            string sql = GenerateSqlInsert(obj);
            int result = (int)StatusQuery.RowsNotAfected;
            SqlConnection sqlConnection = new SqlConnection(_connectionStr);
            using (sqlConnection)
            {
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand(sql, sqlConnection);

                foreach (PropertyInfo prop in obj.GetType().GetProperties())
                {
                    sqlCommand.Parameters.AddWithValue($"@{prop.Name}", prop.GetValue(obj));
                }

                if (sqlCommand.ExecuteNonQuery() >= 1)
                    result = (int)StatusQuery.Ok;
            }

            return result;
        }

        public StatusQuery Update(T obj)
        {
            string sql = GenerateSqlUpdate();
            StatusQuery result = StatusQuery.RowsNotAfected;
            SqlConnection sqlConnection = new SqlConnection(_connectionStr);
            using (sqlConnection)
            {
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand(sql, sqlConnection);

                foreach (PropertyInfo prop in obj.GetType().GetProperties())
                {
                    sqlCommand.Parameters.AddWithValue($"@{prop.Name}", prop.GetValue(obj));
                }

                if (sqlCommand.ExecuteNonQuery() >= 1)
                    result = StatusQuery.Ok;
            }

            return result;
        }

        public StatusQuery Delete<K>(K id)
        {
            string sql = GenerateSqlDelete();
            StatusQuery result = StatusQuery.RowsNotAfected;
            SqlConnection sqlConnection = new SqlConnection(_connectionStr);

            using (sqlConnection)
            {
                sqlConnection.Open();
                SqlCommand sqlCommand = new SqlCommand(sql, sqlConnection);

                sqlCommand.Parameters.AddWithValue("id", id);
                if (sqlCommand.ExecuteNonQuery() >= 1)
                    result = StatusQuery.Ok;
            }

            return result;
        }

        #endregion

        #region Private Methods

        private string GenerateSqlInsert(T objectRes)
        {
            string[] selectElemens = { $"INSERT INTO {objectRes.GetType().Name} (", "VALUES(" };
            string sql = selectElemens[0];
            string columns = string.Empty;
            string values = string.Empty;
            TableProps tableProps = GetPropsTable(objectRes);

            foreach (TableColumns column in tableProps.Columns)
            {
                columns += column.Name + ",";
                values += "@" + column.Name + ",";
            }

            columns = columns.Trim(',');
            columns = string.Concat(columns, ")");
            //columns += ")";

            values = values.Trim(',');
            values = string.Concat(values, ")");
            //values += ")";

            //sql = sql + " " + columns + " " + selectElemens[1] + values;
            sql = string.Concat(sql, " ", columns, " ", selectElemens[1], values);
            return sql;
        }

        private string GenerateSqlUpdate()
        {
            T instace = Activator.CreateInstance<T>();
            string sql = $"UPDATE {instace.GetType().Name} SET ";
            string columns = string.Empty;
            TableProps tableProps = GetPropsTable(instace);
            string columnId = string.Empty;
            PropertyInfo[] props = instace.GetType().GetProperties();

            foreach (TableColumns column in tableProps.Columns)
            {
                columns += column.Name + "= @" + column.Name + ",";
            }

            foreach (PropertyInfo prop in props)
            {
                List<CustomAttributeData> attri = prop.CustomAttributes.ToList();
                if (attri.Any(att => att.AttributeType.Name == "Id"))
                    columnId = attri.Where(att => att.AttributeType.Name == "Id").FirstOrDefault().AttributeType.Name;
            }

            columns = columns.Trim(',').Trim();

            //sql = sql + " " + columns + $" WHERE {columnId} = @id";
            sql = string.Concat(sql, " ", columns, $" WHERE {columnId} = @id");
            return sql;
        }

        private string GenerateSqlDelete()
        {
            string columnId = string.Empty;
            T instace = Activator.CreateInstance<T>();
            PropertyInfo[] props = instace.GetType().GetProperties();

            foreach (PropertyInfo prop in props)
            {
                List<CustomAttributeData> attri = prop.CustomAttributes.ToList();
                if (attri.Any(att => att.AttributeType.Name == "Id"))
                    columnId = attri.Where(att => att.AttributeType.Name == "Id").FirstOrDefault().AttributeType.Name;
            }

            string sql = $"DELETE FROM {instace.GetType().Name} WHERE {columnId} = @id;";

            return sql;
        }

        private string GenerateSqlSelect(T objectRes)
        {
            string[] selectElemens = { "Select", "FROM" };
            string sql = selectElemens[0];
            string columns = string.Empty;
            TableProps tableProps = GetPropsTable(objectRes);

            if (tableProps.Columns.Count > 0)
            {
                foreach (TableColumns column in tableProps.Columns)
                    columns += column.Name + ",";

                columns = columns.Trim(',');
            }
            else
                columns = "*";

            sql = sql + " " + columns + " " + selectElemens[1] + " " + tableProps.Name;

            return sql;
        }

        private TableProps GetPropsTable(T obj)
        {
            TableProps tableProps = new TableProps();
            SqlConnection sqlConnection = new SqlConnection(_connectionStr);
            tableProps.Name = obj.GetType().Name;

            string sql = @"SELECT TABLE_NAME, COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
                WHERE INFORMATION_SCHEMA.COLUMNS.TABLE_NAME = @TableName";

            using (sqlConnection)
            {
                sqlConnection.Open();

                SqlCommand sqlCommand = new SqlCommand(sql, sqlConnection);
                sqlCommand.Parameters.AddWithValue("@TableName", tableProps.Name);
                SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
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

        private T MapObjectResult(SqlDataReader res)
        {
            T objectRes = Activator.CreateInstance<T>();
            PropertyInfo[] properties = objectRes.GetType().GetProperties();
            TableProps tableProps = GetPropsTable(objectRes);

            foreach (var property in properties)
            {
                if (tableProps.Columns.Any(perty => perty.Name.ToLower() == property.Name.ToLower()))
                {
                    objectRes.GetType().GetProperty(property.Name).SetValue(
                  objectRes,
                  res.GetValue(res.GetOrdinal(property.Name)).ToString().Trim(),
                  null);
                }

            }

            return objectRes;
        }

        #endregion      

    }

    public enum StatusQuery
    {
        Ok = 1,
        RowsNotAfected = 0
    }

}
