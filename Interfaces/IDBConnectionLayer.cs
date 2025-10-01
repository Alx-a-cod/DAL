using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace DataAccessLayer.Interfaces
{
    public interface IDBConnectionLayer
    {
        /// SQL ========================= 
        DataTable ExecuteOneSelectSQL(string query, Dictionary<string, object> parametersSQL = null);                  // equivale a DoOneSelectQuerySQL
        bool ExecuteOneInsertSQL(string query, Dictionary<string, object> parametersSQL = null);                        // equivale a DoOneInsertQuerySQL
        int ExecuteOneUpdateQuerySQL(string query, Dictionary<string, object> parametersSQL = null);             // equivale a DoOneUpdateQuerySQL
        int ExecuteOneDeleteQuerySQL(string query, Dictionary<string, object> parametersSQL = null);             // equivale a DoOneDeleteQuerySQL
        object ExecuteScalarQuerySQL(string query, Dictionary<string, object> parametersSQL = null);          // equivale a DoScalarQuerySQL
        int ExecuteStoredProcSQL(string storedProcName, Dictionary<string, object> parametersSQL = null); // equivale a DoStoredProcSQL
        /// Multi-query SQL========================= 
        int ExecuteSqlTransaction(List<string> queriesSQL, List<Dictionary<string, object>> parametersList = null);        // equivale a DoSQL

        /// DB2 ========================= 
        DataTable ExecuteOneSelectDB2(string query, Dictionary<string, object> parametersDB2 = null);                  // equivale a DoOneSelectQueryDB
        bool ExecuteOneInsertQueryDB2(string query, Dictionary<string, object> parametersDB2 = null); // equivale a DoOneInsertQueryDB2
        int ExecuteOneUpdateDB2(string query, Dictionary<string, object> parametersDB2 = null); // equivale a DoOneUpdateDB2
        int ExecuteOneDeleteQueryDB2(string query, Dictionary<string, object> parametersDB2 = null); // equivale a DoOneDeleteQueryDB2
        object ExecuteScalarQueryDB2(string query, Dictionary<string, object> parametersDB2 = null); // equivale a DoScalarQueryDB2
        int ExecuteStoredProcDB2(string storedProcName, Dictionary<string, object> parametersDB2 = null); // equivale a DoStoredProcDB2
        /// Multi-query DB2 ========================= 
        int ExecuteDB2Transaction(List<string> queriesDB2, List<Dictionary<string, object>> parametersList = null);       // equivale a DoDB2

        /// Multi-query DB2 + SQL =========================
        int DoAllDB2_SQL(List<string> queriesSQL, List<Dictionary<string, object>> parametersSQL = null, List<string> queriesDB2 = null, List<Dictionary<string, object>> parametersDB2 = null);  // equivale a DoAllDB2_SQL
    }
}

