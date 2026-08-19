using Resulz;

namespace PostForge.Application.Common.Exceptions;

public class DomainValidationException : Exception
{
    public IDictionary<string, string[]> Errors { get; }

    public DomainValidationException(IEnumerable<ErrorMessage> errors)
        : base("One or more domain validation failures have occurred.")
    {
        Errors = errors
            .GroupBy(e => e.Context, e => e.Description)
            .ToDictionary(g => g.Key, g => g.ToArray());
    }
}