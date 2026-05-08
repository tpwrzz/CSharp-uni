using lab4_properties;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

public sealed class MockPersonApiServer : IAsyncDisposable
{
    private readonly HttpListener _listener;
    private readonly Task _serverTask;
    private readonly CancellationTokenSource _cts = new();

    public string Url { get; }

    public MockPersonApiServer(int port = 5099)
    {
        Url = $"http://localhost:{port}/persons/";
        _listener = new HttpListener();
        _listener.Prefixes.Add(Url);
        _listener.Start();
        _serverTask = ServeAsync(_cts.Token);
    }

    private async Task ServeAsync(CancellationToken ct)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            Converters = { new JsonStringEnumConverter() },
        };

        while (!ct.IsCancellationRequested)
        {
            HttpListenerContext ctx;
            try { ctx = await _listener.GetContextAsync(); }
            catch { break; }

            var persons = GeneratePersons(10);
            var json = JsonSerializer.Serialize(persons, options);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);

            ctx.Response.ContentType = "application/json";
            ctx.Response.ContentLength64 = bytes.Length;
            await ctx.Response.OutputStream.WriteAsync(bytes, ct);
            ctx.Response.Close();
        }
    }

    private static IEnumerable<Person> GeneratePersons(int count)
    {
        var rand = Random.Shared;
        var workplaces = new[] { "Chisinau HQ", "Balti Office", "Remote", "Cluj Hub" };
        var projects = new[] { "CRM Alpha", "Portal Beta", "Analytics X", "API Gateway" };
        var names = new[] { "Anna", "John", "Maria", "David", "Elena", "Paul", "Sara", "Tom" };
        var surnames = new[] { "Smith", "Brown", "Jones", "Davis", "Wilson", "Moore", "Taylor" };

        for (int i = 0; i < count; i++)
        {
            var years = rand.Next(0, 15);
            yield return new Person
            {
                Id = Guid.NewGuid(),
                FullName = names[rand.Next(names.Length)] + " " + surnames[rand.Next(surnames.Length)],
                YearsOfExperience = years,
                Level = years switch
                {
                    < 2 => ExperienceLevel.Junior,
                    < 5 => ExperienceLevel.Middle,
                    < 10 => ExperienceLevel.Senior,
                    _ => ExperienceLevel.Lead,
                },
                WorkplaceAddress = workplaces[rand.Next(workplaces.Length)],
                ProjectName = projects[rand.Next(projects.Length)],
                ProjectStatus = (ProjectStatus)rand.Next(0, 4),
                IsSecretProject = rand.NextDouble() > 0.7,
            };
        }
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        _listener.Stop();
        try { await _serverTask; } catch { /* ignore */ }
        _cts.Dispose();
    }
}
