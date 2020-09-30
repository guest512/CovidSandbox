﻿namespace ReportsGenerator
{
    public static class Consts
    {
        public const string CountryReportHeader =
            "Date,Confirmed,Active,Recovered,Deaths,Confirmed_Change,Active_Change,Recovered_Change,Deaths_Change,Rt,Time_To_Resolve";

        public const string DayByDayReportHeader =
            "CountryRegion,Confirmed,Active,Recovered,Deaths,Confirmed_Change,Active_Change,Recovered_Change,Deaths_Change";

        public const string MainCountryRegion = "Main territory";
    }
}