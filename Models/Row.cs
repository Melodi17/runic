namespace runic;

public class Row
{
    private readonly string[] headers;
    private string[] columns;
    public Row(string[] headers, string[] columns)
    {
        this.headers = headers;
        this.columns = columns;
    }
    
    public string[] Columns
    {
        get { return this.columns; }
    }
    
    public string this[string header]
    {
        get
        {
            int index = Array.IndexOf(this.headers, header);
            if (index < 0)
                throw new KeyNotFoundException($"Header '{header}' not found.");
            return this.columns[index];
        }
    }
    
    public string this[int index]
    {
        get
        {
            if (index < 0 || index >= this.columns.Length)
                throw new IndexOutOfRangeException("Index is out of range.");
            return this.columns[index];
        }
    }
}