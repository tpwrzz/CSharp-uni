using lab3_builder;
using System.Runtime.CompilerServices;
using static lab3_builder.Models;
using ValidationException = lab3_builder.ValidationException;

Console.WriteLine("--- STRICT BUILDER ---\n");

var strict = new StrictWorkplaceBuilder()
    .SetAddress("Chisnau, Main st., of. 89")
    .SetRating(4.7)
    .SetDirector("Joe Doe");

var projectAlpha = strict.AddProject(p => p
    .SetName("Alpha")
    .SetDescription("Main Platform CRM")
    .SetStatusAndVisibility(ProjectStatus.Active, isSecret: false));

var projectSecret = strict.AddProject(p => p
    .SetName("BlackBox")
    .SetDescription("Secret module")
    .SetStatusAndVisibility(ProjectStatus.Active, isSecret: true));

var scope = strict.Scope()
    .SetDefaultProject(projectAlpha)
    .SetDefaultLevel(ExperienceLevel.Senior);

scope.AddPerson(p => p.SetFullName("Ann Smith").SetExperience(7));
scope.AddPerson(p => p.SetFullName("Dave Brown").SetExperience(3));

var lead = strict.AddPerson(p => p
    .SetFullName("Marie N.")
    .SetExperience(12)
    .SetProject(projectSecret));

var result = strict.Build();
if (result.IsSuccess)
{
    Console.WriteLine(result.Result.ToString());
}
else
{
    foreach (var e in result.Errors.Errors)
        Console.WriteLine(e);
}
Console.WriteLine("\n--- Simple BUILDER (sets to default) ---\n");

var simple = new SimpleWorkplaceBuilder()
    .SetAddress("Balti, 57th avenue, of.101");

simple.AddProject(p => p.SetName("MVP").SetDescription("Simple prototype"));
simple.AddProject();
simple.AddPerson(p => p.SetFullName("Test Person"));
simple.AddPerson();

var simpleRes = simple.Build();
if (result.IsSuccess)
{
    Console.WriteLine(simpleRes.Result.ToString());
}
else
{
    foreach (var e in simpleRes.Errors.Errors)
        Console.WriteLine(e);
}
Console.WriteLine("\n--- Validation ---\n");


var bad = new StrictWorkplaceBuilder()
    .SetAddress("")
    .SetRating(9.9)
    .SetDirector("OK");

bad.AddProject();
bad.AddPerson();

var resBad = bad.Build();
if (result.IsSuccess)
{
    Console.WriteLine(resBad.Result.ToString());
}
else
{
    Console.WriteLine($"Found {resBad.Errors.Errors.Count} errors:");
    foreach (var e in resBad.Errors.Errors)
        Console.WriteLine(e);
}

Console.WriteLine("\n--- CopyWorkplaceInfoFrom HighEnd Function---\n");

var source = new StrictWorkplaceBuilder()
    .SetAddress("Copied address, 1")
    .SetRating(4.2)
    .SetDirector("CEO-resource");

var target = new StrictWorkplaceBuilder()
    .CopyWorkplaceInfoFrom(source);

var proj = target.AddProject(p => p
    .SetName("Copied")
    .SetDescription("Project after copy")
    .SetStatusAndVisibility(ProjectStatus.New, false));

target.AddPerson(p => p
    .SetFullName("New John")
    .SetExperience(4)
    .SetProject(proj));

var copied = target.Build();
if (copied.Result != null)
{
    Console.WriteLine(copied.Result.ToString());
}
else
{
    Console.WriteLine($"Found {copied.Errors.Errors.Count} errors:");
    foreach (var e in copied.Errors.Errors)
        Console.WriteLine(e);
}


public static class WorkplaceBuilderExtensions
{
    public static WorkplaceScopeBuilder Scope(this AbstractWorkplaceBuilder builder)
        => new(builder);
}

public sealed class WorkplaceScopeBuilder
{
    private readonly AbstractWorkplaceBuilder _workplace;
    private readonly MutablePerson _defaults = new();

