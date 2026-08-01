using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Serilog;

namespace Nexus.Shared.Observability;

/// <summary>
/// Service Registration para o módulo de Observabilidade.
/// Configura OpenTelemetry (tracing + metrics) e Serilog (logs estruturados).
/// Cada serviço deve chamar AddNexusObservability("nome-do-servico") no Program.cs.
/// </summary>
public static class ServiceRegistration
{
    /// <summary>
    /// Adiciona tracing (OpenTelemetry), métricas (Prometheus) e logs (Serilog + Seq).
    /// O serviceName é usado para identificar o serviço nos traces e logs.
    /// </summary>
    public static IServiceCollection AddNexusObservability(
        this IServiceCollection services, string serviceName)
    {
        // ============================================================
        // OpenTelemetry: Tracing distribuído + Métricas
        // ============================================================
        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName)           // Identifica o serviço nos traces
                .AddEnvironmentVariableDetector()) // Lê OTEL_RESOURCE_ATTRIBUTES
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()    // Auto-instrumentação de requests HTTP
                .AddHttpClientInstrumentation()    // Auto-instrumentação de chamadas HTTP
                .AddOtlpExporter())                // Exporta para OpenTelemetry Collector
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()    // Métricas HTTP (requests, duração)
                .AddRuntimeInstrumentation()       // Métricas .NET (GC, threads, memória)
                .AddOtlpExporter());               // Exporta para Prometheus via Collector

        // ============================================================
        // Serilog: Logs estruturados com enriquecimento
        // ============================================================
        Log.Logger = new LoggerConfiguration()
            .Enrich.WithProperty("Service", serviceName) // Marca cada log com o serviço
            .WriteTo.Console()                            // Log no terminal (desenvolvimento)
            .WriteTo.Seq("http://localhost:5341")          // Log centralizado no Seq
            .CreateLogger();

        // Substitui o provedor de logging padrão pelo Serilog
        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddSerilog();
        });

        return services;
    }
}
