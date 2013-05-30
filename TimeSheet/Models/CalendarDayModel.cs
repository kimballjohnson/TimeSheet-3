using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint.Client;

namespace TimeSheet
{
    public class CalendarDayModel
    {
        public CalendarDayModel(string title, DateTime startDay, DateTime? endDay = null, bool isHoliday = false)
        {
            Title = title;
            StartDay = startDay.Date;
            if (endDay == null)
                EndDay = startDay.Date;
            else
                EndDay = ((DateTime)endDay).Date;
            IsHoliday = isHoliday;
        }

        public CalendarDayModel(ListItem item)
            : this((string)item["Title"], (DateTime)item["EventDate"], (DateTime)item["EndDate"])
        {

        }

        public string Title { get; set; }
        public DateTime StartDay { get; set; }
        public DateTime EndDay { get; set; }
        public bool IsHoliday { get; set; }
    }
}
