using System;
using System.Linq;
using System.Collections.Generic;
using System.Windows.Forms;
using log4net.Config;
using log4net;

namespace WBeParkingPDA
{
    static class Program
    {
        /// <summary>
        /// 應用程式的主要進入點。
        /// </summary>
        [MTAThread]
        static void Main()
        {
            XmlConfigurator.Configure();
            ILog logger = LogManager.GetLogger("Program");
            logger.Info("Program started");
            Application.Run(new Main());
        }
    }
}