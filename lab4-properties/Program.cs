using lab4_properties;
using lab4_properties_lib;

var store = new EntityStore();

var wp1 = store.Create();
wp1.Set(WorkplaceKeys.Id, Guid.NewGuid());
wp1.Set(WorkplaceKeys.Address, "Chisinau, Main St. 10");
wp1.Set(WorkplaceKeys.Rating, 4.7);
wp1.Set(WorkplaceKeys.DirectorName, "Ion Popescu");

var wp2 = store.Create();
wp2.Set(WorkplaceKeys.Id, Guid.NewGuid());
wp2.Set(WorkplaceKeys.Address, "Balti, 57th Avenue");
wp2.Set(WorkplaceKeys.Rating, 2.9);
wp2.Set(WorkplaceKeys.DirectorName, "Maria Ionescu");

var p1 = store.Create();
p1.Set(PersonKeys.FullName, "Anna Smith");
p1.Set(PersonKeys.YearsOfExperience, 7);
p1.Set(PersonKeys.Level, ExperienceLevel.Senior);
p1.Set(PersonKeys.WorkplaceId, wp1.Get(WorkplaceKeys.Id));

var p2 = store.Create();
p2.Set(PersonKeys.FullName, "Dave Brown");
p2.Set(PersonKeys.YearsOfExperience, 3);
p2.Set(PersonKeys.Level, ExperienceLevel.Middle);
p2.Set(PersonKeys.WorkplaceId, wp1.Get(WorkplaceKeys.Id));

var p3 = store.Create();
p3.Set(PersonKeys.FullName, "Marie N.");
p3.Set(PersonKeys.YearsOfExperience, 12);
p3.Set(PersonKeys.Level, ExperienceLevel.Lead);
p3.Set(PersonKeys.WorkplaceId, wp2.Get(WorkplaceKeys.Id));

var proj = store.Create();
proj.Set(ProjectKeys.Name, "CRM Alpha");
proj.Set(ProjectKeys.Status, ProjectStatus.Active);
proj.Set(ProjectKeys.IsSecret, false);

Console.WriteLine("--- All Workplaces ---");
store.RunOperation(new PrintWorkplaceOperation());

Console.WriteLine("\n--- All Persons ---");
store.RunOperation(new PrintPersonOperation());

Console.WriteLine("\n--- Flag low-rated workplaces (< 3.5) ---");
store.RunOperation(new FlagLowRatedWorkplaceOperation(minRating: 3.5));

Console.WriteLine("\n--- Promote persons with 5+ years ---");
store.RunOperation(new PromoteLevelOperation(yearsThreshold: 5));

Console.WriteLine("\n--- Persons after promotion ---");
store.RunOperation(new PrintPersonOperation());

Console.WriteLine("\n--- Filter: only entities with WorkplaceId ---");
var persons = store.Where(PersonKeys.WorkplaceId);
foreach (var p in persons)
    Console.WriteLine($"  {p.Get(PersonKeys.FullName)} at workplace: {p.Get(PersonKeys.WorkplaceId)}");

Console.WriteLine("\n--- Flagged workplaces ---");
var flagged = store.Where(FlagLowRatedWorkplaceOperation.NeedsAttention);
foreach (var w in flagged)
    Console.WriteLine($"  {w.Get(WorkplaceKeys.Address)} needs attention!");


Console.WriteLine("\n--- Registry: duplicate key demo ---");
Console.WriteLine("Registered keys:");
foreach (var (name, id) in KeyRegistry.AllKeys)
    Console.WriteLine($"  [{id}] {name}");

Console.WriteLine("\nTry to register 'Person.FullName' again:");
try
{
    KeyRegistry.Register<string>("Person.FullName");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"  Error (expected): {ex.Message}");
}

Console.WriteLine("\nTry to register 'Person.FullName' with different type:");
try
{
    KeyRegistry.Register<int>("Person.FullName");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"  Error (expected): {ex.Message}");
}