namespace lab2_pipeline
{
    public static class Functions
    {
        internal static void AgeCheck_Adapter<TContext>(TContext context, int minAge) where TContext : IRelationshipContext
        {
            if (context is IRelationshipPairContext pair)
            {
                AgeCheckPair(pair, minAge);
                return;
            }
            if (context is IRelationshipSingleContext single)
            {
                AgeCheckSingle(single, minAge);
                return;
            }
        }
        internal static void StartRelationship_Adapter<TContext>(TContext context) where TContext : IRelationshipContext
        {
            if (context is IRelationshipPairContext pair)
            {
                StartRelationPair(pair);
                return;
            }
            if (context is IRelationshipSingleContext single)
            {
                StartRelationSingle(single);
                return;
            }
        }
        internal static void Marry_Adapter<TContext>(TContext context) where TContext : IRelationshipContext
        {
            if (context is IRelationshipPairContext pair)
            {
                MarryPair(pair);
                return;
            }
            if (context is IRelationshipSingleContext single)
            {
                MarrySingle(single);
                return;
            }
        }

        internal static void HaveChild_Adapter<TContext>(TContext context, string childName, int childAge) where TContext : IRelationshipContext
        {
            if (context is IRelationshipPairContext pair)
            {
                HaveChildPair(pair, childName, childAge);
                return;
            }
            if (context is IRelationshipSingleContext single)
            {
                HaveChildSingle(single, childName, childAge);
                return;
            }
        }
        internal static void Breakup_Adapter<TContext>(TContext context) where TContext : IRelationshipContext
        {
            if (context is IRelationshipPairContext pair)
            {
                BreakupPair(pair);
                return;
            }
            if (context is IRelationshipSingleContext single)
            {
                BreakupSingle(single);
                return;
            }
        }

        internal static void Divorce_Adapter<TContext>(TContext context) where TContext : IRelationshipContext
        {
            if (context is IRelationshipPairContext pair)
            {
                DivorcePair(pair);
                return;
            }
            if (context is IRelationshipSingleContext single)
            {
                DivorceSingle(single);
                return;
            }
        }
        internal static void Print_Adapter<TContext>(TContext context) where TContext : IRelationshipContext
        {
            if (context is IRelationshipPairContext pair)
            {
                PrintPair(pair);
                return;
            }
            if (context is IRelationshipSingleContext single)
            {
                PrintSingle(single);
                return;
            }
        }
        internal static void Celebrate()
        {
            Console.WriteLine(@"
.-._.-._.-._.-._.-._.-._.-._.-.
-._."".--.._.-._.-._""_._.-._.-._
  ( (  ° )  .--.""  (°'.___   (
 * ) '--' *(  ° )   '.____) * )
  (     (   '--'   *     _   (
   )     )   __ _.      /,'   )
* (     (  .'  ' '-   ° °    (
   )     )-  /.;, ) °  °  *   )
  (     * . ( ^ ('  > POP! < (
   )       /)\_v/   /'.°      )
  (       .-|\/|-../""/.      (
   )     /_/    /'. °\_)      *
  (     (___'. /# / //
   )       \\_;'./ //
  *        /--'.___/
          ( _/ ' '.
      °    \  \-.  '. <- me uncorking
            \  \ '.  \   a  bottle of
             \  )  )  )  *CHAMPAGNE*!
           mrf) | /  |
             /  |(___|
         °  |   | \ -\
      °     \.--:_ \__)
    ___,_____\____) _____._______

");
        }

        private static void PrintPair(IRelationshipPairContext context)
        {
            Console.WriteLine($"  --- Info ---");
            PersonFieldMasksFunc.Print(context.PersonA);
            PersonFieldMasksFunc.Print(context.PersonB);
            static string rel(PersonExt p)
                => p.InRelationWith != null ? $"In relationship with {p.InRelationWith.FullName}" : "nobody";

            Console.WriteLine($"  --- Status ---");
            Console.WriteLine($"  {context.PersonA.FullName}: {context.PersonA.Status}, partner: {rel(context.PersonA)}, children: {context.PersonA.Children.Count}, exes: {context.PersonA.PreviousRelations.Count}");
            Console.WriteLine($"  {context.PersonB.FullName}: {context.PersonB.Status}, partner: {rel(context.PersonB)}, children: {context.PersonB.Children.Count}, exes: {context.PersonB.PreviousRelations.Count}");
        }

        private static void PrintSingle(IRelationshipSingleContext context)
        {
            Console.WriteLine($"  --- Info ---");
            PersonFieldMasksFunc.Print(context.PersonA);
        }

