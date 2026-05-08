namespace lab4_properties_lib;

public static class KeyRegistry
{
    private static readonly object _lock = new();

    private static readonly Dictionary<string, int> _nameToId = new();

    private static readonly Dictionary<int, string> _idToName = new();

    private static int _nextId = 0;

    public static TypedKey<T> Register<T>(string name)
    {
        lock (_lock)
        {
            if (_nameToId.TryGetValue(name, out var existingId))
            {
                throw new InvalidOperationException(
                    $"Key '{name}' is already registered with id={existingId}. " +
                    $"Two consumers cannot register incompatible data under the same key name.");
            }

            var id = _nextId++;
            _nameToId[name] = id;
            _idToName[id] = name;

            return new TypedKey<T>(id, name);
        }
    }

    public static string GetName(int id) =>
        _idToName.TryGetValue(id, out var name) ? name : $"<unknown#{id}>";

    public static IReadOnlyDictionary<string, int> AllKeys => _nameToId;
}
