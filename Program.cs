using Catalog.Accessor;
using Catalog.Engine;

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