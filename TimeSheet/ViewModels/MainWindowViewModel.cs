﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using Microsoft.SharePoint.Client;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace TimeSheet
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Constant fields

        private static readonly CalendarDayModel[] holidays = 
        {
            new CalendarDayModel("New Years Day", new DateTime(2013, 1, 1), isHoliday:true),
            new CalendarDayModel("MLK Day", new DateTime(2013, 1, 21), isHoliday:true),
            new CalendarDayModel("Presidents Day", new DateTime(2013, 2, 18), isHoliday:true),
            new CalendarDayModel("Memorial Day", new DateTime(2013, 5, 27), isHoliday:true),
            new CalendarDayModel("Independence Day", new DateTime(2013, 7, 4), isHoliday:true),
            new CalendarDayModel("Labor Day", new DateTime(2013, 9, 2), isHoliday:true),
            new CalendarDayModel("Columbus Day", new DateTime(2013, 10, 14), isHoliday:true),
            new CalendarDayModel("Veterans Day", new DateTime(2013, 11, 11), isHoliday:true),
            new CalendarDayModel("Thanksgiving Day", new DateTime(2013, 11, 28), isHoliday:true),
            new CalendarDayModel("Christmas Day", new DateTime(2013, 12, 25), isHoliday:true),
        };

        #endregion

        #region Constructor

        public MainWindowViewModel()
        {
            DateTime now = DateTime.Now;
            DateTime startOfThisWeek = now.AddDays(-(int)now.DayOfWeek + 1).Date;

            Weeks = new List<WeekModel>();

            Weeks.Add(new WeekModel(startOfThisWeek));
            for(int i = 1; i <= 3; i++)
            {
                Weeks.Add(new WeekModel(startOfThisWeek.AddDays(-i * 7)));
            }

            SelectedWeek = Weeks[1];

            GetDataCommand = new GetDataCommand();

            CloseErrorStatusBarCommand = new CloseErrorStatusBarCommand();
        }

        #endregion

        #region Properties

        public List<WeekModel> Weeks { get; set; }

        public List<ChangesetModel> ChangesetsForWeek { get; set; }

        public List<CalendarDayModel> CalendarDaysForWeek { get; set; }

        public GetDataCommand GetDataCommand { get; set; }

        public CloseErrorStatusBarCommand CloseErrorStatusBarCommand { get; set; }

        public string CurrentUser
        {
            get { return "Current User: " + Environment.UserName; }
        }

        private string _output;

        public string Output
        {
            get { return _output; }
            set
            {
                if(_output != value)
                {
                    _output = value;
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

        private bool _gettingWorkItems = false;
        private bool _checkingCalendar = false;

        #endregion

        #region Public Methods

        public void GetData()
        {
            BackgroundWorker workItemsWorker = new BackgroundWorker();
            workItemsWorker.DoWork += new DoWorkEventHandler(GetWorkItemsAsync);
            workItemsWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(GetWorkItemsCompleted);

            BackgroundWorker calendarWorker = null;
            if(CheckCalendar)
            {
                calendarWorker = new BackgroundWorker();
                calendarWorker.DoWork += new DoWorkEventHandler(CheckCalendarAsync);
                calendarWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(CheckCalendarCompleted);
            }

            GetDataCommand.SetCanExecute(false);
            IsLoading = true;
            Output = "";

            _gettingWorkItems = true;
            workItemsWorker.RunWorkerAsync();
            if (CheckCalendar)
            {
                _checkingCalendar = true;
                calendarWorker.RunWorkerAsync();
            }
        }

        #endregion

        #region Private Methods

        private void GetDataComplete()
        {
            if (!_gettingWorkItems && (!CheckCalendar || (CheckCalendar && !_checkingCalendar)))
            {
                IsLoading = false;
                Output = GenerateOutput();
            }
        }

        private void CheckCalendarCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _checkingCalendar = false;
            if (e.Error != null)
            {
                ErrorMessages.Add("Error: " + e.Error.Message);
                ShowErrorStatusBar = true;
            }
            GetDataComplete();
        }

        private void CheckCalendarAsync(object sender, DoWorkEventArgs e)
        {
            CalendarDaysForWeek = new List<CalendarDayModel>();

            using (Microsoft.SharePoint.Client.ClientContext client = new Microsoft.SharePoint.Client.ClientContext(ConfigurationManager.AppSettings["SharePointWebUrl"]))
            {
                List list = client.Web.Lists.GetByTitle(ConfigurationManager.AppSettings["SharepointCalendarName"]);
                CamlQuery camlQuery = new CamlQuery();

                camlQuery.ViewXml =
                    @"<View>
                    <Query>
                        <Where>
                        <And>
                            <Eq>
                                <FieldRef Name='Author' LookupId='TRUE' />
                                <Value Type='Integer'><UserID/></Value>
                            </Eq>
                            <Or>
                                <And>
                                    <Geq>
                                        <FieldRef Name='EventDate' />
                                        <Value IncludeTimeValue='FALSE' Type='DateTime'>" + SelectedWeek.StartOfWeek.Date.ToString("o") + @"</Value>
                                    </Geq>
                                    <Leq>
                                        <FieldRef Name='EventDate' />
                                        <Value IncludeTimeValue='FALSE' Type='DateTime'>" + SelectedWeek.EndOfWeek.Date.ToString("o") + @"</Value>
                                    </Leq>
                                </And>
                                <And>
                                    <Geq>
                                        <FieldRef Name='EndDate' />
                                        <Value IncludeTimeValue='FALSE' Type='DateTime'>" + SelectedWeek.StartOfWeek.Date.ToString("o") + @"</Value>
                                    </Geq>
                                    <Leq>
                                        <FieldRef Name='EndDate' />
                                        <Value IncludeTimeValue='FALSE' Type='DateTime'>" + SelectedWeek.EndOfWeek.Date.ToString("o") + @"</Value>
                                    </Leq>
                                </And>
                            </Or>
                        </And>
                        </Where>
                    </Query>
                    <RowLimit>100</RowLimit>
                    </View>";

                ListItemCollection listItems = list.GetItems(camlQuery);
                client.Load(
                    listItems,
                    items => items
                                    .Include(
                                        item => item["Title"],
                                        item => item["EventDate"],
                                        item => item["EndDate"]));
                client.ExecuteQuery();

                foreach (ListItem listItem in listItems)
                    CalendarDaysForWeek.Add(new CalendarDayModel(listItem));
            }
        }

        private void GetWorkItemsCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            _gettingWorkItems = false;
            if (e.Error != null)
            {
                ErrorMessages.Add("Error: " + e.Error.Message);
                ShowErrorStatusBar = true;
            }
            GetDataComplete();
        }

        private void GetWorkItemsAsync(object sender, DoWorkEventArgs e)
        {
            using (TeamFoundationServer tfsServer = TeamFoundationServerFactory.GetServer(ConfigurationManager.AppSettings["TfsServerUrl"]))
            {
                tfsServer.Authenticate();

                VersionControlServer vcServer = tfsServer.GetService<VersionControlServer>();
                string projectPath = vcServer.GetTeamProject(ConfigurationManager.AppSettings["TfsProjectName"]).ServerItem;

                ChangesetsForWeek = vcServer.QueryHistory(projectPath,
                                                            VersionSpec.Latest,
                                                            0,
                                                            RecursionType.Full, 
                                                            Environment.UserName,
                                                            null,
                                                            null,
                                                            200,
                                                            true,
                                                            false).Cast<Changeset>()
                    .Where(
                        cs =>
                        cs.CreationDate.Date >= SelectedWeek.StartOfWeek.Date &&
                        cs.CreationDate.Date <= SelectedWeek.EndOfWeek.Date)
                    .Select(cs => new ChangesetModel(cs))
                    .ToList();
            }
        }

        private string GenerateOutput()
        {
            if(ChangesetsForWeek == null)
                return "";

            StringBuilder _output = new StringBuilder();
            for (int i = 0; i < 7; i++)
            {
                DateTime day = SelectedWeek.StartOfWeek.AddDays(i);
                var changesetsForDay =
                    ChangesetsForWeek.Where(cs => cs.CreationDate.Date == day.Date).OrderBy(
                        cs => cs.CreationDate);
                var workItemsForDay =
                    changesetsForDay.SelectMany(cs => cs.WorkItems).Distinct(new WorkItemModelComparer());
                _output.AppendLine(day.DayOfWeek + " - " + day.ToShortDateString());
                _output.AppendLine("--------------------------");
                if (CheckCalendar)
                {
                    var holidayItemsForDay = holidays.Where(cd => day == cd.StartDay);
                    foreach (var h in holidayItemsForDay)
                        _output.AppendLine("State Holiday: " + h.Title + "\n");

                    if (CalendarDaysForWeek != null)
                    {
                        var calendarItemsForDay = CalendarDaysForWeek.Where(cd => day >= cd.StartDay && day <= cd.EndDay);
                        foreach (var c in calendarItemsForDay)
                            _output.AppendLine("Calendar: " + c.Title + "\n");
                    }
                }
                foreach (var workItem in workItemsForDay)
                {
                    if (_showWorkItemType)
                        _output.Append(workItem.Type + " ");

                    if (ShowCompletedHours && workItem.Type == "Task")
                    {
                        _output.Append(workItem.ID + " - " + workItem.Title);
                        _output.AppendLine(" (" + workItem.CompletedWork + " hours)");
                    }
                    else
                    {
                        _output.AppendLine(workItem.ID + " - " + workItem.Title);
                    }

                    if (ShowComments)
                    {
                        var changesetsForWorkItemOnThisDay =
                            changesetsForDay.Where(cs => cs.WorkItems.Contains(workItem, new WorkItemModelComparer()))
                                .OrderBy(cs => cs.CreationDate);
                        foreach (var changeset in changesetsForWorkItemOnThisDay)
                        {
                            _output.AppendLine("Changeset " + changeset.ID + ": " + changeset.Comment);
                        }
                    }
                    _output.AppendLine();
                }
                _output.AppendLine();
            }

            return _output.ToString();
        }

        #endregion
    }
}
