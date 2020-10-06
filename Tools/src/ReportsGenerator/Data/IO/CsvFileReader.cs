﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ReportsGenerator.Data.IO
{
    public class CsvFileReader
    {
        private readonly string _path;
        private string[]? _fileContents;

        public CsvFileReader(string path)
        {
            _path = path;
        }

        private string[] FileContents
        {
            get { return _fileContents ??= ReadFile(); }
        }

        public IEnumerable<string> GetHeader() => FileContents[0].SplitRowString();

        public IEnumerable<IEnumerable<string>> GetRows() => FileContents.Skip(1).Where(s => !string.IsNullOrWhiteSpace(s)).Select(Utils.SplitRowString);

        private string[] ReadFile()
        {
            using var file = File.OpenRead(_path);
            using var reader = new StreamReader(file);

            return reader.ReadToEnd().Split(Environment.NewLine);
        }
    }
}