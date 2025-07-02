namespace runic;

public interface IContentDataSource
{
    string[] Headers { get; }
    IEnumerable<Row> GetContent();
}