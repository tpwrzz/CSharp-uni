using CsvHelper.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Text.Json;
public enum FormatType { Json, Csv }

public sealed class Format
{
    private Format(FormatType type) => Type = type;

    public FormatType Type { get; }
    public JsonSerializerOptions? JsonOptions { get; private init; }
    public CsvConfiguration? CsvConfig { get; private init; }

    public static Format CreateJson(JsonSerializerOptions? options = null) =>
        new(FormatType.Json)
        {
            JsonOptions = options ?? new JsonSerializerOptions
            {
                WriteIndented = true,
                Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
            },
        };

    public static Format CreateCsv(CsvConfiguration? config = null) =>
        new(FormatType.Csv)
        {
            CsvConfig = config ?? new CsvConfiguration(CultureInfo.InvariantCulture),
        };

    public async Task Write<T>(Stream stream, IEnumerable<T> items)
    {
        switch (Type)
        {
            case FormatType.Json:
                Debug.Assert(JsonOptions is not null);
                await JsonSerializer.SerializeAsync(stream, items, JsonOptions);
                break;

            case FormatType.Csv:
                {
                    Debug.Assert(CsvConfig is not null);
                    await using var writer = new StreamWriter(stream, leaveOpen: true);
                    await using var csv = new CsvHelper.CsvWriter(writer, CsvConfig);
                    await csv.WriteRecordsAsync(items);
                }
                break;

            default:
                throw new NotSupportedException($"Format {Type} is not supported for writing.");
        }
    }

    public async Task<IEnumerable<T>> Read<T>(Stream stream)
    {
        switch (Type)
        {
            case FormatType.Json:
                Debug.Assert(JsonOptions is not null);
                return await JsonSerializer.DeserializeAsync<IEnumerable<T>>(stream, JsonOptions)
                       ?? [];

            case FormatType.Csv:
                {
                    Debug.Assert(CsvConfig is not null);
                    using var reader = new StreamReader(stream, leaveOpen: true);
                    using var csvReader = new CsvHelper.CsvReader(reader, CsvConfig);
                    return [.. csvReader.GetRecords<T>()];
                }
            default:
                throw new NotSupportedException($"Format {Type} is not supported for reading.");
        }
    }
}
