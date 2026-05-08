namespace lab4_properties
{
    public enum ExperienceLevel { Junior, Middle, Senior, Lead }
    public enum ProjectStatus { New, Active, Done, Maintenance }

    public sealed class Person
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FullName { get; set; } = "";
        public int YearsOfExperience { get; set; }
        public ExperienceLevel Level { get; set; }
        public string WorkplaceAddress { get; set; } = "";
        public string ProjectName { get; set; } = "";
        public ProjectStatus ProjectStatus { get; set; }
        public bool IsSecretProject { get; set; }

        public override string ToString() =>
            $"Person({FullName}, {Level}, {YearsOfExperience}y) " +
            $"@ {WorkplaceAddress} [{ProjectName}/{(IsSecretProject ? "/Secret" : string.Empty)}]";
    }
}
