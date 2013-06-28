using Microsoft.SharePoint.Client;
using Microsoft.TeamFoundation.Client;
using Microsoft.TeamFoundation.VersionControl.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using TimeSheet.Interfaces;
using TimeSheet.Models;
using TimeSheet.ViewModels;

namespace TimeSheet.Services
{
    public class DataService : IDataService
    {
        #region Public Methods

        public void GetCalendarDays()
        {
            BackgroundWorker calendarWorker = new BackgroundWorker();
            calendarWorker.DoWork += CheckCalendarAsync;
            calendarWorker.RunWorkerCompleted += CheckCalendarAsyncCompleted;
            calendarWorker.RunWorkerAsync();
        }

        public void GetChangesets()
        {
            BackgroundWorker changesetsWorker = new BackgroundWorker();
            changesetsWorker.DoWork += GetChangesetsAsync;
            changesetsWorker.RunWorkerCompleted += GetChangesetsAsyncCompleted;
            changesetsWorker.RunWorkerAsync();
        }

        #endregion

        #region Private Methods

        private void GetChangesetsAsync(object sender, DoWorkEventArgs e)
        {
            changesetsForWeek.Clear();

            TeamFoundationServer tfs = null;

            try
            {
                var optionsVM = ViewModelLocater.OptionsViewModel;
                var mainWindowVM = ViewModelLocater.MainWindowViewModel;

                if (optionsVM.SpecifyUserCredentials && optionsVM.CredentialsAreValid)
                {
                    NetworkCredential cred = new NetworkCredential(optionsVM.Username, optionsVM.Password, optionsVM.Domain);
                    tfs = new TeamFoundationServer(ConfigManager.TfsServerUrl, cred);

                }
                else
                    tfs = TeamFoundationServerFactory.GetServer(ConfigManager.TfsServerUrl);

                tfs.EnsureAuthenticated();

                VersionControlServer vcServer = tfs.GetService<VersionControlServer>();
                string projectPath = vcServer.GetTeamProject(ConfigManager.TfsProjectName).ServerItem;

                changesetsForWeek = vcServer.QueryHistory(projectPath,
                                                            VersionSpec.Latest,
                                                            0,
                                                            RecursionType.Full,
                                                            mainWindowVM.CurrentUser,
                                                            null,
                                                            null,
                                                            ConfigManager.MaxCheckinsPerQuery,
                                                            true,
                                                            false).Cast<Changeset>()
                    .Where(
                        cs =>
                        cs.CreationDate.Date >= mainWindowVM.SelectedWeek.StartOfWeek.Date &&
                        cs.CreationDate.Date <= mainWindowVM.SelectedWeek.EndOfWeek.Date)
                    .Select(cs => new ChangesetModel(cs))
                    .ToList();
            }
            finally
            {
                if (tfs != null)
                    tfs.Dispose();
            }
        }

        private void GetChangesetsAsyncCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (GetChangesetsCompleted != null)
                    GetChangesetsCompleted(this, new GetChangesetsCompletedEventArgs() { Changesets = changesetsForWeek });
            }
            else
            {
                if (GetChangesetsCompleted != null)
                    GetChangesetsCompleted(this, new GetChangesetsCompletedEventArgs() { Error = e.Error });
            }
        }

        private void CheckCalendarAsync(object sender, DoWorkEventArgs e)
        {
            calendarDaysForWeek.Clear();

            using (Microsoft.SharePoint.Client.ClientContext client = new Microsoft.SharePoint.Client.ClientContext(ConfigManager.SharePointWebUrl))
            {
                var optionsVM = ViewModelLocater.OptionsViewModel;
                var mainWindowVM = ViewModelLocater.MainWindowViewModel;

                if (optionsVM.SpecifyUserCredentials && optionsVM.CredentialsAreValid)
                {
                    NetworkCredential cred = new NetworkCredential(optionsVM.Username, optionsVM.Password, optionsVM.Domain);
                    client.Credentials = cred;
                }

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
                                    <Value IncludeTimeValue='FALSE' Type='DateTime'>" + mainWindowVM.SelectedWeek.StartOfWeek.Date.ToString("o") + @"</Value>
                                </Geq>
                                <Leq>
                                    <FieldRef Name='EventDate' />
                                    <Value IncludeTimeValue='FALSE' Type='DateTime'>" + mainWindowVM.SelectedWeek.EndOfWeek.Date.ToString("o") + @"</Value>
                                </Leq>
                            </And>
                            <And>
                                <Geq>
                                    <FieldRef Name='EndDate' />
                                    <Value IncludeTimeValue='FALSE' Type='DateTime'>" + mainWindowVM.SelectedWeek.StartOfWeek.Date.ToString("o") + @"</Value>
                                </Geq>
                                <Leq>
                                    <FieldRef Name='EndDate' />
                                    <Value IncludeTimeValue='FALSE' Type='DateTime'>" + mainWindowVM.SelectedWeek.EndOfWeek.Date.ToString("o") + @"</Value>
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
                    calendarDaysForWeek.Add(new CalendarDayModel(listItem));
            }
        }

        private void CheckCalendarAsyncCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error == null)
            {
                if (GetCalendarDaysCompleted != null)
                    GetCalendarDaysCompleted(this, new GetCalendarDaysCompletedEventArgs() { CalendarDays = calendarDaysForWeek });
            }
            else
            {
                if (GetCalendarDaysCompleted != null)
                    GetCalendarDaysCompleted(this, new GetCalendarDaysCompletedEventArgs() { Error = e.Error });
            }
        }

        #endregion

        #region Public Properties

        public event GetChangesetsCompletedEventHandler GetChangesetsCompleted;

        public event GetCalendarDaysCompletedEventHandler GetCalendarDaysCompleted;

        #endregion

        #region Private Fields

        private List<CalendarDayModel> calendarDaysForWeek = new List<CalendarDayModel>();

        private List<ChangesetModel> changesetsForWeek = new List<ChangesetModel>();

        #endregion

    }

    #region Event Handlers and Arguments

    public delegate void GetChangesetsCompletedEventHandler(object sender, GetChangesetsCompletedEventArgs e);

    public delegate void GetCalendarDaysCompletedEventHandler(object sender, GetCalendarDaysCompletedEventArgs e);

    public class GetChangesetsCompletedEventArgs : EventArgs
    {
        public List<ChangesetModel> Changesets { get; set; }
        public Exception Error { get; set; }
    }

    public class GetCalendarDaysCompletedEventArgs : EventArgs
    {
        public List<CalendarDayModel> CalendarDays { get; set; }
        public Exception Error { get; set; }
    }

    #endregion
}
