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
using System.Windows;
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
        RelayCommand clearInvalidedWebSiteCommand;

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
                    foreach (var model in webSiteList)
                    {
                        model.WhenDeleted = site => this.webSites.Remove(site);
                    }
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

        public ICommand ClearInvalidedWebSiteCommand
        {
            get
            {
                if (this.clearInvalidedWebSiteCommand == null)
                {
                    this.clearInvalidedWebSiteCommand = new RelayCommand(param => ClearInvalidedWebSite());
                }
                return this.clearInvalidedWebSiteCommand;
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

        void ClearInvalidedWebSite()
        {
            var tobeRemoved = new List<WebSiteViewModel>();
            foreach (var webSite in WebSites)
            {
                if (!webSite.IsValid) tobeRemoved.Add(webSite);
            }
            if (tobeRemoved.Count <= 0) return;
            if (MessageBox.Show($"Are you sure to clear the invalided websites? total:{tobeRemoved.Count}"
                    , "confirm"
                    , MessageBoxButton.YesNo
                    , MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                foreach (var viewModel in tobeRemoved)
                {
                    webSiteManager.Remove(viewModel.WebSiteId);
                    WebSites.Remove(viewModel);
                }
            }
        }

        #endregion
    }
}
