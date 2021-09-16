using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using RestrictIP.Middleware;
using System;
using System.Net;
using static RestrictIP.Middleware.DomainException;

namespace RestrictIP
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // services.AddTransient<ExceptionHandlingMiddleware>();
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // app.UseExceptionHandler("/error"); // Add this
            app.UseExceptionHandler(a => a.Run(async context =>
            {
                var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                var exception = exceptionHandlerPathFeature.Error;
                int statusCode = (int)HttpStatusCode.InternalServerError;
                var result = JsonConvert.SerializeObject(new { statuscode = statusCode, errorMessage = exception.Message });
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(result);
            }));
            app.Run(context => { throw new Exception("Dhruvin"); });
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseIPFilter();


            if (env.IsDevelopment())
            {

            }
            else
            {
                int val = 5;
                if (val == 5)
                    app.Run(context => { throw new Exception("Dhruvin"); });
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

        }
    }
}
