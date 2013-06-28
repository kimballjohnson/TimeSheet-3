using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Windows.Input;
using TimeSheet.Interfaces;
using TimeSheet.Views;
using TimeSheet.Models;

namespace TimeSheet.ViewModels
{
    public class MainWindowViewModel : ViewModelBase, IMainWindowViewModel
    {
        #region Constructor

        public MainWindowViewModel()
        {
            DateTime now = DateTime.Now;
            DateTime startOfThisWeek = now.AddDays(-(int)now.DayOfWeek + 1).Date;

            Weeks = new List<WeekModel>();

            Weeks.Add(new WeekModel(startOfThisWeek));
            for(int i = 1; i <= ConfigManager.NumberOfWeeks - 1; i++)
            {
                Weeks.Add(new WeekModel(startOfThisWeek.AddDays(-i * 7)));
            }

            SelectedWeek = Weeks[1];

            GetDataCommand = new GetDataCommand();

            CloseErrorStatusBarCommand = new CloseErrorStatusBarCommand();

            NavigateCommand = new NavigateCommand();

            HolidayCalculator hc = new HolidayCalculator(now.AddMonths(-11), ConfigManager.HolidayFile);
            foreach (HolidayCalculator.Holiday h in hc.OrderedHolidays)
                holidays.Add(new CalendarDayModel(h));

            CurrentUser = Environment.UserDomainName + "\\" + Environment.UserName;

            DataService.GetChangesetsCompleted += DataService_GetChangesetsCompleted;
            DataService.GetCalendarDaysCompleted += DataService_GetCalendarDaysCompleted;
        }
   
        #endregion

        #region Properties

        public List<WeekModel> Weeks { get; set; }

        public GetDataCommand GetDataCommand { get; set; }

        public ICommand CloseErrorStatusBarCommand { get; set; }

        public ICommand NavigateCommand { get; set; }

        public OptionsView OptionsWindow { get; set; }

        private string _currentUser;

        public string CurrentUser
        {
            get { return _currentUser; }
            set
            {
                if (_currentUser != value)
                {
                    _currentUser = value;
                    RaisePropertyChanged("CurrentUser");
                }
            }
        }

        private string output;

        public string Output
        {
            get { return output; }
            set
            {
                if(output != value)
                {
                    output = value;
                    RaisePropertyChanged("Output");
                }
            }
        }

        private bool _showComments;

        public bool ShowComments
        {
            get { return _showComments; }
            set
            {
                if (_showComments != value)
                {
                    _showComments = value;
                    RaisePropertyChanged("ShowComments");
                    Output = GenerateOutput();
                }
            }
        }

        private bool _showCompletedHours;

        public bool ShowCompletedHours
        {
            get { return _showCompletedHours; }
            set
            {
                if (_showCompletedHours != value)
                {
                    _showCompletedHours = value;
                    RaisePropertyChanged("ShowCompletedHours");
                    Output = GenerateOutput();
                }
            }
        }

        private bool _showWorkItemType;

        public bool ShowWorkItemType
        {
            get { return _showWorkItemType; }
            set
            {
                if (_showWorkItemType != value)
                {
                    _showWorkItemType = value;
                    RaisePropertyChanged("ShowWorkItemType");
                    Output = GenerateOutput();
                }
            }
        }

