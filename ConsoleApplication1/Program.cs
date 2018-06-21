using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Saga.GMD.ReturnFeed.Service;
using Saga.GMD.ReturnFeed.Service.BLL;

namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            FolderSweepBLL foldersweep = FolderSweepBLL.Instance;

            foldersweep.FileSweep();

            Console.ReadLine();
        }
    }
}
