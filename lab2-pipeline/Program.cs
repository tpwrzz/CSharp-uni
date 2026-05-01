using lab2_pipeline;
using System.Text;

var alice = new PersonExt
{
    Id = Guid.NewGuid(),
    FullName = "Alice",
    Age = 16,
    Height = 165.0,
    Status = SocialStatus.Single,
};
var bob = new PersonExt
{
    Id = Guid.NewGuid(),
    FullName = "Bob",
    Age = 27,
    Height = 180.0,
    Status = SocialStatus.Single,
};

var context = new RelationshipPairContext { PersonA = alice, PersonB = bob };

var pipeline = new Pipeline<RelationshipPairContext>();
pipeline.Steps.Add(Step.Log(Step.AgeCheck(new StartRelationshipStep<RelationshipPairContext>())));
pipeline.Steps.Add(PrintStatusStep<RelationshipPairContext>.Instance);
pipeline.Steps.Add(Step.Log(new MarryStep<RelationshipPairContext>()));
pipeline.Steps.Add(PrintStatusStep<RelationshipPairContext>.Instance);
pipeline.Steps.Add(new HaveChildStep<RelationshipPairContext> { ChildName = "Charlie", ChildAge = 0 });
pipeline.Steps.Add(PrintStatusStep<RelationshipPairContext>.Instance);
pipeline.Steps.Add(Step.Log(Step.Celebrate(new DivorceStep<RelationshipPairContext>())));
pipeline.Steps.Add(PrintStatusStep<RelationshipPairContext>.Instance);

Console.WriteLine("\n--- Basic Pipeline & Decorators---\n");
pipeline.PrintAllSteps();
pipeline.Execute(context);

//  Pipeline with Responsibility Chain & Diff Context 
var dave = new PersonExt
{
    Id = Guid.NewGuid(),
    FullName = "Dave",
    Age = 24,
    Height = 175.0,
    Status = SocialStatus.Single,
};

var context2 = new RelationshipSingleContext { PersonA = dave };

var stopStep = Step.Log(Step.Celebrate(new StopAfterBreakupStep<RelationshipSingleContext>()));

var pipeline2 = new Pipeline<RelationshipSingleContext>();
pipeline2.Steps.Add(Step.Log(Step.Celebrate(new StartRelationshipStep<RelationshipSingleContext>())));
pipeline2.Steps.Add(PrintStatusStep<RelationshipSingleContext>.Instance);
pipeline2.Steps.Add(stopStep);
pipeline2.Steps.Add(new MarryStep<RelationshipSingleContext>());
pipeline2.Steps.Add(PrintStatusStep<RelationshipSingleContext>.Instance);

Console.WriteLine("\n--- Pipeline with Responsibility Chain & Diff Context ---\n");
pipeline2.PrintAllSteps();
pipeline2.Execute(context2);

#region Context
public interface IRelationshipContext
{
    public bool IsDone { get; set; }
}
public interface IRelationshipPairContext : IRelationshipContext
{
    public PersonExt PersonA { get; set; }
    public PersonExt PersonB { get; set; }
}

public interface IRelationshipSingleContext : IRelationshipContext
{
    public Person PersonA { get; set; }
}

public class RelationshipSingleContext : IRelationshipSingleContext
{
    public required Person PersonA { get; set; }
    public bool IsDone { get; set; }
}

public class RelationshipPairContext : IRelationshipPairContext
{
    public required PersonExt PersonA { get; set; }
    public required PersonExt PersonB { get; set; }
    public bool IsDone { get; set; }
}
#endregion
#region Pipeline
public interface IPipelineStep<TContext> where TContext : IRelationshipContext
{
    public void Execute(TContext context);
    public void Introspect(StringBuilder sb, int indent = 0);
}
public static class Step
{
    public static IPipelineStep<T> Log<T>(IPipelineStep<T> step) where T : IRelationshipContext
        => new LogWrapper<T>(step);

    public static IPipelineStep<T> AgeCheck<T>(IPipelineStep<T> step) where T : IRelationshipContext
        => new AgeCheckDecorator<T>(step);
    public static IPipelineStep<T> Celebrate<T>(IPipelineStep<T> step) where T : IRelationshipContext
       => new CelebratorDecorator<T>(step);
}
public class Pipeline<TContext> where TContext : IRelationshipContext
{
    public List<IPipelineStep<TContext>> Steps { get; } = new();

    public void Execute(TContext context)
    {
        foreach (var step in Steps)
        {
            if (context is TContext rc && rc.IsDone)
            {
                Console.WriteLine("[Pipeline] Execution stopped (IsDone = true)");
                break;
            }
            step.Execute(context);
        }
    }
    public void PrintAllSteps()
    {
        var sb = new StringBuilder();
        sb.AppendLine("--- Pipeline Steps ---");
        foreach (var step in Steps)
        {
            step.Introspect(sb, indent: 2);
        }
        Console.Write(sb.ToString());
    }
}
#endregion

#region logger / decorators
public class LogWrapper<TContext> : IPipelineStep<TContext> where TContext : IRelationshipContext
{
    private readonly IPipelineStep<TContext> _step;

    public LogWrapper(IPipelineStep<TContext> step)
    {
        _step = step;
    }

