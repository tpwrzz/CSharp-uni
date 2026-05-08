using CsvHelper.Configuration;
using lab4_properties;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

var services = new ServiceCollection();

services.AddOptions<FormatFileOptions>(FormatFileOptions.ReaderKey);
services.AddOptions<FormatFileOptions>(FormatFileOptions.WriterKey);
services.AddOptions<HttpReaderOptions>();

services.AddHttpClient("PersonApi");
services.AddSingleton(new FilterByLevelAdapter(new()
{
    MinLevel = ExperienceLevel.Middle,
}));

services.AddSingleton(new FilterByRatingAdapter(new()
{
    MinRating = 3.5,
}));

services.AddSingleton(new SortByExperienceAdapter(new()
{
    Ascending = false, 
}));
services.AddSingleton<ITransformationStep<Person>>(sp =>
    sp.GetRequiredService<FilterByLevelAdapter>());
services.AddSingleton<ITransformationStep<Person>>(sp =>
    sp.GetRequiredService<FilterByRatingAdapter>());
services.AddSingleton<ITransformationStep<Person>>(sp =>
    sp.GetRequiredService<SortByExperienceAdapter>());


services.AddSingleton<SingleStepPipeline>(sp =>
    new SingleStepPipeline(sp.GetRequiredService<FilterByLevelAdapter>()));

services.AddSingleton<ListPipeline>(sp =>
    new ListPipeline(sp.GetServices<ITransformationStep<Person>>()));

await using var serviceProvider = services.BuildServiceProvider();


await using var mockServer = new MockPersonApiServer(port: 5099);
Console.WriteLine($"Mock API running at {mockServer.Url}\n");


async Task Run(string label, Action<OrchestrationBuilder> configure)
{
    Console.WriteLine($"--- {label} ---");
    await using var scope = serviceProvider.CreateAsyncScope();
    var builder = scope.CreatePipelineBuilder();
    configure(builder);
    await builder.Execute();
    Console.WriteLine();
}

var jsonFormat = Format.CreateJson(new JsonSerializerOptions
{
    WriteIndented = true,
    Converters = { new JsonStringEnumConverter() },
});

var csvFormat = Format.CreateCsv(
    new CsvConfiguration(CultureInfo.InvariantCulture));


await Run("HTTP to ListPipeline (filter + sort) to JSON file", b =>
{
    b.UseHttpReader(opts =>
    {
        opts.Url = mockServer.Url;
        opts.Format = jsonFormat;
    });
    b.UsePipeline<ListPipeline>();
    b.UseFileWriter(opts =>
    {
        opts.FileName = "output_filtered.json";
        opts.Format = jsonFormat;
    });
});


await Run("JSON file to SingleStepPipeline (filter Middle+) to CSV file", b =>
{
    b.UseFileReader(opts =>
    {
        opts.FileName = "output_filtered.json";
        opts.Format = jsonFormat;
    });
    b.UsePipeline<SingleStepPipeline>();
    b.UseFileWriter(opts =>
    {
        opts.FileName = "output_middle_plus.csv";
        opts.Format = csvFormat;
    });
});


await Run("CSV file to DoNothing to Console (LoggingWriter)", b =>
{
    b.UseFileReader(opts =>
    {
        opts.FileName = "output_middle_plus.csv";
        opts.Format = csvFormat;
    });
    b.UseLoggingWriter();
});

await Run("HTTP to DoNothing to CSV file (raw data)", b =>
{
    b.UseHttpReader(opts =>
    {
        opts.Url = mockServer.Url;
        opts.Format = jsonFormat;
    });
    b.UseFileWriter(opts =>
    {
        opts.FileName = "output_raw.csv";
        opts.Format = csvFormat;
    });
});

Console.WriteLine("Done! Files written: output_filtered.json, output_middle_plus.csv, output_raw.csv");