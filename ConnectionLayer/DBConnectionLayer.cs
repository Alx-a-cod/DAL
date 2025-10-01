using System;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using Microsoft.Extensions.Configuration;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using DataAccessLayer.Interfaces; // Microsoft.Extensions.Configuration.* NuGet
//using System.Transactions;
//using System.Text.Json.Serialization; <--- <°)))>< Non scommentare o tira fuori errore. Json.Serialization non esiste in .NET Framework 4.x. P.S. ma perchè sta qui ?

namespace DataAccessLayer.ConnectionLayer
{
    public sealed class DbConnectionLayer : IDBConnectionLayer
    {
        private readonly string _sqlConnectionString;
        private readonly string _db2ConnectionString;

        // Costruttore consigliato: passare IConfiguration (es. da host .NET Core/8)
        public DbConnectionLayer(IConfiguration configuration = null, string sqlOverride = null, string db2Override = null)
        {
            // 1) se le connection string sono state passate direttamente -> usale
            if (!string.IsNullOrWhiteSpace(sqlOverride) && !string.IsNullOrWhiteSpace(db2Override))
            {
                _sqlConnectionString = sqlOverride;
                _db2ConnectionString = db2Override;
                return;
            }

            // 2) se è passato IConfiguration (es. appsettings.json del host) -> legge da lì
            if (configuration != null)
            {
                _sqlConnectionString = configuration.GetConnectionString("ConnSQLOffices")
                                       ?? configuration["ConnectionStrings:ConnSQLOffices"];
                _db2ConnectionString = configuration.GetConnectionString("ConnDB2")
                                       ?? configuration["ConnectionStrings:ConnDB2"];

                if (!string.IsNullOrWhiteSpace(_sqlConnectionString) && !string.IsNullOrWhiteSpace(_db2ConnectionString))
                    return;
                // altrimenti continueremo con i fallback
            }

            // prova a leggere ConfigurationManager.ConnectionStrings (se App.config / .NET Framework)
            try
            {
                var cmSql = System.Configuration.ConfigurationManager.ConnectionStrings["ConnSQLOffices"].ConnectionString;
                var cmDb2 = System.Configuration.ConfigurationManager.ConnectionStrings["ConnDB2"].ConnectionString;

                if (!string.IsNullOrWhiteSpace(cmSql) && !string.IsNullOrWhiteSpace(cmDb2))
                {
                    _sqlConnectionString = cmSql;
                    _db2ConnectionString = cmDb2;
                    return;
                }
            }
            catch
            {
                // ignore: System.Configuration potrebbe non essere disponibile in alcuni target
            }

            // prova a caricare appsettings.json (se la libreria viene eseguita da un progetto che ha il file in output)
            try
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: true)
                    .Build();

                var jsSql = config.GetConnectionString("ConnSQLOffices") ?? config["ConnectionStrings:ConnSQLOffices"];
                var jsDb2 = config.GetConnectionString("ConnDB2") ?? config["ConnectionStrings:ConnDB2"];

