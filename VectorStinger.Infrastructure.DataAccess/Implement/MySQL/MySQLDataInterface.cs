using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using MySqlConnector;

namespace VectorStinger.Infrastructure.DataAccess.Implement.MySQL
{
    internal class MySQLDataInterface
    {
        public string ConnectionString { get; set; }
        private MySqlCommand gCommand { get; set; }
        private MySqlConnection gConnection { get; set; }

        public List<T> GetListByProcedure<T>(string nameProcedure, params object[] parameters) where T : class, new()
        {
            List<T> vListado = new List<T>();

            gCommand = new MySqlCommand();
            gCommand.CommandText = nameProcedure;
            gCommand.Connection = gConnection;
            gCommand.CommandText = "call " + nameProcedure + "(";
            string nomParametro = "parametro";
            int a = 0;
            OpenConnection();
            foreach (object item in parameters)
            {


                gCommand.CommandText += "@" + nomParametro + a.ToString();

                MySqlParameter parametro = null;

                if (item == null)
                {
                    parametro = new MySqlParameter
                    {
                        ParameterName = "@" + nomParametro + a.ToString(),
                        Value = DBNull.Value,
                        Direction = ParameterDirection.Input
                    };
                    a++;
                    gCommand.CommandText += ",";
                    gCommand.Parameters.Add(parametro);
                    continue;
                }

                parametro = new MySqlParameter
                {
                    ParameterName = "@" + nomParametro + a.ToString(),
                    Direction = ParameterDirection.Input
                };

                string typeParameter = item.GetType().Name;
                if (item is IEnumerable)
                {
                    parametro.Value = item;
                }
                else
                {
                    switch (typeParameter)
                    {
                        case "Int32":
                            parametro.MySqlDbType = MySqlDbType.Int32;
                            parametro.Value = item;
                            break;

                        case "Int64":
                            parametro.MySqlDbType = MySqlDbType.Int64;
                            parametro.Value = item;
                            break;

                        case "Decimal":
                            parametro.MySqlDbType = MySqlDbType.Decimal;
                            parametro.Value = item;
                            break;

                        case "String":
                            parametro.MySqlDbType = MySqlDbType.VarChar;
                            parametro.Value = item;
                            break;

                        case "Boolean":
                            parametro.MySqlDbType = MySqlDbType.Bit;
                            parametro.Value = Convert.ToBoolean(item);
                            break;

                        case "DateTime":
                            parametro.MySqlDbType = MySqlDbType.DateTime;
                            parametro.Value = item;
                            break;
                        case "Byte[]":
                            parametro.MySqlDbType = MySqlDbType.Byte;
                            parametro.Value = item;
                            break;

                        default:
                            break;
                    }

                }

                gCommand.CommandText += ",";
                a++;
                gCommand.Parameters.Add(parametro);
            }
            if (parameters.Length > 0)
            {
                gCommand.CommandText = gCommand.CommandText.Substring(0, gCommand.CommandText.Length - 1);
            }
            gCommand.CommandText += "); ";
            IDataReader dr = GetList(gCommand);

            while (dr.Read())
            {
                vListado.Add(FactoryEntity<T>(ref dr));
            }
            DisposeReader(ref dr);
            DisposeConnection();


            return vListado;
        }

