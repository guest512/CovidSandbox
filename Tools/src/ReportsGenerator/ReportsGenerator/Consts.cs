using System;

namespace ReportsGenerator
{
    /// <summary>
    /// Application constants.
    /// </summary>
    public static class Consts
    {
        /// <summary>
        /// Default name for the main country region. Usually used to substitute regions names such as  country name, 'unknown', 'unassigned', empty name, etc.
        /// </summary>
        public const string MainCountryRegion = "Main territory";

        public const string OtherCountryRegion = "Other";

        // DEBUG ONLY
        internal const bool DisableExtensiveAssertMethods = true;
    }
}