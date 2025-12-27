namespace runic.Models;

public interface IContentDataSource
{
    string[] Headers { get; }
    IEnumerable<Row> GetContent();
}