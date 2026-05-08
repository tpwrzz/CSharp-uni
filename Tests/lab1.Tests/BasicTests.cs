namespace lab1_fieldMask.lab1.Tests;

public class BasicTests : IClassFixture<PersonTestFixture>
{
    private readonly PersonTestFixture _fix;

    public BasicTests(PersonTestFixture fixture)
    {
        _fix = fixture;
    }

    [Fact]
    public void DefaultMask_AllFieldsFalse()
    {
        var mask = new PersonFieldMask();

        Assert.False(mask.Id);
        Assert.False(mask.Age);
        Assert.False(mask.FullName);
        Assert.False(mask.Height);
        Assert.False(mask.Status);
    }

    [Fact]
    public void Mask_SetFields_ReturnsCorrectValues()
    {
        var mask = new PersonFieldMask { Age = true, FullName = true };

        Assert.False(mask.Id);
        Assert.True(mask.Age);
        Assert.True(mask.FullName);
        Assert.False(mask.Height);
        Assert.False(mask.Status);
    }


    [Fact]
    public void Print_WithAgeMask_OutputContainsAge()
    {
        var mask = new PersonFieldMask { Age = true };

        var output = ConsoleCapture.Run(() => PersonFieldMasksFunc.Print(_fix.P1, mask));

        Assert.Contains("21", output);
        Assert.DoesNotContain("test1", output);
    }

    [Fact]
    public void Print_WithFullNameMask_OutputContainsName()
    {
        var mask = new PersonFieldMask { FullName = true };

        var output = ConsoleCapture.Run(() => PersonFieldMasksFunc.Print(_fix.P1, mask));

        Assert.Contains("test1", output);
        Assert.DoesNotContain("21", output);
    }

    [Fact]
    public void Print_WithAllFields_OutputContainsEverything()
    {
        var mask = new PersonFieldMask
        {
            Id = true,
            Age = true,
            FullName = true,
            Height = true,
            Status = true
        };

        var output = ConsoleCapture.Run(() => PersonFieldMasksFunc.Print(_fix.P1, mask));

        Assert.Contains("21", output);
        Assert.Contains("test1", output);
        Assert.Contains("161.5", output);
        Assert.Contains("Single", output);
    }

    [Fact]
    public void Print_WithEmptyMask_OutputIsEmpty()
    {
        var mask = new PersonFieldMask();

        var output = ConsoleCapture.Run(() => PersonFieldMasksFunc.Print(_fix.P1, mask));

        Assert.True(string.IsNullOrWhiteSpace(output));
    }

    [Fact]
    public void Print_NullMask_PrintsAllFields()
    {
        var output = ConsoleCapture.Run(() => PersonFieldMasksFunc.Print(_fix.P1, null));

        Assert.Contains("21", output);
        Assert.Contains("test1", output);
        Assert.Contains("161.5", output);
        Assert.Contains("Single", output);
    }


}