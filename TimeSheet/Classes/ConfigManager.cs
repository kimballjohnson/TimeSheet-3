using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace TimeSheet
{
    public static class ConfigManager
    {
        public static string TfsServerUrl
        {
            get { return ConfigurationManager.AppSettings["TfsServerUrl"]; }
        }

        public static string TfsCollectionName
        {
            get { return ConfigurationManager.AppSettings["TfsCollectionName"]; }
        }

        public static string TfsProjectName
        {
            get { return ConfigurationManager.AppSettings["TfsProjectName"]; }
        }

        public static string SharePointWebUrl
        {
            get { return ConfigurationManager.AppSettings["SharePointWebUrl"]; }
        }

        public static string SharepointCalendarName
        {
            get { return ConfigurationManager.AppSettings["SharepointCalendarName"]; }
        }

        public static string HolidayFile
        {
            get { return ConfigurationManager.AppSettings["HolidayFile"]; }
        }

        private static int _numberOfWeeks = 0;

        public static int NumberOfWeeks
        {
            get
            {
                if (_numberOfWeeks == 0)
                {
                    int x = 4;
                    int.TryParse(ConfigurationManager.AppSettings["NumberOfWeeks"], out x);
                    _numberOfWeeks = x;
                }
                return _numberOfWeeks;
            }
        }

        private static int _estimatedMaxCheckinsPerWeek = 0;
        public static int EstimatedMaxCheckinsPerWeek
        {
            get
            {
                if (_estimatedMaxCheckinsPerWeek == 0)
                {
                    int x = 50;
                    int.TryParse(ConfigurationManager.AppSettings["EstimatedMaxCheckinsPerWeek"], out x);
                    _estimatedMaxCheckinsPerWeek = x;
                }
                return _estimatedMaxCheckinsPerWeek;
            }
        }

        private static int _maxCheckinsPerQuery = 0;
        public static int MaxCheckinsPerQuery
        {
            get
            {
                if (_maxCheckinsPerQuery == 0)
                    _maxCheckinsPerQuery = NumberOfWeeks * EstimatedMaxCheckinsPerWeek;
                return _maxCheckinsPerQuery;
            }
        }

        public static bool StartTimeSheetDataWebService
        {
            get 
            {
                string value = ConfigurationManager.AppSettings["StartTimeSheetDataWebService"];
                return value.ToLower() == "true"; 
            }
        }

        public static string TimeSheetDataWebServiceUrl
        {
            get { return ConfigurationManager.AppSettings["TimeSheetDataWebServiceUrl"]; }
        }
    }
}
