using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Configuration;
using log4net;
using System.Data.OleDb;

namespace Saga.GMD.ReturnFeed.Service
{
    partial class ReturnFeedFileWatcher
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private static readonly log4net.ILog _log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.FSWatcher = new System.IO.FileSystemWatcher();
            ((System.ComponentModel.ISupportInitialize)(this.FSWatcher)).BeginInit();
            // 
            // FSWatcher
            // 
            this.FSWatcher.EnableRaisingEvents = true;
            this.FSWatcher.IncludeSubdirectories = true;
            this.FSWatcher.NotifyFilter = ((System.IO.NotifyFilters)((((((((System.IO.NotifyFilters.FileName | System.IO.NotifyFilters.DirectoryName)
            | System.IO.NotifyFilters.Attributes)
            | System.IO.NotifyFilters.Size)
            | System.IO.NotifyFilters.LastWrite)
            | System.IO.NotifyFilters.LastAccess)
            | System.IO.NotifyFilters.CreationTime)
            | System.IO.NotifyFilters.Security)));
            // 
            // ReturnFeedFileWatcher
            // 
            this.ServiceName = "FileWatcher";
            ((System.ComponentModel.ISupportInitialize)(this.FSWatcher)).EndInit();

        }

        #endregion

        private System.IO.FileSystemWatcher FSWatcher;

        //    private void FSWWatcher_Changed(object sender, System.IO.FileSystemEventArgs e)
        //    {
        //        // newly changed file or directory
        //    }

        //    private void FSWatcher_Created(object sender, System.IO.FileSystemEventArgs e)
        //    {
        //        _log.Info("A new file has been loaded");
        //        string destinationPath = ConfigurationManager.AppSettings["DestinationPath"];
        //        // newly created file or directory
        //        string fileName = e.Name.ToString();
        //        fileName = fileName.Replace(".txt", "");

        //        string fileType = GetFileType(fileName);

        //        DateTime landedTimestamp = new DateTime();

        //        UpdateDB(fileName, fileType, landedTimestamp);

        //        System.IO.File.Move(e.FullPath, destinationPath + e.Name);

        //    }

        //    private string GetFileType(string fileName)
        //    {
        //        string fileType = string.Empty;
        //        fileType = fileName.ToLower();
        //        fileType = fileType.Replace("-", "");
        //        fileType = Regex.Replace(fileType, "[0-9]", "");

        //        return fileType;
        //    }


        //    private void UpdateDB(string fileName, string fileType, DateTime landedTimestamp)
        //    {
        //        List<string> GMD_DWH = new List<string>() { "sagbounceexport" };
        //        if (GMD_DWH.Contains(fileType))
        //        {
        //            string CONNSTR = "Provider=SQLOLEDB;Data Source=gmdsqldev01;Initial Catalog=GMD_DWH;Integrated Security=SSPI";
        //            using (OleDbConnection conn = new OleDbConnection(CONNSTR))
        //            {
        //                string command = "insert into return_feed.processing_queue ([Filename],File_Type, Landed_Timestamp, Is_Processed, Failed_Flag) values ("
        //                    + fileName + ", " + fileType + ", " + landedTimestamp + ", 'N', 0)";
        //                OleDbCommand cmd = new OleDbCommand(command, conn);
        //                cmd.ExecuteScalar();
        //            }
        //        }
        //    }
        //}
    }
}
