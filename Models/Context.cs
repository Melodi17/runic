namespace runic;

public class Context
{
    public Template Template { get; set; }
    public IContentDataSource DataSource { get; set; }
    public Row CurrentRow { get; set; }
    public int CurrentRowIndex { get; set; }
    
    public Context(Template template, IContentDataSource dataSource)
    {
        Template = template;
        DataSource = dataSource;
        CurrentRowIndex = -1; // Initialize to -1 to indicate no current row
    }
    
    public void SetCurrentRow(Row row, int index)
    {
        CurrentRow = row;
        CurrentRowIndex = index;
    }
}