    public void Execute(TContext context)
    {
        var name = _step.GetType().Name;
        var sb = new StringBuilder();
        sb.AppendLine(name);
        _step.Introspect(sb);
        _step.Execute(context);
    }

    public void Introspect(StringBuilder sb, int indent = 0)
    {
        sb.AppendLine($"{new string(' ', indent)}Log Wrapper (");
        _step.Introspect(sb, indent + 2);
        sb.AppendLine($"{new string(' ', indent)})");
    }
}

public class AgeCheckDecorator<TContext> : IPipelineStep<TContext> where TContext : IRelationshipContext
{
    private readonly IPipelineStep<TContext> _step;
    private readonly int _minAge;

    public AgeCheckDecorator(IPipelineStep<TContext> step, int minAge = 18)
    {
        _step = step;
        _minAge = minAge;
    }

    public void Execute(TContext context)
    {
        Functions.AgeCheck_Adapter(context, _minAge);
        _step.Execute(context);
    }

    public void Introspect(StringBuilder sb, int indent = 0)
    {
        sb.AppendLine($"{new string(' ', indent)}AgeCheckDecorator (min={_minAge}) (");
        _step.Introspect(sb, indent + 2);
        sb.AppendLine($"{new string(' ', indent)})");
    }
}

public class CelebratorDecorator<TContext> : IPipelineStep<TContext> where TContext : IRelationshipContext
{
    private readonly IPipelineStep<TContext> _step;

    public CelebratorDecorator(IPipelineStep<TContext> step)
    {
        _step = step;
    }

    public void Execute(TContext context)
    {
        _step.Execute(context);
        Functions.Celebrate();
    }

    public void Introspect(StringBuilder sb, int indent = 0)
    {
        sb.AppendLine($"{new string(' ', indent)}CelebratorDecorator (");
        _step.Introspect(sb, indent + 2);
        sb.AppendLine($"{new string(' ', indent)})");
    }
}
#endregion

#region pipelineSteps
public class StartRelationshipStep<TContext> : IPipelineStep<TContext> where TContext : IRelationshipContext
{
    public void Execute(TContext context)
    {
        Functions.StartRelationship_Adapter(context);
    }

    public void Introspect(StringBuilder sb, int indent = 0)
        => sb.AppendLine($"{new string(' ', indent)}StartRelationshipStep");
}

public class MarryStep<TContext> : IPipelineStep<TContext> where TContext : IRelationshipContext
{
    public void Execute(TContext context)
    {
        Functions.Marry_Adapter(context);
    }

    public void Introspect(StringBuilder sb, int indent = 0)
        => sb.AppendLine($"{new string(' ', indent)}MarryStep");
}

public class HaveChildStep<TContext> : IPipelineStep<TContext> where TContext : IRelationshipContext
{
    public required string ChildName { get; set; }
    public required int ChildAge { get; set; }

    public void Execute(TContext context)
    {
        Functions.HaveChild_Adapter(context, ChildName, ChildAge);
    }

    public void Introspect(StringBuilder sb, int indent = 0)
        => sb.AppendLine($"{new string(' ', indent)}HaveChildStep (ChildName={ChildName})");
}

public class BreakupStep<TContext> : IPipelineStep<TContext> where TContext : IRelationshipContext
{
    public void Execute(TContext context)
    {
        Functions.Breakup_Adapter(context);
    }

    public void Introspect(StringBuilder sb, int indent = 0)
        => sb.AppendLine($"{new string(' ', indent)}BreakupStep");
}

public class DivorceStep<TContext> : IPipelineStep<TContext> where TContext : IRelationshipContext
{
    public void Execute(TContext context)
    {
        Functions.Divorce_Adapter(context);
    }

    public void Introspect(StringBuilder sb, int indent = 0)
        => sb.AppendLine($"{new string(' ', indent)}DivorceStep");
}

public class PrintStatusStep<TContext> : IPipelineStep<TContext> where TContext : IRelationshipContext
{
    private static readonly PrintStatusStep<TContext> _instance = new();
    public static PrintStatusStep<TContext> Instance => _instance;
    private PrintStatusStep() { }

    public void Execute(TContext context)
    {
        Functions.Print_Adapter(context);
    }

    public void Introspect(StringBuilder sb, int indent = 0)
        => sb.AppendLine($"{new string(' ', indent)}PrintStatusStep [Singleton]");
}
public class StopAfterBreakupStep<TContext> : IPipelineStep<TContext> where TContext : IRelationshipContext
{
    public void Execute(TContext context)
    {
        new BreakupStep<TContext>().Execute(context);
        context.IsDone = true;
        Console.WriteLine("  [StopAfterBreakup] Next steps stopped. (IsDone=true)");
    }

    public void Introspect(StringBuilder sb, int indent = 0)
        => sb.AppendLine($"{new string(' ', indent)}StopAfterBreakupStep (sets IsDone=true)");
}
#endregion

public class PersonExt : Person
{
    public PersonExt? InRelationWith { get; set; } = null;
    public List<PersonExt> PreviousRelations = new List<PersonExt>();
    public List<PersonExt> Children = new List<PersonExt>();
}
