using System;

namespace TimeSheet
{
    public class WeekModel
    {
        public WeekModel(DateTime startOfWeek)
        {
            StartOfWeek = startOfWeek.Date;
        }
        public DateTime StartOfWeek { get; set; }
        public DateTime EndOfWeek
        {
            get { return StartOfWeek.Date.AddDays(6); }
        }
        public string Name
        {
            get
            {
                var now = DateTime.Now;
                if (StartOfWeek <= now && now <= EndOfWeek)
                    return StartOfWeek.ToShortDateString() + " - " + EndOfWeek.ToShortDateString() + " (this week)";
                else if (StartOfWeek <= now.AddDays(-7) && now.AddDays(-7) <= EndOfWeek)
                    return StartOfWeek.ToShortDateString() + " - " + EndOfWeek.ToShortDateString() + " (last week)";
                else
                    return StartOfWeek.ToShortDateString() + " - " + EndOfWeek.ToShortDateString();
            }
        }
    }
}
