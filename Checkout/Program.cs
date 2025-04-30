using Bookstore.Checkout.Data;
using Bookstore.Checkout.Infrastructure.Filters;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to container
builder.Services.AddControllers(options => 
{
    options.Filters.Add<ApiExceptionFilter>();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "Bookstore Checkout API", Version = "v1" });
});

// Configure database
builder.Services.AddDbContext<CheckoutDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("CheckoutDB")));

// Register application services
builder.Services.AddScoped<ICheckoutService, CheckoutService>();
builder.Services.AddScoped<IPaymentValidator, PaymentValidator>();
builder.Services.AddScoped<IShippingCalculator, ShippingCalculator>();
builder.Services.AddScoped<IOrderAccessor, OrderAccessor>();

// Configure HttpClient for shipping calculator
builder.Services.AddHttpClient("ShippingAPI", client =>
{
    client.BaseAddress = new Uri("https://api.shipping-provider.com");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(builder.Configuration["AllowedOrigins"].Split(';'))
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure middleware pipeline
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

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthorization();
app.MapControllers();

app.Run();
