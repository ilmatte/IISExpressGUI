using IISExpressGui.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IISExpressGui.Presentation.ViewModel
{
    public class Mediator : IMediator
    {
        /// <summary>
        /// Raised when a website is added.
        /// </summary>
        public event EventHandler<WebSiteAddedEventArgs> WebSiteAdded;

        public void OnWebSiteAdded(WebSite webSite)
        {
            if (this.WebSiteAdded != null)
            {
                this.WebSiteAdded(this, new WebSiteAddedEventArgs(webSite));
            }
        }
    }
}
