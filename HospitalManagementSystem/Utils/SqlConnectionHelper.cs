using System;
using System.Configuration;
using System.Data.SqlClient;

namespace HospitalManagementSystem.Utils
{
    /// <summary>
    /// SqlConnectionHelper - Singleton Pattern
    /// Manages SQL Server database connections throughout the application
    /// </summary>
    public class SqlConnectionHelper
    {
        private readonly string _connectionString;

        // Singleton instance
        private static SqlConnectionHelper _instance;

        // Private constructor for Singleton
        private SqlConnectionHelper()
        {
            // Read connection string from App.config
            var cs = ConfigurationManager.ConnectionStrings["HospitalDB"];
            if (cs == null)
                throw new Exception("Connection string 'HospitalDB' not found in App.config");
            _connectionString = cs.ConnectionString;
        }

        /// <summary>
        /// Get singleton instance of SqlConnectionHelper
        /// </summary>
        public static SqlConnectionHelper Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new SqlConnectionHelper();
                return _instance;
            }
        }

        /// <summary>
        /// Get a new SQL connection (caller must manage disposal)
        /// </summary>
        public SqlConnection GetConnection()
        {
            try
            {
                SqlConnection connection = new SqlConnection(_connectionString);
                return connection;
            }
            catch (Exception ex)
            {
                throw new Exception("Error creating database connection: " + ex.Message, ex);
            }
        }

        // The rest of your methods (ExecuteQuery, ExecuteNonQuery, ExecuteScalar, TestConnection, ConnectionString property) remain exactly the same as before

        public SqlDataReader ExecuteQuery(string query, SqlParameter[] parameters = null)
        {
            try
            {
                SqlConnection connection = GetConnection();
                try
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    if (parameters != null)
                        command.Parameters.AddRange(parameters);
                    connection.Open();
                    return command.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
                }
                catch
                {
                    connection?.Dispose();
                    throw;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error executing query: " + ex.Message, ex);
            }
        }

        public int ExecuteNonQuery(string query, SqlParameter[] parameters = null)
        {
            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    if (parameters != null)
                        command.Parameters.AddRange(parameters);
                    connection.Open();
                    return command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error executing non-query: " + ex.Message, ex);
            }
        }

        public object ExecuteScalar(string query, SqlParameter[] parameters = null)
        {
            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    if (parameters != null)
                        command.Parameters.AddRange(parameters);
                    connection.Open();
                    return command.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error executing scalar query: " + ex.Message, ex);
            }
        }

        public bool TestConnection()
        {
            try
            {
                using (SqlConnection connection = GetConnection())
                {
                    connection.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        public string ConnectionString => _connectionString;
    }
}