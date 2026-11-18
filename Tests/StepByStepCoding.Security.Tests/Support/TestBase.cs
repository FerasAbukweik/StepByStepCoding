using AutoFixture;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit.Abstractions;

namespace StepByStepCoding.Security.Tests.Support
{
    public abstract class TestBase
    {
        private readonly ITestOutputHelper output;

        private static readonly JsonSerializerOptions serializerOptions = new()
        {
            ReferenceHandler = ReferenceHandler.IgnoreCycles,
            MaxDepth = 16
        };

        protected IFixture Fixture { get; } = new Fixture();

        protected TestBase(ITestOutputHelper output)
        {
            this.output = output;

            // Omit circular navigation properties when AutoFixture generates entity graphs.
            foreach (var behavior in Fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToArray())
                Fixture.Behaviors.Remove(behavior);

            Fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }

        protected void WriteOutput(string phase, params (string Name, object? Value)[] values)
        {
            output.WriteLine(phase);

            foreach (var (name, value) in values)
            {
                var printable = value is Exception exception
                    ? new { Type = exception.GetType().Name, exception.Message }
                    : value;

                output.WriteLine($"{name}: {JsonSerializer.Serialize(printable, serializerOptions)}");
            }
        }
    }
}
