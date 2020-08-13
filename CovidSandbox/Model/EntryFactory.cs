using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using CovidSandbox.Data;
using CovidSandbox.Model.Processors;

namespace CovidSandbox.Model
{
    public class EntryFactory
    {
        private readonly IRowProcessor _yandexProcessor = new YandexRussiaRowProcessor();
        private readonly IRowProcessor _jhopkinsProcessor = new JHopkinsRowProcessor();

        public ConcurrentBag<Row> Rows { get; } = new ConcurrentBag<Row>();

        public Entry CreateEntry(Row row)
        {
            Rows.Add(row);
            return row.Version switch
            {
                RowVersion.JHopkinsV1 => new Entry(row, _jhopkinsProcessor),
                RowVersion.JHopkinsV2 => new Entry(row, _jhopkinsProcessor),
                RowVersion.JHopkinsV3 => new Entry(row, _jhopkinsProcessor),
                RowVersion.JHopkinsV4 => new Entry(row, _jhopkinsProcessor),

                RowVersion.YandexRussia => new Entry(row, _yandexProcessor),

                RowVersion.Unknown => throw new ArgumentOutOfRangeException(),

                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}