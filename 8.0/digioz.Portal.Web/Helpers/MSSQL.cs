using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Data.SqlClient;
using System.Data.Common;
using System.Linq;

namespace digioz.Portal.Web.Helpers
{
    /// <summary>
    /// MSSQL Database Communication Class
    /// </summary>
    //public class MSSQL : IDisposable
    //{
    //    /// <summary>
    //    /// DataSet for Storing Results
    //    /// </summary>
    //    public DataSet DS;
    //    /// <summary>
    //    /// Data Adapter for storing data
    //    /// </summary>
    //    public SqlDataAdapter DA;
    //    public DataTable DT = new DataTable();
    //    public SqlCommandBuilder CB;
    //    public SqlConnection CN;
    //    public SqlDataReader Reader;
    //    public string ConnectionString;
    //    public string ErrorMessage;
    //    public int ErrorNumber;

    //    /// <summary>
    //    /// Class Constructor
    //    /// </summary>
    //    public MSSQL(string connectionString)
    //    {
    //        ConnectionString = connectionString;
    //    }

    //    /// <summary>
    //    /// Method to open connection to Database
    //    /// </summary>
    //    public void openConnection()
    //    {
    //        CN = new SqlConnection(ConnectionString);

    //        try
    //        {
    //            CN.Open();
    //            ErrorMessage = "";
    //        }
    //        catch (SqlException ex)
    //        {
    //            ErrorMessage = ex.Message.ToString();
    //        }
    //    }

    //    /// <summary>
    //    /// Method to close Database Connection
    //    /// </summary>
    //    public void closeConnection()
    //    {
    //        try
    //        {
    //            CN.Close();
    //        }
    //        catch (SqlException ex)
    //        {
    //            ErrorMessage = ex.Message.ToString();
    //        }
    //    }

    //    /// <summary>
    //    /// Method to Query Database and Obtain Data Table
    //    /// </summary>
    //    /// <param name="sql">SQL String</param>
    //    /// <returns>Data Table</returns>
    //    public DataTable QueryDBDataset(string sql)
    //    {
    //        return _QueryDBDataset(sql, null);
    //    }

    //    /// <summary>
    //    /// Method to Query Database and Obtain Data Table
    //    /// </summary>
    //    /// <param name="sql">SQL String</param>
    //    /// <param name="par">Parameter Object</param>
    //    /// <returns>Data Table</returns>
    //    public DataTable QueryDBDataset(string sql, DbParameter[] par)
    //    {
    //        return _QueryDBDataset(sql, par);
    //    }

    //    /// <summary>
    //    /// Method to Query Database and Obtain Data Table
    //    /// </summary>
    //    /// <param name="sql">SQL String</param>
    //    /// <param name="par">Parameter Object</param>
    //    /// <returns></returns>
    //    private DataTable _QueryDBDataset(string sql, DbParameter[] par)
    //    {
    //        DataTable loReturnTable;
    //        try
    //        {
    //            SqlCommand cmd = new SqlCommand(sql, CN);

    //            if (par != null)
    //            {
    //                foreach (DbParameter p in par)
    //                {
    //                    cmd.Parameters.Add((SqlParameter)p);
    //                }
    //            }

    //            DA = new SqlDataAdapter(cmd);
    //            DS = new DataSet();
    //            DA.SelectCommand = cmd;
    //            CB = new SqlCommandBuilder(DA);
    //            DA.Fill(DS);
    //            loReturnTable = DS.Tables[0];
    //        }
    //        catch (SqlException ex)
    //        {
    //            ErrorMessage = ex.Message.ToString();
    //            loReturnTable = new DataTable();
    //        }

    //        return loReturnTable;
    //    }

    //    /// <summary>
    //    /// Method to Query Database and Obtain Data Table with Proc Support
    //    /// </summary>
    //    /// <param name="sql">SQL String</param>
    //    /// <param name="count">Count</param>
    //    /// <param name="par">Parameter Object</param>
    //    /// <returns>Data Table</returns>
    //    public DataTable QueryDBDatasetProc(string sql, out int count, DbParameter[] par)
    //    {
    //        count = 0;
    //        DataTable loReturnTable;

    //        try
    //        {
    //            SqlCommand cmd = new SqlCommand(sql, CN);
    //            cmd.CommandType = CommandType.StoredProcedure;

    //            foreach (DbParameter p in par)
    //            {
    //                cmd.Parameters.Add((SqlParameter)p);
    //            }

