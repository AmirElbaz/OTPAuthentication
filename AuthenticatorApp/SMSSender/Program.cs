
using AuthenticatorService;
using Microsoft.EntityFrameworkCore;
using OTPService;
using OTPService.DataAccess;
using OTPService.Services;
using SMSSender.Services;

namespace SMSSender
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddDbContext<OTPContext>(options => options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddTransient<IAuthenticatorService,SMSService>();
            builder.Services.AddTransient<IOTPService,OtpService>();
            builder.Services.AddHttpClient();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
