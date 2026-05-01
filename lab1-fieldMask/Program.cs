using System.Text;

var repo = new InMemoryPersonRepository();
var p1 = new Person { Id = Guid.NewGuid(), Age = 21, FullName = "test1", Height = 161.5, Status = SocialStatus.Single };
var p2 = new Person { Id = Guid.NewGuid(), Age = 22, FullName = "test2", Height = 162.5, Status = SocialStatus.Divorced };
var p3 = new Person { Id = Guid.NewGuid(), Age = 23, FullName = "test3", Height = 163.5, Status = SocialStatus.Divorced };
repo.Add(p1);
repo.Add(p2);
repo.Add(p3);

PersonFieldMask mask = new()
{
    Age = true,
    FullName = true
};
Console.WriteLine("--- Basic ---");
PersonFieldMasksFunc.Print(p1, mask);
PersonFieldMasksFunc.Print(p2, mask);
PersonFieldMasksFunc.Print(p3, mask);

PersonFieldMaskByte byteMask1 = new()
{
    Id = true,
    Age = true
};
PersonFieldMaskByte byteMask2 = new()
{
    Height = true,
    Status = true
};
Console.WriteLine("---#1: Byte Mask (mask1) ---");
PersonFieldMasksFunc.BytePrint(p1, byteMask1);
PersonFieldMasksFunc.BytePrint(p2, byteMask1);
PersonFieldMasksFunc.BytePrint(p3, byteMask1);

Console.WriteLine("---#2: 3 mask methods ---");
Console.WriteLine("Base mask2");
PersonFieldMasksFunc.BytePrint(p1, byteMask2);
Console.WriteLine("Union mask1 & mask2");
PersonFieldMasksFunc.BytePrint(p1, PersonFieldMasksFunc.UnionMask(byteMask1, byteMask2));
Console.WriteLine("Invert mask1");
PersonFieldMasksFunc.BytePrint(p1, PersonFieldMasksFunc.InvertMask(byteMask1));
Console.WriteLine("Invert mask2");
PersonFieldMasksFunc.BytePrint(p1, PersonFieldMasksFunc.InvertMask(byteMask2));
Console.WriteLine("Does byteMask 1 contain byteMask 2? :\t" +
    PersonFieldMasksFunc.Contains(byteMask1, byteMask2));
var invertedMask1 = PersonFieldMasksFunc.InvertMask(byteMask1);
Console.WriteLine("Does INVERTED byteMask 1 contain byteMask 2? :\t" +
    PersonFieldMasksFunc.Contains(invertedMask1, byteMask2));

Console.WriteLine("\n---#3: Copy on mask ---");
Console.WriteLine("Same by STATUS \t Fields to Copy: AGE, HEIGHT");
Console.WriteLine("Reference person:");
PersonFieldMasksFunc.BytePrint(p2, PersonFieldMasksFunc.InvertMask(new PersonFieldMaskByte()));
PersonFieldMaskByte maskSame = new() { Status = true };
PersonFieldMaskByte maskCopy = new() { Age = true, Height = true };
var copies = PersonFieldMasksFunc.CopySameByMask(repo.GetAll(), p2, maskSame, maskCopy);
Console.WriteLine($"Found And Copied: {copies.Count}");
foreach (var c in copies)
    PersonFieldMasksFunc.BytePrint(c, PersonFieldMasksFunc.InvertMask(new PersonFieldMaskByte()));


public static class PersonFieldMasksFunc
{
    public static PersonFieldMaskByte UnionMask(PersonFieldMaskByte a, PersonFieldMaskByte b) { return new PersonFieldMaskByte { value = (byte)(a.value | b.value) }; }
    public static bool Contains(PersonFieldMaskByte a, PersonFieldMaskByte b)
    {
        return (a.value & b.value) == b.value;
    }
    public static PersonFieldMaskByte InvertMask(PersonFieldMaskByte a)
    {
        var mask = a;
        mask.value = (byte)~a.value;
        return mask;
    }
    public static List<Person> CopySameByMask(List<Person> repo, Person reference, PersonFieldMaskByte maskSame, PersonFieldMaskByte maskToCopy)
    {
        var result = new List<Person>();

        foreach (var person in repo)
        {
            if (!AreEqualByMask(person, reference, maskSame))
                continue;

            var copy = new Person
            {
                Id = person.Id,
                Age = person.Age,
                FullName = person.FullName,
                Height = person.Height,
                Status = person.Status,
            };
            if (maskToCopy.Id) copy.Id = reference.Id;
            if (maskToCopy.Age) copy.Age = reference.Age;
            if (maskToCopy.FullName) copy.FullName = reference.FullName;
            if (maskToCopy.Height) copy.Height = reference.Height;
            if (maskToCopy.Status) copy.Status = reference.Status;

            result.Add(copy);
        }

        return result;
    }
    private static bool AreEqualByMask(Person a, Person b, PersonFieldMaskByte mask)
    {
        if (mask.Id && a.Id != b.Id) return false;
        if (mask.Age && a.Age != b.Age) return false;
        if (mask.FullName && a.FullName != b.FullName) return false;
        if (mask.Height && a.Height != b.Height) return false;
        if (mask.Status && a.Status != b.Status) return false;
        return true;
    }


