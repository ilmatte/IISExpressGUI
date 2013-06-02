using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace IISExpressGui.Presentation
{
    /// <summary>
    /// A command whose sole purpose is to relay its functionality to other objects by invoking delegates. 
    /// The default return value for the CanExecute method is 'true'.
    /// 
    /// Per ogni Command bindato a un elemento dell'interfaccia, WPF invoca CanExecute e se ritorna false
    /// l'elemento grafico viene disabilitato e non verrà mai invocato Execute.
    /// WPF è anche in ascolto dell'evento CanExecuteChanged per effettuare una nuova invocazione di CanExecute.
    /// Per i RoutedEvents tale evento è scatenato ad ogni interazione sostanziale con l'interfaccia (e.g.: cambio di
    /// selezione, di focus, etc). Per i comandi custom occorre scatenare noi l'evento quando necessario.
    /// Per simulare il comportamento che si ha per i RoutedEvents basta implementare legando l'evento
    /// all'evento. CommandManager.RequerySuggested che prende le decisioni in base alle interazioni con la GUI suddette.
    /// </summary>
    public class RelayCommand : ICommand
    {
        #region Fields

        readonly Action<object> executeAction;
        readonly Predicate<object> canExecuteAction;

        #endregion

        #region Ctor

        /// <summary>
        /// Creates a new command that can always execute.
        /// </summary>
        /// <param name="execute">The execution logic.</param>
        public RelayCommand(Action<object> execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Creates a new command.
        /// </summary>
        /// <param name="execute">The execution logic. The delegate to be executed as command action.</param>
        /// <param name="canExecute">The execution status logic. The delegate to be
        /// executed to check if the command is allowed to be executed.</param>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute)
        {
            if (execute == null) { throw new ArgumentNullException("execute"); }

            this.executeAction = execute;
            this.canExecuteAction = canExecute;
        }

        #endregion

        #region ICommand Members

        [DebuggerStepThrough]
        public bool CanExecute(object parameter)
        {
            return (canExecuteAction == null) ? true : canExecuteAction(parameter);
        }

        /// <summary>
        /// WPF è in ascolto dell'evento CanExecuteChanged per effettuare una nuova invocazione di CanExecute.
        /// Per i RoutedEvents tale evento è scatenato ad ogni interazione sostanziale con l'interfaccia (e.g.: cambio di
        /// selezione, di focus, etc). Per i comandi custom occorre scatenare noi l'evento quando necessario.
        /// Per simulare il comportamento che si ha per i RoutedEvents basta implementare legando l'evento
        /// all'evento. CommandManager.RequerySuggested che prende le decisioni in base alle interazioni con la GUI suddette.
        /// 
        /// 1. CanExecuteChanged notifies any command sources (like a Button or MenuItem) that are bound to that ICommand
        /// that the value returned by CanExecute has changed. 
        /// Command sources care about this because they generally need to update their status accordingly 
        /// (eg. a Button will disable itself if CanExecute() returns false).
        /// 2. The CommandManager.RequerySuggested event is raised whenever the CommandManager thinks that
        /// something has changed that will affect the ability of commands to execute. 
        /// This might be a change of focus, for example. Turns out that this event fires a lot.
        /// </summary>
        /// <remarks>Per una spiegazione sommaria su questa implementazione, l'autore:
        /// http://joshsmithonwpf.wordpress.com/2008/06/17/allowing-commandmanager-to-query-your-icommand-objects/
        /// Sarà comunque da migliorare per permettere di decidere esplicitamente quando scatendare l'evento.
        /// Mi pare di capire che si fa invocando al momento opportuno: 
        /// CommandManager.InvalidateRequerySuggested()
        /// </remarks>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public void Execute(object parameter)
        {
            executeAction(parameter);
        }

        #endregion
    }
}
