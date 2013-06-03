using IISExpressGui.Domain;
using IISExpressGui.IISManagement.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.IO;

namespace IISExpressGui.IISManagement
{
    public class WebSiteManager : IWebSiteManager
    {
        string applicationHostConfigPath;
        XmlDocument applicationHostConfig = new XmlDocument();
        Dictionary<long, Process> runningProcesses = new Dictionary<long, Process>();

        public WebSiteManager(string applicationHostConfigPath)
        {
            if (string.IsNullOrWhiteSpace(applicationHostConfigPath))
            {
                throw new ArgumentNullException("applicationHostConfigPath");
            }
            this.applicationHostConfigPath = applicationHostConfigPath;
            this.applicationHostConfig.Load(applicationHostConfigPath);
        }

        public IList<WebSite> GetAllWebSites()
        {
            var sitesList = this.applicationHostConfig.SelectNodes("descendant::site");
            var webSites = new List<WebSite>();
            foreach (XmlNode site in sitesList)
            {
                var physicalPathNode = site.SelectSingleNode("descendant::virtualDirectory/@physicalPath ");
                var bindingNode = site.SelectSingleNode("descendant::binding");
                string protocol;
                string url = null;
                string port = null;
                if (bindingNode != null)
                {
                    protocol = (bindingNode.Attributes["protocol"] != null) ? bindingNode.Attributes["protocol"].Value : string.Empty;
                    var bindingInfo = (bindingNode.Attributes["bindingInformation"] != null) ? bindingNode.Attributes["bindingInformation"].Value : string.Empty;
                    if (!string.IsNullOrWhiteSpace(bindingInfo))
                    {
                        // TODO: replace url property with: protocol, address, port and change here and add and update
                        //:8081:localhost
                        var regex = new Regex(@"^\*?:(?<port>\d+):(?<address>.+)$");
                        var match = regex.Match(bindingInfo);
                        if (match.Groups.Count != 3)
                        {
                            throw new Exception("invalid binding" + bindingInfo);
                        }
                        var format = "{0}://{1}";
                        url = string.Format(format, protocol, match.Groups["address"].Value);
                        port = match.Groups["port"].Value;
                    }
                }

                var webSite = new WebSite
                {
                    Id = Convert.ToInt64(site.Attributes["id"].Value),
                    Name = site.Attributes["name"].Value,
                    PhysicalPath = (physicalPathNode == null) ? string.Empty : physicalPathNode.Value,
                    Url = url,
                    Port = port
                };
                webSites.Add(webSite);
            }
            return webSites;
        }

        public void Add(WebSite webSite)
        {
            if (webSite == null)
            {
                throw new ArgumentNullException("webSite");
            }

            // TODO: validazione path e url

            var xDocument = XDocument.Parse(this.applicationHostConfig.OuterXml);
            long maxId = xDocument.Root.Descendants("site")
               .Max(x => (long)x.Attribute("id"));
            long nextId = maxId + 1;

            var sitesNode = this.applicationHostConfig.SelectSingleNode("descendant::sites");
            var newSiteNode = this.applicationHostConfig.CreateElement("site");
            newSiteNode.SetAttribute("name", webSite.Name);
            newSiteNode.SetAttribute("id", nextId.ToString());
            newSiteNode.SetAttribute("serverAutoStart", "true");

            var newApplicationNode = this.applicationHostConfig.CreateElement("application");
            newApplicationNode.SetAttribute("path", "/");
            var newVirtualDirectoryNode = this.applicationHostConfig.CreateElement("virtualDirectory");
            newVirtualDirectoryNode.SetAttribute("path", "/");
            newVirtualDirectoryNode.SetAttribute("physicalPath", webSite.PhysicalPath);
            newApplicationNode.AppendChild(newVirtualDirectoryNode);
            newSiteNode.AppendChild(newApplicationNode);

            var newBindingsNode = this.applicationHostConfig.CreateElement("bindings");
            var newBindingNode = this.applicationHostConfig.CreateElement("binding");
            newBindingNode.SetAttribute("protocol", "http");
            newBindingNode.SetAttribute("bindingInformation", string.Format(":{0}:localhost", webSite.Port));
            newBindingsNode.AppendChild(newBindingNode);
            newSiteNode.AppendChild(newBindingsNode);

            sitesNode.PrependChild(newSiteNode);
            this.applicationHostConfig.Save(this.applicationHostConfigPath);
            webSite.Id = nextId;
        }

        public void Update(WebSite webSite)
        {
            if (webSite == null)
            {
                throw new ArgumentNullException("webSite");
            }

            if (webSite.IsRunning)
            {
                Stop(webSite);
            }
            var siteByIdQuery = string.Format("descendant::site[@id='{0}']", webSite.Id);
            var siteNode = this.applicationHostConfig.SelectSingleNode(siteByIdQuery);
            siteNode.Attributes["name"].Value = webSite.Name;
            var virtualDirectoryNode = siteNode.SelectSingleNode("descendant::virtualDirectory");
            virtualDirectoryNode.Attributes["physicalPath"].Value = webSite.PhysicalPath;
            var bindingNode = siteNode.SelectSingleNode("descendant::binding");
            bindingNode.Attributes["bindingInformation"].Value = string.Format(":{0}:localhost", webSite.Port);
            this.applicationHostConfig.Save(this.applicationHostConfigPath);
        }

        public void ToggleStatus(WebSite webSite)
        {
            if (webSite == null)
            {
                throw new ArgumentNullException("webSite");
            }

            if (!webSite.IsRunning)
            {
                var programFilesFolder = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                var startInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(programFilesFolder, @"IIS Express\IISExpress.exe"),
                    Arguments = string.Format("/site:{0}", webSite.Name),
                    RedirectStandardError = true,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    //WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden
                };

                var process = new Process { StartInfo = startInfo };
                this.runningProcesses[webSite.Id] = process;
                process.Start();
            }
            else
            {
                Stop(webSite);
            }
        }

        private void Stop(WebSite webSite)
        {
            var process = this.runningProcesses[webSite.Id];
            if ((process != null) && !process.HasExited)
            {
                process.Kill();
            }        
        }
    }
}
