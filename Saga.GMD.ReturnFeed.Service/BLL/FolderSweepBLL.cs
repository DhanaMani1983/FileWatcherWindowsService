using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using System.IO;
using System.Text.RegularExpressions;
using System.Data.OleDb;
using Saga.GMD.ReturnFeed.Service.BLL.Models;

namespace Saga.GMD.ReturnFeed.Service.BLL
{
    public sealed class FolderSweepBLL
    {
        //Log4Net config
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        // Extended 07/07/2017 - Paul Lenton
        // Requirement to have a second FileWatcher that watches for MetroMail files

        //File Paths from Config
        private string watchPath = ConfigurationManager.AppSettings["WatchPath"];
        private string destinationPath = ConfigurationManager.AppSettings["DestinationPath"];
        private string destinationPathMM = ConfigurationManager.AppSettings["DestinationPathForMetromailManual"];
        private string destinationPathMMAuto = ConfigurationManager.AppSettings["DestinationPathForMetromailAuto"];
        private string unknownFileTypes = ConfigurationManager.AppSettings["UnknownFileTypes"];

        //flag to state if a file sweep is running
        private bool isProcessing = false;

        //DB variables from config file
        private string GDwhConn = ConfigurationManager.AppSettings["GMD_DWH_Connection"];
        private string GDwhLand = ConfigurationManager.AppSettings["GMD_DWH_LandingTable"];
        private string CPMConn = ConfigurationManager.AppSettings["Customer_Product_Management_Connection"];
        private string CPMLand = ConfigurationManager.AppSettings["Customer_Product_Management_LandingTable"];
        private string CDMConn = ConfigurationManager.AppSettings["Campaign_Data_Management_Connection"];
        private string CDMLand = ConfigurationManager.AppSettings["Campaign_Data_Management_LandingTable"];
        private string MCMConn =ConfigurationManager.AppSettings["Membership_Core_Management_Connection"];
        private string MCMLand = ConfigurationManager.AppSettings["Membership_Core_Management_LandingTable"];
        private string PDMConn = ConfigurationManager.AppSettings["Publication_Data_Management_Connection"];
        private string PDMLand = ConfigurationManager.AppSettings["Publication_Data_Management_LandingTable"];

        //file extension variables from config file
        private List<string> GMD_DWHExtype = new List<string>(ConfigurationManager.AppSettings["GMD_DWH_FileTypes"].Split(new char[] { ';' }));
        private List<string> Customer_Product_ManagementExtype = new List<string>(ConfigurationManager.AppSettings["Customer_Product_Management_FileTypes"].Split(new char[] { ';' }));
        private List<string> Campaign_Data_ManagementExtype = new List<string>(ConfigurationManager.AppSettings["Campaign_Data_Management_FileTypes"].Split(new char[] { ';' }));
        private List<string> Membership_Core_ManagementExtype = new List<string>(ConfigurationManager.AppSettings["Membership_Core_Management_FileTypes"].Split(new char[] { ';' }));
        private List<string> Publication_Data_ManagementExtype = new List<string>(ConfigurationManager.AppSettings["Publication_Data_Management_FileTypes"].Split(new char[] { ';' }));
        private List<string> MetroMail_FilesManualExtype = new List<string>(ConfigurationManager.AppSettings["MetroMail_FileTypesManual"].Split(new char[] { ';' }));
        private List<string> MetroMail_FilesAutoExtype = new List<string>(ConfigurationManager.AppSettings["MetroMail_FileTypesAuto"].Split(new char[] { ';' }));


        //singleton 
        private static readonly FolderSweepBLL instance = new FolderSweepBLL();

        private FolderSweepBLL() { }

        public static FolderSweepBLL Instance
        {
            get
            {
                return instance;
            }
        }

