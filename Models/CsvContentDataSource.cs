namespace runic.Models;

using System.Text;

public class CsvContentDataSource : IContentDataSource
{
    private readonly List<Row> Rows;
    public string[] Headers { get; }

    public CsvContentDataSource(string filePath)
    {
        IEnumerable<string> lines = File.ReadLines(filePath);
        this.Headers = CsvContentDataSource.SafeSplit(lines.First(), ',');
        this.Rows = new();

        foreach (string line in lines.Skip(1))
        {
            string[] values = CsvContentDataSource.SafeSplit(line, ',');
            if (values.Length != this.Headers.Length)
                throw new InvalidOperationException("CSV file has inconsistent number of columns.");

            string[] columns = new string[this.Headers.Length];
            for (int i = 0; i < this.Headers.Length; i++)
            {
                string header = this.Headers[i].Trim();
                string value = values[i].Trim();

                if (string.IsNullOrEmpty(value))
                    continue;

                columns[i] = value;
            }

            this.Rows.Add(new Row(this.Headers, columns));
        }
    }

    /// A safe split method that handles cases where the separator might be inside quotes.
    /// Also allows for backslashes to escape quotes.
    private static string[] SafeSplit(string line, char separator)
    {
        var sb = new StringBuilder();
        var result = new List<string>();
        bool inQuotes = false;
        bool escapeNext = false;

        foreach (char c in line)
        {
            if (escapeNext)
            {
                sb.Append(c);
                escapeNext = false;
            }
            else if (c == '\\')
            {
                escapeNext = true;
            }
            else if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (c == separator && !inQuotes)
            {
                result.Add(sb.ToString());
                sb.Clear();
            }
            else
            {
                sb.Append(c);
            }
        }

        // Add the last segment
        result.Add(sb.ToString());

        return result.ToArray();
    }

    public IEnumerable<Row> GetContent() => this.Rows;
}