        public void CallProcedure<T>(string nameProcedure, params object[] parameters) where T : class, new()
        {
            gCommand = new MySqlCommand();
            gCommand.CommandText = nameProcedure;
            gCommand.Connection = gConnection;
            gCommand.CommandText = "call " + nameProcedure + "(";
            string nomParametro = "parametro";
            int a = 0;
            OpenConnection();
            foreach (object item in parameters)
            {


                gCommand.CommandText += nomParametro + a.ToString();

                MySqlParameter parametro = null;

                if (item == null)
                {
                    parametro = new MySqlParameter
                    {
                        ParameterName = nomParametro + a.ToString(),
                        Value = DBNull.Value,
                        Direction = ParameterDirection.Input
                    };
                    a++;
                    gCommand.CommandText += ",";
                    gCommand.Parameters.Add(parametro);
                    continue;
                }

                parametro = new MySqlParameter
                {
                    ParameterName = nomParametro + a.ToString(),
                    Direction = ParameterDirection.Input
                };

                string typeParameter = item.GetType().Name;
                if (item is IEnumerable)
                {
                    parametro.Value = item;
                }
                else
                {
                    switch (typeParameter)
                    {
                        case "Int32":
                            parametro.MySqlDbType = MySqlDbType.Int32;
                            parametro.Value = item;
                            break;

                        case "Int64":
                            parametro.MySqlDbType = MySqlDbType.Int64;
                            parametro.Value = item;
                            break;

                        case "Decimal":
                            parametro.MySqlDbType = MySqlDbType.Decimal;
                            parametro.Value = item;
                            break;

                        case "String":
                            parametro.MySqlDbType = MySqlDbType.VarChar;
                            parametro.Value = item;
                            break;

                        case "Boolean":
                            parametro.MySqlDbType = MySqlDbType.Bit;
                            parametro.Value = Convert.ToBoolean(item);
                            break;

                        case "DateTime":
                            parametro.MySqlDbType = MySqlDbType.DateTime;
                            parametro.Value = item;
                            break;
                        case "Byte[]":
                            parametro.MySqlDbType = MySqlDbType.Byte;
                            parametro.Value = item;
                            break;

                        default:
                            break;
                    }

                }

                gCommand.CommandText += ",";
                a++;
                gCommand.Parameters.Add(parametro);
            }
            if (parameters.Length > 0)
            {
                gCommand.CommandText = gCommand.CommandText.Substring(0, gCommand.CommandText.Length - 1);
            }
            gCommand.CommandText += "); ";
            gCommand.ExecuteNonQuery();
            DisposeConnection();
        }

        public List<T> SimpleSelect<T>((string, object) parameterFilter) where T : class, new()
        {
            T seed = new T();
            List<T> resul = new List<T>();
            string comandSQL = $@"Select * from {seed.GetType().Name}";
            gCommand = new MySqlCommand();

            if (!string.IsNullOrEmpty(parameterFilter.Item1))
            {
                comandSQL += " where ";
                comandSQL += $" {parameterFilter.Item1} = @param{gCommand.Parameters.Count.ToString()}";
                AddParameter(parameterFilter.Item2, gCommand);
            }
            gCommand.CommandText = comandSQL;

            IDataReader dr = GetList(gCommand);

            while (dr.Read())
            {
                resul.Add(FactoryEntity<T>(ref dr));
            }
            DisposeReader(ref dr);
            DisposeConnection();
            return resul;
        }

        private IDataReader GetList(MySqlCommand pCommand)
        {
            IDataReader vDataReader = null;
            MySqlCommand vCommand = pCommand;
            OpenConnection();
            vCommand.Connection = gConnection;

            try
            {
                vDataReader = vCommand.ExecuteReader();
            }
            catch
            {
                throw;
            }
            return vDataReader;
        }

        private void OpenConnection()
        {
            try
            {
                if (gConnection != null)
                {
                    if (gConnection.State == ConnectionState.Closed)
                    {
                        gConnection.Open();
                    }
                }
                else
                {
                    gConnection = new MySqlConnection();
                    gConnection.ConnectionString = ConnectionString;
                    gConnection.Open();
                }
            }
            catch
            {
                throw;
            }
        }

        private void DisposeConnection()
        {
            try
            {
                if (gConnection != null)
                {
                    if (gConnection.State != ConnectionState.Closed)
                        gConnection.Close();
                }
            }
            catch
            {
                throw;
            }
        }

        private void DisposeReader(ref IDataReader pDataReader)
        {
            try
            {
                if (pDataReader != null)
                {
                    if (!pDataReader.IsClosed)
                        pDataReader.Close();
                    pDataReader.Dispose();
                }
            }
            catch
            {
                throw;
            }
        }