    //            DA = new SqlDataAdapter(cmd);
    //            DS = new DataSet();
    //            DA.SelectCommand = cmd;
    //            CB = new SqlCommandBuilder(DA);
    //            DA.Fill(DS);
    //            loReturnTable = DS.Tables[0];

    //            if (cmd.Parameters.Contains("@Count"))
    //            {
    //                count = Convert.ToInt32(cmd.Parameters["@Count"].Value);
    //            }
    //            else
    //            {
    //                count = DS.Tables[0].Rows.Count;
    //            }
    //        }
    //        catch (SqlException ex)
    //        {
    //            ErrorMessage = ex.Message.ToString();
    //            loReturnTable = new DataTable();
    //        }

    //        return loReturnTable;
    //    }

    //    /// <summary>
    //    /// Method to Obtain Data Table with specific connection string
    //    /// </summary>
    //    /// <param name="sql">SQL String</param>
    //    /// <param name="connectionString">Database Connection String</param>
    //    /// <returns>Data Table</returns>
    //    public DataTable GetDBDataset(string sql, string connectionString)
    //    {
    //        DataTable returnTable;
    //        openConnection();

    //        try
    //        {
    //            SqlCommand cmd = new SqlCommand(sql, CN);
    //            DA.SelectCommand = cmd;
    //            CB = new SqlCommandBuilder(DA);
    //            DA.Fill(DS);
    //            returnTable = DS.Tables[0];
    //        }
    //        catch (SqlException ex)
    //        {
    //            ErrorMessage += "Error: " + ex.Message.ToString();
    //            ErrorNumber = ex.Number;
    //            returnTable = new DataTable();
    //        }
    //        closeConnection();

    //        return returnTable;
    //    }

    //    /// <summary>
    //    /// Method to execute SQL Statement in Scalar Mode
    //    /// </summary>
    //    /// <param name="sql">SQL Statement</param>
    //    /// <returns>Object</returns>
    //    public object ExecDBScalar(string sql)
    //    {
    //        return _ExecDBScalar(sql, null);
    //    }

    //    /// <summary>
    //    /// Method to execute SQL Statement in Scalar Mode
    //    /// </summary>
    //    /// <param name="sql">SQL Statement</param>
    //    /// <param name="openTransaction">SQL Transaction Object</param>
    //    /// <returns>object</returns>
    //    public object ExecDBScalar(string sql, SqlTransaction openTransaction)
    //    {
    //        return _ExecDBScalar(sql, null, openTransaction);
    //    }

    //    /// <summary>
    //    /// Method to execute SQL Statement in Scalar Mode
    //    /// </summary>
    //    /// <param name="sql">SQL Statement</param>
    //    /// <param name="par">Parameter Object</param>
    //    /// <returns>Object</returns>
    //    public object ExecDBScalar(string sql, DbParameter[] par)
    //    {
    //        return _ExecDBScalar(sql, par);
    //    }

    //    /// <summary>
    //    /// Method to Execute SQL Command in Scalar Mode
    //    /// </summary>
    //    /// <param name="sql">SQL String</param>
    //    /// <param name="par">Parameter Object</param>
    //    /// <param name="openTransaction">SQL Transaction Object</param>
    //    /// <returns>Object</returns>
    //    public object ExecDBScalar(string sql, DbParameter[] par, SqlTransaction openTransaction)
    //    {
    //        return _ExecDBScalar(sql, par, openTransaction);
    //    }

    //    /// <summary>
    //    /// Method to Execute SQL Command in Scalar Mode with Proc
    //    /// </summary>
    //    /// <param name="procName">Procedure Name</param>
    //    /// <param name="par">Parameter Object</param>
    //    /// <returns>object</returns>
    //    public object ExecDBScalarProc(string procName, DbParameter[] par)
    //    {
    //        return _ExecDBScalarProc(procName, par);
    //    }

    //    /// <summary>
    //    /// Method to Execute SQL Command in Scalar Mode with Proc
    //    /// </summary>
    //    /// <param name="procName">Procedure Name</param>
    //    /// <param name="par">Parameter object</param>
    //    /// <param name="openTransaction">SQL Transaction Object</param>
    //    /// <returns>object</returns>
    //    public object ExecDBScalarProc(string procName, DbParameter[] par, SqlTransaction openTransaction)
    //    {
    //        return _ExecDBScalarProc(procName, par, openTransaction);
    //    }

