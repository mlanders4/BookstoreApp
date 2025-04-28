using Microsoft.Extensions.DependencyInjection;
using Bookstore.Checkout.Engine;
using Bookstore.Checkout.Accessors;
using Bookstore.Checkout.Contracts;

namespace Bookstore.Checkout.Extensions
{
    public static class CheckoutServiceExtensions
    {
        public static IServiceCollection AddCheckoutServices(this IServiceCollection services)
        {
            // Register all Checkout services
            services.AddScoped<ICheckoutService, CheckoutEngine>();
            services.AddScoped<IPaymentValidator, PaymentValidator>();
            services.AddScoped<IShippingCalculator, ShippingCalculator>();
            services.AddScoped<IOrderAccessor, OrderAccessor>();
            services.AddScoped<IPaymentAccessor, PaymentAccessor>();
            services.AddScoped<IShippingAccessor, ShippingAccessor>();
            
            return services;
        }
    }
}
