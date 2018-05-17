using IISExpressGui.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using IISExpressGui.IISManagement.Interfaces;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using Ookii.Dialogs.Wpf;

namespace IISExpressGui.Presentation.ViewModel
{
    /// <summary>
    /// A UI-friendly wrapper for a WebSite object.
    /// </summary>
    public class WebSiteViewModel : WorkspaceViewModel, IDataErrorInfo
    {
        #region Fields

        readonly WebSite webSite;
        readonly IMediator mediator;
        readonly IWebSiteManager webSiteManager;

        bool isNewWebSite;
        bool isModified;
        RelayCommand saveCommand;
        RelayCommand toggleStatusCommand;
        RelayCommand browseCommand;
        RelayCommand deleteWebSiteCommand;
        RelayCommand openBrowserCommand;

        #endregion

        #region Ctor

        public WebSiteViewModel(WebSite webSite,
                                IWebSiteManager webSiteRepository,
                                IMediator mediator)
        {
            if (webSite == null) { throw new ArgumentNullException("webSite"); }
            if (webSiteRepository == null) { throw new ArgumentNullException("webSiteRepository"); }
            if (mediator == null) { throw new ArgumentNullException("mediator"); }

            this.webSite = webSite;
            this.webSiteManager = webSiteRepository;
            this.mediator = mediator;
        }

        #endregion

        #region Properties

        public string Name
        {
            get { return this.webSite.Name; }
            set
            {
                if (value == this.webSite.Name)
                {
                    return;
                }

                this.webSite.Name = value;
                IsModified = true;
                base.OnPropertyChanged("Name");
            }
        }

        public string Url
        {
            get { return this.webSite.Url; }
            set
            {
                if (value == this.webSite.Url)
                {
                    return;
                }

                this.webSite.Url = value;
                IsModified = true;
                base.OnPropertyChanged("Url");
                base.OnPropertyChanged("ViewUrl");
            }
        }

        public string Port
        {
            get { return this.webSite.Port; }
            set
            {
                if (value == this.webSite.Port)
                {
                    return;
                }

                this.webSite.Port = value;
                IsModified = true;
                base.OnPropertyChanged("Port");
                base.OnPropertyChanged("ViewUrl");
            }
        }

        public string ViewUrl => $"{Url}:{Port}";

        public string PhysicalPath
        {
            get { return IISExpress.GetActualPhysicalPath(this.webSite); }
            set
            {
                if (value == this.webSite.PhysicalPath ||
                    value == IISExpress.GetActualPhysicalPath(this.webSite))
                {
                    return;
                }

                this.webSite.PhysicalPath = IISExpress.GetEscapedPhysicalPath(value);
                IsModified = true;
                base.OnPropertyChanged("PhysicalPath");
            }
        }

        public bool IsRunning
        {
            get { return this.webSite.IsRunning; }
            set
            {
                if (value == this.webSite.IsRunning)
                {
                    return;
                }

                this.webSite.IsRunning = value;

                base.OnPropertyChanged("IsRunning");
            }
        }

        public long WebSiteId => webSite?.Id ?? 0;
        public Action<WebSiteViewModel> WhenDeleted { get; set; }

        #endregion

        #region Presentation Properties

        public override string DisplayName
        {
            get
            {
                return String.Format("{0}", webSite.Name);
            }
        }

        /// <summary>
        /// Returns true if this website was created by the user and it has not yet
        /// been saved.
        /// </summary>
        public bool IsNewWebSite
        {
            get { return this.isNewWebSite; }
            set
            {
                if (value == this.isNewWebSite)
                {
                    return;
                }

                this.isNewWebSite = value;

                base.OnPropertyChanged("IsNewWebSite");
            }
        }

        /// <summary>
        /// Returns true if this website properties have
        /// been modified by the user and not saved yet or 
        /// if it is a new website not saved yet.
        /// </summary>
        public bool IsModified
        {
            get { return this.isModified; }
            set
            {
                if (value == this.isModified)
                {
                    return;
                }

                this.isModified = value;

                base.OnPropertyChanged("IsModified");
            }
        }

        /// <summary>
        /// Returns true if this object has no validation errors.
        /// </summary>
        public bool IsValid
        {
            get
            {
                if (IsPhysicalPathValid() && IsPortValid())
                {
                    return true;
                }

                return false;
            }
        }

        #endregion

        #region Commands

        /// <summary>
        /// Returns a command that saves the webSite.
        /// </summary>
        public ICommand SaveCommand
        {
            get
            {
                if (this.saveCommand == null)
                {
                    this.saveCommand = new RelayCommand(param => this.Save(), param => this.CanSave);
                }
                return this.saveCommand;
            }
        }

        /// <summary>
        /// Returns a command that starts/stops the webSite.
        /// </summary>
        public ICommand ToggleStatusCommand
        {
            get
            {
                if (this.toggleStatusCommand == null)
                {
                    this.toggleStatusCommand = new RelayCommand(param => ToggleStatus(), param => CanToggleStatus);
                }
                return this.toggleStatusCommand;
            }
        }

