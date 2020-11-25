using System;

namespace ReportsGenerator
{
    /// <summary>
    /// Application constants.
    /// </summary>
    public static class Consts
    {
        /// <summary>
        /// Default name for the main country region. Usually used to substitute regions names such as  country name, 'none',  empty name, etc.
        /// </summary>
        public const string MainCountryRegion = "Main territory";

        /// <summary>
        /// Default name for the country region that marked as 'unknown' or 'unassigned'. 
        /// </summary>
        public const string OtherCountryRegion = "Other";
    }
}