        private static void DivorcePair(IRelationshipPairContext context)
        {
            var a = context.PersonA;
            var b = context.PersonB;

            if (a.InRelationWith != null) a.PreviousRelations.Add(b);
            if (b.InRelationWith != null) b.PreviousRelations.Add(a);

            a.InRelationWith = null;
            b.InRelationWith = null;
            a.Status = SocialStatus.Divorced;
            b.Status = SocialStatus.Divorced;

            Console.WriteLine($"  {a.FullName} and {b.FullName} are divorced");
        }

        private static void DivorceSingle(IRelationshipSingleContext context)
        {
            var a = context.PersonA;
            a.Status = SocialStatus.Divorced;

            Console.WriteLine($"  {a.FullName} is divorced");
        }

        private static void BreakupSingle(IRelationshipSingleContext context)
        {
            var a = context.PersonA;

            a.Status = SocialStatus.Single;

            Console.WriteLine($"  {a.FullName} broke up");
        }

        private static void BreakupPair(IRelationshipPairContext context)
        {
            var a = context.PersonA;
            var b = context.PersonB;

            if (a.InRelationWith != null) a.PreviousRelations.Add(b);
            if (b.InRelationWith != null) b.PreviousRelations.Add(a);

            a.InRelationWith = null;
            b.InRelationWith = null;
            a.Status = SocialStatus.Single;
            b.Status = SocialStatus.Single;

            Console.WriteLine($"  {a.FullName} and {b.FullName} broke up");
        }

        private static void HaveChildSingle(IRelationshipSingleContext context, string childName, int childAge)
        {
            context.PersonA.Status = SocialStatus.WithChildren;

            Console.WriteLine($"  {context.PersonA.FullName} had a child: {childName}");
        }

        private static void HaveChildPair(IRelationshipPairContext context, string childName, int childAge)
        {
            var child = new PersonExt
            {
                Id = Guid.NewGuid(),
                FullName = childName,
                Age = childAge,
                Height = 50.0,
                Status = SocialStatus.Underage,
            };

            context.PersonA.Children.Add(child);
            context.PersonB.Children.Add(child);
            context.PersonA.Status = SocialStatus.WithChildren;
            context.PersonB.Status = SocialStatus.WithChildren;

            Console.WriteLine($"  {context.PersonA.FullName} and {context.PersonB.FullName} had a child: {childName}");
        }

        private static void MarryPair(IRelationshipPairContext context)
        {
            var a = context.PersonA;
            var b = context.PersonB;
            if (a.InRelationWith != b)
            {
                Console.WriteLine("  [Marry] Step canceled: not in valid relationship");
                return;
            }

            if (a.Status != SocialStatus.InRelationship || b.Status != SocialStatus.InRelationship)
            {
                Console.WriteLine("  [Marry] Step canceled: invalid status");
                return;
            }

            a.Status = SocialStatus.Married;
            b.Status = SocialStatus.Married;

            Console.WriteLine($"  {a.FullName} and {b.FullName} are married now");
        }

        private static void MarrySingle(IRelationshipSingleContext context)
        {
            var a = context.PersonA;
            if (a.Status == SocialStatus.Married)
            {
                Console.WriteLine($"  [Marry] {a.FullName} is already {a.Status}. Step Canceled.");
                return;
            }

            a.Status = SocialStatus.Married;

            Console.WriteLine($"  {a.FullName} is married now");
        }

        private static void AgeCheckPair(IRelationshipPairContext context, int minAge)
        {
            if (context.PersonA.Age < minAge || context.PersonB.Age < minAge)
            {
                Console.WriteLine($"[AgeCheck] Step skipped. Person is younger than {minAge} years");
                return;
            }
        }
        private static void AgeCheckSingle(IRelationshipSingleContext context, int minAge)
        {

            if (context.PersonA.Age < minAge)
            {
                Console.WriteLine($"[AgeCheck] Step skipped. {context.PersonA.FullName} is younger than {minAge} years");
                return;
            }
        }



        private static void StartRelationSingle(IRelationshipSingleContext context)
        {
            var a = context.PersonA;

            if (a.Status == SocialStatus.InRelationship || a.Status == SocialStatus.Married)
            {
                Console.WriteLine($"  [StartRelationship] {a.FullName} is already {a.Status.ToString()}. Step Canceled.");
                return;
            }

            a.Status = SocialStatus.InRelationship;

            Console.WriteLine($"  {a.FullName} started to date");
        }

        private static void StartRelationPair(IRelationshipPairContext context)
        {
            var a = context.PersonA;
            var b = context.PersonB;

            if (a.InRelationWith != null || b.InRelationWith != null)
            {
                Console.WriteLine($"  [StartRelationship] Someone is already in a relationship. Step Canceled.");
                return;
            }

            a.InRelationWith = b;
            b.InRelationWith = a;
            a.Status = SocialStatus.InRelationship;
            b.Status = SocialStatus.InRelationship;

            Console.WriteLine($"  {a.FullName} and {b.FullName} started to date");
        }

    }
}
