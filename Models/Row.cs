namespace runic;

public class Row
{
    private readonly string[] headers;
    private string[] columns;
    public Row(string[] headers, string[] columns)
    {
        this.columns = columns;
    }
    
    public string[] Columns
    {
        get { return columns; }
    }
    
    public string this[string header]
    {
        get
        {
            int index = Array.IndexOf(headers, header);
            if (index < 0)
                throw new KeyNotFoundException($"Header '{header}' not found.");
            return columns[index];
        }
    }
    
    public string this[int index]
    {
        get
        {
            if (index < 0 || index >= columns.Length)
                throw new IndexOutOfRangeException("Index is out of range.");
            return columns[index];
        }
    }
}