        public ICommand DeleteWebSiteCommand
        {
            get
            {
                if (this.deleteWebSiteCommand == null)
                {
                    this.deleteWebSiteCommand = new RelayCommand(param => DeleteWebSite());
                }
                return this.deleteWebSiteCommand;
            }
        }

        public ICommand OpenBrowserCommand
        {
            get
            {
                if (this.openBrowserCommand == null)
                {
                    this.openBrowserCommand = new RelayCommand(param => OpenWebSite());
                }
                return this.openBrowserCommand;
            }
        }

        /// <summary>
        /// Returns a command that opens a dialog to browse folders.
        /// </summary>
        public ICommand BrowseCommand
        {
            get
            {
                if (this.browseCommand == null)
                {
                    this.browseCommand = new RelayCommand(param => this.Browse(), param => this.CanBrowse);
                }
                return this.browseCommand;
            }
        }

        #endregion

        #region Methods

        public static WebSiteViewModel CreateNew(IWebSiteManager webSiteManager, IMediator mediator)
        {
            var webSite = new WebSite
            {
                Name = "New WebSite",
                Url = "http://localhost",
                Port = "8080",
                IsRunning = false
            };
            return new WebSiteViewModel(webSite, webSiteManager, mediator)
            {
                IsNewWebSite = true,
                IsModified = true
            };
        }

        /// <summary>
        /// Opens Browse Folder Dialog.  This method is invoked by the BrowseCommand.
        /// </summary>
        public void Browse()
        {
            var browseFolderDialog = new VistaFolderBrowserDialog
            {
                ShowNewFolderButton = true
            };
            if (Directory.Exists(PhysicalPath))
            {
                browseFolderDialog.SelectedPath = PhysicalPath;
            }
            if (browseFolderDialog.ShowDialog() == true &&
                !string.IsNullOrWhiteSpace(browseFolderDialog.SelectedPath))
            {
                PhysicalPath = browseFolderDialog.SelectedPath;
            }
        }

        public void OpenWebSite()
        {
            try
            {
                Process.Start(ViewUrl);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Launch website error:{ex}");
            }
        }

        /// <summary>
        /// Returns true if the BrowseCommand can be run.
        /// </summary>
        bool CanBrowse
        {
            get { return true; }
        }

        /// <summary>
        /// Inserts or updates the website to the repository.  This method is invoked by the SaveCommand.
        /// </summary>
        public void Save()
        {
            if (!IsValid)
            {
                throw new InvalidOperationException("Cannot Save an invalid WebSite");
            }

            if (IsNewWebSite)
            {
                this.webSiteManager.Add(webSite);
            }
            else
            {
                this.webSiteManager.Update(webSite);
            }

            //base.OnPropertyChanged("DisplayName");

            IsNewWebSite = false;
            IsModified = false;
        }

        public void DeleteWebSite()
        {
            if (MessageBox.Show("Are you sure to delete this website?", "confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                this.webSiteManager.Remove(webSite.Id);
                WhenDeleted?.Invoke(this);
            }
        }

        /// <summary>
        /// Returns true if the website  is valid and can be saved.
        /// </summary>
        bool CanSave
        {
            get { return IsModified && IsValid; }
        }

        /// <summary>
        /// Starts/Stops website. This method is invoked by the ToggleStatusCommand.
        /// </summary>
        public void ToggleStatus()
        {
            if (IsNewWebSite)
            {
                return;
            }
            else
            {
                this.webSiteManager.ToggleStatus(webSite);
                IsRunning = !IsRunning;
            }
        }

        /// <summary>
        /// Returns true if the website is not a new website
        /// </summary>
        bool CanToggleStatus
        {
            get { return !IsNewWebSite; }
        }

        private bool IsPhysicalPathValid()
        {
            return Directory.Exists(PhysicalPath);
        }

        private bool IsPortValid()
        {
            int port;
            return int.TryParse(Port, out port);
        }

        #endregion

        #region IDataErrorInfo Members

        string IDataErrorInfo.Error
        {
            get { return (this.webSite as IDataErrorInfo).Error; }
        }

        string IDataErrorInfo.this[string propertyName]
        {
            get
            {
                // TODO: add validation to warn if empty folder
                string error = null;
                if (propertyName == "PhysicalPath")
                {
                    error = !IsPhysicalPathValid() ? "Not a Valid Path" : null;
                }
                else if (propertyName == "Port")
                {
                    error = !IsPortValid() ? "Port must be a number" : null;
                }

                // Dirty the commands registered with CommandManager,
                // such as our Save command, so that they are queried
                // to see if they can execute now.
                CommandManager.InvalidateRequerySuggested();
                return error;
            }
        }

        #endregion
    }
}
