using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ProgrammingQuotesApi.Helpers;
using ProgrammingQuotesApi.Services;
using System.IO;
using System;

namespace ProgrammingQuotesApi
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private readonly string version = "v2";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // called by the runtime, use to configure services
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DataContext>();
            services.AddCors();
            services.AddControllers().AddNewtonsoftJson();
            // services.AddMvc()
            //  .AddJsonOptions(options =>
            //  {
            //      options.JsonSerializerOptions.IgnoreNullValues = true;
            //  });
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(version, new OpenApiInfo
                {
                    Title = "ProgrammingQuotesApi",
                    Description = "Programming Quotes API for open source projects.",
                    Contact = new OpenApiContact
                    {
                        Name = "Damjan Pavlica",
                        Email = "mudroljub@gmail.com",
                        Url = new Uri("https://github.com/mudroljub"),
                    },
                    Version = version
                });
                // generate documentation file
                string xmlPath = Path.Combine(AppContext.BaseDirectory, "ProgrammingQuotesApi.xml");
                c.IncludeXmlComments(xmlPath);
            }).AddSwaggerGenNewtonsoftSupport();

            // Dependency Injection
            services.AddScoped<UserService>();
            services.AddScoped<QuoteService>();
            services.AddScoped<AuthorService>();

            // authentication
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Settings.Secret),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
        }

        // called by the runtime. use to configure HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger(c =>
            {
                c.SerializeAsV2 = true;
            });
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/{version}/swagger.json", $"ProgrammingQuotesApi {version}");
                c.RoutePrefix = string.Empty;
            });

            app.UseHttpsRedirection();
            app.UseRouting();

            // global cors policy
            app.UseCors(x => x
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
