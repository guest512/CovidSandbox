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
            Name = Path.GetFileNameWithoutExtension(_path);
        }

        public string Name { get; }

        private string[] FileContents
        {
            get { return _fileContents ??= ReadFile(); }
        }

        public IEnumerable<string> GetHeader() => !string.IsNullOrEmpty(FileContents[0]) ? FileContents[0].SplitCsvRowString() : Enumerable.Empty<string>();

        public IEnumerable<IEnumerable<string>> GetRows() => FileContents.Skip(1).Where(s => !string.IsNullOrWhiteSpace(s)).Select(Utils.SplitCsvRowString);

        private string[] ReadFile()
        {
            using var file = File.OpenRead(_path);
            using var reader = new StreamReader(file);

            var contents = reader.ReadToEnd();

            return contents.Split("\r\n").SelectMany(l => l.Split("\n")).ToArray();
        }
    }
}