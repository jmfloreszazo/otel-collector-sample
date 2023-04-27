using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddOpenTelemetry();


builder.Services.AddOpenTelemetry()
    .WithTracing(builder => builder
        .AddAspNetCoreInstrumentation()
        .AddConsoleExporter()
        .AddOtlpExporter(opts => { opts.Endpoint = new Uri("http://otelcollector:4317"); }))
    .WithMetrics(builder => builder
        .AddAspNetCoreInstrumentation()
        .AddConsoleExporter()
        .AddPrometheusExporter()
        .AddOtlpExporter((exporterOptions, readerOptions) =>
        {
            exporterOptions.Endpoint = new Uri("http://otelcollector:4317");
            readerOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 5000;
        })
        .AddView("processor.lag", new ExplicitBucketHistogramConfiguration()
        {
            Boundaries = new double[] { 0, 500, 1000, 2500, 5000, 7500, 10000, 25000, 50000 },
            RecordMinMax = false
        })
        .ConfigureResource(resource => resource.AddService("otellocollectorsample")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseOpenTelemetryPrometheusScrapingEndpoint();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
