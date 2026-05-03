using System.Runtime.CompilerServices;

namespace lab3_builder
{
    public class Models
    {
        public class TrackableObject
        {
            public List<ValidationError> Errors { get; } = new();
            public string CreatedByFile { get; }
            public int CreatedAtLine { get; }

            public TrackableObject(
                [CallerFilePath] string file = "",
                [CallerLineNumber] int line = 0)
            {
                CreatedByFile = file;
                CreatedAtLine = line;
            }
        }
        public sealed class MutableWorkplace : TrackableObject
        {
            public string? Address;
            public double? Rating;
            public string? DirectorName;
            public List<MutableProject> Projects = new();
            public List<MutablePerson> Persons = new();
        }

        public sealed class MutableProject : TrackableObject
        {
            public Guid Id = Guid.NewGuid();
            public string? Name;
            public string? Description;
            public ProjectStatus? Status;
            public bool? IsSecret;
        }

        public sealed class MutablePerson : TrackableObject
        {
            public Guid Id = Guid.NewGuid();
            public string? FullName;
            public ExperienceLevel? Level;
            public int? YearsOfExperience;
            public MutableProject? Project;
        }

        public enum ProjectStatus { New, Active, Done, Maintenance }
        public enum ExperienceLevel { Junior, Middle, Senior, Lead }

        public sealed class Workplace
        {
            public required string Address { get; init; }
            public required double Rating { get; init; }
            public required string DirectorName { get; init; }
            public required Project[] Projects { get; init; }
            public required Person[] Persons { get; init; }
            public override string ToString()
            {
                var sb = new System.Text.StringBuilder();

                sb.AppendLine($"Workplace: {Address}, rating: {Rating}, CEO: {DirectorName}");
                sb.AppendLine($"Project nr.: {Projects.Length}, Employees nr.: {Persons.Length}");
                sb.AppendLine();
                sb.AppendLine("Projects:");
                foreach (var pr in Projects)
                {
                    sb.AppendLine($"  {pr.Name} | {pr.Description} | {pr.Status} | is secret: {pr.IsSecret}");
                }
                var projectMap = Projects.ToDictionary(p => p.Id);
                sb.AppendLine("Emploees:");
                foreach (var person in Persons)
                {
                    var projectName = projectMap.TryGetValue(person.ProjectId, out var p)
                        ? p.Name
                        : "Unknown";

                    sb.AppendLine($"  {person.FullName} | {person.Level} | {person.YearsOfExperience} years | project name: {projectName}");
                }

                return sb.ToString();
            }
        }

        public sealed class Project
        {
            public required Guid Id { get; init; }
            public required string Name { get; init; }
            public required string Description { get; init; }
            public required ProjectStatus Status { get; init; }
            public required bool IsSecret { get; init; }
        }

        public sealed class Person
        {
            public required Guid Id { get; init; }
            public required string FullName { get; init; }
            public required ExperienceLevel Level { get; init; }
            public required int YearsOfExperience { get; init; }
            public required Guid ProjectId { get; init; }
        }
    }
}
