using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace TimeSheet
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Startup += new StartupEventHandler(App_Startup);

        }

        void App_Startup(object sender, StartupEventArgs e)
        {
            var view = new MainWindowView();
            var viewModel = ViewModelLocater.MainWindowViewModel;
            view.DataContext = viewModel;

            this.MainWindow = view;
            this.MainWindow.Show();
        }
    }
}
