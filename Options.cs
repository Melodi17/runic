namespace runic;

using CommandLine;

public class Options
{
    [Option('t', "template", Required = true, HelpText = "Path to the template file.")]
    public string TemplatePath { get; set; }
    
    [Option('d', "data-source", Required = true, HelpText = "Path to the data source file.")]
    public string DataSourcePath { get; set; }
    
    [Option('o', "output", Required = true, HelpText = "Path to the output folder.")]
    public string OutputPath { get; set; }
    
    [Option('v', "verbose", Default = false, HelpText = "Enable verbose output.")]
    public bool Verbose { get; set; }
    
    [Option('c', "clean-output", Default = false, HelpText = "Clean the output folder before processing.")]
    public bool CleanOutput { get; set; }
    
    [Option('D', "debug", Default = false, HelpText = "Enable debug mode for additional logging.")]
    public bool DebugMode { get; set; }
}