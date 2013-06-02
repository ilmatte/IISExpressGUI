using IISExpressGui.Domain;
using IISExpressGui.IISManagement;
using IISExpressGui.IISManagement.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Input;

namespace IISExpressGui.Presentation.ViewModel
{
    public class MainWindowViewModel : WorkspaceViewModel
    {
        #region Fields

        ObservableCollection<WebSiteViewModel> webSites;
        readonly IMediator mediator;
        readonly IWebSiteManager webSiteManager;
        RelayCommand createWebSiteCommand;

        #endregion 

        #region Ctor

        public MainWindowViewModel(IWebSiteManager webSiteManager)
        {
            if (webSiteManager == null)
            {
                throw new ArgumentNullException("webSiteManager");
            }
            this.mediator = new Mediator();
            this.webSiteManager = webSiteManager;
            base.DisplayName = "IIS Express GUI";
        } 

        #endregion

        #region Properties

        /// <summary>
        /// Returns the collection of available Websites to display.
        /// </summary>
        public ObservableCollection<WebSiteViewModel> WebSites
        {
            get
            {
                if (this.webSites == null)
                {
                    List<WebSiteViewModel> webSiteList = LoadAvailableWebSites();
                    this.webSites = new ObservableCollection<WebSiteViewModel>(webSiteList);
                }
                return this.webSites;
            }
        }
        
        #endregion

        #region Commands

        /// <summary>
        /// Returns the command that, when invoked, adds a new WebSite
        /// to the collection of websites managed from the user interface
        /// without adding it to IIS Express yet.
        /// </summary>
        public ICommand CreateWebSiteCommand
        {
            get
            {
                if (this.createWebSiteCommand == null)
                {
                    this.createWebSiteCommand = new RelayCommand(param => CreateWebSite());
                }
                return this.createWebSiteCommand;
            }
        }
        
        #endregion

        #region Methods

        List<WebSiteViewModel> LoadAvailableWebSites()
        {
            // TODO: manage exceptions
            List<WebSiteViewModel> allWebSites = new List<WebSiteViewModel>();
            allWebSites = (from webSite in this.webSiteManager.GetAllWebSites()
                           select new WebSiteViewModel(webSite, this.webSiteManager, this.mediator)
                           ).ToList();
            return allWebSites;
        }
       
        void CreateWebSite()
        {
            var newWebSite = WebSiteViewModel.CreateNew(this.webSiteManager, this.mediator);
            WebSites.Add(newWebSite);
            SetActiveWebSite(newWebSite);
        }

        void SetActiveWebSite(WebSiteViewModel webSite)
        {
            Debug.Assert(WebSites.Contains(webSite));

            ICollectionView collectionView = CollectionViewSource.GetDefaultView(WebSites);
            if (collectionView != null)
            {
                collectionView.MoveCurrentTo(webSite);
            }
        }

        #endregion
    }    
}