        public void FileSweep()
        {

            if (!isProcessing)
            {
                try
                {
                    isProcessing = true;

                    foreach (string fileName in Directory.EnumerateFiles(watchPath, "*", SearchOption.TopDirectoryOnly)
                                                .Select(Path.GetFileName))
                    {

                        try
                        {
                            string fileType = GetFileType(fileName, watchPath);
                            string newFileName = fileName;
                            string destinationPath = GetDestinationPath(fileType);
                            Boolean badFileName = false;

                            if (CheckFileIsReady(watchPath, fileName))
                            {

                                if (!FileAlreadyLoaded(destinationPath, fileName))
                                {
                                    var connectionDetails = SetConnection(fileType);

                                    if (connectionDetails.Valid)
                                    {
                                        //log.Debug(fileType);

                                        DateTime landedTimestamp = new DateTime();
                                        landedTimestamp = DateTime.Now;

                                        UpdateDB(destinationPath, fileName, fileType, landedTimestamp, connectionDetails);
                                    }
                                    else
                                    {
                                        badFileName = true;

                                    }
                                }
                                else
                                {
                                    newFileName = GenerateNewFileName(watchPath, destinationPath, fileName);
                                    log.Fatal(string.Format("Duplicate of file '{0}' received. File has been renamed {1} and moved to {2}",
                                        watchPath, newFileName, destinationPath));
                                }

                                if (MetroMail_FilesAutoExtype.Contains(fileType) || MetroMail_FilesManualExtype.Contains(fileType))
                                {
                                    string copyDestination = ConfigurationManager.AppSettings["AS400DestinationPath"];
                                    File.Copy(watchPath + newFileName, copyDestination + newFileName);
                                }

                                if (!badFileName)
                                {
                                    System.IO.File.Move(watchPath + newFileName, destinationPath + newFileName);
                                    log.Debug("File " + watchPath + newFileName + " moved to " + destinationPath + newFileName);
                                }
                                /*else
                                {

                                    System.IO.File.Move(watchPath + newFileName, unknownFileTypes + newFileName);

                                }*/

                                
                                
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Fatal(ex.ToString());
                        }

                    }

                    isProcessing = false;

                }
                catch (Exception ex)
                {
                    log.Fatal(ex.ToString());
                }
                finally
                {
                    isProcessing = false;
                }
            }

        }


        private string GetDestinationPath(string fileType)
        {
            if (MetroMail_FilesManualExtype.Contains(fileType))
            {
                return destinationPathMM;
            }
            else if (MetroMail_FilesAutoExtype.Contains(fileType))
            {
                return destinationPathMMAuto;
            }
            else
            {
                return destinationPath;
            }

        }


        private bool CheckFileIsReady(string FullPath, string FileName)
        {
            // Get the subdirectories for the specified directory.'  
            bool IsFileExist = false;
            DirectoryInfo dir = new DirectoryInfo(FullPath);
            if (!dir.Exists)
                IsFileExist = false;
            else
            {
                string FileFullPath = Path.Combine(FullPath, FileName);
                if (File.Exists(FileFullPath))
                {
                    bool fileReady = false;
                    while (!fileReady)
                    {
                        fileReady = IsFileReady(FileFullPath);
                    }
                }
                IsFileExist = true;
            }
            return IsFileExist;
        }

        private bool FileAlreadyLoaded(string destinationPath, string fileName)
        {
            if (File.Exists(destinationPath + fileName))
            {
                return true;
            }
            return false;
        }

        private string GetFileType(string fileName, string FullPath)
        {
            string fileType = Path.GetFileNameWithoutExtension(FullPath + fileName);
            fileType = fileType.ToLower();
            fileType = fileType.Replace("-", "");
            fileType = fileType.Replace(".", "");
            fileType = Regex.Replace(fileType, "[0-9]", "");

            return fileType;
        }


        private bool IsFileReady(string filename)
        {
            try
            {
                using (FileStream inputStream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.None))
                {
                    if (inputStream.Length > 0) // File can be opened, therefore has finished being written
                    {
                        return true;
                    }
                    else // File is still writing
                    {
                        return false;
                    }
                }
            }
            catch (Exception exc)
            {
                // Something very bad happened. It's probably not worth checking and re-raising given how rare this will be
                // but we shouldn't try and do anything with that file
                log.Error(string.Format("Filename {0} caused exception when checking if written: {1}", filename, exc.ToString()));
                return false;
            }
        }

