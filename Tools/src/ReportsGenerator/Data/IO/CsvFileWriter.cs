using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ReportsGenerator.Data.IO
{
    /// <summary>
    /// An implementation of <see cref="IReportDataWriter"/> interface to write reports in CSV files.
    /// </summary>
    public class CsvFileWriter : IReportDataWriter
    {
        private readonly bool _append;
        private readonly FileStream _file;
        private readonly TextWriter _fileWriter;

        /// <summary>
        /// Initializes a new instance of the <see cref="CsvFileWriter"/> class.
        /// </summary>
        /// <param name="folder">A folder path where file should be created or rewritten, or appended.</param>
        /// <param name="fileName">A file name to create/rewrite or append.</param>
        /// <param name="append">A flag indicating whether or not file should be created.</param>
        public CsvFileWriter(string folder, string fileName, bool append)
        {
            _append = append;
            _file = File.Open(Path.Combine(folder, $"{fileName}.csv"), append ? FileMode.Append : FileMode.Create);
            _fileWriter = new StreamWriter(_file, Encoding.UTF8, 1024, true);
        }

        /// <inheritdoc />
        public void Close()
        {
            _fileWriter.Close();
            _file.Close();
        }

        /// <inheritdoc />
        public void WriteDataLine(IEnumerable<string> data)
        {
            _fileWriter.WriteLine(string.Join(',', data));
        }

        /// <inheritdoc />
        public void WriteHeader(IEnumerable<string> header)
        {
            if (_append && _file.Position > 0)
                return;

            _fileWriter.Flush();
            _file.Position = 0;
            _fileWriter.WriteLine(string.Join(',', header));
        }
    }
}