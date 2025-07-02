namespace runic;

using System.Drawing;
using CommandLine;
using Components;
using JsonSubTypes;
using Newtonsoft.Json;

class Program
{
    static void Main(string[] args)
    {
        Parser
            .Default.ParseArguments<Options>(args)
            .WithParsed(options => Run(options));
    }

    static void Run(Options options)
    {
        if (options.Verbose)
        {
            Console.WriteLine($"Template Path: {options.TemplatePath}");
            Console.WriteLine($"Data Source Path: {options.DataSourcePath}");
            Console.WriteLine($"Output Path: {options.OutputPath}");
        }

        IContentDataSource dataSource = new CsvContentDataSource(options.DataSourcePath);
        if (options.Verbose)
            Console.WriteLine(
                $"Headers: [{string.Join(", ", dataSource.Headers)}], Rows: {dataSource.GetContent().Count()}");
        
        var settings = new JsonSerializerSettings();
        settings.Converters.Add(JsonSubtypesConverterBuilder
            .Of(typeof(Component), "Type") // type property is only defined here
            .RegisterSubtype(typeof(ImageComponent), "Image")
            .RegisterSubtype(typeof(TextComponent), "Text")
            .SerializeDiscriminatorProperty() // ask to serialize the type property
            .Build());
        
        Template template = JsonConvert.DeserializeObject<Template>(
            System.IO.File.ReadAllText(options.TemplatePath), settings);
        
        if (template == null)
        {
            Console.WriteLine("Failed to load template. Please check the template file format.");
            Environment.Exit(1);
        }
        
        if (options.Verbose)
            Console.WriteLine($"Template Name: {template.NameFormat}, Components: {template.Components.Count}");
        
        if (!Directory.Exists(options.OutputPath))
            Directory.CreateDirectory(options.OutputPath);

        Context ctx = new(template, dataSource);
        Image baseImage = Image.FromFile(Path.Combine(Path.GetDirectoryName(options.TemplatePath), template.BaseImage)) ?? throw new FileNotFoundException("Base image not found.", template.BaseImage);
        foreach ((Row row, int i) in dataSource.GetContent().Select((x, i) => (x, i)))
        {
            ctx.SetCurrentRow(row, i);
            if (options.Verbose)
                Console.WriteLine($"Processing row {i + 1}: {string.Join(", ", row.Columns)}");
            string outputFileName = Helpers.ResolveVariables(template.NameFormat, ctx);
            outputFileName = Path.Combine(options.OutputPath, outputFileName);
            
            // Create a new image based on the base image
            Bitmap outputImage = new Bitmap(baseImage.Width, baseImage.Height);
            using (Graphics graphics = Graphics.FromImage(outputImage))
            {
                graphics.DrawImage(baseImage, 0, 0, baseImage.Width, baseImage.Height);
                foreach (Component component in template.Components)
                {
                    component.Render(graphics, ctx);
                }
            }
            // Save the output image
            try
            {
                outputImage.Save(outputFileName + ".png", System.Drawing.Imaging.ImageFormat.Png);
                if (options.Verbose)
                    Console.WriteLine($"Saved: {outputFileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving image '{outputFileName}': {ex.Message}");
            }
            finally
            {
                outputImage.Dispose();
            }
        }
    }

    static void HandleParseError(IEnumerable<Error> errors)
    {
        foreach (var error in errors)
        {
            Console.WriteLine($"Error: {error.Tag}");
        }

        Environment.Exit(1);
    }
}