    public static void BytePrint(Person person, PersonFieldMaskByte mask)
    {
        StringBuilder res = new();
        if (mask.Id)
            res.Append("Id:\t" + person.Id + "\n");
        if (mask.Age)
            res.Append("Age:\t" + person.Age + "\n");
        if (mask.FullName)
            res.Append("Full Name:\t" + person.FullName + "\n");
        if (mask.Height)
            res.Append("Height:\t" + person.Height + "\n");
        if (mask.Status)
            res.Append("Status:\t" + person.Status + "\n");
        var output = res.ToString();
        Console.WriteLine(output);
    }

    public static void Print(Person person, PersonFieldMask? mask = null)
    {

        mask ??= new PersonFieldMask
        {
            Status = true,
            Id = true,
            Age = true,
            Height = true,
            FullName = true
        };
        StringBuilder res = new();
        if (mask.Id)
            res.Append("Id:\t" + person.Id + "\n");
        if (mask.Age)
            res.Append("Age:\t" + person.Age + "\n");
        if (mask.FullName)
            res.Append("Full Name:\t" + person.FullName + "\n");
        if (mask.Height)
            res.Append("Height:\t" + person.Height + "\n");
        if (mask.Status)
            res.Append("Status:\t" + person.Status + "\n");
        var output = res.ToString();
        Console.WriteLine(output);
    }
}

public record struct PersonFieldMaskByte
{
    public byte value;
    public readonly bool Get(PersonField field)
    {
        int idIdx = (int)field;
        byte v = (byte)(1 << idIdx);
        return (value & v) != 0;
    }
    public void Set(PersonField field, bool val)
    {
        int idIdx = (int)field;
        byte v = (byte)(1 << idIdx);
        if (val == true)
        {
            value = (byte)(value | v);
        }
        else
        {
            value = (byte)(value & (byte)~v);
        }
    }
    public bool Id
    {
        readonly get => Get(PersonField.Id);
        set => Set(PersonField.Id, value);
    }
    public bool Age
    {
        readonly get => Get(PersonField.Age);
        set => Set(PersonField.Age, value);
    }
    public bool FullName
    {
        readonly get => Get(PersonField.FullName);
        set => Set(PersonField.FullName, value);
    }
    public bool Height
    {
        readonly get => Get(PersonField.Height);
        set => Set(PersonField.Height, value);
    }
    public bool Status
    {
        readonly get => Get(PersonField.Status);
        set => Set(PersonField.Status, value);
    }

}

public enum PersonField
{
    Id = 0, Age = 1, FullName = 2, Height = 3, Status = 4
}

public class PersonFieldMask
{
    public bool Id { get; set; } = false;
    public bool Age { get; set; } = false;
    public bool FullName { get; set; } = false;
    public bool Height { get; set; } = false;
    public bool Status { get; set; } = false;
}
public class Person
{
    public required Guid Id { get; set; }
    public required int Age { get; set; }
    public required string FullName { get; set; }
    public required double Height { get; set; }
    public required SocialStatus Status { get; set; }
}

public enum SocialStatus
{
    Underage,
    Single,
    InRelationship,
    Married,
    Divorced,
    WithChildren,
    Childfree
}

public interface IRepository<T>
{
    void Add(T item);
    void Remove(Guid id);
    T GetById(Guid id);
    List<T> GetAll();
}

public interface IPersonRepository : IRepository<Person>
{
    List<Person> FindByName(string name);
}

public class InMemoryPersonRepository : IPersonRepository
{
    private readonly List<Person> _data = [];

    public void Add(Person item)
    {
        _data.Add(item);
    }

    public void Remove(Guid id)
    {
        var person = _data.FirstOrDefault(p => p.Id == id);
        if (person != null)
        {
            _data.Remove(person);
        }
    }

    public Person GetById(Guid id)
    {
        return _data.First(p => p.Id == id);
    }

    public List<Person> GetAll()
    {
        return [.. _data];
    }

    public List<Person> FindByName(string name)
    {
        return [.. _data.Where(p => p.FullName.Contains(name))];
    }
}