        private string GenerateNewFileName(string sourcePath, string destinationPath, string fileName)
        {
            // Needs new filename if the file already exists in the destination. Format is
            // {original_filename.original_extension.nnn}
            // Does not handle over 999. This should never be a problem
            int append = -1;
            bool fileExists = true;
            string newFileName = string.Empty;
            string fileExtension = Path.GetExtension(sourcePath + fileName);
            string extensionAppend = string.Empty;
            fileName = Path.GetFileNameWithoutExtension(sourcePath + fileName);

            while (fileExists) // loop until an available filename is found
            {
                append += 1;
                if (append >= 0 && append <= 9)
                {
                    extensionAppend = ".00";
                }
                else if (append >= 9 && append <= 99)
                {
                    extensionAppend = ".0";
                }
                else
                {
                    extensionAppend = ".";
                }
                fileExists = FileAlreadyLoaded(destinationPath, fileName + extensionAppend + append.ToString() + fileExtension);
            }
            newFileName = fileName + extensionAppend + append.ToString() + fileExtension;

            return newFileName;
        }

        private ConnectionDetails SetConnection(string fileType)
        {
            // These lists could all be made public and set up at service startup if required. It may reduce overhead if the file type lists get too long
            // but they have been left in their own method for now to make it easier to see what's going on with them

            var connectionDetails = new ConnectionDetails();

            if  (GMD_DWHExtype.Contains(fileType) || MetroMail_FilesManualExtype.Contains(fileType) || MetroMail_FilesAutoExtype.Contains(fileType)) // Using GMD_DWH for Metromail file logging
            {
                connectionDetails.CONNSTR = GDwhConn;
                connectionDetails.LandingTable = GDwhLand;
            }
            else if (Customer_Product_ManagementExtype.Contains(fileType))
            {
                connectionDetails.CONNSTR = CPMConn;
                connectionDetails.LandingTable = CPMLand;
            }
            else if (Campaign_Data_ManagementExtype.Contains(fileType))
            {
                connectionDetails.CONNSTR = CDMConn;
                connectionDetails.LandingTable = CDMLand;
            }
            else if (Membership_Core_ManagementExtype.Contains(fileType))
            {
                connectionDetails.CONNSTR = MCMConn;
                connectionDetails.LandingTable = MCMLand;
            }
            else if (Publication_Data_ManagementExtype.Contains(fileType))
            {
                connectionDetails.CONNSTR = PDMConn;
                connectionDetails.LandingTable = PDMLand;
            }
            else
            {
                //log.Fatal("New file type '" + fileType + "' has been loaded to " + watchPath + " and moved to  " + unknownFileTypes + ". Please review and resolve.");
                connectionDetails.Valid = false;
                return connectionDetails;
            }

            connectionDetails.Valid = true;

            return connectionDetails;
            
        }

        private void UpdateDB(string filePath, string fileName, string fileType, DateTime landedTimestamp, ConnectionDetails connectionDetails)
        {
            string command = string.Empty;

            if (MetroMail_FilesManualExtype.Contains(fileType) || MetroMail_FilesAutoExtype.Contains(fileType))
            // Metromail file - set to Is_Processed = 1, and includes a processed timestamp
            {
                command = "insert into " + connectionDetails.LandingTable + " (File_Path, [Filename],File_Type, Landed_Timestamp, Processed_Timestamp, Is_Processed) values ('"
                            + filePath + "', '" + fileName + "', '" + fileType + "', '" + landedTimestamp.ToString("yyyy/MM/dd HH:mm:ss.fff") + "', '"
                            + landedTimestamp.ToString("yyyy/MM/dd HH:mm:ss.fff") + "'" + ", 1)";
            }
            else
            {
                command = "insert into " + connectionDetails.LandingTable + " (File_Path, [Filename],File_Type, Landed_Timestamp) values ('"
                    + filePath + "', '" + fileName + "', '" + fileType + "', '" + landedTimestamp.ToString("yyyy/MM/dd HH:mm:ss.fff") + "')";
            }

            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionDetails.CONNSTR))
                {
                    conn.Open();
                    OleDbCommand cmd = new OleDbCommand(command, conn);
                    //log.Debug(command);
                    cmd.ExecuteNonQuery();
                    conn.Close();
                }
            }
            catch (Exception e)
            {
                log.Fatal(string.Format("Failed to load file {0}. Exception was raised: {1}", fileName, e.ToString()));
            }
        }

    }
}