                if (!string.IsNullOrWhiteSpace(jsSql) && !string.IsNullOrWhiteSpace(jsDb2))
                {
                    _sqlConnectionString = jsSql;
                    _db2ConnectionString = jsDb2;
                }
                else
                {
                    // fallback hard-coded (solo per sviluppo)
                    _sqlConnectionString = "Data Source=FAKE_SQL_SERVER;Initial Catalog=FakeDatabase;User ID=fakeUser;Password=fakePassword;";
                    _db2ConnectionString = "Server=FAKE_DB2_SERVER:50000;Database=FAKEDB;UID=fakeUser;PWD=fakePassword;";
                }
            }
            catch
            {
                // fallback definitivo se qualcosa va storto
                _sqlConnectionString = "Data Source=FAKE_SQL_SERVER;Initial Catalog=FakeDatabase;User ID=fakeUser;Password=fakePassword;";
                _db2ConnectionString = "Server=FAKE_DB2_SERVER:50000;Database=FAKEDB;UID=fakeUser;PWD=fakePassword;";
            }
        }


        #region Public SQL Methods

        // apro con using per essere sicuri che si chiuda sempre

        public DataTable ExecuteOneSelectSQL(string query, Dictionary<string, object> parametersSQL = null)
        {
            using (SqlConnection connection = new SqlConnection(_sqlConnectionString))
            {
                connection.Open();
                return DoOneSelectQuerySQL(connection, query, parametersSQL);
            }
        }
        public bool ExecuteOneInsertSQL(string query, Dictionary<string, object> parametersSQL = null)
        {
            using (SqlConnection connection = new SqlConnection(_sqlConnectionString))
            {
                connection.Open();
                return DoOneInsertQuerySQL(connection, query, parametersSQL);
            }
        }
        public int ExecuteOneUpdateQuerySQL(string query, Dictionary<string, object> parametersSQL = null)
        {
            using (SqlConnection connection = new SqlConnection(_sqlConnectionString))
            {
                connection.Open();
                return DoOneUpdateQuerySQL(connection, query, parametersSQL);
            }
        }
        public int ExecuteOneDeleteQuerySQL(string query, Dictionary<string, object> parametersSQL = null)
        {
            using (SqlConnection connection = new SqlConnection(_sqlConnectionString))
            {
                connection.Open();
                return DoOneDeleteQuerySQL(connection, query, parametersSQL);
            }
        }
        public object ExecuteScalarQuerySQL(string query, Dictionary<string, object> parametersSQL = null)
        {
            using (SqlConnection connection = new SqlConnection(_sqlConnectionString))
            {
                connection.Open();
                return DoScalarQuerySQL(connection, query, parametersSQL);
            }
        }
        public int ExecuteStoredProcSQL(string storedProcName, Dictionary<string, object> parametersSQL = null)
        {
            using (SqlConnection conn = new SqlConnection(_sqlConnectionString))
            {
                conn.Open();
                return DoStoredProcSQL(conn, storedProcName, parametersSQL);
            }
        }
        /// Multi-query ========================= 
        public int ExecuteSqlTransaction(List<string> queriesSQL, List<Dictionary<string, object>> parametersList = null)
        {
            using (SqlConnection connection = new SqlConnection(_sqlConnectionString))
            {
                connection.Open();
                return DoSQL(connection, queriesSQL, parametersList);
            }
        }

        #endregion Public SQL Methods

        #region Public DB2 Methods

        public DataTable ExecuteOneSelectDB2(string query, Dictionary<string, object> parametersDB2 = null)
        {
            using (OleDbConnection connection = new OleDbConnection(_db2ConnectionString))
            {
                connection.Open();
                return DoOneSelectQueryDB2(connection, query, parametersDB2);
            }
        }
        public bool ExecuteOneInsertQueryDB2(string query, Dictionary<string, object> parametersDB2 = null)
        {
            using (OleDbConnection conn = new OleDbConnection(_db2ConnectionString))
            {
                conn.Open();
                return DoOneInsertQueryDB2(conn, query, parametersDB2);
            }
        }
        public int ExecuteOneUpdateDB2(string query, Dictionary<string, object> parametersDB2 = null)
        {
            using (OleDbConnection conn = new OleDbConnection(_db2ConnectionString))
            {
                conn.Open();
                return DoOneUpdateDB2(conn, query, parametersDB2);
            }
        }
        public int ExecuteOneDeleteQueryDB2(string query, Dictionary<string, object> parametersDB2 = null)
        {
            using (OleDbConnection conn = new OleDbConnection(_db2ConnectionString))
            {
                conn.Open();
                return DoOneDeleteQueryDB2(conn, query, parametersDB2);
            }
        }
        public object ExecuteScalarQueryDB2(string query, Dictionary<string, object> parametersDB2 = null)
        {
            using (OleDbConnection conn = new OleDbConnection(_db2ConnectionString))
            {
                conn.Open();
                return DoScalarQueryDB2(conn, query, parametersDB2);
            }
        }
        public int ExecuteStoredProcDB2(string storedProcName, Dictionary<string, object> parametersDB2 = null)
        {
            using (OleDbConnection conn = new OleDbConnection(_db2ConnectionString))
            {
                conn.Open();
                return DoStoredProcDB2(conn, storedProcName, parametersDB2);
            }
        }
        /// Multi-query ========================= 
        public int ExecuteDB2Transaction(List<string> queriesDB2, List<Dictionary<string, object>> parametersList = null)
        {
            using (OleDbConnection conn = new OleDbConnection(_db2ConnectionString))
            {
                conn.Open();
                return DoDB2(conn, queriesDB2, parametersList);
            }
        }

        #endregion Public DB2 Methods

        #region Public Mixed Methods

        public int DoAllDB2_SQL(List<string> queriesSQL, List<Dictionary<string, object>> parametersSQL = null, List<string> queriesDB2 = null, List<Dictionary<string, object>> parametersDB2 = null)
        {
            using (OleDbConnection db2Connection = new OleDbConnection(_db2ConnectionString))
            {
                db2Connection.Open();

                using (SqlConnection sqlConnection = new SqlConnection(_sqlConnectionString))
                {
                    sqlConnection.Open();

                    // Chiama il private DoAllDB2_SQL con connessioni già aperte
                    return DoAllDB2_SQL(db2Connection, sqlConnection, queriesSQL, parametersSQL, queriesDB2, parametersDB2);
                } // sqlConnection chiusa qui
            } // db2Connection chiusa qui
        }

        #endregion Public Mixed Methods

        // metodi privati rimangono così, ma passo la connection aperta dai pubblici + aggiungo using così chiudono automaticamente

        #region Private SQL Methods

        private DataTable DoOneSelectQuerySQL(SqlConnection connection, string query, Dictionary<string, object> parametersSQL = null)
        {
            DataTable dt = new DataTable();
            try
            {
                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    // aggiungo i parametri se ci sono
                    command.Parameters.Clear();
                    if (parametersSQL != null)
                    {
                        foreach (var p in parametersSQL)
                        {
                            command.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
                        }
                    }

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errore nell'esecuzione della query: " + query);
                Console.WriteLine("Messaggio di errore: " + ex.Message);
            }
            return dt;
        }
        private bool DoOneInsertQuerySQL(SqlConnection connection, string query, Dictionary<string, object> parametersSQL = null, SqlTransaction transaction = null)
        {
            bool localTransaction = false;
            try
            {
                // Se non viene passata una transazione, ne creo una locale
                if (transaction == null)
                {
                    transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
                    localTransaction = true;
                }

                using (SqlCommand command = new SqlCommand(query, connection, transaction))
                {
                    command.Parameters.Clear();
                    if (parametersSQL != null)
                    {
                        foreach (var p in parametersSQL)
                            command.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
                    }

                    int retQ = command.ExecuteNonQuery();

                    if (localTransaction)
                        transaction.Commit();  // commit solo se locale

                    return retQ > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errore INSERT SQL: " + ex.Message);
                if (localTransaction)
                    transaction.Rollback(); // rollback solo se locale
                return false;
            }
        }
        private int DoOneUpdateQuerySQL(SqlConnection connection, string query, Dictionary<string, object> parametersSQL = null, SqlTransaction transaction = null)
        {
            bool localTransaction = false;
            int rowsAffected = 0;

            try
            {
                // Se non viene passata una transazione, ne creo una locale
                if (transaction == null)
                {
                    transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
                    localTransaction = true;
                }

                using (SqlCommand command = new SqlCommand(query, connection, transaction))
                {
                    command.Parameters.Clear();
                    if (parametersSQL != null)
                    {
                        foreach (var p in parametersSQL)
                            command.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
                    }

                    rowsAffected = command.ExecuteNonQuery();
                }

                if (localTransaction)
                    transaction.Commit();

                return rowsAffected;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errore UPDATE SQL: " + ex.Message);
                if (localTransaction)
                    transaction.Rollback();
                return -1;
            }
        }
        private int DoOneDeleteQuerySQL(SqlConnection connection, string query, Dictionary<string, object> parametersSQL = null, SqlTransaction transaction = null)
        {
            bool localTransaction = false;
            int rowsAffected = 0;

            try
            {
                // Se non viene passata una transazione, ne creo una locale
                if (transaction == null)
                {
                    transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
                    localTransaction = true;
                }

                using (SqlCommand command = new SqlCommand(query, connection, transaction))
                {
                    command.Parameters.Clear();
                    if (parametersSQL != null)
                    {
                        foreach (var p in parametersSQL)
                            command.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
                    }

                    rowsAffected = command.ExecuteNonQuery();
                }

                if (localTransaction)
                    transaction.Commit();

                return rowsAffected;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errore DELETE SQL: " + ex.Message);
                if (localTransaction)
                    transaction.Rollback();
                return -1;
            }
        }
        private object DoScalarQuerySQL(SqlConnection connection, string query, Dictionary<string, object> parametersSQL = null, SqlTransaction transaction = null)
        {
            bool localTransaction = false;
            object result = null;

            try
            {
                if (transaction == null)
                {
                    transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
                    localTransaction = true;
                }

                using (SqlCommand command = new SqlCommand(query, connection, transaction))
                {
                    command.Parameters.Clear();
                    if (parametersSQL != null)
                    {
                        foreach (var p in parametersSQL)
                            command.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
                    }

                    result = command.ExecuteScalar();
                }

                if (localTransaction)
                    transaction.Commit();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errore ExecuteScalar SQL: " + ex.Message);
                if (localTransaction)
                    transaction.Rollback();
                return null;
            }

        }
        private int DoStoredProcSQL(SqlConnection connection, string storedProcName, Dictionary<string, object> parametersSQL = null, SqlTransaction transaction = null)
        {
            bool localTransaction = false;
            int rowsAffected = 0;

            try
            {
                // Se non viene passata una transazione, ne creo una locale
                if (transaction == null)
                {
                    transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
                    localTransaction = true;
                }

                using (SqlCommand command = new SqlCommand(storedProcName, connection, transaction))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    // Applica i parametri se presenti
                    command.Parameters.Clear();
                    if (parametersSQL != null)
                    {
                        foreach (var p in parametersSQL)
                            command.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
                    }

                    rowsAffected = command.ExecuteNonQuery();
                }

                if (localTransaction)
                    transaction.Commit();

                return rowsAffected;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errore Stored Procedure SQL: " + ex.Message);
                if (localTransaction)
                    transaction.Rollback();
                return -1;
            }
        }

        /// Multi-query ========================= 

        private int DoSQL(SqlConnection connection, List<string> queriesSQL, List<Dictionary<string, object>> parametersList = null) // N query con una singola chiamata al metodo, può fare INSERT, UPDATE, DELETE o combinazioni diverse in una sola chiamata.
        {
            if (queriesSQL == null || queriesSQL.Count == 0)
                throw new ArgumentException("Devi passare almeno una query.");

            if (parametersList != null && parametersList.Count != queriesSQL.Count)
                throw new ArgumentException("La lista dei parametri deve avere la stessa lunghezza delle query.");

            int retQ = 0;
            SqlTransaction transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);

            try
            {
                for (int i = 0; i < queriesSQL.Count; i++)
                {
                    string query = queriesSQL[i];
                    Dictionary<string, object> parameters = parametersList != null ? parametersList[i] : null;

                    using (SqlCommand command = new SqlCommand(query, connection, transaction))
                    {
                        command.Parameters.Clear(); // pulisce parametri precedenti
                        if (parameters != null)
                        {
                            foreach (var p in parameters)
                                command.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
                        }

                        retQ += command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Eccezione SQL: " + ex.Message);
                transaction.Rollback();
                return -1;
            }

            return retQ;
        }

        #endregion Private  SQL Methods

        #region Private DB2 Methods

        private DataTable DoOneSelectQueryDB2(OleDbConnection connection, string query, Dictionary<string, object> parametersDB2 = null)
        {
            DataTable dt = new DataTable();
            try
            {
                using (OleDbCommand command = new OleDbCommand(query, connection))
                {
                    // aggiungo parametri se passati
                    command.Parameters.Clear();
                    if (parametersDB2 != null)
                    {
                        foreach (var p in parametersDB2)
                        {
                            command.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
                        }
                    }

                    using (OleDbDataReader reader = command.ExecuteReader())
                    {
                        dt.Load(reader);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errore nell'esecuzione della query DB2: " + query);
                Console.WriteLine("Messaggio di errore: " + ex.Message);
            }

            return dt;
        }
        private bool DoOneInsertQueryDB2(OleDbConnection connection, string query, Dictionary<string, object> parametersDB2 = null, OleDbTransaction transaction = null)
        {
            try
            {
                using (OleDbCommand command = new OleDbCommand(query, connection, transaction))
                {
                    command.Parameters.Clear();
                    if (parametersDB2 != null)
                    {
                        foreach (var p in parametersDB2)
                            command.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
                    }

                    int retQ = command.ExecuteNonQuery();
                    return retQ > 0;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errore INSERT DB2: " + ex.Message);
                if (transaction != null)
                    transaction.Rollback();
                return false;
            }
        }
        private int DoOneUpdateDB2(OleDbConnection connection, string query, Dictionary<string, object> parametersDB2 = null, OleDbTransaction transaction = null)
        {
            bool localTransaction = false;
            int rowsAffected = 0;

            try
            {
                // Se non viene passata una transazione, ne creo una locale
                if (transaction == null)
                {
                    transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);
                    localTransaction = true;
                }

                using (OleDbCommand command = new OleDbCommand(query, connection, transaction))
                {
                    command.Parameters.Clear();
                    if (parametersDB2 != null)
                    {
                        foreach (var p in parametersDB2)
                            command.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
                    }

                    rowsAffected = command.ExecuteNonQuery();
                }

                if (localTransaction)
                    transaction.Commit();

                return rowsAffected;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errore UPDATE DB2: " + ex.Message);
                if (localTransaction)
                    transaction.Rollback();
                return -1;
            }
        }
        private int DoOneDeleteQueryDB2(OleDbConnection connection, string query, Dictionary<string, object> parametersDB2 = null, OleDbTransaction transaction = null)
        {
            try
            {
                using (OleDbCommand command = new OleDbCommand(query, connection, transaction))
                {
                    command.Parameters.Clear();
                    if (parametersDB2 != null)
                    {
                        foreach (var p in parametersDB2)
                            command.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
                    }

                    return command.ExecuteNonQuery(); // ritorna il numero di righe cancellate
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errore DELETE DB2: " + ex.Message);
                if (transaction != null)
                    transaction.Rollback();
                return -1;
            }
        }
        private object DoScalarQueryDB2(OleDbConnection connection, string query, Dictionary<string, object> parametersDB2 = null, OleDbTransaction transaction = null)
        {
            try
            {
                using (OleDbCommand command = new OleDbCommand(query, connection, transaction))
                {
                    command.Parameters.Clear();
                    if (parametersDB2 != null)
                    {
                        foreach (var p in parametersDB2)
                            command.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
                    }

                    return command.ExecuteScalar();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errore ExecuteScalar DB2: " + ex.Message);
                if (transaction != null)
                    transaction.Rollback();
                return null;
            }
        }
        private int DoStoredProcDB2(OleDbConnection connection, string storedProcName, Dictionary<string, object> parametersDB2 = null, OleDbTransaction transaction = null)
        {
            try
            {
                using (OleDbCommand command = new OleDbCommand(storedProcName, connection, transaction))
                {
                    command.CommandType = CommandType.StoredProcedure;

                    command.Parameters.Clear();
                    if (parametersDB2 != null)
                    {
                        foreach (var p in parametersDB2)
                            command.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
                    }

                    return command.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errore Stored Procedure DB2: " + ex.Message);
                if (transaction != null)
                    transaction.Rollback();
                return -1;
            }
        }

        /// Multi-query ========================= 

        private int DoDB2(OleDbConnection connection, List<string> queriesDB2, List<Dictionary<string, object>> parametersList = null)
        {
            int retQ = 0;

            if (parametersList != null && parametersList.Count != queriesDB2.Count)
                throw new ArgumentException("La lista dei parametri deve avere la stessa lunghezza delle query.");

            OleDbTransaction transaction = connection.BeginTransaction(IsolationLevel.ReadCommitted);

            try
            {
                for (int i = 0; i < queriesDB2.Count; i++)
                {
                    string query = queriesDB2[i];
                    Dictionary<string, object> parameters = parametersList != null ? parametersList[i] : null;

                    using (OleDbCommand command = new OleDbCommand(query, connection, transaction))
                    {
                        command.Parameters.Clear();
                        if (parameters != null)
                        {
                            foreach (var p in parameters)
                            {
                                command.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
                            }
                        }

                        retQ += command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Eccezione DB2: " + ex.Message);
                transaction.Rollback();
                return -1;
            }

            return retQ;
        }

        #endregion Private DB2 Methods

        #region Private Mixed Methods

        private int DoAllDB2_SQL(OleDbConnection db2Connection, SqlConnection sqlConnection, List<string> queriesSQL, List<Dictionary<string, object>> parametersSQL = null, List<string> queriesDB2 = null, List<Dictionary<string, object>> parametersDB2 = null)
        {
            int totalRowsDB2 = 0;
            int totalRowsSQL = 0;

            OleDbTransaction db2Transaction = db2Connection.BeginTransaction(IsolationLevel.ReadCommitted);
            SqlTransaction sqlTransaction = sqlConnection.BeginTransaction(IsolationLevel.ReadCommitted);

            try
            {
                // ===================== DB2 =====================
                if (queriesDB2 != null)
                {
                    for (int i = 0; i < queriesDB2.Count; i++)
                    {
                        using (OleDbCommand cmdDB2 = new OleDbCommand(queriesDB2[i], db2Connection, db2Transaction))
                        {
                            cmdDB2.Parameters.Clear();
                            if (parametersDB2 != null && parametersDB2.Count > i)
                            {
                                foreach (var p in parametersDB2[i])
                                    cmdDB2.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
                            }

                            totalRowsDB2 += cmdDB2.ExecuteNonQuery();
                        }
                    }
                }

                // ===================== SQL =====================
                if (queriesSQL != null)
                {
                    for (int i = 0; i < queriesSQL.Count; i++)
                    {
                        using (SqlCommand cmdSQL = new SqlCommand(queriesSQL[i], sqlConnection, sqlTransaction))
                        {
                            cmdSQL.Parameters.Clear();
                            if (parametersSQL != null && parametersSQL.Count > i)
                            {
                                foreach (var p in parametersSQL[i])
                                    cmdSQL.Parameters.AddWithValue(p.Key, p.Value ?? DBNull.Value);
                            }

                            totalRowsSQL += cmdSQL.ExecuteNonQuery();
                        }
                    }
                }

                // ===================== COMMIT =====================
                db2Transaction.Commit();
                sqlTransaction.Commit();

                return totalRowsDB2 + totalRowsSQL;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Errore DoAllDB2_SQL: " + ex.Message);

                // Rollback entrambe le transazioni
                db2Transaction.Rollback();
                sqlTransaction.Rollback();

                return -1;
            }
        }


        #endregion Private Mixed Methods
    }
}