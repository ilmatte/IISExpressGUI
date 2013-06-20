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
    public class InitializationViewModel : WorkspaceViewModel
    {
        #region Ctor

        public InitializationViewModel(string applicationHostConfigPath)
        {
            if (string.IsNullOrWhiteSpace(applicationHostConfigPath))
            {
                throw new ArgumentNullException("applicationHostConfigPath");
            }

            base.DisplayName = "IIS Express GUI";
            ApplicationHostConfigPath = applicationHostConfigPath;
        } 

        #endregion

        #region Properties

        public string ApplicationHostConfigPath { get; set; }
        
        #endregion
    }    
}
