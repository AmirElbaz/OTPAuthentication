using AuthenticatorService;
using Microsoft.EntityFrameworkCore;
using OTPService.DataAccess;
using OTPService.Services;
using OTPService;

namespace SMSSender.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEmailServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<OTPContext>(options => options.UseSqlServer(
           configuration.GetConnectionString("DefaultConnection")));
            services.AddTransient<IOTPService, OtpService>();
            services.AddTransient<IAuthenticatorService, SMSService>();
            return services;
        }
    }
}
