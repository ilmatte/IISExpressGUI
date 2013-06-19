using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IISExpressGui.Domain
{
    public class IISExpress
    {
        Process associatedProcess;

        static IISExpress()
        {
            string programFilesFolder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            IISDefaultPath = Path.Combine(programFilesFolder, @"IIS Express\IISExpress.exe");
            var documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            ApplicationHostConfigDefaultPath = Path.Combine(documentsFolder, @"IISExpress\config\applicationhost.config");
             //= Path.GetFullPath(applicationHostConfig);            
        }

        private IISExpress(Process iisExpressProcess)
        {
            if (iisExpressProcess == null)
            {
                throw new ArgumentNullException("iisExpressProcess");
            }

            this.associatedProcess = iisExpressProcess;
        }

        public static string IISDefaultPath { get; private set; }

        public static string ApplicationHostConfigDefaultPath { get; private set; }

        public static void Initialize()
        {
            var startInfo = GetProcessStartInfo();
            var process = new Process { StartInfo = startInfo };
            process.Start();
            int elapsed = 0;
            var timeout = TimeSpan.FromSeconds(10);
            while (!File.Exists(ApplicationHostConfigDefaultPath) && (elapsed < timeout.TotalMilliseconds))
            {
                Thread.Sleep(1000);
                elapsed += 1000;
            }
            if (!process.HasExited)
            {
                process.Kill();
            }  
        }
        
        public static IISExpress Start(WebSite webSite)
        {
            var startInfo = GetProcessStartInfo();
            startInfo.Arguments = string.Format("/site:{0}", webSite.Name);
            var process = new Process { StartInfo = startInfo };
            process.Start();
            return new IISExpress(process);
        }

        public void Stop()
        {
            if ((this.associatedProcess != null) && !this.associatedProcess.HasExited)
            {
                this.associatedProcess.Kill();
            }        
        }

        public static string GetActualPhysicalPath(WebSite webSite)
        {
            if (webSite.PhysicalPath == null)
            {
                return string.Empty;
            }

            var documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var iisSitesHome = Path.Combine(documentsFolder, @"My Web Sites");
            return webSite.PhysicalPath.Replace("%IIS_SITES_HOME%", iisSitesHome);
        }

        public static string GetEscapedPhysicalPath(string inputPhysicalPath)
        {
            var documentsFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var iisSitesHome = Path.Combine(documentsFolder, @"My Web Sites");
            return inputPhysicalPath.Replace(iisSitesHome, "%IIS_SITES_HOME%");
        }

        private static ProcessStartInfo GetProcessStartInfo()
        {
            return new ProcessStartInfo
            {
                FileName = IISDefaultPath,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                //WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
            };
        }
    }
}
