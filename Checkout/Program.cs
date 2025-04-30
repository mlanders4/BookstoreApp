using Bookstore.Checkout.Data;
using Bookstore.Checkout.Infrastructure.Configurations;
using Bookstore.Checkout.Infrastructure.Logging;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ======================
// 1. Configuration Setup
// ======================
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

// ======================
// 2. Service Registration
// ======================

// A. Database
DatabaseConfig.ConfigureDbContext(builder.Services, builder.Configuration);

// B. Core Services
builder.Services.AddScoped<ICheckoutService, CheckoutService>();
builder.Services.AddScoped<IOrderAccessor, OrderAccessor>();
builder.Services.AddScoped<IPaymentValidator, PaymentValidator>();
builder.Services.AddScoped<IShippingCalculator, ShippingCalculator>();

// C. API Support
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Bookstore Checkout API", Version = "v1" });
});

// D. Frontend Services
builder.Services.AddSpaStaticFiles(configuration =>
{
    configuration.RootPath = "Frontend/build";
});

// E. Cross-cutting Concerns
builder.Services.AddTransient<RequestLogger>();

// ======================
// 3. App Building
// ======================
var app = builder.Build();

// ======================
// 4. Middleware Pipeline
// ======================

// A. Development Tools
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseDeveloperExceptionPage();
    
    // Apply migrations in development
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<CheckoutDbContext>();
    dbContext.Database.Migrate();
}

// B. Security & Routing
app.UseHttpsRedirection();
app.UseCors(policy => policy
    .WithOrigins("http://localhost:3000") // React dev server
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseRouting();

// C. Custom Middleware
app.UseMiddleware<RequestLogger>();

// D. API Endpoints
app.MapControllers();

// E. Frontend Handling
app.UseSpaStaticFiles();
app.UseSpa(spa =>
{
    spa.Options.SourcePath = "Frontend";
    
    if (app.Environment.IsDevelopment())
    {
        spa.UseProxyToSpaDevelopmentServer("http://localhost:3000");
    }
});

// ======================
// 5. Startup
// ======================
app.Run();
