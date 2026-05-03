using static lab3_builder.Models;

namespace lab3_builder
{

    public static class Validator
    {
        public static ValidationRule<T> For<T>(
            T value,
            List<ValidationError> errors,
            string file,
            int line)
        {
            return new ValidationRule<T>(value, errors, file, line);
        }
    }
    public class ValidationRule<T>
    {
        private readonly T? _value;
        private readonly List<ValidationError> _errors;
        private readonly string _file;
        private readonly int _line;

        public ValidationRule(T? value, List<ValidationError> errors, string file, int line)
        {
            _value = value;
            _errors = errors;
            _file = file;
            _line = line;
        }

        public ValidationRule<T> NotEmpty(string message)
        {
            if (_value == null || (_value is string s && string.IsNullOrWhiteSpace(s)))
            {
                _errors.Add(new ValidationError(message, _file, _line));
            }

            return this;
        }

        public ValidationRule<T> InRange(double min, double max, string message)
        {
            if (_value is double d && (d < min || d > max))
                _errors.Add(new(message, _file, _line));

            return this;
        }
        public ValidationRule<T> Must(Func<T, bool> predicate, string message)
        {
            if (_value == null || !predicate(_value))
                _errors.Add(new ValidationError(message, _file, _line));

            return this;
        }
    }
    public sealed class ValidationError
    {
        public string Message { get; set; }
        public string CallerFile { get; }
        public int CallerLine { get; }

        public ValidationError(string message, string callerFile = "", int callerLine = 0)
        {
            Message = message;
            CallerFile = callerFile;
            CallerLine = callerLine;
        }

        public override string ToString() =>
            string.IsNullOrEmpty(CallerFile)
                ? $"  {Message}"
                : $"  [{Path.GetFileName(CallerFile)}:{CallerLine}] {Message}";
    }

    public sealed class ValidationException : Exception
    {
        public IReadOnlyList<ValidationError> Errors { get; }

        public ValidationException(List<ValidationError> errors)
            : base("Validation failed with " + errors.Count + " error(s):\n" + string.Join("\n", errors))
        {
            Errors = errors;
        }
    }
}