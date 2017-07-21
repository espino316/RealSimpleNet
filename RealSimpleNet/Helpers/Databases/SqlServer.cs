using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;

namespace RealSimpleNet.Helpers.Databases
{
    public class SqlServer
    {
        public static string connStr = "Data Source=@server;Initial Catalog=@db;Persist Security Info=True;User ID=@user;Password=@pwd";
        private static SqlConnection conn;
        private static SqlTransaction tran;
        private static bool IsTransaction = false;
        
        public static bool TryConnect(string connectionstring)
        {
            SqlConnection sqlcon = new SqlConnection();

            try
            {
                sqlcon.ConnectionString = connectionstring;
                sqlcon.Open();
                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                if (sqlcon.State == ConnectionState.Open) sqlcon.Close();
            }
        }

        public static void BeginTransaction()
        {
            IsTransaction = true;
            conn = new SqlConnection(connStr);
            conn.Open();
            tran = conn.BeginTransaction();
        }

        public static void CommitTransaction()
        {
            if (tran != null)
            {
                tran.Commit();
            }

            conn.Close();

            IsTransaction = false;
        }

        public static void RollBackTransaction()
        {
            if (tran != null)
            {
                tran.Rollback();
            }

            conn.Close();

            IsTransaction = false;
        }

        public static void CloseConnection()
        {
            if (conn.State == ConnectionState.Open)
                conn.Close();
        }

        private static string TableName(Type t)
        {
            string tipo = t.ToString();
            string[] separators = { "." };
            string[] results = tipo.Split(separators, StringSplitOptions.RemoveEmptyEntries);
            return results[results.Length - 1];
        }

        public static List<T> TableData<T>()
        {
            SqlCommand command = new SqlCommand();
            command.Connection = new SqlConnection(connStr);
            try
            {
                List<T> result = new List<T>();
                object instance;
                string tableName;
                tableName = SqlServer.TableName(typeof(T));

                instance = (T)Activator.CreateInstance(typeof(T));

                DataTable dt = SqlServer.Select(tableName);
                foreach (DataRow dr in dt.Rows)
                {
                    //instance = (T)Activator.CreateInstance(typeof(T));
                    foreach (DataColumn dc in dt.Columns)
                    {
                        object val = dr[dc.ColumnName];
                        if (Convert.IsDBNull(val)) val = null;
                        PropertyInfo pi = instance.GetType().GetProperty(dc.ColumnName);
                        pi.SetValue(instance, val, null);
                    }
                    result.Add((T)instance);
                }
                return result;
            }
            finally
            {
                command.Connection.Close();
                command.Dispose();
            }
        } // public static object QueryScalar

        public static List<T> TableDataReader<T>()
        {
            SqlCommand command = new SqlCommand();
            command.Connection = new SqlConnection(connStr);
            try
            {
                List<T> result = new List<T>();
                object instance;
                PropertyInfo[] props;
                string tableName;
                tableName = SqlServer.TableName(typeof(T));

                instance = (T)Activator.CreateInstance(typeof(T));
                props = typeof(T).GetProperties();

                if (command.Connection.State == ConnectionState.Closed) command.Connection.Open();
                command.CommandType = CommandType.Text;
                command.CommandText = SqlServer.SelectStatement(tableName);
                command.Parameters.Clear();

                SqlDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    instance = (T)Activator.CreateInstance(typeof(T));
                    foreach (PropertyInfo pi in props)
                    {
                        object val = reader[pi.Name];
                        if (Convert.IsDBNull(val)) val = null;
                        pi.SetValue(instance, val, null);
                    }
                    result.Add((T)instance);
                }
                return result;
            }
            finally
            {
                command.Connection.Close();
                command.Dispose();
            }
        } // end void

        public static IEnumerable<T> QueryList<T>(string sqlqry, string column)
        {
            DataTable dt = Query(sqlqry);
            List<T> list = new List<T>();

            foreach (DataRow dr in dt.Rows)
            {
                list.Add((T)dr[column]);
            }

            return list;
        }

