using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web.Script.Serialization;
using TimeSheet.Classes;
using TimeSheet.Interfaces;
using TimeSheet.Models;
using TimeSheet.ViewModels;

namespace TimeSheet.Services
{
    public class TimeSheetDataWebService : ITimeSheetDataWebService
    {
        private WebServer server;

        public void StartHttpWebService()
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();
            headers.Add("Access-Control-Allow-Origin", "*");
            server = new WebServer(SendResponse, headers, ConfigManager.TimeSheetDataWebServiceUrl);
            server.Run();
        }

        public void StopHttpWebService()
        {
            server.Stop();
        }

        private string SendResponse(HttpListenerRequest request)
        {
            var mainWindowVM = ViewModelLocater.MainWindowViewModel;

            NotesDTO[] notes = new NotesDTO[7];

            for (int i = 0; i < 7; i++)
            {
                StringBuilder output = new StringBuilder();
                NotesDTO note = new NotesDTO() { DayId = i };
                notes[i] = note;

                DateTime day = mainWindowVM.SelectedWeek.StartOfWeek.AddDays(i);

                if (mainWindowVM.ChangesetsForWeek == null)
                    continue;

                var changesetsForDay =
                    mainWindowVM.ChangesetsForWeek.Where(cs => cs.CreationDate.Date == day.Date).OrderBy(
                        cs => cs.CreationDate);
                var workItemsForDay =
                    changesetsForDay.SelectMany(cs => cs.WorkItems).Distinct(new WorkItemModelComparer());

                foreach (var workItem in workItemsForDay)
                {
                    if (mainWindowVM.ShowWorkItemType)
                        output.Append(workItem.Type + " ");

                    if (mainWindowVM.ShowCompletedHours && workItem.Type == "Task")
                    {
                        output.Append(workItem.ID + " - " + workItem.Title);
                        output.Append(" (" + workItem.CompletedWork + " hours)\\n");
                    }
                    else
                    {
                        output.Append(workItem.ID + " - " + workItem.Title + "\\n");
                    }

                    if (mainWindowVM.ShowComments)
                    {
                        var changesetsForWorkItemOnThisDay =
                            changesetsForDay.Where(cs => cs.WorkItems.Contains(workItem, new WorkItemModelComparer()))
                                .OrderBy(cs => cs.CreationDate);
                        foreach (var changeset in changesetsForWorkItemOnThisDay)
                        {
                            output.Append("Changeset " + changeset.ID + ": " + changeset.Comment + "\\n");
                        }
                    }
                    output.Append("\\n");
                }

                note.Description = output.ToString();
            }

            MemoryStream stream1 = new MemoryStream();
            DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(NotesDTO[]));
            ser.WriteObject(stream1, notes);
            stream1.Position = 0;
            StreamReader sr = new StreamReader(stream1);
            string response = sr.ReadToEnd();
            return response;
        }
    }
}
