using lab4_properties_lib;

namespace lab4_properties
{
    public sealed class PrintPersonOperation : IEntityOperation
    {
        public void Execute(Entity entity)
        {
            if (!entity.Has(PersonKeys.FullName)) return;

            var name = entity.Get(PersonKeys.FullName);
            var years = entity.GetOrDefault(PersonKeys.YearsOfExperience);
            var level = entity.GetOrDefault(PersonKeys.Level);

            Console.WriteLine($"  Person: {name} | {level} | {years} years");
        }
    }

    public sealed class PromoteLevelOperation : IEntityOperation
    {
        private readonly int _yearsThreshold;

        public PromoteLevelOperation(int yearsThreshold)
        {
            _yearsThreshold = yearsThreshold;
        }

        public void Execute(Entity entity)
        {
            if (!entity.TryGet(PersonKeys.YearsOfExperience, out var years)) return;
            if (!entity.TryGet(PersonKeys.Level, out var level)) return;

            if (years >= _yearsThreshold && level < ExperienceLevel.Lead)
            {
                var promoted = level + 1;
                entity.Set(PersonKeys.Level, promoted);
                Console.WriteLine($"  Promoted: {entity.Get(PersonKeys.FullName)} to {promoted} position");
            }
        }
    }
    public sealed class PrintWorkplaceOperation : IEntityOperation
    {
        public void Execute(Entity entity)
        {
            if (!entity.Has(WorkplaceKeys.Address)) return;

            var address = entity.Get(WorkplaceKeys.Address);
            var rating = entity.Get(WorkplaceKeys.Rating);
            var director = entity.Get(WorkplaceKeys.DirectorName);

            Console.WriteLine($"  Workplace: {address} | rating {rating} | CEO {director}");
        }
    }
    public sealed class FlagLowRatedWorkplaceOperation : IEntityOperation
    {
        public static readonly TypedKey<bool> NeedsAttention =
            KeyRegistry.Register<bool>("Workplace.NeedsAttention");

        private readonly double _minRating;

        public FlagLowRatedWorkplaceOperation(double minRating)
        {
            _minRating = minRating;
        }

        public void Execute(Entity entity)
        {
            if (!entity.TryGet(WorkplaceKeys.Rating, out var rating)) return;

            if (rating < _minRating)
            {
                entity.Set(NeedsAttention, true);
                Console.WriteLine(
                    $"  [!] Workplace '{entity.Get(WorkplaceKeys.Address)}' " +
                    $"flagged (rating {rating} < {_minRating})");
            }
        }
    }

}
