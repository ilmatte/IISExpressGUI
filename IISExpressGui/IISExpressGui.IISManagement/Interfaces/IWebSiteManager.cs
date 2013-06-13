using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IISExpressGui.Domain;

namespace IISExpressGui.IISManagement.Interfaces
{
    public interface IWebSiteManager
    {
        IList<WebSite> GetAllWebSites();

        void Add(WebSite webSite);

        void Update(WebSite webSite);

        void ToggleStatus(WebSite webSite);

        string GetActualPhysicalPath(WebSite webSite);

        string GetEscapedPhysicalPath(string inputPhysicalPath);
    }
}
