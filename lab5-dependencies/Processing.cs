using lab4_properties;
using System.Collections.Immutable;
public readonly struct FilterByLevelConfig
{
    public required ExperienceLevel MinLevel { get; init; }
}

public readonly struct FilterByRatingConfig
{
    public required double MinRating { get; init; }
}

public readonly struct SortByExperienceConfig
{
    public required bool Ascending { get; init; }
}


public static class Processing
{
    public static IEnumerable<Person> FilterByLevel(
        IEnumerable<Person> persons,
        FilterByLevelConfig config)
    {
        foreach (var p in persons)
        {
            if (p.Level >= config.MinLevel)
                yield return p;
        }
    }
    public static IEnumerable<Person> FilterByIsSecret(
        IEnumerable<Person> persons,
        FilterByRatingConfig config)
    {
        foreach (var p in persons)
        {
            if (p.IsSecretProject)
                yield return p;
        }
    }
    public static IEnumerable<Person> SortByExperience(
        IEnumerable<Person> persons,
        SortByExperienceConfig config)
    {
        return config.Ascending
            ? persons.OrderBy(p => p.YearsOfExperience)
            : persons.OrderByDescending(p => p.YearsOfExperience);
    }
}

public interface ITransformationStep<T>
{
    IEnumerable<T> Apply(IEnumerable<T> items);
}


public sealed class FilterByLevelAdapter : ITransformationStep<Person>
{
    private readonly FilterByLevelConfig _config;
    public FilterByLevelAdapter(FilterByLevelConfig config) => _config = config;

    public IEnumerable<Person> Apply(IEnumerable<Person> items) =>
        Processing.FilterByLevel(items, _config);
}

public sealed class FilterByRatingAdapter : ITransformationStep<Person>
{
    private readonly FilterByRatingConfig _config;
    public FilterByRatingAdapter(FilterByRatingConfig config) => _config = config;

    public IEnumerable<Person> Apply(IEnumerable<Person> items) =>
        Processing.FilterByIsSecret(items, _config);
}

public sealed class SortByExperienceAdapter : ITransformationStep<Person>
{
    private readonly SortByExperienceConfig _config;
    public SortByExperienceAdapter(SortByExperienceConfig config) => _config = config;

    public IEnumerable<Person> Apply(IEnumerable<Person> items) =>
        Processing.SortByExperience(items, _config);
}


public interface ITransformationPipeline<T>
{
    IEnumerable<T> Run(IEnumerable<T> items);
}

public sealed class DoNothingPipeline : ITransformationPipeline<Person>
{
    public static readonly DoNothingPipeline Instance = new();
    private DoNothingPipeline() { }

    public IEnumerable<Person> Run(IEnumerable<Person> items) => items;
}
public sealed class SingleStepPipeline : ITransformationPipeline<Person>
{
    private readonly ITransformationStep<Person> _step;

    public SingleStepPipeline(ITransformationStep<Person> step) => _step = step;

    public IEnumerable<Person> Run(IEnumerable<Person> items) =>
        _step.Apply(items);
}
public sealed class ListPipeline : ITransformationPipeline<Person>
{
    private readonly ImmutableArray<ITransformationStep<Person>> _steps;

    public ListPipeline(IEnumerable<ITransformationStep<Person>> steps)
    {
        _steps = [.. steps];
    }

    public IEnumerable<Person> Run(IEnumerable<Person> items)
    {
        foreach (var step in _steps)
            items = step.Apply(items);
        return items;
    }
}
