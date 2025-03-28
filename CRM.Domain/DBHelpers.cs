using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Data.SqlClient;
using System.Dynamic;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using System.Data.Linq;

namespace CRM.Domain {
    internal static class DBHelpers {
        #region Private Members
        private static dynamic ToDynamic(this DataTable value) {
            string[] pageProps = typeof(Pagination).GetProperties(BindingFlags.Public | BindingFlags.Instance).Select(p => p.Name).ToArray();
            string[] columns = (from r in value.Columns.Cast<DataColumn>() where !pageProps.Contains(r.ColumnName) select r.ColumnName).ToArray();

            List<IDictionary<string, object>> rows = new List<IDictionary<string, object>>();
            IDictionary<string, object> row;
            object dbValue;
            IDictionary<string, object> firstRow = null;
            foreach (DataRow dRow in value.Rows) {
                row = new ExpandoObject();
                foreach (string col in columns) {
                    dbValue = dRow[col];
                    if (dbValue == DBNull.Value) { dbValue = null; }
                    row.Add(col, dbValue);
                }
                if (firstRow == null) { firstRow = row; }
                rows.Add(row);
            }

            if (firstRow != null) { //Add all of the first row's columns to all of the other rows
                var results = (from r1 in rows
                               let k = firstRow.Keys.Where(k => !r1.ContainsKey(k))
                               where k.Any()
                               select new { keys = k, row = r1 }).ToArray();
                foreach (var item in results) {
                    foreach (string k in item.keys) {
                        item.row.Add(k, null);
                    }
                }
            }
            return rows.ToArray();
        }

        private static void SetParameterValues(this SqlCommand command, IDictionary<string, object> parameter) {
            string propName;
            object value;
            string key;
            if (parameter != null && command.Parameters.Count > 0) {
                foreach (SqlParameter param in command.Parameters) {
                    propName = param.ParameterName.Substring(1);
                    key = (from k in parameter.Keys
                           where k.Equals(propName, StringComparison.OrdinalIgnoreCase)
                           select k).SingleOrDefault();
                    if (key.HasValue()) {
                        if (null != (value = parameter[key])) {
                            switch (param.SqlDbType) {
                                case SqlDbType.Int:
                                    param.Value = Convert.ToInt32(value);
                                    break;
                                case SqlDbType.BigInt:
                                    param.Value = Convert.ToInt64(value);
                                    break;
                                case SqlDbType.Bit:
                                    param.Value = Convert.ToBoolean(value);
                                    break;
                                case SqlDbType.Date:
                                case SqlDbType.DateTime:
                                    param.Value = Convert.ToDateTime(value);
                                    break;
                                default:
                                    param.Value = value;
                                    break;
                            }
                        } else {
                            param.Value = DBNull.Value;
                        }
                    }
                }
            }
        }