    public WorkplaceScopeBuilder(AbstractWorkplaceBuilder workplace)
    {
        _workplace = workplace;
    }

    public WorkplaceScopeBuilder SetDefaultProject(ProjectBuilder project)
    {
        _defaults.Project = project.Model;
        return this;
    }

    public WorkplaceScopeBuilder SetDefaultLevel(ExperienceLevel level)
    {
        _defaults.Level = level;
        return this;
    }

    public PersonBuilder AddPerson(Action<PersonBuilder>? configure = null)
    {
        var builder = _workplace.AddPerson();
        if (_defaults.Project != null) builder.Model.Project = _defaults.Project;
        if (_defaults.Level != null) builder.Model.Level = _defaults.Level;
        configure?.Invoke(builder);
        return builder;
    }
}

public sealed class PersonBuilder
{
    public readonly MutablePerson Model;
    public PersonBuilder(MutablePerson model) => Model = model;

    public PersonBuilder SetFullName(
       string name,
       [CallerFilePath] string file = "",
       [CallerLineNumber] int line = 0)
    {
        Validator.For(name, Model.Errors, file, line)
            .NotEmpty("PersonBuilder: FullName is required");

        if (!string.IsNullOrWhiteSpace(name))
            Model.FullName = name;

        return this;
    }

    public PersonBuilder SetYearsOfExperience(
        int years,
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
    {
        Validator.For(years, Model.Errors, file, line)
            .Must(x => x >= 0, "PersonBuilder: YearsOfExperience must be >= 0");

        Model.YearsOfExperience = years;
        return this;
    }

    public PersonBuilder SetLevel(
     ExperienceLevel level,
     [CallerFilePath] string file = "",
     [CallerLineNumber] int line = 0)
    {
        Model.Level = level;
        return this;
    }

    public PersonBuilder SetProject(
    ProjectBuilder project,
    [CallerFilePath] string file = "",
    [CallerLineNumber] int line = 0)
    {
        Validator.For(project, Model.Errors, file, line)
            .NotEmpty("PersonBuilder: Project must be provided");

        if (project?.Model == null)
        {
            Model.Errors.Add(new ValidationError("PersonBuilder: Project builder is invalid", file, line));
            return this;
        }

        Model.Project = project.Model;
        return this;
    }
    public PersonBuilder SetExperience(
    int years,
    [CallerFilePath] string file = "",
    [CallerLineNumber] int line = 0)
    {
        Validator.For(years, Model.Errors, file, line)
            .Must(x => x >= 0, "PersonBuilder: YearsOfExperience must be >= 0")
            .Must(x => x <= 60, "PersonBuilder: YearsOfExperience looks unrealistic");

        if (years < 0) return this;

        Model.YearsOfExperience = years;
        Model.Level = years switch
        {
            < 2 => ExperienceLevel.Junior,
            < 5 => ExperienceLevel.Middle,
            < 10 => ExperienceLevel.Senior,
            _ => ExperienceLevel.Lead,
        };

        return this;
    }

    public PersonBuilder CopyRoleFrom(PersonBuilder other)
    {
        if (other.Model.Project != null) Model.Project = other.Model.Project;
        if (other.Model.Level != null) Model.Level = other.Model.Level;
        return this;
    }
}
public sealed class ProjectBuilder
{
    public readonly MutableProject Model;
    public ProjectBuilder(MutableProject model) => Model = model;

