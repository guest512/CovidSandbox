using System.Collections.Generic;

namespace ReportsGenerator.Data
{
    /// <summary>
    /// Represents a collection of helper functions.
    /// </summary>
    public static class Utils
    {
        /// <summary>
        /// Extension method to treat string as csv file row and split it by csv rules.
        /// </summary>
        /// <param name="row">String to process.</param>
        /// <returns>Collection of split strings, with trimmed quotation marks.</returns>
        public static IEnumerable<string> SplitCsvRowString(this string row)
        {
            var lastSeparatorIndex = -1;
            var quotes = false;

            for (var i = 0; i < row.Length; i++)
            {
                if (row[i] == ',' && !quotes)
                {
                    if (i == lastSeparatorIndex)
                        yield return string.Empty;
                    else
                        yield return row[(lastSeparatorIndex + 1)..i].Trim('\"');

                    lastSeparatorIndex = i;
                }

                if (row[i] == '\"')
                    quotes ^= true;
            }

            yield return row[(lastSeparatorIndex + 1)..].Trim('\"');
        }
    }
}