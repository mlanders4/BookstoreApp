using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Polly;

namespace Bookstore.Checkout.Infrastructure.Configurations
{
    public static class DatabaseConfig
    {
        public static void ConfigureDbContext(IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<CheckoutDbContext>(options =>
            {
                options.UseSqlServer(
                    config.GetConnectionString("CheckoutDB"),
                    sqlOptions => 
                    {
                        sqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 5,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null);
                    });
            });
        }
    }
}
