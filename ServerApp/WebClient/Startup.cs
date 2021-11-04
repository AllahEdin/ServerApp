using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Autofac;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

namespace WebClient
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
            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "WebClient", Version = "v1" });
            });
        }

        /// <summary>
        /// ConfigureContainer is where you can register things directly
        /// with Autofac. This runs after ConfigureServices so the things
        /// here will override registrations made in ConfigureServices.
        /// Don't build the container; that gets done for you. If you
        /// need a reference to the container, you need to use the
        /// "Without ConfigureContainer" mechanism shown later.
        /// </summary>
        public void ConfigureContainer(ContainerBuilder builder)
        {
            Assembly[] assemblies = GetAssemblies();
            builder.RegisterAssemblyModules(assemblies);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "WebClient v1"));
            }

            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        public static Assembly[] GetAssemblies()
        {

            string baseDirectoryPath =
                AppDomain.CurrentDomain.BaseDirectory;

            var files = Directory.EnumerateFiles(baseDirectoryPath, "*.dll")
                .Union(Directory.EnumerateFiles(baseDirectoryPath, "*.dll"));

            foreach (string file in files)
            {
                if (AppDomain.CurrentDomain
                    .GetAssemblies()
                    .Any(t => !t.IsDynamic &&
                              file.Equals(t.Location,
                                  StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                AppDomain.CurrentDomain.Load(AssemblyName.GetAssemblyName(file));
            }

            return AppDomain.CurrentDomain
                .GetAssemblies()
                .ToArray();
        }
    }
}
