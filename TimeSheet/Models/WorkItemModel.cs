using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.VersionControl.Client;
using Microsoft.TeamFoundation.WorkItemTracking.Client;

namespace TimeSheet
{
    public class WorkItemModel
    {
        public WorkItemModel(WorkItem workItem)
        {
            Title = workItem.Title;
            ID = workItem.Id;
            Type = workItem.Type.Name;
            CompletedWork = Type == "Task" && workItem["Completed Work"] != null ? (double)workItem["Completed Work"] : 0;
        }

        public int ID { get; set; }
        public string Title { get; set; }
        public double CompletedWork  { get; set; }
        public string Type { get; set; }
    }
}
