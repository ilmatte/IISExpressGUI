using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Threading;
using IISExpressGui.Presentation.ViewModel;
using System.IO;
using System.Xml;
using IISExpressGui.IISManagement;
using IISExpressGui.Domain;

namespace IISExpressGui.Presentation
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Application.Current.DispatcherUnhandledException += App_DispatcherUnhandledException;

            // TODO: modify url with link and edit button to change it to http label + 2 textboxes
            // TODO: filesystemwatch su applicationhost.config to monitor changes and reload sites list
            // TODO: check which sites are running at startup
            var appHostPath = IISExpress.ApplicationHostConfigDefaultPath;
            var webSiteManager = new WebSiteManager(appHostPath);

            // TODO: extract in a startup manager
            if (!webSiteManager.IsIISExpressInstalled())
            {
                MessageBoxButton buttons = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Error;
                var message = string.Format("IISExpress is not installed in the following path:\r\n\r\n{0}\r\n\r\nThe application cannot Start.",
                                            webSiteManager.IISPath);
                MessageBox.Show(message, "Application ShutDown", buttons, icon);
                Application.Current.Shutdown();
            }
            if (!webSiteManager.ApplicationHostConfigExists())
            {
                MessageBoxButton buttons = MessageBoxButton.YesNo;
                MessageBoxImage icon = MessageBoxImage.Question;
                var message = string.Format(
@"applicationhost.config file not found in the following path:

{0}

The application will run an IIS Express instance.
Such instance will create the applicationhost.config file with the default website: WebSite1.
Once created the IIS Express instance will be stopped.
Continue?",
                                            appHostPath);
                var result = MessageBox.Show(message, "Confirm Initialization", buttons, icon);
                if (result == MessageBoxResult.Yes)
                {
                    IISExpress.Initialize();
                }
                else
                {
                    MessageBox.Show("Canno Start IIS Express GUI", "Application ShutDown", buttons, icon);
                    Application.Current.Shutdown();
                }
            }

            // Create the ViewModel to which the main window binds.
            var viewModel = new MainWindowViewModel(webSiteManager);
            MainWindow window = new MainWindow();

            // When the ViewModel asks to be closed, close the window.
            EventHandler handler = null;
            handler = delegate
            {
                viewModel.RequestClose -= handler;
                window.Close();
            };
            viewModel.RequestClose += handler;

            // Allow all controls in the window to bind to the ViewModel by setting the 
            // DataContext, which propagates down the element tree.
            window.DataContext = viewModel;
            window.Show();
        }

        void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // Process unhandled exception

            string errorMessage = string.Format("An unhandled exception occurred: {0}", e.Exception.Message);
            MessageBox.Show(errorMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

            // Prevent default unhandled exception processing
            e.Handled = true;
        }
    }
}
