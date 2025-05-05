using BookstoreApp.Login.Accessor;
using BookstoreApp.Login.Contracts;
using BookstoreApp.Login.Engine;
using BookstoreApp.Login.Manager;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3004")
              .AllowAnyHeader()
              .AllowAnyMethod();
              
    });
});

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<UserEngine>();
builder.Services.AddScoped<UserAccessor>();
builder.Services.AddScoped<IUserManager, UserManager>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");    

app.UseHttpsRedirection();        
app.UseAuthorization();          

app.MapControllers();            

app.Run();
