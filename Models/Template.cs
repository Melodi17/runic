namespace runic.Models;

using Components;

public class Template
{
    public string BaseImage { get; set; }
    public List<Component> Components { get; set; } = new();
    public Dictionary<string, string> Variables { get; set; } = new();
    public string NameFormat { get; set; } = "output_$#";
}