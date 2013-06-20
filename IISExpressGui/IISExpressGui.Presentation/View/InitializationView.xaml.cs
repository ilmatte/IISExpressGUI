using IISExpressGui.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace IISExpressGui.Presentation.View
{
    /// <summary>
    /// Logica di interazione per InitializationView.xaml
    /// </summary>
    public partial class InitializationView : Window
    {
        public InitializationView()
        {
            InitializeComponent();
        }

        public string ResultText
        {
            set
            {
                this.lblProgress.Text = value;
            }
        }
    
        private void NoButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void YesButton_Click(object sender, RoutedEventArgs e)
        {
            busyIndicator.IsBusy = true;            
            BackgroundWorker worker = new BackgroundWorker();
            //worker.WorkerReportsProgress = true;

            worker.DoWork += delegate(object s, DoWorkEventArgs args)
            {
                Thread.Sleep(1000);
                args.Result = IISExpress.Initialize();
            };

            worker.RunWorkerCompleted += delegate(object s, RunWorkerCompletedEventArgs args)
            {
                busyIndicator.IsBusy = false;
                bool isTimedOut = false;
                if (args.Error != null)
                {
                    ResultText = "There was an error! " + args.Error;
                }
                else
                {
                    isTimedOut = !((bool)args.Result);
                    ResultText = isTimedOut ? "Timeout During Initialization" : "Initialization Done!";
                }
                this.DialogResult = !isTimedOut;
            };

            //worker.ProgressChanged += delegate(object s, ProgressChangedEventArgs args)
            //{
            //    ResultText = args.ProgressPercentage.ToString() + "%";
            //};

            if (worker.IsBusy != true)
            {
                worker.RunWorkerAsync();
            }
        }
    }
}