        private bool _isLoading;

        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    GetDataCommand.SetCanExecute(!value);
                    RaisePropertyChanged("IsLoading");
                }
            }
        }

        private bool _checkCalendar;

        public bool CheckCalendar
        {
            get { return _checkCalendar; }
            set
            {
                if (_checkCalendar != value)
                {
                    _checkCalendar = value;
                    RaisePropertyChanged("CheckCalendar");
                    Output = GenerateOutput();
                }
            }
        }

        private WeekModel _selectedWeek;

        public WeekModel SelectedWeek
        {
            get { return _selectedWeek; }
            set
            {
                if (_selectedWeek != value)
                {
                    _selectedWeek = value;
                    RaisePropertyChanged("SelectedWeek");
                }
            }
        }

        private bool _showErrorStatusBar;

        public bool ShowErrorStatusBar
        {
            get { return _showErrorStatusBar; }
            set
            {
                if (_showErrorStatusBar != value)
                {
                    _showErrorStatusBar = value;
                    RaisePropertyChanged("ShowErrorStatusBar");
                }
            }
        }

        public ObservableCollection<string> _errorMessages = new ObservableCollection<string>();

        public ObservableCollection<string> ErrorMessages
        {
            get { return _errorMessages; }
            set
            {
                if (_errorMessages != value)
                {
                    _errorMessages = value;
                    RaisePropertyChanged("ErrorMessages");
                }
            }
        }

        #endregion

        #region Private Fields

        private List<ChangesetModel> changesetsForWeek = new List<ChangesetModel>(); 
        private List<CalendarDayModel> calendarDaysForWeek = new List<CalendarDayModel>();
        private List<CalendarDayModel> holidays = new List<CalendarDayModel>();

        private bool gettingChangesets = false;
        private bool checkingCalendar = false;

        #endregion

        #region Public Methods

        public void GetData()
        {
            GetDataCommand.SetCanExecute(false);
            IsLoading = true;
            Output = "";

            gettingChangesets = true;
            DataService.GetChangesets();
            if (CheckCalendar)
            {
                checkingCalendar = true;
                DataService.GetCalendarDays();
            }
        }

        #endregion

        #region Private Methods

        private void GetDataComplete()
        {
            if (!gettingChangesets && (!CheckCalendar || (CheckCalendar && !checkingCalendar)))
            {
                IsLoading = false;
                Output = GenerateOutput();
            }
        }

        private void DataService_GetCalendarDaysCompleted(object sender, Services.GetCalendarDaysCompletedEventArgs e)
        {
            checkingCalendar = false;
            if (e.Error != null)
            {
                ErrorMessages.Add("Error: " + e.Error.Message);
                ShowErrorStatusBar = true;
            }
            else
            {
                calendarDaysForWeek = e.CalendarDays;
            }
            GetDataComplete();
        }

        private void DataService_GetChangesetsCompleted(object sender, Services.GetChangesetsCompletedEventArgs e)
        {
            gettingChangesets = false;
            if (e.Error != null)
            {
                ErrorMessages.Add("Error: " + e.Error.Message);
                ShowErrorStatusBar = true;
            }
            else
            {
                changesetsForWeek = e.Changesets;
            }
            GetDataComplete();
        }

        private string GenerateOutput()
        {
            if (changesetsForWeek == null && !CheckCalendar)
                return "";

            StringBuilder output = new StringBuilder();

            for (int i = 0; i < 7; i++)
            {
                DateTime day = SelectedWeek.StartOfWeek.AddDays(i);
                
                output.AppendLine(day.DayOfWeek + " - " + day.ToShortDateString());
                output.AppendLine("--------------------------");

                if (CheckCalendar)
                {
                    var holidayItemsForDay = holidays.Where(cd => day == cd.StartDay);
                    foreach (var h in holidayItemsForDay)
                        output.AppendLine("State Holiday: " + h.Title + "\n");

                    if (calendarDaysForWeek != null)
                    {
                        var calendarItemsForDay = calendarDaysForWeek.Where(cd => day >= cd.StartDay && day <= cd.EndDay);
                        foreach (var c in calendarItemsForDay)
                            output.AppendLine("Calendar: " + c.Title + "\n");
                    }
                }

                if(changesetsForWeek == null)
                    continue;

                var changesetsForDay =
                    changesetsForWeek.Where(cs => cs.CreationDate.Date == day.Date).OrderBy(
                        cs => cs.CreationDate);
                var workItemsForDay =
                    changesetsForDay.SelectMany(cs => cs.WorkItems).Distinct(new WorkItemModelComparer());

                foreach (var workItem in workItemsForDay)
                {
                    if (_showWorkItemType)
                        output.Append(workItem.Type + " ");

                    if (ShowCompletedHours && workItem.Type == "Task")
                    {
                        output.Append(workItem.ID + " - " + workItem.Title);
                        output.AppendLine(" (" + workItem.CompletedWork + " hours)");
                    }
                    else
                    {
                        output.AppendLine(workItem.ID + " - " + workItem.Title);
                    }

                    if (ShowComments)
                    {
                        var changesetsForWorkItemOnThisDay =
                            changesetsForDay.Where(cs => cs.WorkItems.Contains(workItem, new WorkItemModelComparer()))
                                .OrderBy(cs => cs.CreationDate);
                        foreach (var changeset in changesetsForWorkItemOnThisDay)
                        {
                            output.AppendLine("Changeset " + changeset.ID + ": " + changeset.Comment);
                        }
                    }
                    output.AppendLine();
                }
                output.AppendLine();
            }

            return output.ToString();
        }

        #endregion
    }
}
