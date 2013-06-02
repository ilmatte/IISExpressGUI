using IISExpressGui.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IISExpressGui.Presentation.ViewModel
{
    public interface IMediator
    {
        event EventHandler<WebSiteAddedEventArgs> WebSiteAdded;

        void OnWebSiteAdded(WebSite webSite);
    }
}