    public ProjectBuilder SetName(string name,
        [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {

        Validator.For(name, Model.Errors, file, line)
            .NotEmpty("ProjectBuilder: Name is required");

        if (!string.IsNullOrWhiteSpace(name))
            Model.Name = name;

        return this;
    }
    public ProjectBuilder SetDescription(
        string desc,
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
    {
        Validator.For(desc, Model.Errors, file, line)
            .NotEmpty("ProjectBuilder: Description is required");

        if (!string.IsNullOrWhiteSpace(desc))
            Model.Description = desc;

        return this;
    }
    public ProjectBuilder SetStatus(
         ProjectStatus status,
         [CallerFilePath] string file = "",
         [CallerLineNumber] int line = 0)
    {
        Model.Status = status;
        return this;
    }

    public ProjectBuilder SetSecret(
        bool isSecret,
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0)
    {
        Model.IsSecret = isSecret;
        return this;
    }

    public ProjectBuilder SetStatusAndVisibility(ProjectStatus status, bool isSecret)
    {
        Model.Status = status;
        Model.IsSecret = isSecret;
        return this;
    }

    public ProjectBuilder CopyMetaFrom(ProjectBuilder other)
    {
        if (other.Model.Status != null) Model.Status = other.Model.Status;
        if (other.Model.IsSecret != null) Model.IsSecret = other.Model.IsSecret;
        return this;
    }
}

public sealed class SimpleWorkplaceBuilder : AbstractWorkplaceBuilder
{
    public override SimpleWorkplaceBuilder SetAddress(string address,
        [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        Model.Address = address;
        return this;
    }

    public override SimpleWorkplaceBuilder SetRating(double rating,
        [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        Model.Rating = rating;
        return this;
    }

    public override SimpleWorkplaceBuilder SetDirector(string name,
        [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        Model.DirectorName = name;
        return this;
    }

    public override ProjectBuilder AddProject(Action<ProjectBuilder>? configure = null)
    {
        var m = new MutableProject();
        Model.Projects.Add(m);
        var b = new ProjectBuilder(m);
        configure?.Invoke(b);
        return b;
    }

    public override PersonBuilder AddPerson(Action<PersonBuilder>? configure = null)
    {
        var m = new MutablePerson();
        Model.Persons.Add(m);
        var b = new PersonBuilder(m);
        configure?.Invoke(b);
        return b;
    }

    public override BuildResult Build()
    {
        var projects = Model.Projects.Select(p => new Project
        {
            Id = p.Id,
            Name = p.Name ?? "Unnamed Project",
            Description = p.Description ?? "No description yet",
            Status = p.Status ?? ProjectStatus.New,
            IsSecret = p.IsSecret ?? false,
        }).ToArray();

        var projMap = Model.Projects.Zip(projects).ToDictionary(x => x.First, x => x.Second);

        var persons = Model.Persons.Select(p => new Person
        {
            Id = p.Id,
            FullName = p.FullName ?? "Unknown Name",
            Level = p.Level ?? ExperienceLevel.Junior,
            YearsOfExperience = p.YearsOfExperience ?? 0,
            ProjectId = p.Project != null ? projMap[p.Project].Id : projects.FirstOrDefault()?.Id ?? Guid.NewGuid()
        }).ToArray();

        return new BuildResult
        {
            Result = new Workplace
            {
                Address = Model.Address ?? "No address",
                Rating = Model.Rating ?? 0.0,
                DirectorName = Model.DirectorName ?? "Unknown Director",
                Projects = projects,
                Persons = persons,
            }
        };
    }
}

public sealed class StrictWorkplaceBuilder : AbstractWorkplaceBuilder
{
    public override StrictWorkplaceBuilder SetAddress(string address,
        [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        Validator.For(address, Model.Errors, file, line)
        .NotEmpty("Address cannot be empty");

        if (!string.IsNullOrWhiteSpace(address))
            Model.Address = address;

        return this;
    }

    public override StrictWorkplaceBuilder SetRating(double rating,
        [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        Validator.For(rating, Model.Errors, file, line)
        .InRange(0, 5, $"Rating must be 1 - 5 (received {rating})");

        if (rating >= 1 && rating <= 5)
            Model.Rating = rating;

        return this;
    }

    public override StrictWorkplaceBuilder SetDirector(string name,
        [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        Validator.For(name, Model.Errors, file, line)
        .NotEmpty("Director name cannot be empty");

        if (!string.IsNullOrWhiteSpace(name))
            Model.DirectorName = name;

        return this;
    }

    public override ProjectBuilder AddProject(Action<ProjectBuilder>? configure = null)
    {
        var m = new MutableProject();
        Model.Projects.Add(m);
        var b = new ProjectBuilder(m);
        configure?.Invoke(b);
        return b;
    }

    public override PersonBuilder AddPerson(Action<PersonBuilder>? configure = null)
    {
        var m = new MutablePerson();
        Model.Persons.Add(m);
        var b = new PersonBuilder(m);
        configure?.Invoke(b);
        return b;
    }

    public override BuildResult Build()
    {
        var errors = new List<ValidationError>();

        errors.AddRange(Model.Projects.SelectMany(p => p.Errors));
        errors.AddRange(Model.Persons.SelectMany(p => p.Errors));
        ValidateRelations(errors);

        if (errors.Any())
        {
            return new BuildResult
            {
                Errors = new ValidationException(errors)
            };
        }
        var projects = Model.Projects.Select(p => new Project
        {
            Id = p.Id,
            Name = p.Name!,
            Description = p.Description!,
            Status = p.Status!.Value,
            IsSecret = p.IsSecret!.Value,
        }).ToArray();

        var projMap = Model.Projects.Zip(projects).ToDictionary(x => x.First, x => x.Second);

        var persons = Model.Persons.Select(p => new Person
        {
            Id = p.Id,
            FullName = p.FullName!,
            Level = p.Level!.Value,
            YearsOfExperience = p.YearsOfExperience!.Value,
            ProjectId = projMap[p.Project!].Id,
        }).ToArray();

        return new BuildResult
        {
            Result = new Workplace
            {
                Address = Model.Address!,
                Rating = Model.Rating!.Value,
                DirectorName = Model.DirectorName!,
                Projects = projects,
                Persons = persons,
            }
        };
    }
    private void ValidateRelations(List<ValidationError> errors)
    {
        foreach (var p in Model.Persons)
        {
            if (p.Project == null)
            {
                errors.Add(new ValidationError(
                    "Person must have a project",
                    p.CreatedByFile,
                    p.CreatedAtLine));
            }
            else if (!Model.Projects.Contains(p.Project))
            {
                errors.Add(new ValidationError(
                    "Person references project not in workplace",
                    p.CreatedByFile,
                    p.CreatedAtLine));
            }
        }
    }
}
public class BuildResult
{
    public Workplace? Result { get; set; } = null;
    public ValidationException? Errors { get; set; } = null;
    public bool IsSuccess => Result != null;

}

public abstract class AbstractWorkplaceBuilder
{
    protected readonly MutableWorkplace Model = new();
    public abstract AbstractWorkplaceBuilder SetAddress(string address,
        [CallerFilePath] string file = "", [CallerLineNumber] int line = 0);
    public abstract AbstractWorkplaceBuilder SetRating(double rating,
        [CallerFilePath] string file = "", [CallerLineNumber] int line = 0);
    public abstract AbstractWorkplaceBuilder SetDirector(string name,
        [CallerFilePath] string file = "", [CallerLineNumber] int line = 0);

    public abstract ProjectBuilder AddProject(Action<ProjectBuilder>? configure = null);
    public abstract PersonBuilder AddPerson(Action<PersonBuilder>? configure = null);

    public abstract BuildResult Build();
    public AbstractWorkplaceBuilder CopyWorkplaceInfoFrom(AbstractWorkplaceBuilder other,
        [CallerFilePath] string file = "", [CallerLineNumber] int line = 0)
    {
        if (other.Model.Address != null) SetAddress(other.Model.Address, file, line);
        if (other.Model.DirectorName != null) SetDirector(other.Model.DirectorName, file, line);
        if (other.Model.Rating != null) SetRating(other.Model.Rating.Value, file, line);
        return this;
    }
}