    //    /// <summary>
    //    /// Method to Execute SQL Command in Scalar Mode with Proc
    //    /// </summary>
    //    /// <param name="procName">Procedure Name</param>
    //    /// <param name="par">Parameter Object</param>
    //    /// <param name="openTransaction">SQL Transaction Object</param>
    //    /// <returns>Object</returns>
    //    private object _ExecDBScalarProc(string procName, DbParameter[] par, SqlTransaction openTransaction)
    //    {
    //        object returnObject;
    //        SqlCommand cmd = new SqlCommand(procName, CN);

    //        if (CN != null && CN.State == ConnectionState.Open && openTransaction != null)
    //        {
    //            cmd.Transaction = openTransaction;
    //        }
                
            
    //        cmd.CommandType = CommandType.StoredProcedure;

    //        try
    //        {
    //            if (par != null)
    //            {
    //                foreach (DbParameter p in par)
    //                {
    //                    cmd.Parameters.Add((SqlParameter)p);
    //                }
    //            }

    //            // This creates an exception when return value is null.
    //            //oReturn = cmd.ExecuteScalar().ToString();
    //            returnObject = cmd.ExecuteScalar();

    //            if (returnObject != null)
    //            {
    //                returnObject = returnObject.ToString();
    //            }
    //        }
    //        catch (SqlException ex)
    //        {
    //            ErrorMessage = ex.Message.ToString();
    //            ErrorNumber = ex.Number;
    //            returnObject = null;
    //        }

    //        return returnObject;
    //    }

    //    /// <summary>
    //    /// Method to Execute SQL Command in Scalar Mode with Proc
    //    /// </summary>
    //    /// <param name="procName">Procedure Name</param>
    //    /// <param name="par">Parameter Object</param>
    //    /// <returns>Object</returns>
    //    private object _ExecDBScalarProc(string procName, DbParameter[] par)
    //    {

    //        SqlCommand cmd = new SqlCommand(procName, CN);
    //        object returnObject;
    //        cmd.CommandType = CommandType.StoredProcedure;

    //        try
    //        {
    //            if (par != null)
    //            {
    //                foreach (DbParameter p in par)
    //                {
    //                    cmd.Parameters.Add((SqlParameter)p);
    //                }
    //            }

    //            // This creates an exception when return value is null.
    //            //oReturn = cmd.ExecuteScalar().ToString();
    //            returnObject = cmd.ExecuteScalar();

    //            if (returnObject != null)
    //            {
    //                returnObject = returnObject.ToString();
    //            }
    //        }
    //        catch (SqlException ex)
    //        {
    //            ErrorMessage = ex.Message.ToString();
    //            ErrorNumber = ex.Number;
    //            returnObject = null;
    //        }

    //        return returnObject;
    //    }

    //    /// <summary>
    //    /// Method to Execute SQL Command in Scalar Mode with Proc
    //    /// </summary>
    //    /// <param name="sql">SQL String</param>
    //    /// <param name="par">Parameter Object</param>
    //    /// <returns>object</returns>
    //    private object _ExecDBScalar(string sql, DbParameter[] par)
    //    {
    //        SqlCommand cmd = new SqlCommand(sql, CN);
    //        object returnObject;

    //        try
    //        {
    //            if (par != null)
    //            {
    //                foreach (DbParameter p in par)
    //                {
    //                    cmd.Parameters.Add((SqlParameter)p);
    //                }
    //            }

    //            // This creates an exception when return value is null.
    //            //oReturn = cmd.ExecuteScalar().ToString();
    //            returnObject = cmd.ExecuteScalar();

    //            if (returnObject != null)
    //            {
    //                returnObject = returnObject.ToString();
    //            }
    //        }
    //        catch (SqlException ex)
    //        {
    //            ErrorMessage = ex.Message.ToString();
    //            ErrorNumber = ex.Number;
    //            returnObject = null;
    //        }

    //        return returnObject;
    //    }

    //    /// <summary>
    //    /// Method to Execute SQL Command in Scalar Mode with Proc
    //    /// </summary>
    //    /// <param name="sql">SQL String</param>
    //    /// <param name="par">Parameter Object</param>
    //    /// <param name="openTransaction">SQL Transaction Object</param>
    //    /// <returns>object</returns>
    //    private object _ExecDBScalar(string sql, DbParameter[] par, SqlTransaction openTransaction)
    //    {
    //        object returnObject;
    //        SqlCommand cmd = new SqlCommand(sql, CN);

