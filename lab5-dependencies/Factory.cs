using Microsoft.Extensions.Options;

public interface IReaderFactory { IReader Create(); }
public interface IWriterFactory { IWriter Create(); }

public sealed class FormatFileOptions
{
    public const string ReaderKey = "Reader";
    public const string WriterKey = "Writer";

    public string FileName { get; set; } = string.Empty;
    public Format? Format { get; set; }
}

public sealed class FileReaderFactory : IReaderFactory
{
    private readonly IOptionsSnapshot<FormatFileOptions> _options;
    public FileReaderFactory(IOptionsSnapshot<FormatFileOptions> o) => _options = o;

    public IReader Create()
    {
        var val = _options.Get(FormatFileOptions.ReaderKey);
        if (val.Format is null)
            throw new InvalidOperationException("Format is required for FileReader.");
        return new FileReader(val.FileName, val.Format);
    }
}

public sealed class FileWriterFactory : IWriterFactory
{
    private readonly IOptionsSnapshot<FormatFileOptions> _options;
    public FileWriterFactory(IOptionsSnapshot<FormatFileOptions> o) => _options = o;

    public IWriter Create()
    {
        var val = _options.Get(FormatFileOptions.WriterKey);
        if (val.Format is null)
            throw new InvalidOperationException("Format is required for FileWriter.");
        return new FileWriter(val.FileName, val.Format);
    }
}
public sealed class HttpReaderOptions
{
    public string Url { get; set; } = string.Empty;
    public Format? Format { get; set; }
}

public sealed class HttpReaderFactory : IReaderFactory
{
    private readonly IOptionsSnapshot<HttpReaderOptions> _options;
    private readonly IHttpClientFactory _httpFactory;

    public HttpReaderFactory(
        IOptionsSnapshot<HttpReaderOptions> options,
        IHttpClientFactory httpFactory)
    {
        _options = options;
        _httpFactory = httpFactory;
    }

    public IReader Create()
    {
        var val = _options.Value;
        if (val.Format is null)
            throw new InvalidOperationException("Format is required for HttpReader.");
        if (string.IsNullOrWhiteSpace(val.Url))
            throw new InvalidOperationException("Url is required for HttpReader.");

        var client = _httpFactory.CreateClient("PersonApi");
        return new HttpReader(client, val.Url, val.Format);
    }
}

public sealed class LoggingWriterFactory : IWriterFactory
{
    public IWriter Create() => new LoggingWriter();
}
