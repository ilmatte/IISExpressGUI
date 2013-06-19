using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
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
        
        public static IISExpress Start(WebSite webSite)
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = IISDefaultPath,
                Arguments = string.Format("/site:{0}", webSite.Name),
                RedirectStandardError = true,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                //WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
            };

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
    }
}
