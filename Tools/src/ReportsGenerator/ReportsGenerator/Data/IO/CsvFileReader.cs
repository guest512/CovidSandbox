using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ReportsGenerator.Data.IO
{
    /// <summary>
    /// Represents a CSV file reader.
    /// </summary>
    public class CsvFileReader
    {
        private readonly string _path;
        private string[]? _fileContents;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvFileReader"/> class.
        /// </summary>
        /// <param name="path">Path to file to read.</param>
        public CsvFileReader(string path)
        {
            _path = path;
            Name = Path.GetFileNameWithoutExtension(_path);
        }

        /// <summary>
        /// Gets a filename without extension.
        /// </summary>
        public string Name { get; }

        private string[] FileContents
        {
            get { return _fileContents ??= ReadFile(); }
        }

        /// <summary>
        /// Gets file header.
        /// </summary>
        /// <returns>A file header as a <see cref="IEnumerable{T}"/> of <see cref="string"/>.</returns>
        public IEnumerable<string> GetHeader() => !string.IsNullOrEmpty(FileContents[0]) ? FileContents[0].SplitCsvRowString() : Enumerable.Empty<string>();

        /// <summary>
        /// Gets file contents row-by-row split and cleaned up from extra quotation marks.
        /// </summary>
        /// <returns>A file contents as a <see cref="IEnumerable{T}"/> of <see cref="IEnumerable{T}"/> of <see cref="string"/>.</returns>
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