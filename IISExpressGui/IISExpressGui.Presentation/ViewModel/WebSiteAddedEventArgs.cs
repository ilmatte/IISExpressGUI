using IISExpressGui.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IISExpressGui.Presentation.ViewModel
{
    /// <summary>
    /// Event arguments used by WebSiteAdded event.
    /// </summary>
    public class WebSiteAddedEventArgs : EventArgs
    {
        public WebSiteAddedEventArgs(WebSite newSite)
        {
            this.NewSite = newSite;
        }

        public WebSite NewSite { get; private set; }
    }
}
