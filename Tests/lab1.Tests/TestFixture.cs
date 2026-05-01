namespace lab1_fieldMask.lab1.Tests
{
    public class PersonTestFixture
    {
        public Person P1 { get; }
        public Person P2 { get; }
        public Person P3 { get; }

        public PersonTestFixture()
        {
            P1 = new Person { Id = Guid.NewGuid(), Age = 21, FullName = "test1", Height = 161.5, Status = SocialStatus.Single };
            P2 = new Person { Id = Guid.NewGuid(), Age = 22, FullName = "test2", Height = 162.5, Status = SocialStatus.Divorced };
            P3 = new Person { Id = Guid.NewGuid(), Age = 23, FullName = "test3", Height = 163.5, Status = SocialStatus.Divorced };
        }
    }
    public static class ConsoleCapture
    {
        public static string Run(Action action)
        {
            var sw = new StringWriter();
            Console.SetOut(sw);
            action();
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
            return sw.ToString();
        }
    }
}
