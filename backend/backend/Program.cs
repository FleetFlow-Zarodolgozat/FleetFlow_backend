
using Microsoft.EntityFrameworkCore;

namespace backend
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddDbContext<FlottakezeloDbContext>(option =>
            {
                string? coonectionString = builder.Configuration.GetConnectionString("DefaultConnection");
                if (coonectionString == null)
                    throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
                option.UseMySQL(coonectionString);
            });

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/openapi/v1.json", "Backend API v1");
                });
            }

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
