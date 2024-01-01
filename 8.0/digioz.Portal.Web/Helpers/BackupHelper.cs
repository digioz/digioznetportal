using digioz.Portal.Bll;
//using digioz.Portal.Web.Areas.Admin.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace digioz.Portal.Web.Helpers
{
    /// <summary>
    /// Helper Class for backing up and restoring
    /// both databases and files
    /// </summary>
    public static class BackupHelper
    {
        ///// <summary>
        ///// Method to backup all selected table contents to XML files
        ///// stored in the App_Data folder of the site
        ///// </summary>
        ///// <param name="backup"></param>
        ///// <returns></returns>
        //public static bool BackupDatabaseToXml(BackupToXMLViewModel backup)
        //{
        //    bool result = false;

        //    try
        //    {
        //        var appDataFolderPath = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();

        //        // Create Export Folder
        //        var exportFolder = appDataFolderPath + "\\ExportData_" + DateTime.Now.Year + DateTime.Now.Month
        //                                             + DateTime.Now.Day + DateTime.Now.Hour + DateTime.Now.Minute
        //                                             + DateTime.Now.Second;
        //        System.IO.Directory.CreateDirectory(exportFolder);

        //        if (backup.ExportAllTables)
        //        {
        //            backup.ListOfTables = DBLogic.GetTables();
        //        }

        //        MSSQL db = new MSSQL();
        //        db.openConnection();

        //        foreach (var tableName in backup.ListOfTables)
        //        {
        //            DataTable dt = new DataTable();
        //            dt = db.QueryDBDataset("SELECT * FROM " + tableName + ";");

        //            if (dt != null && dt.Rows.Count > 0)
        //            {
        //                //// Generic List Solution
        //                //Type type = Assembly.Load("digioz.Portal.Domain").GetTypes().First(t => t.Name == tableName);
        //                //var method = typeof(Utility).GetMethod("BindList").MakeGenericMethod(type);
        //                //var bindResult = method.Invoke(null, new[] { dt });

        //                // Serialize and write to disk as backup
        //                dt.WriteXml(exportFolder + "\\" + tableName + ".xml", XmlWriteMode.WriteSchema);
        //            }
        //        }

        //        db.closeConnection();
        //        result = true;
        //    }
        //    catch (Exception ex)
        //    {
        //        string output = "Error exporting tables" +  ". Message: " + ex.Message + " Stack Trace: " + ex.InnerException;
        //        Utility.AddLogEntry(output);
        //        result = false;
        //    }

        //    return result;
        //}

        ///// <summary>
        ///// Method to restore all selected table files from the App_Data's 
        ///// selected Export Folder to the Database tables. As part of this
        ///// import, all existing table contents are deleted and the new content 
        ///// is inserted into the tables.
        ///// </summary>
        ///// <param name="backup"></param>
        ///// <returns></returns>
        //public static bool RestoreDatabaseFromXml(RestoreFromXMLViewModel backup)
        //{
        //    var filters = new String[] { "xml" };
        //    var appDataFolderPath = AppDomain.CurrentDomain.GetData("DataDirectory").ToString();
        //    var importFolder = appDataFolderPath + "\\" + backup.SelectedFolder;
        //    bool result = false;

        //    if (backup.ImportAllTables)
        //    {
        //        backup.ListOfTables = Utility.GetFilesFrom(appDataFolderPath + "\\" + backup.SelectedFolder, filters, false);
        //    }

        //    foreach (var tableName in backup.ListOfTables)
        //    {
        //        string importFile = importFolder + "\\" + tableName;
        //        string sqlTableName = tableName.Replace(".xml", "");

        //        try
        //        {
        //            DataTable dt = new DataTable();
        //            dt.ReadXml(importFile);

        //            MSSQL db = new MSSQL();
        //            db.openConnection();

        //            string sql = "DELETE FROM " + sqlTableName + ";"
        //                       + "DBCC CHECKIDENT ('" + sqlTableName + "', RESEED, 0);";

        //            db.ExecDB(sql);
        //            db.BulkImportData(dt, sqlTableName);
        //            db.closeConnection();

        //            result = true;
        //        }
        //        catch (Exception ex)
        //        {
        //            string output = "Error importing table" + sqlTableName + ". Message: " + ex.Message + " Stack Trace: " + ex.InnerException;
        //            Utility.AddLogEntry(output);
        //            result = false;
        //        }
        //    }

        //    return result;
        //}
    }
}