using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using System.Linq;
using FrameworkSql;

namespace Framework.Sql2023
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

        public IEnumerable<T> SelectList()
        {
            List<T> objectResList = Activator.CreateInstance<List<T>>();
            T objectRes = Activator.CreateInstance<T>();
            string sql = GenerateSqlSelect(objectRes);
            SsqlConnection = new SqlConnection(ConnectionStr);
            SqlDataReader sqlDataReade;
            using (SsqlConnection)
            {
                SsqlConnection.Open();

                sqlCommand = new SqlCommand(sql, SsqlConnection);
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

        public StatusQuery Insert(T obj)
        {
            string sql = GenerateSqlInsert(obj);
            StatusQuery result = StatusQuery.RowsNotAfected;
            SsqlConnection = new SqlConnection(ConnectionStr);
            using (SsqlConnection)
            {
                SsqlConnection.Open();
                sqlCommand = new SqlCommand(sql, SsqlConnection);

                foreach (PropertyInfo prop in obj.GetType().GetProperties())
                {
                    sqlCommand.Parameters.AddWithValue($"@{prop.Name}",prop.GetValue(obj));
                }

                if (sqlCommand.ExecuteNonQuery() >= 1)
                    result = StatusQuery.Ok;
            }

            return result;
        }

        public StatusQuery Update(T obj)
        {
            string sql = GenerateSqlUpdate();
            StatusQuery result = StatusQuery.RowsNotAfected;
            SsqlConnection = new SqlConnection(ConnectionStr);
            using (SsqlConnection)
            {
                SsqlConnection.Open();
                sqlCommand = new SqlCommand(sql, SsqlConnection);

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
            //string columnId = string.Empty;
            //string value = string.Empty;
            string sql = GenerateSqlDelete();
            StatusQuery result = StatusQuery.RowsNotAfected;
            SsqlConnection = new SqlConnection(ConnectionStr);

            using (SsqlConnection)
            {
                SsqlConnection.Open();
                sqlCommand = new SqlCommand(sql, SsqlConnection);

                ////Add Parameters

                //PropertyInfo[] props = Activator.CreateInstance<T>().GetType()
                //    .GetProperties();

                //foreach (PropertyInfo prop in props)
                //{
                //    List<CustomAttributeData> attri = prop.CustomAttributes.ToList();
                //    if (attri.Any(att => att.AttributeType.Name == "Id"))
                //        columnId = attri.Where(att => att.AttributeType.Name == "Id").FirstOrDefault().AttributeType.Name;
                //}

                //foreach (PropertyInfo prop in props)
                //{
                //    string nameProp = prop.Name;
                //    if (nameProp == columnId)
                //         value = prop.GetValue(type).ToString();                 
                        
                //}

                ///

                sqlCommand.Parameters.AddWithValue("id", id);
                if (sqlCommand.ExecuteNonQuery() >= 1)
                    result = StatusQuery.Ok;

            }

            return result;
        }

        public T Query(string sql)
        {
            return Activator.CreateInstance<T>();
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
            {
                columns += column.Name + ",";
                values += "@"+column.Name + ",";
            }
                

            columns = columns.Trim(',');
            columns += ")";

            //foreach (PropertyInfo prop in objectRes.GetType().GetProperties())
            //{
            //    string type = prop.PropertyType.Name;

            //    values += "'" + prop.GetValue(objectRes) + "'"  + ",";
            //}

            values = values.Trim(',');
            values += ")";

            sql = sql + " " + columns + " " + selectElemens[1] + values;
            return sql;
        }

        public  string GenerateSqlUpdate()
        {
            T instace = Activator.CreateInstance<T>();
            string sql =  $"UPDATE {instace.GetType().Name} SET " ;
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

            sql = sql + " " + columns + $" WHERE {columnId} = @id";
            return sql;
        }

        private string GenerateSqlDelete()
        {
            string columnId = string.Empty;
            T instace = Activator.CreateInstance<T>();
            PropertyInfo[] props = instace.GetType().GetProperties();

            foreach (PropertyInfo prop in props)
            {
                List<CustomAttributeData>  attri = prop.CustomAttributes.ToList();
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

        private TableProps GetPropsSql(string sql)
        {
            TableProps tableProps = new TableProps();
            SsqlConnection = new SqlConnection(ConnectionStr);
   
            string procedureName = @"GetPropsQuery";

            using (SsqlConnection)
            {
                SsqlConnection.Open();

                sqlCommand = new SqlCommand(sql, SsqlConnection);
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

        private T MapObjectResult(SqlDataReader res)
        {

            T objectRes = Activator.CreateInstance<T>();

            PropertyInfo[] properties = objectRes.GetType().GetProperties();

            TableProps tableProps = GetPropsTable(objectRes);

            foreach (var property in properties)
            {
                if(tableProps.Columns.Any(perty => perty.Name == property.Name))
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