    //        if (CN != null && CN.State == ConnectionState.Open && openTransaction != null)
    //        {
    //            cmd.Transaction = openTransaction;
    //        }

    //        try
    //        {
    //            if (par != null)
    //            {
    //                foreach (DbParameter p in par)
    //                {
    //                    cmd.Parameters.Add((SqlParameter)p);
    //                }
    //            }

    //            // This creates an exception when return value is null.
    //            //oReturn = cmd.ExecuteScalar().ToString();
    //            returnObject = cmd.ExecuteScalar();

    //            if (returnObject != null)
    //            {
    //                returnObject = returnObject.ToString();
    //            }
    //        }
    //        catch (SqlException ex)
    //        {
    //            ErrorMessage = ex.Message.ToString();
    //            ErrorNumber = ex.Number;
    //            returnObject = null;
    //        }

    //        return returnObject;
    //    }

    //    /// <summary>
    //    /// Execute SQL Query No Return
    //    /// </summary>
    //    /// <param name="sql">SQL String</param>
    //    public void ExecDB(string sql)
    //    {
    //        int cnt;
    //        _ExecDB(sql, out cnt, null);
    //    }

    //    /// <summary>
    //    /// Execute SQL Query No Return
    //    /// </summary>
    //    /// <param name="sql">SQL String</param>
    //    /// <param name="openTransaction">SQL Transaction Object</param>
    //    public void ExecDB(string sql, SqlTransaction openTransaction)
    //    {
    //        int cnt;
    //        _ExecDB(sql, out cnt, null, openTransaction);
    //    }

    //    /// <summary>
    //    /// Execute SQL Query No Return
    //    /// </summary>
    //    /// <param name="sql">SQL String</param>
    //    /// <param name="count">Count</param>
    //    /// <param name="par">Parameter Object</param>
    //    public void ExecDB(string sql, out int count, DbParameter[] par)
    //    {
    //        _ExecDB(sql, out count, par);
    //    }

    //    /// <summary>
    //    /// Execute SQL Query No Return
    //    /// </summary>
    //    /// <param name="sql">SQL String</param>
    //    /// <param name="count">Count</param>
    //    /// <param name="par">Parameter Object</param>
    //    /// <param name="openTransaction">SQL Transaction Object</param>
    //    public void ExecDB(string sql, out int count, DbParameter[] par, SqlTransaction openTransaction)
    //    {
    //        _ExecDB(sql, out count, par, openTransaction);
    //    }

    //    /// <summary>
    //    /// Execute SQL Query No Return
    //    /// </summary>
    //    /// <param name="sql">SQL String</param>
    //    /// <param name="count">Count</param>
    //    /// <param name="par">Parameter Object</param>
    //    private void _ExecDB(string sql, out int count, DbParameter[] par)
    //    {
    //        count = 0;

    //        try
    //        {
    //            SqlCommand cmd = new SqlCommand(sql, CN);

    //            if (par != null)
    //            {
    //                foreach (DbParameter p in par)
    //                {
    //                    cmd.Parameters.Add((SqlParameter)p);
    //                }
    //            }

    //            count = cmd.ExecuteNonQuery();
    //        }
    //        catch (SqlException ex)
    //        {
    //            ErrorMessage = ex.Message.ToString();
    //        }
    //    }

    //    /// <summary>
    //    /// Execute SQL Query No Return
    //    /// </summary>
    //    /// <param name="sql">SQL String</param>
    //    /// <param name="count">Count</param>
    //    /// <param name="par">Parameter Object</param>
    //    /// <param name="openTransaction">SQL Transaction String</param>
    //    private void _ExecDB(string sql, out int count, DbParameter[] par, SqlTransaction openTransaction)
    //    {
    //        count = 0;

    //        try
    //        {
    //            SqlCommand cmd = new SqlCommand(sql, CN);

    //            if (CN != null && CN.State == ConnectionState.Open && openTransaction != null)
    //            {
    //                cmd.Transaction = openTransaction;
    //            }  

    //            if (par != null)
    //            {
    //                foreach (DbParameter p in par)
    //                {
    //                    cmd.Parameters.Add((SqlParameter)p);
    //                }
    //            }

    //            count = cmd.ExecuteNonQuery();
    //        }
    //        catch (SqlException ex)
    //        {
    //            ErrorMessage = ex.Message.ToString();
    //        }
    //    }

