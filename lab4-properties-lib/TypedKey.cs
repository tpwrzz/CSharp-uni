namespace lab4_properties_lib
{
    public sealed class TypedKey<T>
    {
        public int Id { get; }
        public string Name { get; }

        internal TypedKey(int id, string name)
        {
            Id = id;
            Name = name;
        }

        public override string ToString() => $"Key<{typeof(T).Name}>({Name}#{Id})";
    }
}