
using Authenticator.DataAccess;
using AuthenticatorService;
using EmailSender.InterFaces;
using EmailSender.Options;
using EmailSender.Services;
using Microsoft.EntityFrameworkCore;
using OTPService;
using OTPService.DataAccess;
using OTPService.Services;

namespace Authenticator
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
            builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(
            builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddEmailServices(builder.Configuration);
            builder.Services.AddHttpClient();
            builder.Services.AddCors(CorsOptions =>
            {
                CorsOptions.AddPolicy("mypolicy", CorsPolicyBuilder =>
                {
                    CorsPolicyBuilder.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
                });
            });

            var app = builder.Build();
            app.UseCors("mypolicy");
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
