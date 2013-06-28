using System.Collections.Generic;

namespace TimeSheet.Models
{
    public class WorkItemModelComparer : IEqualityComparer<WorkItemModel>
    {
        public bool Equals(WorkItemModel x, WorkItemModel y)
        {
            return x.ID == y.ID;
        }
        public int GetHashCode(WorkItemModel wi)
        {
            return wi.ID;
        }
    }
}
