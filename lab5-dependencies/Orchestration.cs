using lab4_properties;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

public static class Orchestration
{
    public static async Task WholeProcess(
        IReader reader,
        IWriter writer,
        ITransformationPipeline<Person> pipeline)
    {
        var items = await reader.Read<Person>();
        var processedItems = pipeline.Run(items);
        await writer.Write(processedItems);
    }
}

public readonly struct OrchestrationExecutor
{
    private readonly IReader _reader;
    private readonly IWriter _writer;
    private readonly ITransformationPipeline<Person> _pipeline;

    public OrchestrationExecutor(
        IReader reader,
        IWriter writer,
        ITransformationPipeline<Person> pipeline)
    {
        _reader = reader;
        _writer = writer;
        _pipeline = pipeline;
    }

    public Task Execute() =>
        Orchestration.WholeProcess(_reader, _writer, _pipeline);
}

public sealed class OrchestrationBuilderModel
{
    public IReaderFactory? Reader { get; set; }
    public IWriterFactory? Writer { get; set; }
    public ITransformationPipeline<Person>? Pipeline { get; set; }
}
public sealed class OrchestrationBuilder
{
    public required IServiceProvider ServiceProvider { get; init; }
    public OrchestrationBuilderModel Model { get; } = new();

    public OrchestrationExecutor Build()
    {
        if (Model.Reader is null)
            throw new InvalidOperationException("Reader factory is not set.");
        if (Model.Writer is null)
            throw new InvalidOperationException("Writer factory is not set.");

        var reader = Model.Reader.Create();
        var writer = Model.Writer.Create();
        var pipeline = Model.Pipeline ?? DoNothingPipeline.Instance;

        return new OrchestrationExecutor(reader, writer, pipeline);
    }

    public Task Execute() => Build().Execute();
}

public static class OrchestratorBuilderExtensions
{
    public static OrchestrationBuilder CreatePipelineBuilder(this IServiceScope scope) =>
        new() { ServiceProvider = scope.ServiceProvider };

    public static void UseFileReader(
        this OrchestrationBuilder b,
        Action<FormatFileOptions> configure)
    {
        EnsureReaderNotSet(b);
        b.Model.Reader = ActivatorUtilities
            .GetServiceOrCreateInstance<FileReaderFactory>(b.ServiceProvider);

        var opts = b.ServiceProvider
            .GetRequiredService<IOptionsSnapshot<FormatFileOptions>>()
            .Get(FormatFileOptions.ReaderKey);
        configure(opts);
    }

    public static void UseHttpReader(
        this OrchestrationBuilder b,
        Action<HttpReaderOptions> configure)
    {
        EnsureReaderNotSet(b);
        b.Model.Reader = ActivatorUtilities
            .GetServiceOrCreateInstance<HttpReaderFactory>(b.ServiceProvider);

        var opts = b.ServiceProvider
            .GetRequiredService<IOptionsSnapshot<HttpReaderOptions>>().Value;
        configure(opts);
    }
    public static void UseFileWriter(
        this OrchestrationBuilder b,
        Action<FormatFileOptions> configure)
    {
        EnsureWriterNotSet(b);
        b.Model.Writer = ActivatorUtilities
            .GetServiceOrCreateInstance<FileWriterFactory>(b.ServiceProvider);

        var opts = b.ServiceProvider
            .GetRequiredService<IOptionsSnapshot<FormatFileOptions>>()
            .Get(FormatFileOptions.WriterKey);
        configure(opts);
    }

    public static void UseLoggingWriter(this OrchestrationBuilder b)
    {
        EnsureWriterNotSet(b);
        b.Model.Writer = new LoggingWriterFactory();
    }

    public static void UsePipeline<TPipeline>(this OrchestrationBuilder b)
        where TPipeline : ITransformationPipeline<Person>
    {
        b.Model.Pipeline = b.ServiceProvider.GetRequiredService<TPipeline>();
    }
    private static void EnsureReaderNotSet(OrchestrationBuilder b)
    {
        if (b.Model.Reader is not null)
            throw new InvalidOperationException("Reader is already set.");
    }

    private static void EnsureWriterNotSet(OrchestrationBuilder b)
    {
        if (b.Model.Writer is not null)
            throw new InvalidOperationException("Writer is already set.");
    }
}
