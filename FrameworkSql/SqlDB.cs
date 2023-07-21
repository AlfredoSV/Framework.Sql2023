using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;

namespace FrameworkSql
{
    public class SqlDB<T>
    {
        private SqlConnection SsqlConnection;
        private SqlCommand sqlCommand;
        private SqlDataReader sqlDataReader;
        private string ConnectionStr { get; set; }

        public SqlDB(string connectionStr)
        {
            this.ConnectionStr = connectionStr;
        }

        public List<T> SelectList()
        {
            List<T> objectResList = Activator.CreateInstance<List<T>>();
            T objectRes = Activator.CreateInstance<T>();
            string sql = GenerateSqlSelect(objectRes);
            SsqlConnection = new SqlConnection(ConnectionStr);
            using (SsqlConnection)
            {
                SsqlConnection.Open();

                sqlCommand = new SqlCommand(sql, SsqlConnection);
                sqlDataReader = sqlCommand.ExecuteReader();

                while (sqlDataReader.Read())
                {
                    objectRes = MapObjectResult(sqlDataReader);
                    objectResList.Add(objectRes);
                }

            }

            return objectResList;
        }

        public T Select()
        {
            T objectRes = Activator.CreateInstance<T>();
            string sql = GenerateSqlSelect(objectRes);
            SsqlConnection = new SqlConnection(ConnectionStr);
            using (SsqlConnection)
            {
                SsqlConnection.Open();

                sqlCommand = new SqlCommand(sql, SsqlConnection);
                sqlDataReader = sqlCommand.ExecuteReader();
                sqlDataReader.Read();
                objectRes = MapObjectResult(sqlDataReader);
            }

            return objectRes;
        }

        public void Insert(T obj)
        {
            string sql = GenerateSqlInsert(obj);
            SsqlConnection = new SqlConnection(ConnectionStr);
            using (SsqlConnection)
            {
                SsqlConnection.Open();
                sqlCommand = new SqlCommand(sql, SsqlConnection);
                sqlCommand.ExecuteNonQuery();

            }

        }

        #region Private Methods

        private string GenerateSqlInsert(T objectRes)
        {
            string[] selectElemens = { $"INSERT INTO {objectRes.GetType().Name} (", "VALUES(" };
            string sql = selectElemens[0];
            string columns = string.Empty;
            string values = string.Empty;
            TableProps tableProps = GetPropsTable(objectRes);

            foreach (TableColumns column in tableProps.Columns)
                columns += column.Name + ",";

            columns = columns.Trim(',');
            columns += ")";

            foreach (PropertyInfo prop in objectRes.GetType().GetProperties())
            {
                string type = prop.PropertyType.Name;

                values += "'" + prop.GetValue(objectRes) + "'"  + ",";
            }

            values = values.Trim(',');
            values += ")";

            sql = sql + " " + columns + " " + selectElemens[1] + values;
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
            SsqlConnection = new SqlConnection(ConnectionStr);
            tableProps.Name = obj.GetType().Name;

            string sql = @"SELECT TABLE_NAME, COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS
                WHERE INFORMATION_SCHEMA.COLUMNS.TABLE_NAME = @TableName";

            using (SsqlConnection)
            {
                SsqlConnection.Open();

                sqlCommand = new SqlCommand(sql, SsqlConnection);
                sqlCommand.Parameters.AddWithValue("@TableName", tableProps.Name);
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

        private T MapObjectResult(SqlDataReader res)
        {

            T objectRes = Activator.CreateInstance<T>();

            PropertyInfo[] properties = objectRes.GetType().GetProperties();

            foreach (var property in properties)
            {
                objectRes.GetType().GetProperty(property.Name).SetValue(
                    objectRes,
                    res.GetValue(res.GetOrdinal(property.Name)).ToString().Trim(),
                    null);
            }

            return objectRes;
        }

        #endregion

    }

}
