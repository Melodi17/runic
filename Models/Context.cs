namespace runic.Models;

public class Context
{
    public Template Template { get; set; }
    public IContentDataSource DataSource { get; set; }
    public Row CurrentRow { get; set; }
    public int CurrentRowIndex { get; set; }
    public Options Options { get; set; }

    public Context(Template template, IContentDataSource dataSource, Options options)
    {
        this.Template = template;
        this.DataSource = dataSource;
        this.Options = options;
        this.CurrentRowIndex = -1; // Initialize to -1 to indicate no current row
    }
    
    public void SetCurrentRow(Row row, int index)
    {
        this.CurrentRow = row;
        this.CurrentRowIndex = index;
    }
}