using APICore.API.Filters;
using APICore.API.Middlewares;
using APICore.Azure;
using APICore.Azure.Impls;
using APICore.Data.Repository;
using APICore.Data.UoW;
using APICore.Services;
using APICore.Services.Impls;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Localization.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Serilog;
using System.Collections.Generic;
using System.Globalization;

namespace APICore.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            Log.Logger = new LoggerConfiguration()
                         .MinimumLevel.Information()
                         .WriteTo.File("logs/apicore-.log", rollingInterval: RollingInterval.Day)
                         .CreateLogger();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.ConfigureI18N();
            services.ConfigureCors();
            services.AddMvc(config =>
            {
                config.Filters.Add(typeof(ApiValidationFilterAttribute));
                config.EnableEndpointRouting = false;
            }).AddNewtonsoftJson(opt => opt.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore);

            services.ConfigureHsts();

            services.ConfigureDbContext(Configuration);
            services.ConfigureSwagger();
            services.ConfigureTokenAuth(Configuration);
            services.ConfigurePerformance();

            services.ConfigureHealthChecks(Configuration);
            services.ConfigureDetection();

            services.AddHttpContextAccessor();
            services.AddAutoMapper(typeof(Startup));

            services.ConfigureSecuritySettings(Configuration);
            // Creating the blob clients
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Configuration.GetConnectionString("Azure"));
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            // Adding the Azure blob clients as singletons
            services.AddSingleton(blobClient);
            services.AddTransient<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddTransient<IAccountService, AccountService>();
            services.AddTransient<ISettingService, SettingService>();
            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<ILogService, LogService>();

            services.AddTransient<IAzureService, AzureService>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDetection();
            app.UseCors();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API Core V1");
                });
            }
            else
            {
                app.UseHsts();
            }

            #region Localization

            IList<CultureInfo> supportedCultures = new List<CultureInfo>
            {
                new CultureInfo("en-US")
            };
            var localizationOptions = new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture(culture: "en-US", uiCulture: "en-US"),
                SupportedCultures = supportedCultures,
                SupportedUICultures = supportedCultures
            };
            app.UseRequestLocalization(localizationOptions);

            var requestProvider = new RouteDataRequestCultureProvider();
            localizationOptions.RequestCultureProviders.Insert(0, requestProvider);
            var locOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(locOptions.Value);

            #endregion Localization

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware(typeof(ErrorWrappingMiddleware));
            app.UseResponseCompression();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health", new HealthCheckOptions()
                {
                    ResultStatusCodes =
                    {
                        [HealthStatus.Healthy] = StatusCodes.Status200OK,
                        [HealthStatus.Degraded] = StatusCodes.Status200OK,
                        [HealthStatus.Unhealthy] = StatusCodes.Status503ServiceUnavailable
                    }
                });

                endpoints.MapControllers();
            });
        }
    }
}