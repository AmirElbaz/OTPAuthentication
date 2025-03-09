using AuthenticatorService;
using EmailSender.Options;
using OTPService.DataAccess;
using OTPService.Services;
using OTPService;
using Microsoft.EntityFrameworkCore;
using EmailSender.InterFaces;

namespace EmailSender.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEmailServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<OTPContext>(options => options.UseSqlServer(
           configuration.GetConnectionString("DefaultConnection")));
            services.AddScoped<IOTPService>(provider => {
                var dbContext = provider.GetRequiredService<OTPContext>();
                var blockTimeoutMinutes = configuration.GetValue<int>("OTPSettings:BlockTimeoutMinutes", 30);
                return new OtpService(dbContext, blockTimeoutMinutes);
            });
            services.Configure<GmailOptions>(
              configuration.GetSection(GmailOptions.GmailOptionsKey));
            services.AddTransient<IAuthenticatorService, EmailService>();

            services.AddTransient<IEmailSender, EmailSenderService>();
            return services;
        }
    }
}
