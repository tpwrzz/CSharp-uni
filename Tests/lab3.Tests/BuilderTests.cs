using static lab3_builder.Models;

namespace Tests.lab3.Tests
{
    public class BuilderTests
    {
        [Fact]
        public void PersonWithoutProject_ShouldFail()
        {
            var builder = new StrictWorkplaceBuilder()
                .SetAddress("A")
                .SetRating(4)
                .SetDirector("Boss");

            builder.AddPerson(p => p
                .SetFullName("Test")
                .SetExperience(5));

            var result = builder.Build();

            Assert.Null(result.Result);
            Assert.Contains(result.Errors!.Errors,
                e => e.Message.Contains("must have a project"));
        }

        [Fact]
        public async Task SetStatusAndVisibility_ShouldMatchSnapshot()
        {
            var builder = new StrictWorkplaceBuilder()
                .SetAddress("Addr")
                .SetRating(5)
                .SetDirector("CEO");

            var project = builder.AddProject(p => p
                .SetName("SecretProj")
                .SetDescription("Hidden stuff")
                .SetStatusAndVisibility(ProjectStatus.Active, true));

            builder.AddPerson(p => p
                .SetFullName("Agent")
                .SetExperience(10)
                .SetProject(project));

            var result = builder.Build();

            Assert.NotNull(result.Result);
            await Verifier.Verify(result.Result);
        }

        [Fact]
        public void SetStatusAndVisibility_ShouldOverridePreviousValues()
        {
            var builder = new StrictWorkplaceBuilder()
                .SetAddress("Addr")
                .SetRating(4)
                .SetDirector("Boss");

            builder.AddProject(p => p
                .SetName("Test")
                .SetDescription("Desc")
                .SetStatus(ProjectStatus.New)
                .SetSecret(false)
                .SetStatusAndVisibility(ProjectStatus.Maintenance, true));

            var result = builder.Build();

            var project = result.Result!.Projects.First();

            Assert.Equal(ProjectStatus.Maintenance, project.Status);
            Assert.True(project.IsSecret);
        }

        [Fact]
        public void SetStatusAndVisibility_ShouldSetBothFields()
        {
            var builder = new StrictWorkplaceBuilder()
                .SetAddress("Addr")
                .SetRating(4)
                .SetDirector("Boss");

            var project = builder.AddProject(p => p
                .SetName("Test")
                .SetDescription("Desc")
                .SetStatusAndVisibility(ProjectStatus.Done, true));

            var result = builder.Build();

            var builtProject = result.Result!.Projects.First();

            Assert.Equal(ProjectStatus.Done, builtProject.Status);
            Assert.True(builtProject.IsSecret);
        }

        [Fact]
        public void CopyWorkplaceInfo_ShouldCopyFields()
        {
            var source = new StrictWorkplaceBuilder()
                .SetAddress("A")
                .SetRating(4)
                .SetDirector("Boss");

            var target = new StrictWorkplaceBuilder()
                .CopyWorkplaceInfoFrom(source);

            var result = target.Build();

            Assert.Equal("A", result.Result!.Address);
        }

        [Fact]
        public void SimpleBuilder_ShouldFillDefaults()
        {
            var builder = new SimpleWorkplaceBuilder()
                .SetAddress("Addr");

            builder.AddProject();
            builder.AddPerson();

            var result = builder.Build();

            Assert.NotNull(result.Result);
            Assert.Equal("Unknown Name", result.Result!.Persons[0].FullName);
        }

        [Fact]
        public async Task Build_ValidWorkplace_ShouldMatchSnapshot()
        {
            var builder = new StrictWorkplaceBuilder()
                .SetAddress("Test Address")
                .SetRating(4.5)
                .SetDirector("John");

            var project = builder.AddProject(p => p
                .SetName("Proj")
                .SetDescription("Desc")
                .SetStatusAndVisibility(ProjectStatus.Active, false));

            builder.AddPerson(p => p
                .SetFullName("Alice")
                .SetExperience(5)
                .SetProject(project));

            var result = builder.Build();

            Assert.NotNull(result.Result);

            await Verifier.Verify(result.Result);
        }

        [Fact]
        public async Task Build_InvalidWorkplace_ShouldReturnErrorsSnapshot()
        {
            var builder = new StrictWorkplaceBuilder()
                .SetAddress("")
                .SetRating(999)
                .SetDirector("");

            builder.AddProject();
            builder.AddPerson();

            var result = builder.Build();

            Assert.Null(result.Result);

            await Verifier.Verify(result.Errors!.Errors.Select(e => e.ToString()));
        }

        [Fact]
        public void Scope_ShouldApplyDefaults()
        {
            var builder = new SimpleWorkplaceBuilder()
                .SetAddress("Addr")
                .SetRating(4)
                .SetDirector("Boss");

            var project = builder.AddProject(p => p
                .SetName("Proj")
                .SetDescription("Desc")
                .SetStatusAndVisibility(ProjectStatus.Active, false));

            var scope = builder.Scope()
                .SetDefaultProject(project)
                .SetDefaultLevel(ExperienceLevel.Senior);

            var person = scope.AddPerson(p => p.SetFullName("Test"));

            var result = builder.Build();

            var builtPerson = result.Result!.Persons.First();

            Assert.Equal(ExperienceLevel.Senior, builtPerson.Level);
        }
    }
}