        private static void AddParameters(this SqlCommand command, string stroredProcedure) {
            Match match = stroredProcedure.Match(@"(?:([\w\[\]]+)\.([\w\[\]]+\.))?([\w\[\]]+$)");
            if (match.Success) {
                string dbName = match.Groups[1].Value;
                string spName = match.Groups[3].Value;
                if (command.Connection.State != ConnectionState.Open) {
                    command.Connection.Open();
                }

                command.CommandText = string.Format("USE {0}", dbName);
                command.CommandType = CommandType.Text;
                command.ExecuteNonQuery();
                command.CommandText = string.Format(@"
SELECT PARAMETER_NAME,DATA_TYPE,CHARACTER_MAXIMUM_LENGTH,PARAMETER_MODE
FROM information_schema.parameters
WHERE specific_name='{0}'", spName);
                ParameterDirection dir;
                using (SqlDataReader rdr = command.ExecuteReader()) {
                    while (rdr.Read()) {
                        switch (rdr["PARAMETER_MODE"].ToString()) {
                            case "OUT":
                                dir = ParameterDirection.Output;
                                break;
                            case "INOUT":
                                dir = ParameterDirection.InputOutput;
                                break;
                            default:
                                dir = ParameterDirection.Input;
                                break;
                        }

                        SqlDbType type = (SqlDbType)Enum.Parse(typeof(SqlDbType), rdr["DATA_TYPE"].ToString(), true);
                        int length = 0;
                        if (int.TryParse(rdr["CHARACTER_MAXIMUM_LENGTH"].ToString(), out length)) {
                            command.Parameters.Add(rdr["PARAMETER_NAME"].ToString(), type, length).Direction = dir;
                        } else {
                            command.Parameters.Add(rdr["PARAMETER_NAME"].ToString(), type).Direction = dir;
                        }
                    }
                    command.CommandText = stroredProcedure;
                    command.CommandType = CommandType.StoredProcedure;
                }
            }
        }

        private static SqlParameter newSqlParameter(string name, object value) {
            if (value != null) {
                return new SqlParameter(name, value);
            } else {
                return new SqlParameter(name, DBNull.Value);
            }
        }
        #endregion

        public static dynamic ExecuteStoredProcedure<T>(string procedureName, T sqlParams) {
            Type type = typeof(T);
            return ExecuteStoredProcedure(procedureName, sqlParams.ToDictionary(), false);
        }

        public static dynamic ExecuteStoredProcedure(string procedureName, IDictionary<string, object> sqlParams) {
            return ExecuteStoredProcedure(procedureName, sqlParams, false);
        }

        public static IEnumerable<T> Where<T>(Expression<Func<T, bool>> filter) where T: class {
            Type type = typeof(T);
            using (CRMEntities context = new CRMEntities()) {
                using (SqlConnection conn = new SqlConnection(context.Database.Connection.ConnectionString)) {
                    
                    DataContext dc = new DataContext(conn);
                    Table<T> table =  dc.GetTable<T>();
                    return table.Where(filter).AsEnumerable();
                }
            }
        }

        public static dynamic ExecuteStoredProcedure(string procedureName, IDictionary<string, object> sqlParams, bool getResults) {
            using (CRMEntities context = new CRMEntities()) {
                using (SqlConnection conn = new SqlConnection(context.Database.Connection.ConnectionString)) {
                    using (SqlCommand cmd = conn.CreateCommand()) {
                        cmd.AddParameters(procedureName);
                        cmd.SetParameterValues(sqlParams);
                        cmd.CommandTimeout = 300;
                        if (getResults) {
                            DataSet ds = new DataSet();
                            SqlDataAdapter adapter = new SqlDataAdapter(cmd);
                            adapter.Fill(ds);
                            IDictionary<string, object> retObj = new ExpandoObject();
                            switch (ds.Tables.Count) {
                                case 0:
                                    return null;
                                case 1:
                                    return ds.Tables[0].ToDynamic();
                                default:
                                    foreach (DataTable dt in ds.Tables) {
                                        retObj.Add(dt.TableName, dt.ToDynamic());
                                    }
                                    return retObj;
                            }
                        } else {
                            return cmd.ExecuteScalar();
                        }
                    }
                }
            }
        }

        public static dynamic[] ExecuteStoredProcedure<P>(string name, DateTime? startDate, DateTime? endDate, int? pageNum, int? maxRows, int? leadType, out P pageInfo) {
            pageInfo = default(P);
            using (CRMEntities context = new CRMEntities()) {
                //((IObjectContextAdapter)context).ObjectContext.CommandTimeout = int.MaxValue;

                using (SqlConnection conn = new SqlConnection(context.Database.Connection.ConnectionString)) {
                    using (SqlCommand cmd = conn.CreateCommand()) {
                        cmd.CommandTimeout = int.MaxValue;
                        cmd.CommandText = string.Format("sp_{0}", name);
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.Add(newSqlParameter("@FromDate", startDate));
                        cmd.Parameters.Add(newSqlParameter("@ToDate", endDate));
                        cmd.Parameters.Add(newSqlParameter("@PageNum", pageNum));
                        cmd.Parameters.Add(newSqlParameter("@MaxRows", maxRows));
                        cmd.Parameters.Add(newSqlParameter("@LeadType", leadType));
                        conn.Open();
                        using (SqlDataReader rdr = cmd.ExecuteReader()) {
                            string[] columns = Enumerable.Range(0, rdr.FieldCount).Select(rdr.GetName).ToArray();
                            List<dynamic> results = new List<dynamic>();
                            Type pageType = typeof(P);
                            //Type returnType = typeof(T);
                            PropertyInfo prop;
                            object value;
                            while (rdr.Read()) {
                                if (pageInfo == null) {
                                    pageInfo = (P)Activator.CreateInstance(typeof(P), rdr);
                                }
                                dynamic item = new ExpandoObject();
                                //T item = (T)Activator.CreateInstance(returnType);
                                foreach (string col in columns) {
                                    if ((prop = pageType.GetProperty(col)) == null) {
                                        value = rdr[col];
                                        if (value != DBNull.Value) {
                                            ((IDictionary<string, object>)item).Add(col, value);
                                            //prop.SetValue(item, value);
                                        } else {
                                            ((IDictionary<string, object>)item).Add(col, null);
                                            //prop.SetValue(item, null);
                                        }
                                    }
                                }
                                results.Add(item);
                            }
                            return results.ToArray();
                        }
                    }
                }
            }
        }
    }
}
