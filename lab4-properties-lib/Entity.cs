namespace lab4_properties_lib
{
    public sealed class Entity
    {
        private object?[] _props = new object?[8]; 

        public Guid Id { get; } = Guid.NewGuid();

        public void Set<T>(TypedKey<T> key, T value)
        {
            EnsureCapacity(key.Id);
            _props[key.Id] = value;
        }

        public T Get<T>(TypedKey<T> key)
        {
            if (!TryGet(key, out var value))
                throw new KeyNotFoundException(
                    $"Property '{key}' is not set on entity {Id}.");
            return value!;
        }

        public T? GetOrDefault<T>(TypedKey<T> key)
        {
            TryGet(key, out var value);
            return value;
        }

        public bool TryGet<T>(TypedKey<T> key, out T? value)
        {
            if (key.Id < _props.Length && _props[key.Id] is T typed)
            {
                value = typed;
                return true;
            }
            value = default;
            return false;
        }

        public bool Has<T>(TypedKey<T> key) =>
            key.Id < _props.Length && _props[key.Id] is not null;

        public void Remove<T>(TypedKey<T> key)
        {
            if (key.Id < _props.Length)
                _props[key.Id] = null;
        }

        private void EnsureCapacity(int id)
        {
            if (id < _props.Length) return;
            var newSize = _props.Length;
            while (newSize <= id) newSize *= 2;

            Array.Resize(ref _props, newSize);
        }
    }
}
