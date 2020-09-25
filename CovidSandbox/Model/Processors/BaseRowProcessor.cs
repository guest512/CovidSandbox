﻿using CovidSandbox.Data;
using CovidSandbox.Utils;
using System;
using System.Diagnostics;
using System.Globalization;

namespace CovidSandbox.Model.Processors
{
    public abstract class BaseRowProcessor : IRowProcessor
    {
        protected readonly ILogger Logger;

        protected BaseRowProcessor(ILogger logger)
        {
            Logger = logger;
        }

        public virtual long GetActive(Row row)
        {
            var active = TryGetValue(row[Field.Active], long.MinValue);
            return active == long.MinValue ? GetConfirmed(row) - GetDeaths(row) - GetRecovered(row) : active;
        }

        public virtual long GetConfirmed(Row row) => TryGetValue(row[Field.Confirmed]);

        public abstract string GetCountryName(Row row);

        public abstract string GetCountyName(Row row);

        public virtual long GetDeaths(Row row) => TryGetValue(row[Field.Deaths]);

        public abstract uint GetFips(Row row);

        public abstract Origin GetOrigin(Row row);

        public abstract string GetProvinceName(Row row);

        public virtual long GetRecovered(Row row) => TryGetValue(row[Field.Recovered]);

        public virtual DateTime GetLastUpdate(Row row) => Convertors.ParseDate(row[Field.LastUpdate]);

        public int GetIsoLevel(Row row)
        {
            var countyNameSet = !string.IsNullOrEmpty(GetCountyName(row));
            var provinceNameSet = !string.IsNullOrEmpty(GetProvinceName(row));
            var countryNameSet = !string.IsNullOrEmpty(GetCountryName(row));
            int level;

            if (countryNameSet)
            {
                if (provinceNameSet)
                {
                    level = countyNameSet ? 3 : 2;
                }
                else
                {
                    if (countyNameSet)
                    {
                        Logger.WriteWarning("The row has country and county names but doesn't have province name. Treat it as country level.");
                        Debugger.Break();
                    }

                    level = 1;
                }
            }
            else
            {
                Logger.WriteError("The row doesn't have a country name.");
                Debugger.Break();
                throw new InvalidOperationException("The row doesn't have a country name.");
            }

            return level;
        }

        protected long TryGetValue(string stringValue, long defaultValue = 0)
        {
            var value = long.TryParse(stringValue, out var intValue) ? intValue : defaultValue;

            if (!stringValue.Contains('.'))
                return value;

            var floatValue = float.Parse(stringValue, CultureInfo.InvariantCulture);
            if (!(floatValue % 1 < float.Epsilon))
                return value;

            value = (long)floatValue;
            Logger.WriteWarning($"Expected 'long' but get 'float' for '{stringValue}'. Parsed as '{value}'");

            return value;
        }
    }
}