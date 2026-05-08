namespace lab4_properties_lib
{
    public interface IEntityOperation
    {
        void Execute(Entity entity);
    }

    public sealed class EntityStore
    {
        private readonly List<Entity> _entities = new();

        public Entity Create()
        {
            var e = new Entity();
            _entities.Add(e);
            return e;
        }

        public IReadOnlyList<Entity> All => _entities;

        public void RunOperation(IEntityOperation operation)
        {
            foreach (var entity in _entities)
                operation.Execute(entity);
        }

        public IEnumerable<Entity> Where<T>(TypedKey<T> key) =>
            _entities.Where(e => e.Has(key));
    }
}
