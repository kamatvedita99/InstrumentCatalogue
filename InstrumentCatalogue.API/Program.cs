using HealthChecks.UI.Client;
using InstrumentCatalogue.API.Middleware;
using InstrumentCatalogue.Application.Extensions;
using InstrumentCatalogue.Infrastructure.Extensions;
using InstrumentCatalogue.Infrastructure.Persistence;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Polly;
using Serilog;
using Serilog.Events;
using Serilog.Exceptions;
using Serilog.Sinks.Grafana.Loki;

Serilog.Debugging.SelfLog.Enable(msg => Console.WriteLine($"SERILOG: {msg}"));
Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateBootstrapLogger();

try
{
    AppContext.SetSwitch("Npgsql.DisableDateTimeInfinityConversions", true);

    var builder = WebApplication.CreateBuilder(args);

    
    

    builder.Host.UseSerilog((context, services, configuration) => {
        configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithMachineName()
            .Enrich.WithProcessId()
            .Enrich.WithThreadId()
            .Enrich.WithCorrelationId()
            .Enrich.WithClientIp()
            .Enrich.WithExceptionDetails();
            
            if(context.HostingEnvironment.IsProduction())
            {
                var lokiLogin = Environment.GetEnvironmentVariable("LOKI_LOGIN");
                var lokiPassword = Environment.GetEnvironmentVariable("LOKI_PASSWORD");

                if (!string.IsNullOrEmpty(lokiLogin) && !string.IsNullOrEmpty(lokiPassword))
                {
                    configuration.WriteTo.GrafanaLoki(
                        "https://logs-prod-028.grafana.net",
                        credentials: new LokiCredentials
                        {
                            Login = lokiLogin,
                            Password = lokiPassword
                        },
                        labels: new[]
                        {
                    new LokiLabel { Key = "app", Value = "instrument-catalogue" },
                    new LokiLabel { Key = "env", Value = "production" }
                        }
                    );
                }
            }
    });
        
        

    // Add services to the container.
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
            options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter(allowIntegerValues: false)));

    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddHttpContextAccessor();
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);


    var app = builder.Build();

    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                          ForwardedHeaders.XForwardedProto
    });

    app.UseHttpsRedirection();

    app.UseSerilogRequestLogging(options =>
    {
        options.GetLevel = (httpContext, elapsed, ex) =>
            httpContext.Response.StatusCode >= 500
                ? LogEventLevel.Error
                : httpContext.Response.StatusCode >= 400
                    ? LogEventLevel.Warning
                    : LogEventLevel.Information;
    });

    app.UseMiddleware<GlobalExceptionMiddleware>();

    // Configure the HTTP request pipeline.
  
    app.UseSwagger();
    app.UseSwaggerUI();

   
   
  

    app.UseAuthorization();

    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        Predicate = _ => false
    });

    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    app.MapControllers();

    //Run migrations
    try
    {
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CatalogueDbContext>();
            await db.Database.MigrateAsync();
            Log.Information("Migration applied successfully");
        }
    }

    catch(Exception ex)
    {
        Log.Fatal(ex, "Database migration failed. Application cannot start.");
        throw;
    }
    

    app.Run();
}
catch(Exception e)
{
    Log.Fatal(e, "Application failed to start");
    throw;
}

finally
{
    Log.CloseAndFlush();
}