        public static IEnumerable<T> QueryList<T>(string sqlqry, string column, params KeyValuePair<string, object>[] args)
        {
            DataTable dt = QueryCommand(sqlqry, GetParams(args));
            List<T> list = new List<T>();

            foreach (DataRow dr in dt.Rows)
            {
                list.Add((T)dr[column]);
            }

            return list;
        }

        public static byte[] PwdEncrypt(string pwd)
        {
            return (byte[])QueryScalar(String.Format("SELECT PWDENCRYPT('{0}')", pwd));
        }

        public static bool Exists(string tableName, params KeyValuePair<string, object>[] args)
        {
            Hashtable data = new Hashtable();
            foreach (KeyValuePair<string, object> kp in args)
            {
                data.Add(kp.Key, kp.Value);
            }

            if (GetCount(tableName, data) == 0) return false;
            return true;
        }

        public static bool IsNullOrEmpty(object expression)
        {
            if (expression == null || Convert.IsDBNull(expression))
            {
                return true;
            }
            else
            {
                if (expression.GetType() == typeof(string))
                {
                    if ((string)expression == "" || (string)expression == string.Empty)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public static KeyValuePair<string, object> Param(string key, object value)
        {
            return new KeyValuePair<string, object>(key, value);
        }

        public static T Select<T>(string sqlqry, params KeyValuePair<string, object>[] args)
        {
            return (T)QueryScalar(sqlqry, GetParams(args));
        }

        public static DataTable Read(string tableName, string filter, string sort, params KeyValuePair<string, object>[] args)
        {
            return QueryCommand(SelectStatement(tableName, filter, sort), GetParams(args));
        }

        public static DataTable Read(string tableName, int top, string filter, string sort, params KeyValuePair<string, object>[] args)
        {
            return QueryCommand(SelectStatement(tableName, top, filter, sort), GetParams(args));
        }

        public static DataTable Read(string tableName, params KeyValuePair<string, object>[] args)
        {
            Hashtable data = new Hashtable();
            foreach (KeyValuePair<string, object> kp in args)
            {
                data.Add(kp.Key, kp.Value);
            }
            return Select(tableName, data);
        }

        public static object ReadScalar(string sqlQry, params KeyValuePair<string, object>[] args)
        {
            Hashtable data = new Hashtable();
            foreach (KeyValuePair<string, object> kp in args)
            {
                data.Add(kp.Key, kp.Value);
            }

            return QueryScalar(sqlQry, data);
        }

        public static object ReadScalar(string tableName, string fieldName, params KeyValuePair<string, object>[] args)
        {
            Hashtable data = new Hashtable();
            foreach (KeyValuePair<string, object> kp in args)
            {
                data.Add(kp.Key, kp.Value);
            }

            return QueryScalar(SelectScalarStatement(tableName, fieldName, data), data);
        }

        public static object Ident_Current(string tableName)
        {
            string sqlstr = String.Format("SELECT IDENT_CURRENT('{0}');", tableName);
            return QueryScalar(sqlstr);
        }

        public static DateTime GetDate()
        {
            string sqlstr = "SELECT GETDATE()";
            return (DateTime)QueryScalar(sqlstr);
        }

        public static T QueryScalar<T>(string sqlQry, params KeyValuePair<string, object>[] @params)
        {
            SqlCommand command = new SqlCommand();
            command.Connection = new SqlConnection(connStr);
            try
            {
                object result;
                if (command.Connection.State == ConnectionState.Closed) command.Connection.Open();
                command.CommandType = CommandType.Text;
                command.CommandText = sqlQry;
                command.Parameters.Clear();

                foreach (KeyValuePair<string, object> kvp in @params)
                {
                    command.Parameters.AddWithValue(kvp.Key, kvp.Value);
                }

                result = command.ExecuteScalar();
                return (T)result;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                command.Connection.Close();
                command.Dispose();
            }
        } // public static object QueryScalar

        public static object QueryScalar(string sqlQry, params KeyValuePair<string, object>[] @params)
        {
            SqlCommand command = new SqlCommand();

            if (IsTransaction)
            {
                command.Connection = conn; command.Transaction = tran;
            }
            else
            {
                command.Connection = new SqlConnection(connStr);
            }

            try
            {
                object result;

                if (command.Connection.State == ConnectionState.Closed) if (command.Connection.State == ConnectionState.Closed) command.Connection.Open();
                command.CommandType = CommandType.Text;
                command.CommandText = sqlQry;
                command.Parameters.Clear();

                foreach (KeyValuePair<string, object> kvp in @params)
                {
                    command.Parameters.AddWithValue(kvp.Key, kvp.Value);
                }

                result = command.ExecuteScalar();
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (!IsTransaction)
                {
                    command.Connection.Close();
                }
                command.Dispose();
            }
        } // public static object QueryScalar

        public static object QueryScalar(string sqlQry, Hashtable @params)
        {
            SqlCommand command = new SqlCommand();

            if (IsTransaction)
            {
                command.Connection = conn; command.Transaction = tran;
            }
            else
            {
                command.Connection = new SqlConnection(connStr);
            }

            try
            {
                object result;
                if (command.Connection.State == ConnectionState.Closed) command.Connection.Open();
                command.CommandType = CommandType.Text;
                command.CommandText = sqlQry;
                command.Parameters.Clear();
                foreach (string k in @params.Keys)
                {
                    command.Parameters.AddWithValue(k, @params[k]);
                }
                result = command.ExecuteScalar();
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (!IsTransaction)
                {
                    command.Connection.Close();
                }
                command.Dispose();
            }
        } // public static object QueryScalar

        public static object QueryScalar(string sqlQry)
        {
            SqlCommand command = new SqlCommand();

            if (IsTransaction)
            {
                command.Connection = conn; command.Transaction = tran;
            }
            else
            {
                command.Connection = new SqlConnection(connStr);
            }

            try
            {
                if (command.Connection.State == ConnectionState.Closed) command.Connection.Open();
                command.CommandType = CommandType.Text;
                command.CommandText = sqlQry;

                Object Result = command.ExecuteScalar();
                return Result;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (!IsTransaction)
                {
                    command.Connection.Close();
                }
                command.Dispose();
            }
        } // public static object QueryScalar

        public static IDataReader QueryReader(string query)
        {
            SqlCommand command = new SqlCommand();

            if (IsTransaction)
            {
                command.Connection = conn; command.Transaction = tran;
            }
            else
            {
                command.Connection = new SqlConnection(connStr);
            }

            try
            {
                if (command.Connection.State == ConnectionState.Closed) command.Connection.Open();
                command.CommandType = CommandType.Text;
                command.CommandText = query;
                IDataReader reader;
                reader = command.ExecuteReader();
                reader.Read();
                return reader;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (!IsTransaction)
                {
                    command.Connection.Close();
                }
                command.Dispose();
            }
        } // public static DataTable Query

        public static SqlDataReader QueryReader(string m_command, Hashtable m_params)
        {
            SqlCommand command = new SqlCommand();

            if (IsTransaction)
            {
                command.Connection = conn; command.Transaction = tran;
            }
            else
            {
                command.Connection = new SqlConnection(connStr);
            }

            try
            {
                if (command.Connection.State == ConnectionState.Closed) command.Connection.Open();
                command.CommandType = CommandType.Text;
                command.CommandText = m_command;
                command.Parameters.Clear();

                foreach (string key in m_params.Keys)
                {
                    if (m_params[key] == null)
                    {
                        command.Parameters.AddWithValue(key, DBNull.Value);
                    }
                    else
                    {
                        command.Parameters.AddWithValue(key, m_params[key]);
                    }
                }

                return command.ExecuteReader();
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (!IsTransaction)
                {
                    command.Connection.Close();
                }
                command.Dispose();
            }
        } // public static DataTable QueryCommand

        public static DataTable Query(string query)
        {
            SqlCommand command = new SqlCommand();

            if (IsTransaction)
            {
                command.Connection = conn; command.Transaction = tran;
            }
            else
            {
                command.Connection = new SqlConnection(connStr);
            }

            try
            {
                if (command.Connection.State == ConnectionState.Closed) command.Connection.Open();
                command.CommandType = CommandType.Text;
                command.CommandText = query;

                DataSet ds = new DataSet();
                SqlDataAdapter da = new SqlDataAdapter(command);
                da.Fill(ds, "SICASDATA");

                return ds.Tables[0];
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (!IsTransaction)
                {
                    command.Connection.Close();
                }
                command.Dispose();
            }
        } // public static DataTable Query


        public static DataTable QueryCommand(string m_command, Hashtable m_params)
        {
            SqlCommand command = new SqlCommand();

            if (IsTransaction)
            {
                command.Connection = conn; command.Transaction = tran;
            }
            else
            {
                command.Connection = new SqlConnection(connStr);
            }

            try
            {
                if (command.Connection.State == ConnectionState.Closed) command.Connection.Open();
                command.CommandType = CommandType.Text;
                command.CommandText = m_command;
                command.Parameters.Clear();

                foreach (string key in m_params.Keys)
                {
                    if (m_params[key] == null)
                    {
                        command.Parameters.AddWithValue(key, DBNull.Value);
                    }
                    else
                    {
                        command.Parameters.AddWithValue(key, m_params[key]);
                    }
                }

                DataSet ds = new DataSet();
                SqlDataAdapter da = new SqlDataAdapter(command);
                da.Fill(ds, "SICASDATA");

                return ds.Tables[0];
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (!IsTransaction)
                {
                    command.Connection.Close();
                }
                command.Dispose();
            }
        } // public static DataTable QueryCommand

        public static void ExecuteQuery(string execQuery)
        {
            SqlCommand command = new SqlCommand();

            if (IsTransaction)
            {
                command.Connection = conn; command.Transaction = tran;
            }
            else
            {
                command.Connection = new SqlConnection(connStr);
            }

            try
            {
                if (command.Connection.State == ConnectionState.Closed) command.Connection.Open();
                command.CommandType = CommandType.Text;
                command.CommandText = execQuery;
                command.ExecuteNonQuery();
                command.Dispose();
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (!IsTransaction)
                {
                    command.Connection.Close();
                }
                command.Dispose();
            }
        } // public static void ExecuteQuery

        public static void ExecuteCommand(string execQuery, Hashtable m_params)
        {
            SqlCommand command = new SqlCommand();

            if (IsTransaction)
            {
                command.Connection = conn; command.Transaction = tran;
            }
            else
            {
                command.Connection = new SqlConnection(connStr);
            }

            try
            {
                if (command.Connection.State == ConnectionState.Closed) command.Connection.Open();
                command.CommandType = CommandType.Text;
                command.CommandText = execQuery;
                command.Parameters.Clear();

                foreach (string key in m_params.Keys)
                {
                    command.Parameters.AddWithValue(key, m_params[key]);
                }
                command.ExecuteNonQuery();
                command.Dispose();
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (!IsTransaction)
                {
                    command.Connection.Close();
                }
                command.Dispose();
            }
        } // public static void ExecuteCommand

        #region ActiveRecord

        public static int IdentityInsertRow(string strTabla, Hashtable @params)
        {
            string strSQL = String.Format("SET IDENTITY_INSERT {0} ON \r\n INSERT INTO {0} (", strTabla);
            int cont = 0;
            foreach (string k in @params.Keys)
            {
                if (cont == 0)
                {
                    strSQL += k;
                }
                else
                {
                    strSQL += "," + k;
                }
                cont += 1;
            }

            strSQL += ") VALUES (";
            cont = 0;

            foreach (string k in @params.Keys)
            {
                if (cont == 0)
                {
                    strSQL += "@" + k;
                }
                else
                {
                    strSQL += ", @" + k;
                }
                cont += 1;
            }

            strSQL += String.Format(") \r\n SET IDENTITY_INSERT {0} OFF", strTabla);

            SqlCommand command = new SqlCommand();

            if (IsTransaction)
            {
                command.Connection = conn; command.Transaction = tran;
            }
            else
            {
                command.Connection = new SqlConnection(connStr);
            }

            if (command.Connection.State == ConnectionState.Closed) command.Connection.Open();
            command.Parameters.Clear();
            command.CommandType = CommandType.Text;
            command.CommandText = strSQL;

            foreach (string k in @params.Keys)
            {
                if (@params[k] == null)
                {
                    command.Parameters.AddWithValue("@" + k, DBNull.Value);
                }
                else
                {
                    command.Parameters.AddWithValue("@" + k, @params[k]);
                }
            }

            try
            {
                return command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (!IsTransaction)
                {
                    command.Connection.Close();
                }
                command.Dispose();
            }
        } // public static int InsertRow

        public static int InsertRow(string strTabla, Hashtable @params)
        {
            string strSQL = "INSERT INTO " + strTabla + " (";
            int cont = 0;
            foreach (string k in @params.Keys)
            {
                if (cont == 0)
                {
                    strSQL += k;
                }
                else
                {
                    strSQL += "," + k;
                }
                cont += 1;
            }

            strSQL += ") VALUES (";
            cont = 0;

            foreach (string k in @params.Keys)
            {
                if (cont == 0)
                {
                    strSQL += "@" + k;
                }
                else
                {
                    strSQL += ", @" + k;
                }
                cont += 1;
            }

            strSQL += ")";

            SqlCommand command = new SqlCommand();

            if (IsTransaction)
            {
                command.Connection = conn; command.Transaction = tran;
            }
            else
            {
                command.Connection = new SqlConnection(connStr);
            }

            if (command.Connection.State == ConnectionState.Closed) command.Connection.Open();
            command.Parameters.Clear();
            command.CommandType = CommandType.Text;
            command.CommandText = strSQL;

            foreach (string k in @params.Keys)
            {
                if (@params[k] == null)
                {
                    command.Parameters.AddWithValue("@" + k, DBNull.Value);
                }
                else
                {
                    command.Parameters.AddWithValue("@" + k, @params[k]);
                }
            }

            try
            {
                return command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (!IsTransaction)
                {
                    command.Connection.Close();
                }
                command.Dispose();
            }
        } // public static int InsertRow

        public static DataTable Select(string tablename)
        {
            SqlCommand command = new SqlCommand();

            if (IsTransaction)
            {
                command.Connection = conn; command.Transaction = tran;
            }
            else
            {
                command.Connection = new SqlConnection(connStr);
            }

            try
            {
                if (command.Connection.State == ConnectionState.Closed) command.Connection.Open();
                command.CommandType = CommandType.Text;
                command.CommandText = "SELECT * FROM " + tablename;
                command.Parameters.Clear();

                DataSet ds = new DataSet();
                SqlDataAdapter da = new SqlDataAdapter(command);
                da.Fill(ds, "SICASDATA");

                return ds.Tables[0];
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (!IsTransaction)
                {
                    command.Connection.Close();
                }
                command.Dispose();
            }
        }

        public static DataTable Select(string tablename, Hashtable w_params)
        {
            SqlCommand command = new SqlCommand();

            if (IsTransaction)
            {
                command.Connection = conn; command.Transaction = tran;
            }
            else
            {
                command.Connection = new SqlConnection(connStr);
            }

            try
            {
                if (command.Connection.State == ConnectionState.Closed) command.Connection.Open();
                command.CommandType = CommandType.Text;
                command.CommandText = SelectStatement(tablename, w_params);
                command.Parameters.Clear();

                foreach (string key in w_params.Keys)
                {
                    if (w_params[key] == null)
                    {
                        command.Parameters.AddWithValue(key, DBNull.Value);
                    }
                    else
                    {
                        command.Parameters.AddWithValue(key, w_params[key]);
                    }
                }

                DataSet ds = new DataSet();
                SqlDataAdapter da = new SqlDataAdapter(command);
                da.Fill(ds, "SICASDATA");

                return ds.Tables[0];
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (!IsTransaction)
                {
                    command.Connection.Close();
                }
                command.Dispose();
            }
        }

        public static int GetCount(string tablename, Hashtable w_params)
        {
            SqlCommand command = new SqlCommand();

            if (IsTransaction)
            {
                command.Connection = conn; command.Transaction = tran;
            }
            else
            {
                command.Connection = new SqlConnection(connStr);
            }

            try
            {
                if (command.Connection.State == ConnectionState.Closed) command.Connection.Open();
                command.CommandType = CommandType.Text;
                command.CommandText = SelectCountStatement(tablename, w_params);
                command.Parameters.Clear();

                foreach (string key in w_params.Keys)
                {
                    command.Parameters.AddWithValue(key, w_params[key]);
                }

                Object Result = command.ExecuteScalar();
                return (int)Result;
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (!IsTransaction)
                {
                    command.Connection.Close();
                }
                command.Dispose();
            }
        }

        public static string SelectStatement(string strTabla, int top, string filter, string sort)
        {
            string strSQL = "";

            strSQL += "SELECT TOP " + top.ToString() + " * FROM " + strTabla + " WHERE " + filter;

            if (!String.IsNullOrEmpty(sort)) strSQL += " ORDER BY " + sort;

            return strSQL;

        } // End SelectStatement

        public static string SelectStatement(string strTabla, Hashtable @params, Hashtable @whereParams)
        {
            int cont;
            string strSQL = "";
            strSQL += "SELECT ";
            cont = 0;

            foreach (string k in @params.Keys)
            {
                if (cont == 0)
                {
                    strSQL += k;
                }
                else
                {
                    strSQL += ", " + k;
                }
                cont += 1;
            }

            cont = 0;
            strSQL += " FROM " + strTabla + " WHERE ";
            foreach (string k in @whereParams.Keys)
            {
                if (cont == 0)
                {
                    strSQL += k + " = @" + k;
                }
                else
                {
                    strSQL += " AND " + k + " = @" + k;
                }
                cont += 1;
            }

            return strSQL;
        } // End SelectStatement

        public static string SelectStatement(string strTabla)
        {
            string strSQL = "";
            strSQL += "SELECT * FROM " + strTabla;

            return strSQL;
        } // End SelectStatement

        public static string SelectStatement(string strTabla, string filter, string sort)
        {
            string strSQL = "";
            strSQL += "SELECT * FROM " + strTabla;
            if (!String.IsNullOrEmpty(filter)) strSQL += " WHERE " + filter;
            if (!String.IsNullOrEmpty(sort)) strSQL += " ORDER BY " + sort;

            return strSQL;
        } // End SelectStatement

        public static string SelectScalarStatement(string strTabla, string strCampo, Hashtable @whereParams)
        {
            int cont;
            string strSQL = "";
            strSQL += "SELECT " + strCampo + " FROM " + strTabla + " WHERE ";
            cont = 0;

            foreach (string k in @whereParams.Keys)
            {
                if (cont == 0)
                {
                    strSQL += k + " = @" + k;
                }
                else
                {
                    strSQL += " AND " + k + " = @" + k;
                }
                cont += 1;
            }

            return strSQL;
        } // End SelectStatement

        public static string SelectCountStatement(string strTabla, Hashtable @whereParams)
        {
            int cont;
            string strSQL = "";
            strSQL += "SELECT COUNT(*) FROM " + strTabla + " WHERE ";
            cont = 0;

            foreach (string k in @whereParams.Keys)
            {
                if (cont == 0)
                {
                    strSQL += k + " = @" + k;
                }
                else
                {
                    strSQL += " AND " + k + " = @" + k;
                }
                cont += 1;
            }

            return strSQL;
        } // End SelectStatement

        public static string SelectStatement(string strTabla, Hashtable @whereParams)
        {
            int cont;
            string strSQL = "";
            strSQL += "SELECT * FROM " + strTabla + " WHERE ";
            cont = 0;

            foreach (string k in @whereParams.Keys)
            {
                if (cont == 0)
                {
                    strSQL += k + " = @" + k;
                }
                else
                {
                    strSQL += " AND " + k + " = @" + k;
                }
                cont += 1;
            }

            return strSQL;
        } // End SelectStatement

        public static int DeleteRow(string strTabla, Hashtable @whereParams)
        {
            string strSQL = "DELETE FROM " + strTabla + " ";
            int cont = 0;

            strSQL += " WHERE ";

            foreach (string k in @whereParams.Keys)
            {
                if (cont == 0)
                {
                    strSQL += k + " = @" + k;
                }
                else
                {
                    strSQL += " AND " + k + " = @" + k;
                }
                cont += 1;
            }

            SqlCommand command = new SqlCommand();

            if (IsTransaction)
            {
                command.Connection = conn; command.Transaction = tran;
            }
            else
            {
                command.Connection = new SqlConnection(connStr);
            }

            if (command.Connection.State == ConnectionState.Closed) command.Connection.Open();
            command.Parameters.Clear();
            command.CommandType = CommandType.Text;
            command.CommandText = strSQL;

            foreach (string k in @whereParams.Keys)
            {
                if (@whereParams[k] == null)
                {
                    command.Parameters.AddWithValue("@" + k, DBNull.Value);
                }
                else
                {
                    command.Parameters.AddWithValue("@" + k, @whereParams[k]);
                }
            }

            try
            {
                return command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (!IsTransaction)
                {
                    command.Connection.Close();
                }
                command.Dispose();
            }
        } // public static int DeleteRow

        public static int UpdateRow(string strTabla, Hashtable @params)
        {
            string strSQL = "UPDATE " + strTabla + " SET ";
            int cont = 0;
            foreach (string k in @params.Keys)
            {
                if (cont == 0)
                {
                    strSQL += k + " = @" + k;
                }
                else
                {
                    strSQL += ", " + k + " = @" + k;
                }
                cont += 1;
            }

            SqlCommand command = new SqlCommand();

            if (IsTransaction)
            {
                command.Connection = conn; command.Transaction = tran;
            }
            else
            {
                command.Connection = new SqlConnection(connStr);
            }

            if (command.Connection.State == ConnectionState.Closed) command.Connection.Open();
            command.Parameters.Clear();
            command.CommandType = CommandType.Text;
            command.CommandText = strSQL;

            foreach (string k in @params.Keys)
            {
                if (@params[k] == null)
                {
                    command.Parameters.AddWithValue("@" + k, DBNull.Value);
                }
                else
                {
                    command.Parameters.AddWithValue("@" + k, @params[k]);
                }
            }

            try
            {
                return command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (!IsTransaction)
                {
                    command.Connection.Close();
                }
                command.Dispose();
            }
        } // int UpdateRow


        public static int UpdateRow(string strTabla, Hashtable @params, Hashtable @whereParams)
        {

            string strSQL = "UPDATE " + strTabla + " SET ";
            int cont = 0;
            foreach (string k in @params.Keys)
            {
                if (cont == 0)
                {
                    strSQL += k + " = @" + k;
                }
                else
                {
                    strSQL += ", " + k + " = @" + k;
                }
                cont += 1;
            }

            strSQL += " WHERE ";
            cont = 0;

            foreach (string k in @whereParams.Keys)
            {
                if (cont == 0)
                {
                    strSQL += k + " = @" + k;
                }
                else
                {
                    strSQL += " AND " + k + " = @" + k;
                }
                cont += 1;
            }

            SqlCommand command = new SqlCommand();

            if (IsTransaction)
            {
                command.Connection = conn; command.Transaction = tran;
            }
            else
            {
                command.Connection = new SqlConnection(connStr);
            }

            if (command.Connection.State == ConnectionState.Closed) command.Connection.Open();
            command.Parameters.Clear();
            command.CommandType = CommandType.Text;
            command.CommandText = strSQL;

            foreach (string k in @params.Keys)
            {
                if (@params[k] == null)
                {
                    command.Parameters.AddWithValue("@" + k, DBNull.Value);
                }
                else
                {
                    command.Parameters.AddWithValue("@" + k, @params[k]);
                }
            }

            foreach (string k in @whereParams.Keys)
            {
                if (@whereParams[k] == null)
                {
                    command.Parameters.AddWithValue("@" + k, DBNull.Value);
                }
                else
                {
                    command.Parameters.AddWithValue("@" + k, @whereParams[k]);
                }
            }

            try
            {
                return command.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                if (!IsTransaction)
                {
                    command.Connection.Close();
                }
                command.Dispose();
            }
        } // int UpdateRow

        #endregion

        #region HasthToStr
        public static string GetStrHash(Hashtable m_params)
        {
            string result = "";
            foreach (string k in m_params.Keys)
            {
                if (result == "")
                {
                    result += k + ":    " + m_params[k];
                }
                else
                {
                    result += "\r\n" + k + ":    " + m_params[k];
                }

            }
            return result;
        }

        public static Hashtable Params;

        public static Hashtable GetParams(params KeyValuePair<string, object>[] args)
        {
            Hashtable m_params = new Hashtable();

            if (args != null)
            {
                foreach (KeyValuePair<string, object> kvp in args)
                {
                    m_params.Add(kvp.Key, kvp.Value);
                }
            }

            return m_params;
        }
        #endregion

        /// <summary>
        /// Regresa un entero nulable a partir de una expresión evaluada
        /// </summary>
        /// <param name="expression">La expresión a evaluar</param>
        /// <returns>int?</returns>
        public static int? GetNullableInt32(object expression)
        {
            if (expression == null || Convert.IsDBNull(expression))
            {
                return null;
            }
            else
            {
                if (expression.GetType() == typeof(string))
                {
                    if (string.IsNullOrEmpty((string)expression))
                    {
                        return null;
                    }
                }

                if (!IsNumeric(expression))
                {
                    throw new Exception(String.Format("{0} no es numérica", expression));
                }
                return Convert.ToInt32(expression);
            }
        }

        /// <summary>
        /// Determina si una cadena es numerica
        /// </summary>
        /// <param name="valor">El valor a evaluar</param>
        /// <returns>bool</returns>
        public static bool IsNumeric(object valor)
        {
            Decimal d;
            return Decimal.TryParse(valor.ToString(), out d);
        }

        public static DateTime? GetNullableDateTime(object expression)
        {
            if (expression == null || Convert.IsDBNull(expression))
            {
                return null;
            }
            else
            {
                if (expression.GetType() == typeof(string))
                {
                    if (string.IsNullOrEmpty((string)expression))
                    {
                        return null;
                    }
                }
                return Convert.ToDateTime(expression);
            }
        }

        public static decimal? GetNullableDecimal(object expression)
        {
            if (expression == null || Convert.IsDBNull(expression))
            {
                return null;
            }
            else
            {
                if (expression.GetType() == typeof(string))
                {
                    if (string.IsNullOrEmpty((string)expression))
                    {
                        return null;
                    }
                }
                if (!IsNumeric(expression))
                {
                    throw new Exception(String.Format("{0} no es numérica", expression));
                }
                return Convert.ToDecimal(expression);
            }
        }

        public static Boolean? GetNullableBoolean(object expression)
        {
            if (expression == null || Convert.IsDBNull(expression))
            {
                return null;
            }
            else
            {
                if (expression.GetType() == typeof(string))
                {
                    if (string.IsNullOrEmpty((string)expression))
                    {
                        return null;
                    }
                }
                return Convert.ToBoolean(expression);
            }
        }
    } //  End Class DB
} // end namespace
