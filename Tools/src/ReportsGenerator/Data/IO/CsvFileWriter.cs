using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ReportsGenerator.Data.IO
{
    public class CsvFileWriter : IReportDataWriter
    {
        private readonly FileStream _file;
        private readonly TextWriter _fileWriter;

        public CsvFileWriter(string folder, string fileName)
        {
            _file = File.OpenWrite(Path.Combine(folder, $"{fileName}.csv"));
            _fileWriter = new StreamWriter(_file, Encoding.UTF8, 1024, true);
        }

        public void Close()
        {
            _fileWriter.Close();
            _file.Close();
        }

        public void WriteDataLine(IEnumerable<string> data)
        {
            _fileWriter.WriteLine(string.Join(',', data));
        }

        public void WriteHeader(IEnumerable<string> header)
        {
            _fileWriter.Flush();
            _file.Position = 0;
            _fileWriter.WriteLine(string.Join(',', header));
        }
    }
}