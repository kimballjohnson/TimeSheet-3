using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using TimeSheet.Views;
using TimeSheet.ViewModels;
using TimeSheet.Models;

namespace TimeSheet.Interfaces
{
    public interface IMainWindowViewModel
    {
        List<WeekModel> Weeks { get; set; }

        GetDataCommand GetDataCommand { get; set; }

        CloseErrorStatusBarCommand CloseErrorStatusBarCommand { get; set; }

        NavigateCommand NavigateCommand { get; set; }

        OptionsView OptionsWindow { get; set; }

        string CurrentUser { get; set; }

        string Output { get; set; }

        bool ShowComments { get; set; }

        bool ShowCompletedHours { get; set; }

        bool ShowWorkItemType { get; set; }

        bool IsLoading { get; set; }

        bool CheckCalendar { get; set; }

        WeekModel SelectedWeek { get; set; }

        bool ShowErrorStatusBar { get; set; }

        ObservableCollection<string> ErrorMessages { get; set; }

        void GetData();

    }
}
