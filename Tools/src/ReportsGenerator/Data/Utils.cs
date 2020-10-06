using System.Collections.Generic;

namespace ReportsGenerator.Data
{
    public static class Utils
    {
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