        private T FactoryEntity<T>(ref IDataReader pDataReader) where T : class, new()
        {
            T vEntity = new T();
            var vPropiedades = vEntity.GetType().GetProperties();

            try
            {
                PropertyInfo[] propiedades = new PropertyInfo[vEntity.GetType().GetProperties().Count()];
                vEntity.GetType().GetProperties().CopyTo(propiedades, 0);
                //propiedades = propiedades.Where(a => !typeof(Entity).GetProperties().Any(b => b.Name == a.Name)).ToArray();                
                DataTable dtSchema = pDataReader.GetSchemaTable();

                foreach (PropertyInfo item in propiedades)
                {
                    if (!dtSchema.AsEnumerable().Any(a => a.Field<string>("ColumnName").ToLower().Equals(item.Name.ToLower())))
                    {
                        continue;
                    }
                    if (pDataReader[item.Name] == null ||
                        pDataReader[item.Name] == DBNull.Value)
                    {
                        vEntity.GetType().GetProperty(item.Name).SetValue(vEntity, null, null);
                    }
                    else if (vEntity.GetType().GetProperty(item.Name).PropertyType.FullName.Contains("Int32"))
                    {
                        vEntity.GetType().GetProperty(item.Name).SetValue(vEntity, Convert.ToInt32(pDataReader[item.Name].ToString()), null);
                    }
                    else if (vEntity.GetType().GetProperty(item.Name).PropertyType.FullName.Contains("Int64"))
                    {
                        vEntity.GetType().GetProperty(item.Name).SetValue(vEntity, Convert.ToInt64(pDataReader[item.Name].ToString()), null);
                    }
                    else if (vEntity.GetType().GetProperty(item.Name).PropertyType.FullName.Contains("Decimal"))
                    {
                        vEntity.GetType().GetProperty(item.Name).SetValue(vEntity, Convert.ToDecimal(pDataReader[item.Name]), null);
                    }
                    else if (vEntity.GetType().GetProperty(item.Name).PropertyType.FullName.Contains("Single"))
                    {
                        vEntity.GetType().GetProperty(item.Name).SetValue(vEntity, Convert.ToSingle(pDataReader[item.Name]), null);
                    }
                    else if (vEntity.GetType().GetProperty(item.Name).PropertyType.FullName.Contains("StringBuilder"))
                    {
                        vEntity.GetType().GetProperty(item.Name).SetValue(vEntity, new StringBuilder(pDataReader[item.Name].ToString()), null);
                    }
                    else if (vEntity.GetType().GetProperty(item.Name).PropertyType.FullName.Contains("String"))
                    {
                        vEntity.GetType().GetProperty(item.Name).SetValue(vEntity, pDataReader[item.Name].ToString(), null);
                    }
                    else if (vEntity.GetType().GetProperty(item.Name).PropertyType.FullName.Contains("Boolean"))
                    {
                        vEntity.GetType().GetProperty(item.Name).SetValue(vEntity, Convert.ToBoolean(pDataReader[item.Name]), null);
                    }
                    else if (vEntity.GetType().GetProperty(item.Name).PropertyType.FullName.Contains("DateTime"))
                    {
                        vEntity.GetType().GetProperty(item.Name).SetValue(vEntity, Convert.ToDateTime(pDataReader[item.Name]), null);
                    }
                    else if (vEntity.GetType().GetProperty(item.Name).PropertyType.FullName.Contains("Byte[]"))
                    {
                        vEntity.GetType().GetProperty(item.Name).SetValue(vEntity, (byte[])pDataReader[item.Name], null);
                    }

                }
            }
            catch
            {
                throw;
            }

            return vEntity;
        }

        private void AddParameter(object item, MySqlCommand mySqlCommand)
        {
            MySqlParameter parametro = null;

            if (item == null)
            {
                gCommand.CommandText += ",";
                gCommand.Parameters.Add(parametro);
            }

            parametro = new MySqlParameter
            {
                ParameterName = "@param" + mySqlCommand.Parameters.Count.ToString(),
                Direction = ParameterDirection.Input
            };

            string typeParameter = item.GetType().Name;
            if (item is IEnumerable)
            {
                parametro.Value = item;
            }
            else
            {
                switch (typeParameter)
                {
                    case "Int32":
                        parametro.MySqlDbType = MySqlDbType.Int32;
                        parametro.Value = item;
                        break;

                    case "Int64":
                        parametro.MySqlDbType = MySqlDbType.Int64;
                        parametro.Value = item;
                        break;

                    case "Decimal":
                        parametro.MySqlDbType = MySqlDbType.Decimal;
                        parametro.Value = item;
                        break;

                    case "String":
                        parametro.MySqlDbType = MySqlDbType.VarChar;
                        parametro.Value = item;
                        break;

                    case "Boolean":
                        parametro.MySqlDbType = MySqlDbType.Bit;
                        parametro.Value = Convert.ToBoolean(item);
                        break;

                    case "DateTime":
                        parametro.MySqlDbType = MySqlDbType.DateTime;
                        parametro.Value = item;
                        break;
                    case "Byte[]":
                        parametro.MySqlDbType = MySqlDbType.Byte;
                        parametro.Value = item;
                        break;

                    default:
                        break;
                }

            }

            mySqlCommand.Parameters.Add(parametro);
        }
    }
}
