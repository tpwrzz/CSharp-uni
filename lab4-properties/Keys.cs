using lab4_properties_lib;

namespace lab4_properties
{
    public static class PersonKeys
    {
        public static readonly TypedKey<string> FullName = KeyRegistry.Register<string>("Person.FullName");
        public static readonly TypedKey<int> YearsOfExperience = KeyRegistry.Register<int>("Person.YearsOfExperience");
        public static readonly TypedKey<ExperienceLevel> Level = KeyRegistry.Register<ExperienceLevel>("Person.Level");
        public static readonly TypedKey<Guid> WorkplaceId = KeyRegistry.Register<Guid>("Person.WorkplaceId");
    }
    public static class WorkplaceKeys
    {
        public static readonly TypedKey<Guid> Id = KeyRegistry.Register<Guid>("Workplace.Id");
        public static readonly TypedKey<string> Address = KeyRegistry.Register<string>("Workplace.Address");
        public static readonly TypedKey<double> Rating = KeyRegistry.Register<double>("Workplace.Rating");
        public static readonly TypedKey<string> DirectorName = KeyRegistry.Register<string>("Workplace.DirectorName");
    }

    public static class ProjectKeys
    {
        public static readonly TypedKey<string> Name = KeyRegistry.Register<string>("Project.Name");
        public static readonly TypedKey<ProjectStatus> Status = KeyRegistry.Register<ProjectStatus>("Project.Status");
        public static readonly TypedKey<bool> IsSecret = KeyRegistry.Register<bool>("Project.IsSecret");
    }

    public enum ExperienceLevel { Junior, Middle, Senior, Lead }
    public enum ProjectStatus { New, Active, Done, Maintenance }

}
