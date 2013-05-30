using System;
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

            HolidayCalculator hc = new HolidayCalculator(now.AddMonths(-11), ConfigManager.HolidayFile);
            foreach (HolidayCalculator.Holiday h in hc.OrderedHolidays)
                _holidays.Add(new CalendarDayModel(h));
        }

        #endregion

        #region Properties

        public List<WeekModel> Weeks { get; set; }

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

        private List<ChangesetModel> _changesetsForWeek = new List<ChangesetModel>(); 
        private List<CalendarDayModel> _calendarDaysForWeek = new List<CalendarDayModel>();
        private List<CalendarDayModel> _holidays = new List<CalendarDayModel>();

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

        private void CheckCalendarAsync(object sender, DoWorkEventArgs e)
        {
            _calendarDaysForWeek.Clear();

            using (Microsoft.SharePoint.Client.ClientContext client = new Microsoft.SharePoint.Client.ClientContext(ConfigManager.SharePointWebUrl))
            {
                List list = client.Web.Lists.GetByTitle(ConfigManager.SharepointCalendarName);
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
                    _calendarDaysForWeek.Add(new CalendarDayModel(listItem));
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

        private void GetWorkItemsAsync(object sender, DoWorkEventArgs e)
        {
            _changesetsForWeek.Clear();

            using (TeamFoundationServer tfsServer = TeamFoundationServerFactory.GetServer(ConfigManager.TfsServerUrl))
            {
                tfsServer.Authenticate();

                VersionControlServer vcServer = tfsServer.GetService<VersionControlServer>();
                string projectPath = vcServer.GetTeamProject(ConfigManager.TfsProjectName).ServerItem;

                _changesetsForWeek = vcServer.QueryHistory(projectPath,
                                                            VersionSpec.Latest,
                                                            0,
                                                            RecursionType.Full, 
                                                            Environment.UserName,
                                                            null,
                                                            null,
                                                            ConfigManager.MaxCheckinsPerQuery,
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

        private string GenerateOutput()
        {
            if (_changesetsForWeek == null && !CheckCalendar)
                return "";

            StringBuilder _output = new StringBuilder();

            for (int i = 0; i < 7; i++)
            {
                DateTime day = SelectedWeek.StartOfWeek.AddDays(i);
                
                _output.AppendLine(day.DayOfWeek + " - " + day.ToShortDateString());
                _output.AppendLine("--------------------------");

                if (CheckCalendar)
                {
                    var holidayItemsForDay = _holidays.Where(cd => day == cd.StartDay);
                    foreach (var h in holidayItemsForDay)
                        _output.AppendLine("State Holiday: " + h.Title + "\n");

                    if (_calendarDaysForWeek != null)
                    {
                        var calendarItemsForDay = _calendarDaysForWeek.Where(cd => day >= cd.StartDay && day <= cd.EndDay);
                        foreach (var c in calendarItemsForDay)
                            _output.AppendLine("Calendar: " + c.Title + "\n");
                    }
                }

                if(_changesetsForWeek == null)
                    continue;

                var changesetsForDay =
                    _changesetsForWeek.Where(cs => cs.CreationDate.Date == day.Date).OrderBy(
                        cs => cs.CreationDate);
                var workItemsForDay =
                    changesetsForDay.SelectMany(cs => cs.WorkItems).Distinct(new WorkItemModelComparer());

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
