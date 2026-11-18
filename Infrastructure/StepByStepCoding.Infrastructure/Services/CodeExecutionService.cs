using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using StepByStepCoding.Application.Contracts.Execution;
using StepByStepCoding.Application.Exceptions;
using StepByStepCoding.Application.Models.Execution;
using StepByStepCoding.Domain.Enums;
using StepByStepCoding.Infrastructure.Options;

namespace StepByStepCoding.Infrastructure.Services
{
    public sealed class CodeExecutionService : ICodeExecutionService
    {
        private readonly HttpClient client;
        private readonly CodeExecutionOptions options;

        public CodeExecutionService(HttpClient client, IOptions<CodeExecutionOptions> options)
        {
            this.client = client;
            this.options = options.Value;
        }

        public async Task<CodeExecutionResultDTO> Execute(ProgrammingLanguage language, string fileName, string code,
            string stdin, CancellationToken cancellationToken = default)
        {
            var (runtime, version, extension, defaultName) = language switch
            {
                ProgrammingLanguage.Cpp => ("c++", options.CppVersion, ".cpp", "main.cpp"),
                ProgrammingLanguage.Java => ("java", options.JavaVersion, ".java", "Main.java"),
                ProgrammingLanguage.Python => ("python", options.PythonVersion, ".py", "main.py"),
                _ => throw new CustomValidationException("Unsupported programming language.")
            };
            var name = Path.GetFileName(fileName.Replace('\\', '/'));
            if (!name.EndsWith(extension, StringComparison.Ordinal)) name = defaultName;

            var payload = new
            {
                language = runtime,
                version,
                files = new[] { new { name, content = code } },
                stdin,
                args = Array.Empty<string>(),
                compile_timeout = options.CompileTimeoutMilliseconds,
                run_timeout = options.RunTimeoutMilliseconds,
                compile_cpu_time = options.CompileTimeoutMilliseconds,
                run_cpu_time = options.RunTimeoutMilliseconds,
                compile_memory_limit = options.CompileMemoryLimitBytes,
                run_memory_limit = language == ProgrammingLanguage.Java ? options.JavaRunMemoryLimitBytes : options.RunMemoryLimitBytes
            };

            try
            {
                using var response = await client.PostAsJsonAsync("api/v2/execute", payload, cancellationToken);
                if (!response.IsSuccessStatusCode)
                    throw new CodeExecutionUnavailableException("Code execution is unavailable. Make sure the language runtimes are installed.");

                var result = await response.Content.ReadFromJsonAsync<PistonResult>(cancellationToken);
                if (result is null || result.Compile is null && result.Run is null)
                    throw new CodeExecutionUnavailableException("The execution service returned an invalid result.");

                return new CodeExecutionResultDTO
                {
                    Language = runtime,
                    Version = version,
                    Compile = Map(result.Compile),
                    Run = Map(result.Run)
                };
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                throw new CodeExecutionUnavailableException("The execution service did not respond in time. Please try again.");
            }
            catch (Exception exception) when (exception is HttpRequestException or JsonException)
            {
                throw new CodeExecutionUnavailableException("Could not reach the execution service. Please try again.");
            }
        }

        private static ExecutionStageDTO? Map(PistonStage? stage) => stage is null ? null : new()
        {
            Stdout = Limit(stage.Stdout),
            Stderr = Limit(stage.Stderr),
            ExitCode = stage.Code,
            Signal = stage.Signal,
            Status = stage.Status,
            Message = stage.Message,
            WallTimeMilliseconds = stage.WallTime
        };

        private static string Limit(string? text) => text is null ? "" : text[..Math.Min(text.Length, 16384)];

        private sealed record PistonResult(PistonStage? Compile, PistonStage? Run);

        private sealed record PistonStage(string? Stdout, string? Stderr, int? Code, string? Signal, string? Status,
            string? Message, [property: JsonPropertyName("wall_time")] double? WallTime);
    }
}
