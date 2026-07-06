using InvoiceToCash.Data;
using InvoiceToCash.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Dependency Injection: register layered services ---
builder.Services.AddSingleton<IInvoiceRepository, InMemoryInvoiceRepository>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddScoped<ICommissionService, CommissionService>();
builder.Services.AddScoped<IAgingService, AgingService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Allow the React dev server to call the API during development.
const string DevCors = "DevCors";
builder.Services.AddCors(options =>
{
    options.AddPolicy(DevCors, policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(DevCors);
app.MapControllers();

app.Run();

// Exposed for integration testing (WebApplicationFactory).
public partial class Program { }
