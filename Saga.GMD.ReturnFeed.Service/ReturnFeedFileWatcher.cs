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
using System.Threading;
using System.IO;
using System.Timers;
using Saga.GMD.ReturnFeed.Service.BLL;

namespace Saga.GMD.ReturnFeed.Service
{
    public partial class ReturnFeedFileWatcher : ServiceBase
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        System.Timers.Timer timer = new System.Timers.Timer();

        FolderSweepBLL foldersweep = FolderSweepBLL.Instance;

        public ReturnFeedFileWatcher()
        {
            InitializeComponent();
            //FSWatcher.Created += FSWatcher_Created;
        }

        protected override void OnStart(string[] args)
        {
            //System.IO.File.Create("test.txt");
            log.Info("Service started");

            /*try
            {
                FSWatcher.Path = watchPath;
            }
            catch (Exception ex)
            {
                log.Fatal(ex.ToString());
            }*/

            try
            {

                timer.Elapsed += new ElapsedEventHandler(FSWatcher_Tick);

                timer.Interval = GetInterval();

                timer.Enabled = true;
            }
            catch (Exception ex)
            {
                log.Fatal(ex.ToString());
                throw ex;
            }

        }

       public double GetInterval()
        {

            return Double.Parse(ConfigurationManager.AppSettings["IntervalMinutes"].ToString()) * 60000;

    }


        void FSWatcher_Tick(object source, ElapsedEventArgs e)
        {
            try
            {
                foldersweep.FileSweep();
            }
            catch (Exception ex)
            {
                log.Fatal(ex.ToString());
            }
        }

      

        protected override void OnStop()
        {
            log.Info("Service Stopped");
        }
    }
}
