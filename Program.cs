namespace runic;

using CommandLine;
using Components;
using JsonSubTypes;
using Models;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

class Program
{
    static void Main(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args).WithParsed(options => Run(options));
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

        Template? template = JsonConvert.DeserializeObject<Template>(File.ReadAllText(options.TemplatePath), settings);

        if (template == null)
        {
            Console.WriteLine("Failed to load template. Please check the template file format.");
            Environment.Exit(1);
        }

        if (options.Verbose)
            Console.WriteLine($"Template Name: {template.NameFormat}, Components: {template.Components.Count}");

        if (!Directory.Exists(options.OutputPath))
            Directory.CreateDirectory(options.OutputPath);

        if (options.CleanOutput)
            Directory.GetFiles(options.OutputPath).ToList().ForEach(File.Delete);

        Context ctx = new(template, dataSource, options);
        Image baseImage = Image.Load(Path.Combine(Path.GetDirectoryName(options.TemplatePath), template.BaseImage))
                          ?? throw new FileNotFoundException("Base image not found.", template.BaseImage);

        IEnumerable<(Row x, int i)> rows = dataSource.GetContent().Select((x, i) => (x, i)).ToArray();
        foreach ((Row row, int i) in rows)
        {
            ctx.SetCurrentRow(row, i);
            Helpers.ProgressBar(i, rows.Count(), 50,
                $"({i}) {dataSource.Headers.First()}: {row[dataSource.Headers.First()]}");
            string outputFileName = Helpers.ResolveVariables(template.NameFormat, ctx);
            outputFileName = Path.Combine(options.OutputPath, outputFileName);

            // Create a new image based on the base image
            var outputImage = new Image<Rgba32>(baseImage.Width, baseImage.Height);
            outputImage.Mutate(x => x.DrawImage(baseImage, 1f));

            outputImage.Mutate(x =>
            {
                foreach (Component component in template.Components.Where(x => x.Visible))
                    component.Render(x, ctx);
            });
            // Save the output image
            try
            {
                outputImage.Save(outputFileName + ".png");
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
        Helpers.ProgressBar(rows.Count(), rows.Count(), 50, $"{rows.Count()} rows processed");

        Console.WriteLine("Processing completed successfully.");
    }
}