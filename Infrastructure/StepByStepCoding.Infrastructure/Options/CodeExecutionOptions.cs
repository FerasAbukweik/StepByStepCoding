namespace StepByStepCoding.Infrastructure.Options
{
    public sealed class CodeExecutionOptions
    {
        public const string SectionName = "CodeExecution";

        public string BaseUrl { get; set; } = "http://piston:2000/";

        public string CppVersion { get; set; } = "10.2.0";

        public string PythonVersion { get; set; } = "3.10.0";

        public string JavaVersion { get; set; } = "15.0.2";

        public int CompileTimeoutMilliseconds { get; set; } = 10000;

        public int RunTimeoutMilliseconds { get; set; } = 3000;

        public long CompileMemoryLimitBytes { get; set; } = 536870912;

        public long RunMemoryLimitBytes { get; set; } = 268435456;

        public long JavaRunMemoryLimitBytes { get; set; } = 536870912;

        public int HttpTimeoutSeconds { get; set; } = 25;
    }
}
