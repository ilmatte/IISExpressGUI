﻿using System;
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
            // TODO: filesystemwatch su applicationhost.config per cambiamenti e ricaricare lista siti
            // TODO: check applicationhost.config existence
            // TODO: if not found launch iisexpress to configure default website
            var webSiteManager = new WebSiteManager(IISExpress.ApplicationHostConfigDefaultPath);

            if (!webSiteManager.IsIISExpressInstalled())
            {
                MessageBoxButton buttons = MessageBoxButton.OK;
                MessageBoxImage icon = MessageBoxImage.Error;
                var message = string.Format("IISExpress is not installed in the following path:\r\n\r\n{0}\r\n\r\nThe application cannot Start.",
                                            webSiteManager.IISPath);
                MessageBox.Show(message, "Application ShutDown", buttons, icon);
                Application.Current.Shutdown();
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