    //    /// <summary>
    //    /// Update Data Table Method
    //    /// </summary>
    //    public void UpdateDBDataset()
    //    {
    //        try
    //        {
    //            DT = DS.Tables[0];
    //            DataTable changes = DT.GetChanges();

    //            DA.Update(changes);
    //            DT.AcceptChanges();
    //        }
    //        catch (SqlException ex)
    //        {
    //            ErrorMessage = ex.Message.ToString();
    //        }
    //    }

    //    /// <summary>
    //    /// Delete Data Table Method
    //    /// </summary>
    //    /// <param name="primarykey">Primary Key</param>
    //    public void DeleteDBDataset(int primarykey)
    //    {
    //        try
    //        {
    //            DS.Tables[0].Rows[primarykey].Delete();
    //        }
    //        catch (SqlException ex)
    //        {
    //            ErrorMessage += "Error: " + ex.Message.ToString();
    //            ErrorNumber = ex.Number;
    //        }
    //    }

    //    /// <summary>
    //    /// Create Database Method
    //    /// </summary>
    //    /// <param name="databaseName">Database Name</param>
    //    public void CreateDatabase(string databaseName)
    //    {
    //        try
    //        {
    //            this.ExecDB("CREATE DATABASE " + databaseName + ";");
    //        }
    //        catch (SqlException ex)
    //        {
    //            ErrorMessage = ex.Message.ToString();
    //        }
    //    }

    //    /// <summary>
    //    /// Execute DB SP with existing transaction
    //    /// </summary>
    //    /// <param name="sql">SQL String</param>
    //    /// <param name="count">Count</param>
    //    /// <param name="par">Parameter Object</param>
    //    /// <param name="poTransaction">SQL Transaction Object</param>
    //    public void ExecDBProc(string sql, out int count, DbParameter[] par, SqlTransaction openTransaction)
    //    {
    //        count = 0;
    //        SqlCommand cmd = new SqlCommand(sql, CN);

    //        if (CN != null && CN.State == ConnectionState.Open && openTransaction != null)
    //        {
    //            cmd.Transaction = openTransaction;
    //        }
                
    //        cmd.CommandType = CommandType.StoredProcedure;

    //        foreach (DbParameter p in par)
    //        {
    //            cmd.Parameters.Add((SqlParameter)p);
    //        }

    //        try
    //        {
    //            count = cmd.ExecuteNonQuery();
    //        }
    //        catch (SqlException ex)
    //        {
    //            ErrorMessage = ex.Message.ToString();
    //        }
    //    }

    //    /// <summary>
    //    /// Execute SQL Query Proc
    //    /// </summary>
    //    /// <param name="sql">SQL String</param>
    //    /// <param name="count">Count</param>
    //    /// <param name="par">Parameter Object</param>
    //    public void ExecDBProc(string sql, out int count, DbParameter[] par)
    //    {
    //        count = 0;
    //        SqlCommand cmd = new SqlCommand(sql, CN);
    //        cmd.CommandType = CommandType.StoredProcedure;

    //        foreach (DbParameter p in par)
    //        {
    //            cmd.Parameters.Add((SqlParameter)p);
    //        }

    //        try
    //        {
    //            count = cmd.ExecuteNonQuery();
    //        }
    //        catch (SqlException ex)
    //        {
    //            ErrorMessage = ex.Message.ToString();
    //        }
    //    }

    //    /// <summary>
    //    /// Method to bulk import data in a datatable
    //    /// into the database chosen
    //    /// </summary>
    //    /// <param name="dt"></param>
    //    /// <param name="tableName"></param>
    //    public void BulkImportData(DataTable dt, string tableName)
    //    {
    //        using (SqlBulkCopy bulkCopy = new SqlBulkCopy(ConnectionString))
    //        {
    //            bulkCopy.DestinationTableName = tableName;

    //            try
    //            {
    //                // Write from the source to the destination.
    //                bulkCopy.WriteToServer(dt);
    //            }
    //            catch (Exception ex)
    //            {
    //                ErrorMessage = ex.Message.ToString();
    //            }
    //        }
    //    }

    //    /// <summary>
    //    /// Dispose of all IDisposable Objects
    //    /// </summary>
    //    public void Dispose()
    //    {
    //        DS.Dispose();
    //        DA.Dispose();
    //        DT.Dispose();
    //        CB.Dispose();
    //        CN.Dispose();
    //        Reader.Dispose();
    //    }
    //}
}