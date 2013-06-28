using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.TeamFoundation.VersionControl.Client;

namespace TimeSheet.Models
{
    public class ChangesetModel
    {
        public ChangesetModel(Changeset changeset)
        {
            ID = changeset.ChangesetId;
            Comment = changeset.Comment;
            CreationDate = changeset.CreationDate;
            WorkItems =  new List<WorkItemModel>();
            foreach (var workItem in changeset.WorkItems)
                WorkItems.Add(new WorkItemModel(workItem));

        }

        public int ID { get; set; }
        public string Comment { get; set; }
        public DateTime CreationDate { get; set; }
        public List<WorkItemModel> WorkItems { get; set; }
    }
}
