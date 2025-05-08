using Catalog.Accessor;
using Catalog.Engine;
using Bookstore.Checkout.Accessors;
using Bookstore.Checkout.Core.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();

// Register CatalogAccessor with the connection string
builder.Services.AddScoped<CatalogAccessor>(provider =>
{
    var config = provider.GetRequiredService<IConfiguration>();
    string connectionString = config.GetConnectionString("DefaultConnection");
    return new CatalogAccessor(connectionString);
});

// Register CatalogEngine
builder.Services.AddScoped<CatalogEngine>();

// for React frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy.WithOrigins("http://localhost:3001")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Add Checkout services (NEW)
builder.Services.AddScoped<IOrderAccessor, OrderAccessor>(provider => 
{
    var config = provider.GetRequiredService<IConfiguration>();
    return new OrderAccessor(config, provider.GetService<ILogger<OrderAccessor>>());
});

builder.Services.AddScoped<IShippingAccessor, ShippingAccessor>();
builder.Services.AddScoped<ICheckoutService, CheckoutService>();
builder.Services.AddScoped<ICheckoutValidatorService, CheckoutValidatorService>();

// Add HttpClient for shipping calculations
builder.Services.AddHttpClient("ShippingAPI", client => 
{
    client.BaseAddress = new Uri("https://api.shipping-provider.com");
    client.Timeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseHttpsRedirection();
app.UseCors("AllowReact");
app.UseAuthorization();
app.MapControllers();
app.Run();
