using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TimeSheet.Services;

namespace TimeSheet.Interfaces
{
    public interface IDataService
    {
        void GetChangesets();
        event GetChangesetsCompletedEventHandler GetChangesetsCompleted;

        void GetCalendarDays();
        event GetCalendarDaysCompletedEventHandler GetCalendarDaysCompleted;
    }
}
