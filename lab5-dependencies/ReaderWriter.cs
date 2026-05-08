public interface IReader
{
    Task<IEnumerable<T>> Read<T>();
}

public interface IWriter
{
    Task Write<T>(IEnumerable<T> values);
}

public sealed class FileReader : IReader
{
    private readonly string _fileName;
    private readonly Format _format;

    public FileReader(string fileName, Format format)
    {
        _fileName = fileName;
        _format = format;
    }

    public async Task<IEnumerable<T>> Read<T>()
    {
        await using var stream = File.OpenRead(_fileName);
        return await _format.Read<T>(stream);
    }
}

public sealed class FileWriter : IWriter
{
    private readonly string _fileName;
    private readonly Format _format;

    public FileWriter(string fileName, Format format)
    {
        _fileName = fileName;
        _format = format;
    }

    public async Task Write<T>(IEnumerable<T> values)
    {
        await using var stream = new FileStream(_fileName, FileMode.Create);
        await _format.Write(stream, values);
    }
}

public sealed class HttpReader : IReader
{
    private readonly HttpClient _client;
    private readonly string _url;
    private readonly Format _format;

    public HttpReader(HttpClient client, string url, Format format)
    {
        _client = client;
        _url = url;
        _format = format;
    }

    public async Task<IEnumerable<T>> Read<T>()
    {
        using var response = await _client.GetAsync(_url);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync();
        return await _format.Read<T>(stream);
    }
}

public sealed class LoggingWriter : IWriter
{
    public Task Write<T>(IEnumerable<T> values)
    {
        foreach (var v in values)
            Console.WriteLine(v);
        return Task.CompletedTask;
    }
}
