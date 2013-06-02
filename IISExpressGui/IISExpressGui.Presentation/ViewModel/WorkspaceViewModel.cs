using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace IISExpressGui.Presentation.ViewModel
{
    /// <summary>
    /// This ViewModelBase subclass requests to be removed from the UI when its CloseCommand executes.
    /// This class is abstract.
    /// </summary>
    public abstract class WorkspaceViewModel : ViewModelBase
    {
        #region Fields

        RelayCommand closeCommand;

        #endregion

        //#region Ctor

        //protected WorkspaceViewModel()
        //{
        //}

        //#endregion

        #region Commands

        /// <summary>
        /// Returns the command that, when invoked, attempts
        /// to remove this workspace from the user interface.
        /// </summary>
        public ICommand CloseCommand
        {
            get
            {
                if (closeCommand == null)
                {
                    closeCommand = new RelayCommand(param => this.OnRequestClose());
                }
                return closeCommand;
            }
        }

        #endregion

        #region RequestClose [event]

        /// <summary>
        /// Raised when this workspace should be removed from the UI.
        /// </summary>
        public event EventHandler RequestClose;

        void OnRequestClose()
        {
            EventHandler handler = this.RequestClose;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        #endregion
    }
}
