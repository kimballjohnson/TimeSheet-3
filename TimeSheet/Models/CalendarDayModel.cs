using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SharePoint.Client;

namespace TimeSheet.Models
{
    public class CalendarDayModel
    {
        public CalendarDayModel(string title, DateTime startDay, DateTime? endDay = null)
        {
            Title = title;
            StartDay = startDay.Date;
            if (endDay == null)
                EndDay = startDay.Date;
            else
                EndDay = ((DateTime)endDay).Date;
        }

        public CalendarDayModel(ListItem item)
            : this((string)item["Title"], (DateTime)item["EventDate"], (DateTime)item["EndDate"])
        {

        }

        public CalendarDayModel(HolidayCalculator.Holiday h)
            :this(h.Name, h.Date)
        {

        }

        public string Title { get; set; }
        public DateTime StartDay { get; set; }
        public DateTime EndDay { get; set; }
    }
}
